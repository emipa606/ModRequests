using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using System.Reflection;

// Xenogerm: "implant into prisoner" gizmo
namespace zed_0xff.GeneCollectorQOL {
    [HarmonyPatch(typeof(Xenogerm), nameof(Xenogerm.GetGizmos))]
    static class Patch_GetGizmos
    {
        private static readonly CachedTexture ImplantTex = new CachedTexture("UI/Gizmos/ImplantGenesPrisoner");

        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Xenogerm __instance)
        {
            bool was = !ModConfig.Settings.patchXenogerm_prisonersGizmo;
            foreach( Gizmo gizmo in __result ){
                yield return gizmo;
                if( !was && gizmo is Command_Action ca && (
                        (ca.defaultLabel == "ImplantXenogerm".Translate() + "...") ||
                        (ca.icon.name == "ImplantGenes")
                  )){
                    was = true;
                    yield return new Command_Action{
                        defaultLabel = "Implant into a prisoner...",
                        icon = ImplantTex.Texture,
                        action = delegate
                        {
                            List<FloatMenuOption> list = new List<FloatMenuOption>();
                            foreach (Pawn item in __instance.Map.mapPawns.AllPawnsSpawned)
                            {
                                Pawn pawn = item;
                                if (!pawn.IsQuestLodger() && pawn.genes != null && pawn.IsPrisonerOfColony)
                                {
                                    int num = GeneUtility.MetabolismAfterImplanting(pawn, __instance.GeneSet);
                                    if (num < GeneTuning.BiostatRange.TrueMin)
                                    {
                                        list.Add(new FloatMenuOption((string)(pawn.LabelShortCap + ": " + "ResultingMetTooLow".Translate() + " (") + num + ")", null, pawn, Color.white));
                                    }
                                    else if (__instance.PawnIdeoDisallowsImplanting(pawn))
                                    {
                                        list.Add(new FloatMenuOption(pawn.LabelShortCap + ": " + "IdeoligionForbids".Translate(), null, pawn, Color.white));
                                    }
                                    else
                                    {
                                        list.Add(new FloatMenuOption(pawn.LabelShortCap + ", " + pawn.genes.XenotypeLabelCap, delegate
                                                    {
                                                    __instance.SetTargetPawn(pawn);
                                                    }, pawn, Color.white));
                                    }
                                }
                            }
                            if (!list.Any())
                            {
                                list.Add(new FloatMenuOption("NoImplantablePawns".Translate(), null));
                            }
                            Find.WindowStack.Add(new FloatMenu(list));
                        }
                    };
                }
            }
        }
    }

    // only show "xenogerm implantation ordered" letter if there's something wrong, i.e. no bed or no medicine
    [HarmonyPatch(typeof(Xenogerm), "SendImplantationLetter")]
    static class Patch_SendImplantationLetter
    {
        static bool Prefix(ref Xenogerm __instance, Pawn targetPawn)
        {
            if( !ModConfig.Settings.patchXenogerm_hideLetter ) return true;

            bool show = false;
            if (!targetPawn.InBed() && !targetPawn.Map.listerBuildings.allBuildingsColonist.Any((Building x) => x is Building_Bed && RestUtility.CanUseBedEver(targetPawn, x.def) && ((Building_Bed)x).Medical)){
                show = true;
            }
            int requiredMedicineForImplanting = RequiredMedicineForImplanting;
            if (targetPawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine).Sum((Thing x) => x.stackCount) < requiredMedicineForImplanting) {
                show = true;
            }
            return show;
        }

        private static int RequiredMedicineForImplanting
        {
            get
            {
                int num = 0;
                for (int i = 0; i < RecipeDefOf.ImplantXenogerm.ingredients.Count; i++)
                {
                    IngredientCount ingredientCount = RecipeDefOf.ImplantXenogerm.ingredients[i];
                    if (ingredientCount.filter.Allows(ThingDefOf.MedicineIndustrial))
                    {
                        num += (int)ingredientCount.GetBaseCount();
                    }
                }
                return num;
            }
        }
    }
}
