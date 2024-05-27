using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace RandomChance
{
    [HarmonyPatch(typeof(JobDriver_FixBrokenDownBuilding), "MakeNewToils")]
    public class JobDriverFixBrokenDownBuildingMakeNewToils_Postfix
    {
        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<Toil> __result, JobDriver_FixBrokenDownBuilding __instance)
        {
            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            // new toil with delegate
            Toil customToil = new Toil
            {
                initAction = delegate
                {
                    if (!__instance.pawn.IsColonyMech)
                    {
                        float averageSkills = __instance.pawn.skills.AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);
                        float failureChance = RandomChanceSettings.ElectricalRepairFailureChance; // 5% by default
                        if (Rand.Chance(failureChance))
                        {
                            Building building = __instance.job.GetTarget(TargetIndex.A).Thing as Building;

                            SimpleCurve damageChanceCurve = new()
                            {
                                { 0, 0.5f },
                                { 3, 0.5f },
                                { 6, 0.3f },
                                { 8, 0.2f },
                                { 14, 0.1f },
                                { 18, 0.05f },
                                { 20, 0.02f }
                            };

                            if (Rand.Chance(damageChanceCurve.Evaluate(averageSkills)))
                            {
                                if (building != null)
                                {
                                    IntVec3 explosionCenter = building.Position;
                                    Map explosionMap = building.Map;
                                    int explosionRadius = Rand.RangeInclusive(1, 5);

                                    GenExplosion.DoExplosion(explosionCenter, explosionMap, explosionRadius, DamageDefOf.EMP, null, Rand.RangeInclusive(1, 9));
                                    if (Rand.Value < 0.25f)
                                    {
                                        GenExplosion.DoExplosion(explosionCenter, explosionMap, explosionRadius, DamageDefOf.Bomb, null, Rand.RangeInclusive(3, 12));
                                        FireUtility.TryStartFireIn(building.Position, building.Map, RandomChanceSettings.FailedCookingFireSize);
                                        MoteMaker.MakeColonistActionOverlay(__instance.pawn, ThingDefOf.Mote_ColonistFleeing);
                                    }

                                    if (RandomChanceSettings.AllowMessages)
                                    {
                                        Messages.Message("RC_ElectricalRepairFailure".Translate(__instance.pawn.Named("PAWN")),
                                        __instance.pawn, MessageTypeDefOf.NegativeEvent);
                                    }

                                    if (Rand.Value < 0.20f)
                                    {
                                        IncidentDef incident = IncidentDef.Named("ShortCircuit");
                                        IncidentParms parms = StorytellerUtility.DefaultParmsNow(incident.category, building.Map);
                                        incident.Worker.TryExecute(parms);
                                    }
                                }

                                BodyPartRecord bodyPart = __instance.pawn.RaceProps.body.corePart;

                                if (bodyPart != null)
                                {
                                    HediffDef electricShock = RandomChance_DefOf.RC_ElectricShockHediff;

                                    Hediff hediff = HediffMaker.MakeHediff(electricShock, __instance.pawn);
                                    hediff.Severity = Rand.Value;
                                    __instance.pawn.health.AddHediff(hediff, bodyPart);

                                    /*Effecter shockEffect;

                                    shockEffect = RandomChance_DefOf.RC_ElectricShockBonesEffect.Spawn();
                                    shockEffect.Trigger(__instance.pawn, __instance.pawn);

                                    shockEffect.Cleanup();
                                    shockEffect = null;*/

                                    Find.TickManager.slower.SignalForceNormalSpeedShort();
                                    __instance.pawn.stances.stunner.StunFor(60, __instance.pawn, false, false);

                                    __instance.EndJobWith(JobCondition.Incompletable);
                                }
                            }
                        }
                    }
                }
            };

            // insert new toil before the last one in the list
            int insertIndex = newToils.Count - 1;
            newToils.Insert(insertIndex, customToil);

            __result = newToils;
        }
    }
}