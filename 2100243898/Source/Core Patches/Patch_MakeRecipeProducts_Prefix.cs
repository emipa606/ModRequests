using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using HarmonyLib;
using Verse;

namespace DTechprinting
{
    [HarmonyPatch(typeof(GenRecipe))]
    [HarmonyPatch("MakeRecipeProducts")]
    class Patch_MakeRecipeProducts_Prefix
    {
        private static bool Prefix(RecipeDef recipeDef, out bool __state)
        {
            if (recipeDef == Base.DefOf.DTechprintingRecipe || recipeDef == Base.DefOf.DTechprintingStackRecipe)
            {
                __state = true;
                return false;
            }
            __state = false;
            return true;
        }

        private static IEnumerable<Thing> Postfix(IEnumerable<Thing> values, bool __state, RecipeDef recipeDef, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
        {
            if (!__state)
            {
                foreach (var value in values)
                    yield return value;
                yield break;
            }

            if (ingredients.Count == 0)
                Log.Error("techshard recipe finished with no ingredients");

            Thing ingredient = ingredients[0];

            yield return ShardMaker.MakeShards(ingredient);
            yield break;
        }
    }
}
