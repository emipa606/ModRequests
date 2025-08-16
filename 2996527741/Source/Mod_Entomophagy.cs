using Verse;
using UnityEngine;

namespace Entomophagy
{
    class Mod_Entomophagy : Mod
    {
        Listing_Standard listingStandard = new Listing_Standard();

        public Mod_Entomophagy(ModContentPack content) : base(content)
        {
            GetSettings<ModSettings_Entomophagy>();
        }

        public override string SettingsCategory()
        {
            return "Entomophagy";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(50f, 50f, inRect.width * .8f, inRect.height);
            listingStandard.Begin(rect);
            if (listingStandard.RadioButton("(Auto Meals):      Automatically create insect meals when using insect meat:", ModSettings_Entomophagy.replaceMeals && !ModSettings_Entomophagy.insectRecipes))
            {
                ModSettings_Entomophagy.replaceMeals = true;
                ModSettings_Entomophagy.insectRecipes = false;
            }
            if (listingStandard.RadioButton("(Recipes):           Add insect meal recipes to cooking facilities:", !ModSettings_Entomophagy.replaceMeals && ModSettings_Entomophagy.insectRecipes))
            {
                ModSettings_Entomophagy.replaceMeals = false;
                ModSettings_Entomophagy.insectRecipes = true;
            }
            if (listingStandard.RadioButton("(Both):                Add recipes and auto replace meals:", ModSettings_Entomophagy.replaceMeals && ModSettings_Entomophagy.insectRecipes))
            {
                ModSettings_Entomophagy.replaceMeals = true;
                ModSettings_Entomophagy.insectRecipes = true;
            }
            if (listingStandard.RadioButton("(Trait Only):        No recipes or meals:", !ModSettings_Entomophagy.replaceMeals && !ModSettings_Entomophagy.insectRecipes))
            {
                ModSettings_Entomophagy.replaceMeals = false;
                ModSettings_Entomophagy.insectRecipes = false;
            }
            listingStandard.Gap();
            listingStandard.Label("         NOTE: Restart required to add/remove recipes");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}
