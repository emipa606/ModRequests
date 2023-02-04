using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ComigosMiniaturization
{
    public class ThingDefExtension : DefModExtension
    {

        public static ThingDefExtension Get(Def def)
        {
            return def.GetModExtension<ThingDefExtension>() ?? defaultValues;
        }


        public override IEnumerable<string> ConfigErrors()
        {
            if (inheritRecipesFrom == null)
            {
                yield return "inheritRecipesFrom is null.";
            }
            else
            {
                List<string> nonWorkbenchDefNames = new List<string>();
                List<string> recipelessThingDefNames = new List<string>();
                for (int i = 0; i < inheritRecipesFrom.Count; i++)
                {
                    var thing = inheritRecipesFrom[i];
                    bool flag2 = !thing.IsWorkTable;
                    if (flag2)
                    {
                        nonWorkbenchDefNames.Add(thing.defName);
                    }
                    else
                    {
                        bool flag3 = thing.AllRecipes.NullOrEmpty();
                        if (flag3)
                        {
                            recipelessThingDefNames.Add(thing.defName);
                        }
                    }
                }
                if (nonWorkbenchDefNames.Any())
                {
                    yield return "the following ThingDefs in inheritRecipesFrom are not workbenches: " + nonWorkbenchDefNames.ToCommaList(false);
                }
                if (recipelessThingDefNames.Any())
                {
                    yield return "the following workbenches in inheritRecipesFrom do not have any recipes: " + recipelessThingDefNames.ToCommaList(false);
                }
            }
            yield break;
        }

        public bool Allows(RecipeDef recipe)
        {
            ThingDef producedThingDef = recipe.ProducedThingDef;
            return (producedThingDef == null || (allowedProductFilter == null || allowedProductFilter.Allows(producedThingDef)) && (disallowedProductFilter == null || !disallowedProductFilter.Allows(producedThingDef))) && (allowedRecipes == null || allowedRecipes.Contains(recipe)) && (disallowedRecipes == null || !disallowedRecipes.Contains(recipe));
        }

        private static readonly ThingDefExtension defaultValues = new ThingDefExtension();

        public List<ThingDef> inheritRecipesFrom;

        public ThingFilter allowedProductFilter;

        public ThingFilter disallowedProductFilter;

        public List<RecipeDef> allowedRecipes;

        public List<RecipeDef> disallowedRecipes;

    }


    [StaticConstructorOnStartup]
    public static class StaticConstructorClass
    {

        static StaticConstructorClass()
        {
            // Go through all ThingDefs that are work tables
            for (int i = 0; i < DefDatabase<ThingDef>.AllDefsListForReading.Count; i++)
            {
                var thingDef = DefDatabase<ThingDef>.AllDefsListForReading[i];
                if (thingDef.IsWorkTable)
                {
                    ThingDefExtension thingDefExtension = ThingDefExtension.Get(thingDef);

                    // Sort out recipe inheritance
                    if (thingDefExtension.inheritRecipesFrom != null)
                    {
                        List<RecipeDef> curRecipes = new List<RecipeDef>(thingDef.AllRecipes);
                        NonPublicFields.ThingDef_allRecipesCached.SetValue(thingDef, null);
                        for (int j = 0; j < thingDefExtension.inheritRecipesFrom.Count; j++)
                        {
                            var otherBench = thingDefExtension.inheritRecipesFrom[j];
                            for (int k = 0; k < otherBench.AllRecipes.Count; k++)
                            {
                                var curRecipe = otherBench.AllRecipes[k];
                                if (thingDefExtension.Allows(curRecipe))
                                {
                                    if (thingDef.recipes == null)
                                    {
                                        thingDef.recipes = new List<RecipeDef>();
                                    }

                                    if (!curRecipes.Contains(curRecipe))
                                    {
                                        thingDef.recipes.Add(curRecipe);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
