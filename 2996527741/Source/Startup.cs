using System.Reflection;
using Verse;
using HarmonyLib;

namespace Entomophagy
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            Harmony harmony = new Harmony("com.rimworld.entomophagy");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            MealReplacer.CreateMealList();
            if (ModSettings_Entomophagy.insectRecipes)
            {
                RecipeInserter.CreateRecipeList();
                RecipeInserter.AddRecipes();
            }
            //AddBackStoryTraits();
        }
        /*
        static void AddBackStoryTraits()
        {
            Backstory bs;
            //Entomophagous
            TraitEntry entomophagous = new TraitEntry(DefDatabase<TraitDef>.GetNamed("Entomophagous"), 0);
            List<string> entoIdentifiers = new List<string>();
            entoIdentifiers.Add("AbandonedChild23"); //Entomophagous
            entoIdentifiers.Add("JungleKid31");
            for (int i = 0; i < entoIdentifiers.Count; i++)
            {
                if (BackstoryDatabase.TryGetWithIdentifier(entoIdentifiers[i], out bs, true))
                {
                    if (bs.forcedTraits == null)
                    {
                        bs.forcedTraits = new List<TraitEntry>();
                    }
                    bs.forcedTraits.Add(entomophagous);
                    //Log.Message("Entomophagous added");
                }
            }
        }
        */
    }      
}
