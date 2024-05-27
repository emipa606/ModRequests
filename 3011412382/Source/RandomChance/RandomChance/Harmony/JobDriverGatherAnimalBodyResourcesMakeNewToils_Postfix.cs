using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RandomChance
{
    [HarmonyPatch(typeof(JobDriver_GatherAnimalBodyResources), "MakeNewToils")]
    public static class JobDriverGatherAnimalBodyResourcesMakeNewToils_Postfix
    {
        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<Toil> __result, JobDriver_GatherAnimalBodyResources __instance)
        {
            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            Toil customToil = new Toil
            {
                initAction = delegate
                {
                    if (!__instance.pawn.IsColonyMech)
                    {
                        float workerInjuryChance = RandomChanceSettings.HurtByFarmAnimalChance; // 5% by default

                        if (Rand.Chance(workerInjuryChance))
                        {
                            SimpleCurve chanceCurve = new()
                            {
                                { 0, 0.55f }, // pawn skill lvl, chance %
                                { 3, 0.45f },
                                { 6, 0.35f },
                                { 8, 0.20f },
                                { 14, 0.10f },
                                { 18, 0.05f },
                                { 20, 0.02f }
                            };

                            float pawnsAvgSkillLevel = __instance.pawn.skills.AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);

                            if (Rand.Chance(chanceCurve.Evaluate(pawnsAvgSkillLevel)))
                            {
                                Pawn animal = __instance.job.GetTarget(TargetIndex.A).Thing as Pawn;
                                List<Tool> tools = animal.Tools;
                                if (animal != null && tools != null && tools.Count > 0)
                                {
                                    Tool selectedTool = tools.RandomElement();
                                    if (Rand.Chance(selectedTool.chanceFactor) && selectedTool != null)
                                    {
                                        float damageAmount = Rand.Range(selectedTool.power / 2f, selectedTool.power);
                                        DamageDef damageInflicted = selectedTool.Maneuvers.RandomElement().verb.meleeDamageDef;
                                        DamageInfo damageInfo = new(damageInflicted, damageAmount, 1f);
                                        __instance.pawn.TakeDamage(damageInfo);

                                        if (damageAmount > 0f && RandomChanceSettings.AllowMessages)
                                        {
                                            Messages.Message("RC_HurtByFarmAnimal".Translate(__instance.pawn.Named("PAWN"),
                                                animal.NameShortColored), __instance.pawn, MessageTypeDefOf.NegativeEvent);
                                        }

                                        Find.TickManager.slower.SignalForceNormalSpeedShort();
                                        __instance.pawn.stances.stunner.StunFor(60, __instance.pawn, false, false);
                                        __instance.EndJobWith(JobCondition.Incompletable);
                                    }
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
