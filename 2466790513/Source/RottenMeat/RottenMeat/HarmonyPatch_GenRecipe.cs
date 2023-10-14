using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace RottenMeat
{
    [StaticConstructorOnStartup]
    public class HarmonyPatch_GenRecipe
    {
        //the static constructor
        static HarmonyPatch_GenRecipe()
        {
            //creates a new Harmony instance and assigns it an ID
            var harmonyInstance = new Harmony("RimWorld.Carnagion.RottenMeat.HarmonyPatch_GenRecipe");

            //gets the original method and the method that's supposed to be a prefix to it
            var originalMethod = AccessTools.Method(typeof(GenRecipe), "MakeRecipeProducts");
            var postfixMethod = AccessTools.Method(typeof(HarmonyPatch_GenRecipe), "Postfix_MakeRecipeProducts");

            //calls the patch method
            harmonyInstance.Patch(originalMethod, postfix: new HarmonyMethod(postfixMethod));
        }

        //the passthrough postfix method
        public static IEnumerable<Thing> Postfix_MakeRecipeProducts(IEnumerable<Thing> recipeProducts, RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
        {
            //proceeds if ingredient is a corpse and recipe has butchery special products
            if (recipeDef.specialProducts != null && recipeDef.specialProducts.Contains(SpecialProductType.Butchery))
            {
                //calculates pawn's efficiency to use later
                float workerEfficiency;
                if (recipeDef.efficiencyStat == null)
                {
                    workerEfficiency = 1f;
                }
                else
                {
                    workerEfficiency = worker.GetStatValue(recipeDef.efficiencyStat, true);
                }

                //checks each ingredient to see if it is a corpse and is rotting, and adds rotten meat as product if yes
                foreach (Thing ingredient in ingredients)
                {
                    if (ingredient is Corpse ingredientAsCorpse)
                    {
                        if (ingredientAsCorpse.GetRotStage() == RotStage.Rotting)
                        {
                            int productStackCount = GenMath.RoundRandom(ingredientAsCorpse.InnerPawn.BodySize * workerEfficiency * 30);
                            if (productStackCount > 0)
                            {
                                Thing rottenButcheryProduct = ThingMaker.MakeThing(ThingDefOf.MeatRotten, null);
                                rottenButcheryProduct.stackCount = productStackCount;
                                yield return rottenButcheryProduct;
                            }
                            yield break;
                        }
                    }
                }
            }
            //returns original method's products; will only be reached if one of the ingredients is not a rotting corpse
            foreach (Thing existingThing in recipeProducts)
            {
                yield return existingThing;
            }

        }
    }
}
