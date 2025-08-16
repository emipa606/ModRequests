using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Entomophagy
{
    [HarmonyPatch]
    public class MealList
    {
        private static Dictionary<ThingDef, ThingDef> insectMeals = new Dictionary<ThingDef, ThingDef>()
        {
            {ThingDef.Named("MealSimple"), ThingDef.Named("Insect_MealSimple") },
            {ThingDef.Named("MealFine"), ThingDef.Named("Insect_MealFine") },
            {ThingDef.Named("MealLavish"), ThingDef.Named("Insect_MealLavish") },
            {ThingDef.Named("MealSurvivalPack"), ThingDef.Named("Insect_MealSurvivalPack") },
            {ThingDef.Named("Pemmican"), ThingDef.Named("Insect_Pemmican") },
        };

        private static Dictionary<ThingDef, ThingDef> insectDesserts = new Dictionary<ThingDef, ThingDef>();

        public static void CreateMealList()
        {
            if (ModLister.GetActiveModWithIdentifier("VanillaExpanded.VCookE") != null)
            {
                //Meals
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_MealGourmet"), DefDatabase<ThingDef>.GetNamed("Insect_MealGourmet"));
                //Bakes
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_SimpleBake"), DefDatabase<ThingDef>.GetNamed("Insect_SimpleBake"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_FineBake"), DefDatabase<ThingDef>.GetNamed("Insect_FineBake"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_LavishBake"), DefDatabase<ThingDef>.GetNamed("Insect_LavishBake"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_GourmetBake"), DefDatabase<ThingDef>.GetNamed("Insect_GourmetBake"));
                //Grills
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_SimpleGrill"), DefDatabase<ThingDef>.GetNamed("Insect_SimpleGrill"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_FineGrill"), DefDatabase<ThingDef>.GetNamed("Insect_FineGrill"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_LavishGrill"), DefDatabase<ThingDef>.GetNamed("Insect_LavishGrill"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_GourmetGrill"), DefDatabase<ThingDef>.GetNamed("Insect_GourmetGrill"));
                //Ruined Grills
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_RuinedSimpleGrill"), DefDatabase<ThingDef>.GetNamed("Insect_RuinedSimpleGrill"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_RuinedFineGrill"), DefDatabase<ThingDef>.GetNamed("Insect_RuinedFineGrill"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_RuinedLavishGrill"), DefDatabase<ThingDef>.GetNamed("Insect_RuinedLavishGrill"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_RuinedGourmetGrill"), DefDatabase<ThingDef>.GetNamed("Insect_RuinedGourmetGrill"));
                //Soup
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_UncookedSoupSimple"), DefDatabase<ThingDef>.GetNamed("Insect_UncookedSoupSimple"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_UncookedSoupFine"), DefDatabase<ThingDef>.GetNamed("Insect_UncookedSoupFine"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_UncookedSoupLavish"), DefDatabase<ThingDef>.GetNamed("Insect_UncookedSoupLavish"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_UncookedSoupGourmet"), DefDatabase<ThingDef>.GetNamed("Insect_UncookedSoupGourmet"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_CookedSoupSimple"), DefDatabase<ThingDef>.GetNamed("Insect_CookedSoupSimple"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_CookedSoupFine"), DefDatabase<ThingDef>.GetNamed("Insect_CookedSoupFine"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_CookedSoupLavish"), DefDatabase<ThingDef>.GetNamed("Insect_CookedSoupLavish"));
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_CookedSoupGourmet"), DefDatabase<ThingDef>.GetNamed("Insect_CookedSoupGourmet"));
                //CannedMeat
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_DeepFriedBigMeat"), DefDatabase<ThingDef>.GetNamed("Insect_DeepFriedInsectMeat"));
                //Deep Fried Insects
                insectMeals.Add(DefDatabase<ThingDef>.GetNamed("VCE_CannedMeat"), DefDatabase<ThingDef>.GetNamed("Insect_CannedMeat"));

                //Desserts
                insectDesserts.Add(DefDatabase<ThingDef>.GetNamed("VCE_SimpleDessert"), DefDatabase<ThingDef>.GetNamed("Insect_SimpleDessert"));
                insectDesserts.Add(DefDatabase<ThingDef>.GetNamed("VCE_FineDessert"), DefDatabase<ThingDef>.GetNamed("Insect_FineDessert"));
                insectDesserts.Add(DefDatabase<ThingDef>.GetNamed("VCE_LavishDessert"), DefDatabase<ThingDef>.GetNamed("Insect_LavishDessert"));
                insectDesserts.Add(DefDatabase<ThingDef>.GetNamed("VCE_GourmetDessert"), DefDatabase<ThingDef>.GetNamed("Insect_GourmetDessert"));
            }
        }

        [HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
        [HarmonyPostfix]
        public static Thing MakeInsectMeal(Thing __result, Thing product)
        {
            if (!ModSettings_Entomophagy.replaceMeals)
            {
                return __result;
            }
            CompIngredients compIngredients = product.TryGetComp<CompIngredients>();
            if (compIngredients != null)
            {
                if (insectMeals.ContainsKey(product.def) && compIngredients.ingredients.Contains(ThingDef.Named("Meat_Megaspider")))
                {
                    Thing insectMeal = ThingMaker.MakeThing(insectMeals[product.def], null);
                    insectMeal.stackCount = product.stackCount;
                    insectMeal.TryGetComp<CompIngredients>().ingredients = compIngredients.ingredients;
                    CompFoodPoisonable poisoned = product.TryGetComp<CompFoodPoisonable>();
                    if (poisoned?.PoisonPercent > 0)
                    {
                        insectMeal.TryGetComp<CompFoodPoisonable>().SetPoisoned(poisoned.cause);
                    }
                    return insectMeal;
                }
                if (insectDesserts.ContainsKey(product.def) && compIngredients.ingredients.Contains(ThingDefOf.InsectJelly))
                {
                    Thing insectDessert = ThingMaker.MakeThing(insectDesserts[product.def], null);
                    insectDessert.stackCount = product.stackCount;
                    insectDessert.TryGetComp<CompIngredients>().ingredients = compIngredients.ingredients;
                    CompFoodPoisonable poisoned = product.TryGetComp<CompFoodPoisonable>();
                    if (poisoned?.PoisonPercent > 0)
                    {
                        insectDessert.TryGetComp<CompFoodPoisonable>().SetPoisoned(poisoned.cause);
                    }
                    return insectDessert;
                }
            }
            return __result;
        }
    }
}
