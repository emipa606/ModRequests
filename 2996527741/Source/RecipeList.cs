using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Entomophagy
{
    class RecipeList
    {
        private static List<RecipeDef> insectRecipes = new List<RecipeDef>()
        {
            Entomophagy_DefOf.Insect_CookMealSimple,
            Entomophagy_DefOf.Insect_CookMealSimpleBulk,
            Entomophagy_DefOf.Insect_CookMealFine,
            Entomophagy_DefOf.Insect_CookMealFineBulk,
            Entomophagy_DefOf.Insect_CookMealLavish,
            Entomophagy_DefOf.Insect_CookMealLavishBulk,
            Entomophagy_DefOf.Insect_CookMealSurvival,
            Entomophagy_DefOf.Insect_CookMealSurvivalBulk,
            Entomophagy_DefOf.Insect_MakePemmican,
            Entomophagy_DefOf.Insect_MakePemmicanBulk
        };

        private static List<RecipeDef> insectGrills = new List<RecipeDef>();

        public static void CreateRecipeList()
        {
            if (ModLister.GetActiveModWithIdentifier("VanillaExpanded.VCookE") != null)
            {
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookMealGourmet"));
                //Bakes
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookBakeSimple"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookBakeFine"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookBakeLavish"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookBakeGourmet"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookBakeSimpleBulk"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookBakeFineBulk"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookBakeLavishBulk"));
                //Desserts
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookDessertSimple"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookDessertFine"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookDessertLavish"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookDessertGourmet"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookDessertSimpleBulk"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookDessertFineBulk"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookDessertLavishBulk"));
                //Soup
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookSoupSimple"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookSoupFine"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookSoupLavish"));
                insectRecipes.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookSoupGourmet"));
                //Grills
                insectGrills.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookGrillSimple"));
                insectGrills.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookGrillFine"));
                insectGrills.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookGrillLavish"));
                insectGrills.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookGrillGourmet"));
                insectGrills.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookGrillSimpleBulk"));
                insectGrills.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookGrillFineBulk"));
                insectGrills.Add(DefDatabase<RecipeDef>.GetNamed("Insect_CookGrillLavishBulk"));
            }
        }

        public static void AddRecipes()
        {
            foreach (ThingDef stove in DefDatabase<RecipeDef>.GetNamed("CookMealFine").AllRecipeUsers)
            {
                Log.Message(stove.label + " is getting new recipes");
                if (stove.recipes == null)
                {
                    stove.recipes = new List<RecipeDef>();
                }
                stove.recipes.AddRange(insectRecipes);
            }
            if (ModLister.GetActiveModWithIdentifier("VanillaExpanded.VCookE") != null)
            {
                foreach (ThingDef grill in DefDatabase<RecipeDef>.GetNamedSilentFail("VCE_CookGrillFine").AllRecipeUsers)
                {
                    if (grill.recipes == null)
                    {
                        grill.recipes = new List<RecipeDef>();
                    }
                    grill.recipes.AddRange(insectGrills);
                }
            }
        }
        /*
        public static void RemoveRecipes()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.recipes != null)
                {
                    if (def.recipes.Contains(DefDatabase<RecipeDef>.GetNamed("CookMealFine")))
                    {
                        for (int i = 0; i < insectRecipes.Count; i++)
                        {
                            def.recipes.Remove(insectRecipes[i]);
                        }
                    }
                    if (def.recipes.Contains(DefDatabase<RecipeDef>.GetNamedSilentFail("VCE_CookGrillFine")))
                    {
                        for (int j = 0; j < insectGrills.Count; j++)
                        {
                            def.recipes.Remove(insectGrills[j]);
                        }
                    }
                }
            }
        }
        */
    }
}
