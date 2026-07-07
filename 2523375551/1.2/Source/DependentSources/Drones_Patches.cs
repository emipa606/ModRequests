using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFE.Mechanoids;
using VFE.Mechanoids.HarmonyPatches;
using VFE.Mechanoids.Needs;
using VFEMech;

namespace RedScare
{
    [HarmonyPatch]
    public class MechSpawn_Patch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(SymbolResolver_RandomMechanoidGroup).GetNestedTypes(AccessTools.all)
                                                                 .First(t => t.GetMethods(AccessTools.all)
                                                                           .Any(mi => mi.ReturnType == typeof(bool) && mi.GetParameters()[0].ParameterType == typeof(PawnKindDef)))
                                                                 .GetMethods(AccessTools.all)
                                                                 .First(mi => mi.ReturnType == typeof(bool));

            yield return typeof(MechClusterGenerator).GetNestedTypes(AccessTools.all).MaxBy(t => t.GetMethods(AccessTools.all).Length).GetMethods(AccessTools.all)
                                                  .First(mi => mi.ReturnType == typeof(bool) && mi.GetParameters()[0].ParameterType == typeof(PawnKindDef));
        }

        [HarmonyPostfix]
        public static void Postfix(PawnKindDef __0, ref bool __result) =>
            __result &= !__0.race.defName.StartsWith("FRS");
    }
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.ExitMap))]
    public static class ForbidPlayerPawnsFromDissapearingIntoTheAbyss
    {
        public static bool Prefix(Pawn __instance, bool __0)
        {
            if (!__0)
                return true;
            if (__instance is Machine && __instance.Faction == Faction.OfPlayer)
            {
                if (!CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(__instance))
                {
                    return false;
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(CaravanExitMapUtility), nameof(CaravanExitMapUtility.FindCaravanToJoinFor))]
    public static class DebugStuff
    {
        public static void Postix(Pawn __0, Caravan __result)
        {
            Log.Message($"Find caravan {__0.Name} {__result != null}");
        }
    }
    [HarmonyPatch(typeof(CompMachine), nameof(CompMachine.PostSpawnSetup))]
    public static class FixPowerInMachine
    {
        public static void Prefix(ref CompMachine __instance, out float __state)
        {
            __state = (__instance.parent as Pawn).needs.TryGetNeed<Need_Power>()?.CurLevel ?? 0;
        }
        public static void Postfix(ref CompMachine __instance, float __state)
        {
            (__instance.parent as Pawn).needs.TryGetNeed<Need_Power>().CurLevel = __state;
        }
    }
    [HarmonyPatch(typeof(Need_Power), nameof(Need_Power.NeedInterval))]
    public static class NeedPowerFixNullRefOnMissingChargingPad
    {
        public static bool Prefix(ref Need_Power __instance)
        {
            if (__instance.ChargingStationComp == null)
            {
                __instance.CurLevel = __instance.MaxLevel;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(CaravanUIUtility), nameof(CaravanUIUtility.AddPawnsSections))]
    public static class AddWidgetForLongtermDrones
    {
        public static void Postfix(TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
        {
            IEnumerable<TransferableOneWay> source = from x in transferables
                                                     where x.ThingDef.category == ThingCategory.Pawn
                                                     select x;
            widget.AddSection("MechanoidsSection".Translate(), from x in source
                                                            where (((Pawn)x.AnyThing).GetComp<CompMachine>()?.Props.hoursActive ?? 0) >= 24000 
                                                            select x);
        }
    }
}
