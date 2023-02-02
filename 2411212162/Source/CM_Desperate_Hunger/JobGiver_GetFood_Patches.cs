using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace CM_Desperate_Hunger
{
    [StaticConstructorOnStartup]
    public static class JobGiver_GetFood_Patches
    {
        [HarmonyPatch(typeof(JobGiver_GetFood))]
        [HarmonyPatch("TryGiveJob", MethodType.Normal)]
        public class JobGiver_GetFood_TryGiveJob
        {
            private static List<Pawn> tempPreyCandidates = new List<Pawn>();

            [HarmonyPostfix]
            public static void Postfix(Pawn pawn, JobGiver_GetFood __instance, ref Job __result)
            {
                // Don't need to do this if TryGiveJob was already successful, or if the animal is not starving
                if (__result != null || !DesperateHungerMod.settings.featureEnabled || pawn.needs.food.CurCategory != HungerCategory.Starving || (!DesperateHungerMod.settings.desperateHumans && !pawn.AnimalOrWildMan()))
                    return;

                // Find something to eat
                Pawn prey = BestPawnToHuntForPredator(pawn, __instance.forceScanWholeMap);

                if (prey != null)
                {
                    Hediff malnutrition = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
                    float malnutritionLevel = (malnutrition == null) ? 0.0f : malnutrition.Severity;

                    float preyHealth = prey.health.summaryHealth.SummaryHealthPercent;
                    bool preyWounded = preyHealth < 0.99f;

                    // Check if malnourish enough to attack target
                    bool willAttack = (malnutritionLevel > DesperateHungerMod.settings.minimumMalnutritionToHuntHealthyTarget || (preyWounded && malnutritionLevel > DesperateHungerMod.settings.minimumMalnutritionToHuntWoundedTarget));

                    if (willAttack)
                    {
                        __result = JobMaker.MakeJob(JobDefOf.PredatorHunt, prey);
                        __result.killIncappedTarget = true;
                    }
                }
            }

            // Copied with minor changes so we don't have to call a private function on the FoodUtility object
            private static Pawn BestPawnToHuntForPredator(Pawn predator, bool forceScanWholeMap)
            {
                if (predator.meleeVerbs.TryGetMeleeVerb(null) == null)
                {
                    return null;
                }
                bool seriouslyWounded = (predator.health.summaryHealth.SummaryHealthPercent < 0.25f);

                tempPreyCandidates.Clear();
                if (GetMaxRegionsToScan(predator, forceScanWholeMap) < 0)
                {
                    tempPreyCandidates.AddRange(predator.Map.mapPawns.AllPawnsSpawned);
                }
                else
                {
                    TraverseParms traverseParms = TraverseParms.For(predator);
                    RegionTraverser.BreadthFirstTraverse(predator.Position, predator.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate (Region x)
                    {
                        List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
                        for (int j = 0; j < list.Count; j++)
                        {
                            tempPreyCandidates.Add((Pawn)list[j]);
                        }
                        return false;
                    });
                }

                Pawn bestPrey = null;
                float bestPreyScore = 0f;
                bool tutorialMode = TutorSystem.TutorialMode;
                foreach(Pawn potentialPrey in tempPreyCandidates)
                {
                    bool sameRoom = predator.GetRoom() == potentialPrey.GetRoom();
                    bool acceptablePrey = IsAcceptablePreyFor(predator, potentialPrey);

                    if (predator != potentialPrey && sameRoom && (!seriouslyWounded || potentialPrey.Downed) && acceptablePrey && predator.CanReach(potentialPrey, PathEndMode.ClosestTouch, Danger.Deadly) && !potentialPrey.IsForbidden(predator) && (!tutorialMode || potentialPrey.Faction != Faction.OfPlayer))
                    {
                        float preyScoreFor = GetPreyScoreFor(predator, potentialPrey);

                        if (preyScoreFor > bestPreyScore || bestPrey == null)
                        {
                            bestPreyScore = preyScoreFor;
                            bestPrey = potentialPrey;
                        }
                    }
                }
                tempPreyCandidates.Clear();
                return bestPrey;
            }

            // Copied so we don't have to call a private function on the FoodUtility object
            private static int GetMaxRegionsToScan(Pawn getter, bool forceScanWholeMap)
            {
                if (getter.RaceProps.Humanlike)
                {
                    return -1;
                }
                if (forceScanWholeMap)
                {
                    return -1;
                }
                if (getter.Faction == Faction.OfPlayer)
                {
                    return 100;
                }
                return 30;
            }

            private static bool IsAcceptablePreyFor(Pawn predator, Pawn prey)
            {
                //if (!prey.RaceProps.canBePredatorPrey)
                //{
                //    return false;
                //}
                if (!prey.RaceProps.IsFlesh)
                {
                    return false;
                }
                if (!Find.Storyteller.difficulty.predatorsHuntHumanlikes && prey.RaceProps.Humanlike)
                {
                    return false;
                }
                if (!DesperateHungerMod.settings.desperatePredatorsIgnoreSize && predator.RaceProps.predator && prey.BodySize > predator.RaceProps.maxPreyBodySize)
                {
                    return false;
                }
                if (!predator.RaceProps.predator && !DesperateHungerMod.settings.desperatePreyIgnoreSize && prey.BodySize > predator.BodySize * DesperateHungerMod.settings.preyTargetMaxRelativeSize)
                {
                    return false;
                }
                if (!prey.Downed && !DesperateHungerMod.settings.desperateAnimalsIgnoreCombatPower)
                {
                    if (prey.kindDef.combatPower > 2f * predator.kindDef.combatPower)
                    {
                        return false;
                    }
                    float preyTotalCombatPower = prey.kindDef.combatPower * prey.health.summaryHealth.SummaryHealthPercent * prey.ageTracker.CurLifeStage.bodySizeFactor;
                    float predatorTotalCombatPower = predator.kindDef.combatPower * predator.health.summaryHealth.SummaryHealthPercent * predator.ageTracker.CurLifeStage.bodySizeFactor;
                    if (preyTotalCombatPower >= predatorTotalCombatPower)
                    {
                        return false;
                    }
                }
                if (!DesperateHungerMod.settings.desperateAnimalsAttackAllies && predator.Faction != null && prey.Faction != null && !predator.HostileTo(prey))
                {
                    return false;
                }
                if (!DesperateHungerMod.settings.desperateAnimalsAttackAllies && predator.Faction != null && prey.HostFaction != null && !predator.HostileTo(prey))
                {
                    return false;
                }
                if (!DesperateHungerMod.settings.desperateAnimalsAttackAllies && predator.Faction == Faction.OfPlayer && prey.Faction == Faction.OfPlayer)
                {
                    return false;
                }
                if (!DesperateHungerMod.settings.desperateHerdAnimalsEatOwnKind && predator.RaceProps.herdAnimal && predator.def == prey.def)
                {
                    return false;
                }
                return true;
            }

            // Copied with minor changes so we don't have to call a private function on the FoodUtility object
            private static float GetPreyScoreFor(Pawn predator, Pawn prey)
            {
                if (DesperateHungerMod.settings.desperatePreyTargetRandomly && !predator.RaceProps.predator)
                    return Rand.Value;

                float preyRelativeStrength = prey.kindDef.combatPower / predator.kindDef.combatPower;
                float preyHealth = prey.health.summaryHealth.SummaryHealthPercent;
                float bodySizeFactor = prey.ageTracker.CurLifeStage.bodySizeFactor;
                float lengthHorizontal = (predator.Position - prey.Position).LengthHorizontal;
                if (prey.Downed)
                {
                    preyHealth = Mathf.Min(preyHealth, 0.2f);
                }
                float num3 = 0f - lengthHorizontal - 56f * preyHealth * preyHealth * preyRelativeStrength * bodySizeFactor;
                if (prey.RaceProps.Humanlike)
                {
                    num3 -= 35f;
                }
                return num3;
            }
        }
    }
}
