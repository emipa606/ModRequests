using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace SimpleWarrants.HarmonyPatches
{
    [HarmonyPatch(typeof(GenHostility), "AnyHostileActiveThreatTo",
    new[] { typeof(Map), typeof(Faction), typeof(IAttackTarget), typeof(bool), typeof(bool) },
    new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal })]
    internal static class AnyHostileActiveThreatTo_Patch
    {
        public static Dictionary<Map, Faction> lastFactionThreats = new();

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(ref bool __result, Map map, Faction faction, ref IAttackTarget threat)
        {
            if (__result && !map.IsPlayerHome && threat is Pawn { Faction: { } } pawn && pawn.Faction.def.humanlikeFaction)
            {
                lastFactionThreats[map] = pawn.Faction;
            }
        }

        public static Faction GetLastHostileFactionFromMap(Map map)
        {
            return lastFactionThreats.TryGetValue(map, out Faction faction)
                ? faction
                : map.ParentFaction != null && map.ParentFaction.def.humanlikeFaction && map.ParentFaction.HostileTo(Faction.OfPlayer)
                ? map.ParentFaction
                : null;
        }
    }

    //[HarmonyPatch(typeof(FormCaravanComp), "CompTick")]
    //public static class FormCaravanComp_CompTick_Patch
    //{
    //    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
    //    {
    //        var anyUnexploredFoggedRooms = AccessTools.Method(typeof(FormCaravanComp), "get_AnyUnexploredFoggedRooms");
    //        var codes = codeInstructions.ToList();
    //        for (var i = 0; i < codes.Count; i++)
    //        {
    //            if (i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].Calls(anyUnexploredFoggedRooms) && codes[i + 2].opcode == OpCodes.Brfalse_S)
    //            {
    //                yield return new CodeInstruction(OpCodes.Ldarg_0);
    //                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FormCaravanComp_CompTick_Patch), nameof(RegisterAssault)));
    //            }
    //            yield return codes[i];
    //        }
    //    }
    //
    //    public static void RegisterAssault(FormCaravanComp __instance)
    //    {
    //        if (__instance.parent is MapParent mapParent && mapParent.Map != null && Rand.Chance(0.25f))
    //        {
    //            var faction = AnyHostileActiveThreatTo_Patch.GetLastHostileFactionFromMap(mapParent.Map);
    //            if (faction != null)
    //            {
    //                var pawns = mapParent.Map.mapPawns.FreeHumanlikesOfFaction(Faction.OfPlayer).Where(x => WarrantsManager.Instance.CanPutWarrantOn(x));
    //                if (pawns.Any())
    //                {
    //                    var random = pawns.RandomElement();
    //                    WarrantsManager.Instance.PutWarrantOn(random, "SW.Assault".Translate(), faction);
    //                }
    //            }
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(SettlementDefeatUtility), "CheckDefeated")]
    public static class SettlementDefeatUtility_CheckDefeated_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            System.Reflection.MethodInfo notify_PlayerRaidedSomeone = AccessTools.Method(typeof(IdeoUtility), nameof(IdeoUtility.Notify_PlayerRaidedSomeone));
            List<CodeInstruction> codes = codeInstructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (codes[i].Calls(notify_PlayerRaidedSomeone))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SettlementDefeatUtility_CheckDefeated_Patch), nameof(RegisterAssault)));
                }
            }
        }

        public static void RegisterAssault(Map map)
        {
            Faction faction = AnyHostileActiveThreatTo_Patch.GetLastHostileFactionFromMap(map);
            if (faction == null)
            {
                return;
            }

            // Do not count aggression against enemies as assault.
            if (faction.HostileTo(Faction.OfPlayer))
            {
                return;
            }

            // 50% chance.
            if (!Rand.Chance(0.5f))
            {
                return;
            }

            // Get a random player pawn from the attackers and slap a bounty on them.
            IEnumerable<Pawn> pawns = map.mapPawns.FreeHumanlikesOfFaction(Faction.OfPlayer).Where(WarrantsManager.Instance.CanPutWarrantOn);
            if (pawns.TryRandomElement(out Pawn selectedPawn))
            {
                WarrantsManager.Instance.PutWarrantOn(selectedPawn, "SW.Assault".Translate(), faction);
            }
        }
    }
}
