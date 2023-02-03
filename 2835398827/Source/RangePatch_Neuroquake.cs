using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ShardMods
{
    [HarmonyPatch(typeof(CompAbilityEffect_Neuroquake))]
    [HarmonyPriority(Priority.Last)]
    class RangePatch_Neuroquake
    {

        [HarmonyPrefix]
        [HarmonyPatch("Apply")]
        static bool Apply_Prefix(CompAbilityEffect_Neuroquake __instance, LocalTargetInfo target, LocalTargetInfo dest)
        {

            float TempEffectRadius = __instance.parent.def.EffectRadius * __instance.parent.pawn.GetStatValue(StatDefOf.PsychicSensitivity);
            int TempWorldRangeTiles = (int)(__instance.Props.worldRangeTiles * __instance.parent.pawn.GetStatValue(StatDefOf.PsychicSensitivity));
            float TempMentalStateRadius = __instance.Props.mentalStateRadius * __instance.parent.pawn.GetStatValue(StatDefOf.PsychicSensitivity);
            if (__instance.affectedFactions == null)
                __instance.affectedFactions = new Dictionary<Faction, Pair<bool, Pawn>>();
            else
                __instance.affectedFactions.Clear();
            __instance.giveMentalStateTo.Clear();
            foreach (Pawn pawn in __instance.parent.pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (__instance.CanApplyEffects(pawn) && !pawn.Fogged())
                {
                    bool flag = !pawn.Spawned || pawn.Position.InHorDistOf(__instance.parent.pawn.Position, TempEffectRadius) || !pawn.Position.InHorDistOf(__instance.parent.pawn.Position, TempMentalStateRadius);
                    __instance.AffectGoodwill(pawn.HomeFaction, !flag, pawn);
                    if (!flag)
                        __instance.giveMentalStateTo.Add(pawn);
                    else
                        __instance.GiveNeuroquakeThought(pawn);
                }
            }
            foreach (Map map in Find.Maps)
            {
                if (map != __instance.parent.pawn.Map && Find.WorldGrid.TraversalDistanceBetween(map.Tile, __instance.parent.pawn.Map.Tile, true, TempWorldRangeTiles + 1) <= TempWorldRangeTiles)
                {
                    foreach (Pawn allPawn in map.mapPawns.AllPawns)
                    {
                        if (__instance.CanApplyEffects(allPawn))
                            __instance.GiveNeuroquakeThought(allPawn);
                    }
                }
            }
            foreach (Caravan caravan in Find.WorldObjects.Caravans)
            {
                if (Find.WorldGrid.TraversalDistanceBetween(caravan.Tile, __instance.parent.pawn.Map.Tile, true, TempWorldRangeTiles + 1) <= TempWorldRangeTiles)
                {
                    foreach (Pawn pawn in caravan.pawns)
                    {
                        if (__instance.CanApplyEffects(pawn))
                            __instance.GiveNeuroquakeThought(pawn);
                    }
                }
            }
            foreach (Pawn p in __instance.giveMentalStateTo)
            {
                CompAbilityEffect_GiveMentalState.TryGiveMentalStateWithDuration(p.RaceProps.IsMechanoid ? MentalStateDefOf.BerserkMechanoid : MentalStateDefOf.Berserk, p, __instance.parent.def, StatDefOf.PsychicSensitivity, __instance.parent.pawn);
                RestUtility.WakeUp(p);
            }
            foreach (Faction allFaction in Find.FactionManager.AllFactions)
            {
                if (!allFaction.IsPlayer && !allFaction.defeated && !allFaction.HostileTo(Faction.OfPlayer))
                    __instance.AffectGoodwill(allFaction, false, (Pawn)null);
            }
            if (__instance.parent.pawn.Faction == Faction.OfPlayer)
            {
                foreach (KeyValuePair<Faction, Pair<bool, Pawn>> affectedFaction in __instance.affectedFactions)
                {
                    Faction key = affectedFaction.Key;
                    Pair<bool, Pawn> pair = affectedFaction.Value;
                    int num = pair.First ? 1 : 0;
                    pair = affectedFaction.Value;
                    Pawn second = pair.Second;
                    int goodwillChange = num != 0 ? __instance.Props.goodwillImpactForBerserk : __instance.Props.goodwillImpactForNeuroquake;
                    Faction.OfPlayer.TryAffectGoodwillWith(key, goodwillChange, true, true, HistoryEventDefOf.UsedHarmfulAbility, new GlobalTargetInfo?());
                }
            }
            ReversePatch_CompAbilityEffect.Apply(__instance, target, dest);
            //__instance.base.Apply(target, dest);
            __instance.affectedFactions.Clear();
            __instance.giveMentalStateTo.Clear();
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch("OnGizmoUpdate")]
        static bool OnGizmoUpdate_Prefix(CompAbilityEffect_Neuroquake __instance)
        {
            float TempMentalStateRadius = __instance.Props.mentalStateRadius * __instance.parent.pawn.GetStatValue(StatDefOf.PsychicSensitivity);
            if (!CompAbilityEffect_Neuroquake.cachedRadiusCellsTarget.HasValue || CompAbilityEffect_Neuroquake.cachedRadiusCellsTarget.Value == __instance.parent.pawn.Position || CompAbilityEffect_Neuroquake.cachedRadiusCellsMap != __instance.parent.pawn.Map)
            {
                CompAbilityEffect_Neuroquake.cachedRadiusCells.Clear();
                foreach (IntVec3 allCell in __instance.parent.pawn.Map.AllCells)
                {
                    if (allCell.InHorDistOf(__instance.parent.pawn.Position, (TempMentalStateRadius)))
                        CompAbilityEffect_Neuroquake.cachedRadiusCells.Add(allCell);
                }
                CompAbilityEffect_Neuroquake.cachedRadiusCellsTarget = new IntVec3?(__instance.parent.pawn.Position);
            }
            GenDraw_Custom.DrawFieldEdges(CompAbilityEffect_Neuroquake.cachedRadiusCells);
            return false;
        }
    }
}