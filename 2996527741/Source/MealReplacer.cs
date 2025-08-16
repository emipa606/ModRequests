using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Entomophagy
{
    [HarmonyPatch]
    public class MealReplacer
    {
        private static Dictionary<ThingDef, ThingDef> insectConversionList = new Dictionary<ThingDef, ThingDef>()
        {
            {ThingDef.Named("MealSimple"), ThingDef.Named("Insect_MealSimple") },
            {ThingDef.Named("MealFine"), ThingDef.Named("Insect_MealFine") },
            {ThingDef.Named("MealFine_Veg"), ThingDef.Named("Insect_MealFine") },
            {ThingDef.Named("MealFine_Meat"), ThingDef.Named("Insect_MealFine") },
            {ThingDef.Named("MealLavish"), ThingDef.Named("Insect_MealLavish") },
            {ThingDef.Named("MealLavish_Veg"), ThingDef.Named("Insect_MealLavish") },
            {ThingDef.Named("MealLavish_Meat"), ThingDef.Named("Insect_MealLavish") },
            {ThingDef.Named("MealSurvivalPack"), ThingDef.Named("Insect_MealSurvivalPack") },
            {ThingDef.Named("Pemmican"), ThingDef.Named("Insect_Pemmican") },
        };

        //v1.2
        //private static Dictionary<ThingDef, ThingDef> insectDesserts = new Dictionary<ThingDef, ThingDef>();

        public static void CreateMealList()
        {
            if (ModLister.GetActiveModWithIdentifier("VanillaExpanded.VCookE") != null)
            {
                //Meals
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_MealGourmet"), DefDatabase<ThingDef>.GetNamed("Insect_MealGourmet"));
                //Bakes
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_SimpleBake"), DefDatabase<ThingDef>.GetNamed("Insect_SimpleBake"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_FineBake"), DefDatabase<ThingDef>.GetNamed("Insect_FineBake"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_LavishBake"), DefDatabase<ThingDef>.GetNamed("Insect_LavishBake"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_GourmetBake"), DefDatabase<ThingDef>.GetNamed("Insect_GourmetBake"));
                //Grills
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_SimpleGrill"), DefDatabase<ThingDef>.GetNamed("Insect_SimpleGrill"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_FineGrill"), DefDatabase<ThingDef>.GetNamed("Insect_FineGrill"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_LavishGrill"), DefDatabase<ThingDef>.GetNamed("Insect_LavishGrill"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_GourmetGrill"), DefDatabase<ThingDef>.GetNamed("Insect_GourmetGrill"));
                //Ruined Grills
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_RuinedSimpleGrill"), DefDatabase<ThingDef>.GetNamed("Insect_RuinedSimpleGrill"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_RuinedFineGrill"), DefDatabase<ThingDef>.GetNamed("Insect_RuinedFineGrill"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_RuinedLavishGrill"), DefDatabase<ThingDef>.GetNamed("Insect_RuinedLavishGrill"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_RuinedGourmetGrill"), DefDatabase<ThingDef>.GetNamed("Insect_RuinedGourmetGrill"));
                //Soup
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_UncookedSoupSimple"), DefDatabase<ThingDef>.GetNamed("Insect_UncookedSoupSimple"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_UncookedSoupFine"), DefDatabase<ThingDef>.GetNamed("Insect_UncookedSoupFine"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_UncookedSoupLavish"), DefDatabase<ThingDef>.GetNamed("Insect_UncookedSoupLavish"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_UncookedSoupGourmet"), DefDatabase<ThingDef>.GetNamed("Insect_UncookedSoupGourmet"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_CookedSoupSimple"), DefDatabase<ThingDef>.GetNamed("Insect_CookedSoupSimple"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_CookedSoupFine"), DefDatabase<ThingDef>.GetNamed("Insect_CookedSoupFine"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_CookedSoupLavish"), DefDatabase<ThingDef>.GetNamed("Insect_CookedSoupLavish"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_CookedSoupGourmet"), DefDatabase<ThingDef>.GetNamed("Insect_CookedSoupGourmet"));
                //CannedMeat
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_DeepFriedBigMeat"), DefDatabase<ThingDef>.GetNamed("Insect_DeepFriedInsectMeat"));
                //Deep Fried Insects
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_CannedMeat"), DefDatabase<ThingDef>.GetNamed("Insect_CannedMeat"));

                //Desserts 
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_SimpleDessert"), DefDatabase<ThingDef>.GetNamed("Insect_SimpleDessert"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_FineDessert"), DefDatabase<ThingDef>.GetNamed("Insect_FineDessert"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_LavishDessert"), DefDatabase<ThingDef>.GetNamed("Insect_LavishDessert"));
                insectConversionList.Add(DefDatabase<ThingDef>.GetNamed("VCE_GourmetDessert"), DefDatabase<ThingDef>.GetNamed("Insect_GourmetDessert"));
                //(v1.2 only)
                //insectDesserts.Add(DefDatabase<ThingDef>.GetNamed("VCE_SimpleDessert"), DefDatabase<ThingDef>.GetNamed("Insect_SimpleDessert"));
                //insectDesserts.Add(DefDatabase<ThingDef>.GetNamed("VCE_FineDessert"), DefDatabase<ThingDef>.GetNamed("Insect_FineDessert"));
                //insectDesserts.Add(DefDatabase<ThingDef>.GetNamed("VCE_LavishDessert"), DefDatabase<ThingDef>.GetNamed("Insect_LavishDessert"));
                //insectDesserts.Add(DefDatabase<ThingDef>.GetNamed("VCE_GourmetDessert"), DefDatabase<ThingDef>.GetNamed("Insect_GourmetDessert"));
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
                if (insectConversionList.ContainsKey(product.def) && ContainsInsect(compIngredients))
                {
                    Thing insectMeal = ThingMaker.MakeThing(insectConversionList[product.def], null);
                    insectMeal.stackCount = product.stackCount;
                    insectMeal.TryGetComp<CompIngredients>().ingredients = compIngredients.ingredients;
                    CompFoodPoisonable poisoned = product.TryGetComp<CompFoodPoisonable>();
                    if (poisoned?.PoisonPercent > 0)
                    {
                        insectMeal.TryGetComp<CompFoodPoisonable>().SetPoisoned(poisoned.cause);
                    }
                    return insectMeal;
                }
                //v1.2
                /*
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
                */
            }
            return __result;
        }

        public static bool ContainsInsect(CompIngredients compIngredients)
        {
            for(int i=0; i < compIngredients.ingredients.Count; i++)
            {
                if (compIngredients.ingredients[i] == ThingDefOf.InsectJelly)
                    return true;
                if (FoodUtility.GetMeatSourceCategory(compIngredients.ingredients[i]) == MeatSourceCategory.Insect)
                    return true;
            }
            return false;
        }
    }
}
