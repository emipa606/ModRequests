using System.Collections.Generic;
using HarmonyLib;
using InsectoidBioengineering;
using Verse;

namespace BenLubarsVanillaExpandedPatches.Insectoids
{
    [HarmonyPatch(typeof(Building_BioengineeringIncubator), nameof(Building_BioengineeringIncubator.GetGizmos))]
    static class Building_BioengineeringIncubator_GetGizmos_Patch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building_BioengineeringIncubator __instance)
        {
			string expectedLabel = "VFEI_Engage".Translate();
			string expectedDesc = "VFEI_EngageDesc".Translate();

            foreach (var gizmo in __result)
            {
				if (BenLubarsVanillaExpandedPatches.previewIncubatedInsectoid && gizmo is Command_Action cmd && cmd.defaultLabel == expectedLabel && cmd.defaultDesc == expectedDesc)
                {
                    bool anyMatching = false;
                    InsectoidCombinationDef matchingCombo = null;

					foreach (var combo in DefDatabase<InsectoidCombinationDef>.AllDefs)
					{
						if (MatchingCombination(combo, __instance.theFirstGenomeIAmGoingToInsert, __instance.theSecondGenomeIAmGoingToInsert, __instance.theThirdGenomeIAmGoingToInsert))
                        {
                            if (anyMatching)
                            {
                                matchingCombo = null;
                                break;
                            }

                            matchingCombo = combo;
                            anyMatching = true;
                        }
					}

                    if (!anyMatching)
                    {
                        cmd.disabled = true;
                        cmd.disabledReason = "VFEI_CombinationNoResults".Translate();
                    }
                    else
                    {
                        if (matchingCombo != null && matchingCombo.result.Count == 1)
                        {
                            var pawnKind = PawnKindDef.Named(matchingCombo.result[0]);
                            cmd.defaultLabel = "BenLubarsVanillaExpandedPatches_EngageBioIncubator".Translate(pawnKind.race.label.Named("NAME"));
                            cmd.defaultDesc = "BenLubarsVanillaExpandedPatches_EngageBioIncubatorDesc".Translate(pawnKind.race.label.Named("NAME"), pawnKind.race.description.Named("DESC"));
                            cmd.icon = pawnKind.race.uiIcon;
                        }

                        if (__instance.ExpectingFirstGenome || __instance.ExpectingSecondGenome || __instance.ExpectingThirdGenome)
                        {
                            cmd.disabled = true;
                            cmd.disabledReason = "VFEI_WaitTillJobsEnd".Translate();
                        }
                    }

                    if (!__instance.compPowerTrader.PowerOn)
                    {
                        cmd.disabled = true;
                        cmd.disabledReason = "VFEI_NotPowered".Translate();
                    }
				}

				yield return gizmo;
            }
        }

        public static bool MatchingCombination(InsectoidCombinationDef combo, string genome1, string genome2, string genome3)
        {
            return match(0, 1, 2) || match(0, 2, 1) || match(1, 0, 2) || match(1, 2, 0) || match(2, 0, 1) || match(2, 1, 0);

            bool match(int first, int second, int third)
            {
                return genome1 == combo.genomes[first] && genome2 == combo.genomes[second] && genome3 == combo.genomes[third];
            }
        }
    }
}
