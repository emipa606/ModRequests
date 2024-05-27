using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RandomChance
{
    [HarmonyPatch]
    public static class ToilsRecipeDoRecipeWork_Transpiler
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            MethodInfo targetMethod = typeof(Toils_Recipe).GetNestedTypes(AccessTools.all).SelectMany(innerType => AccessTools.GetDeclaredMethods(innerType))
                    .FirstOrDefault(method => method.Name.Contains("<DoRecipeWork>b__1") && method.ReturnType == typeof(void));
            yield return targetMethod;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo target = AccessTools.Method(typeof(IBillGiverWithTickAction), "UsedThisTick");
            MethodBase addition = AccessTools.Method(typeof(ToilsRecipeDoRecipeWork_Transpiler), nameof(TryGiveRandomFailure));

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.Calls(target))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0); // Load 'actor' onto the stack
                    yield return new CodeInstruction(OpCodes.Ldloc_1); // Load 'curJob' onto the stack
                    yield return new CodeInstruction(OpCodes.Ldloc_2); // Load 'jobDriver' onto the stack
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 7); // Load 'building' onto the stack
                    yield return new CodeInstruction(OpCodes.Call, addition); // Call the TryGiveRandomFailure method
                    continue;
                }
            }
        }

        public static void TryGiveRandomFailure(Pawn actor, Job curJob, JobDriver_DoBill jobDriver, Building_WorkTable building)
        {
            if (!actor.IsColonyMech)
            {
                bool startFire = false;
                bool giveInjury = false;

                float cookingFailureChance = RandomChanceSettings.CookingFailureChance; // 5% by default
                float butcheringMessChance = RandomChanceSettings.ButcheringFailureChance; // 9% by default
                float crematingInjuryChance = RandomChanceSettings.CrematingInjuryChance; // 5% by default
                int pawnsAvgSkillLevel = (int)actor.skills.AverageOfRelevantSkillsFor(actor.CurJob.workGiverDef.workType);

                building = curJob.GetTarget(TargetIndex.A).Thing as Building_WorkTable;

                SimpleCurve chanceCurve = new()
                {
                    { 0, 0.5f },
                    { 3, 0.5f },
                    { 6, 0.3f },
                    { 8, 0.2f },
                    { 14, 0.1f },
                    { 18, 0.05f },
                    { 20, 0.02f }
                };

                if (curJob.RecipeDef.defName.IndexOf("Cook", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (jobDriver.ticksSpentDoingRecipeWork == 1)
                    {
                        if (Rand.Chance(cookingFailureChance))
                        {
                            if (Rand.Chance(chanceCurve.Evaluate(pawnsAvgSkillLevel)))
                            {
                                if (Rand.Bool)
                                {
                                    startFire = true;
                                }
                                else
                                {
                                    giveInjury = true;
                                }
                            }
                        }
                    }

                    if (startFire)
                    {
                        StartFireHandler(actor, curJob, building);
                    }
                    if (giveInjury)
                    {
                        GiveInjuryHandler(actor, curJob, building);
                    }
                }

                if (curJob.RecipeDef == RandomChance_DefOf.ButcherCorpseFlesh)
                {
                    if (jobDriver.ticksSpentDoingRecipeWork == 1)
                    {
                        if (Rand.Chance(butcheringMessChance))
                        {
                            if (Rand.Chance(chanceCurve.Evaluate(pawnsAvgSkillLevel)))
                            {
                                CauseMessHandler(actor, curJob, building);
                            }
                        }
                    }
                }

                if (curJob.RecipeDef == RandomChance_DefOf.CremateCorpse)
                {
                    if (jobDriver.ticksSpentDoingRecipeWork == 1)
                    {
                        if (Rand.Chance(crematingInjuryChance))
                        {
                            if (Rand.Chance(chanceCurve.Evaluate(pawnsAvgSkillLevel)))
                            {
                                GiveInjuryHandler(actor, curJob, building);
                            }
                        }
                    }
                }
            }
        }

        private static void StartFireHandler(Pawn actor, Job curJob, Building_WorkTable building)
        {
            if (RandomChanceSettings.AllowMessages)
            {
                Messages.Message("RC_FireInKitchen".Translate(actor.Named("PAWN")), actor, MessageTypeDefOf.NegativeEvent);
            }

            Thing ingredients = curJob.GetTarget(TargetIndex.B).Thing;
            IntVec3 buildingPos = building.Position;
            Map map = building.Map;

            if (ingredients != null)
            {
                if (!ingredients.Destroyed)
                {
                    ingredients.Destroy();
                }

                FireUtility.TryStartFireIn(buildingPos, map, RandomChanceSettings.FailedCookingFireSize);
                MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistFleeing);
                Find.TickManager.slower.SignalForceNormalSpeedShort();
                actor.stances.stunner.StunFor(120, actor, false, false);
            }
        }

        private static void GiveInjuryHandler(Pawn actor, Job curJob, Building_WorkTable building)
        {
            int pawnsAvgSkillLevel = (int)actor.skills.AverageOfRelevantSkillsFor(actor.CurJob.workGiverDef.workType);
            IntVec3 buildingPos = building.Position;
            Map map = building.Map;
            HediffDef burnHediffDef = HediffDefOf.Burn;
            HediffDef cutHediffDef = HediffDefOf.Cut;

            float severity = pawnsAvgSkillLevel switch
            {
                < 5 => 0.4f,
                <= 10 => 0.2f,
                <= 15 => 0.08f,
                <= 18 => 0.02f,
                <= 20 => 0f,
                _ => 0f,
            };

            if (curJob.RecipeDef.defName.IndexOf("Cook", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                BodyPartRecord fingersPart = actor.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbDigit).RandomElement();

                if (fingersPart != null) 
                {
                    if (Rand.Bool)
                    {
                        Hediff hediff = HediffMaker.MakeHediff(burnHediffDef, actor, fingersPart);
                        hediff.Severity = severity;
                        actor.health.AddHediff(hediff);

                        if (RandomChanceSettings.AllowMessages)
                        {
                            Messages.Message("RC_InjuryInKitchen".Translate(actor.Named("PAWN")),
                                actor, MessageTypeDefOf.NegativeEvent);
                        }
                    }
                    else
                    {
                        Hediff hediff = HediffMaker.MakeHediff(cutHediffDef, actor, fingersPart);
                        hediff.Severity = severity;
                        actor.health.AddHediff(hediff);

                        if (RandomChanceSettings.AllowMessages)
                        {
                            Messages.Message("RC_InjuryInKitchen".Translate(actor.Named("PAWN")),
                                actor, MessageTypeDefOf.NegativeEvent);
                        }

                        IntVec3 adjacentCell = buildingPos + GenAdj.CardinalDirections.RandomElement();
                        FilthMaker.TryMakeFilth(adjacentCell, map, ThingDefOf.Filth_Blood);
                    }
                }
            }
            else if (curJob.RecipeDef == RandomChance_DefOf.CremateCorpse)
            {
                BodyPartRecord bodyPart = actor.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbSegment).RandomElement();

                if (bodyPart != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(burnHediffDef, actor, bodyPart);
                    hediff.Severity = severity;
                    actor.health.AddHediff(hediff);

                    if (RandomChanceSettings.AllowMessages)
                    {
                        Messages.Message("RC_InjuryWhileCremating".Translate(actor.Named("PAWN")),
                            actor, MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
        }

        private static void CauseMessHandler(Pawn actor, Job curJob, Building_WorkTable building)
        {
            if (curJob.GetTarget(TargetIndex.B).Thing is Corpse animalCorpse)
            {
                IntVec3 pawnPos = actor.Position;
                Map map = building.Map;
                int radius = RandomChanceSettings.ButcherMessRadius; // make a mod setting
                IntVec3 centerCell = pawnPos + GenAdj.CardinalDirections.RandomElement();
                Pawn animalPawn = animalCorpse.InnerPawn;

                Region region = centerCell.GetRegion(map);
                if (region != null)
                {
                    foreach (IntVec3 cell in GenRadial.RadialCellsAround(centerCell, radius, true))
                    {
                        if (cell.GetRegion(map) == region)
                        {
                            FilthMaker.TryMakeFilth(cell, map, animalPawn.def.race.BloodDef);
                        }
                    }
                }
                else
                {
                    FilthMaker.TryMakeFilth(centerCell, map, animalPawn.def.race.BloodDef);
                }

                if (RandomChanceSettings.AllowMessages)
                {
                    Messages.Message("RC_HorriblyUncleanKitchen".Translate(actor.Named("PAWN")),
                        actor, MessageTypeDefOf.NegativeEvent);
                }
            }
        }
    }
}
