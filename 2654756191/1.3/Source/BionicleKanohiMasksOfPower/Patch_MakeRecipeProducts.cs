using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BionicleKanohiMasksOfPower
{
    public class Recipe_TransferQuality : RecipeWorker
    {

    }
    [HarmonyPatch(typeof(GenRecipe))]
    [HarmonyPatch("MakeRecipeProducts")]
    public static class Patch_MakeRecipeProducts
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
        {
            var qualities = new List<QualityCategory>();
            foreach (var i in ingredients)
            {
                if (i.TryGetQuality(out QualityCategory qc))
                {
                    qualities.Add(qc);//compiles list of qualities
                }
            }

            foreach (var r in __result)
            {
                if (qualities.Any() && recipeDef.workerClass == typeof(Recipe_TransferQuality))
                {
                    var comp = r.TryGetComp<CompQuality>();
                    if (comp != null)
                    {
                        comp.SetQuality(qualities.Max(), ArtGenerationContext.Colony);//finds best quality
                    }
                }
                yield return r;
            }
        }

        public static bool IsMasterworkOrLegendary(this Thing thing)//checks quality of mask
        {
            if (thing.TryGetQuality(out var qc))
            {
                return qc == QualityCategory.Masterwork || qc == QualityCategory.Legendary;
            }
            return false;
        }
    }
}