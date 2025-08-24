using RimWorld;
using StuffableCore.SCCaching;
using StuffableCore.SCUtils;
using StuffableCore.Settings.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace StuffableCore.Settings
{
    public class StuffableCategorySettings : BaseSettings
    {
        public bool enabled = false;
        public bool usesAltSearch = true;
        public bool altSearch = false;
        public bool rangedSettingsToggle = false;
        public bool meleeSettingsToggle = false;
        public bool otherWeaponSettingsToggle = false;
        public bool clothingSettingsToggle = false;
        public bool armorSettingsToggle = false;
        public bool otherClothingToggle = false;

        public bool usesAdditionalStuffMultiplierArmor = false;
        public float additionalStuffMultiplierArmor = 0.1f;
        public float additionalStuffMultiplierDamageSharp = 0.1f;
        public float defaultStuffCost = 0.25f;

        private float defaultCostMin = 0.01f;
        private float defaultCostMax = 2.5f;
        public bool useDefaultCostInstead = false;
        public bool useRawPlantForStuff = false;

        public Dictionary<string, bool> stuffCategoriesSetting;
        public Dictionary<string, string> stuffCategoriesDescription;
        public Dictionary<string, string> stuffCategoriesModName;

        public Dictionary<string, bool> ingredientStateSetting;
        public Dictionary<string, string> ingredientDescription;
        public Dictionary<string, bool> filter;

        public int DefaultStuffCost { get => (int) (defaultStuffCost * 100); }

        public int FilterSize
        {
            get
            {
                if (filter == null)
                    return 0;
                return filter.Count;
            }
        }

        public virtual void Initialize()
        {
            if (stuffCategoriesSetting == null)
                stuffCategoriesSetting = new Dictionary<string, bool>();
            if (stuffCategoriesDescription == null)
                stuffCategoriesDescription = new Dictionary<string, string>();
            if (stuffCategoriesModName == null)
                stuffCategoriesModName = new Dictionary<string, string>();
            if (ingredientStateSetting == null)
                ingredientStateSetting = new Dictionary<string, bool>();
            if (ingredientDescription == null)
                ingredientDescription = new Dictionary<string, string>();
        }

        public StuffableCategorySettings() 
        {
            Initialize();
        }

        public StuffableCategorySettings(string label) : this()
        {
            this.SettingsLabel = label;
        }

        public virtual void GetDefaultCostSlider(Listing_Standard listingStandard)
        {
            listingStandard.Label("Default stuff cost: {0}".Formatted(defaultStuffCost * 100));
            defaultStuffCost = listingStandard.Slider(defaultStuffCost, defaultCostMin, defaultCostMax);
        }

        public virtual void GetAdditionalStuffMultiplierArmorSlider(Listing_Standard listingStandard)
        {
            listingStandard.CheckboxLabeled("Uses additional stuff multiplier", ref usesAdditionalStuffMultiplierArmor);
            listingStandard.Label("Material effect multiplier: {0}".Formatted(additionalStuffMultiplierArmor * 100), tooltip: "Material effect multiplier.");
            additionalStuffMultiplierArmor = listingStandard.Slider(additionalStuffMultiplierArmor, -1.0f, 1.0f);

        }

        public virtual IEnumerable<string> GetEnabledStuffCategoriesSettingKeys()
        {
            foreach(KeyValuePair<string, bool> kvp in stuffCategoriesSetting.Where(i => i.Value == true))
                yield return kvp.Key;
        }

        public virtual IEnumerable<string> GetDisabledStuffCategoriesSettingKeys()
        {
            foreach (KeyValuePair<string, bool> kvp in stuffCategoriesSetting.Where(i => i.Value == false))
                yield return kvp.Key;
        }

        public virtual List<ThingDef> GetEnabledIngredientsForEnabledCategories()
        {
            List<ThingDef> filter = new List<ThingDef>();
            foreach (string enabledKeys in GetEnabledStuffCategoriesSettingKeys())
            {
                if (CacheUtil.GetFromCache(enabledKeys, out List<ThingDef> value, IngredientSelectionCache.IngredientCache) && !value.EnumerableNullOrEmpty())
                {
                    value.ForEach(i => {
                        string key = i.defName;
                        if (ingredientStateSetting.TryGetValue(key, out bool ivalue) && ivalue)
                                filter.Add(i);
                    });
                }
            }
            return filter;
        }

        public virtual ThingDef GetDefaultStuffFor(ThingDef thingdef)
        {
            string key = stuffCategoriesSetting.Where(i => i.Value == true).RandomElement().Key;
            CacheUtil.GetFromCache(key, out List<ThingDef> value, IngredientSelectionCache.IngredientCache);
            return value.RandomElement();
        }

        public virtual List<ThingDef> GetDefaultStuffListFor()
        {
            List<ThingDef> newList = new List<ThingDef>();
            foreach(var newVals in stuffCategoriesSetting.Where(i => i.Value == true))
            {
                CacheUtil.GetFromCache(newVals.Key, out List<ThingDef> value, IngredientSelectionCache.IngredientCache);
                newList.AddRange(value);
            }
            return newList;
        }

        public virtual void SetSettings(string key, string description, string fromModName, bool enabled = false)
        {
            stuffCategoriesSetting.SetOrAdd(key, enabled);
            stuffCategoriesDescription.SetOrAdd(key, description);
            stuffCategoriesModName.SetOrAdd(key, fromModName);
        }

        public virtual void RemoveSettings(string key)
        {
            stuffCategoriesSetting.Remove(key);
            stuffCategoriesDescription.Remove(key);
            stuffCategoriesModName.Remove(key);
        }

        public virtual bool IsSettingsValid()
        {
            if (!enabled)
                return true;

            bool flag = false;
            foreach (bool val in stuffCategoriesSetting.Values)
                if(val)
                    flag = val;

            return flag;
        }

        public virtual void ToggleAllSettings(bool toggle)
        {
            enabled = toggle;
            altSearch = toggle;
            rangedSettingsToggle = toggle;
            meleeSettingsToggle = toggle;
            otherWeaponSettingsToggle = toggle;
            clothingSettingsToggle = toggle;
            armorSettingsToggle = toggle;
            otherClothingToggle = toggle;
            additionalStuffMultiplierArmor = 0.1f;
            Dictionary<string, bool> oldStuffCategoriesSetting = stuffCategoriesSetting;
            Dictionary<string, bool> newStuffCategoriesSetting = new Dictionary<string, bool>(); ;
            foreach (string key in oldStuffCategoriesSetting.Keys)
                newStuffCategoriesSetting.SetOrAdd(key, toggle);
            stuffCategoriesSetting = newStuffCategoriesSetting;
        }

        public virtual bool ApplySearch(ThingDef item)
        {
            return true;
        }

        public virtual bool ApplyAltSearch(ThingDef item)
        {
            return false;
        }

        public virtual void GetSettingsHeaderEnabled(Listing_Standard listingStandard)
        {
            listingStandard.Label("Enable recipe update:", tooltip: "Enable below checkbox to update recipe on next restart.");
            listingStandard.CheckboxLabeled("Enabled?", ref enabled, "Enabled recipe change?");
        }

        public override void GetSettingsHeaderSimple(Listing_Standard listingStandard)
        {
            listingStandard.CheckboxLabeled("Enabled On Next Restart?", ref enabled, "Enabled On Next Restart?");
            listingStandard.Gap();
        }

        public virtual void GetSettingsHeaderCustom(Listing_Standard listingStandard)
        {

        }

        public override void GetSettingsHeader(Listing_Standard listingStandard)
        {
            listingStandard.CheckboxLabeled("Enabled On Next Restart?", ref enabled, "Enabled On Next Restart?");
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("Use additional search algorithm?", ref altSearch, "Uses an additional search algorithm to try and find potential stuffable items from other mods that may not inherit from base game.");
            listingStandard.Gap();
        }

        public virtual void GetSettingsHeaderAltSearchEnabled(Listing_Standard listingStandard)
        {
            if (usesAltSearch)
            {
                listingStandard.Label("Enable alternate search algorithm:", tooltip: "Enable alternate search algorithm on next restart.");
                listingStandard.CheckboxLabeled("Use additional search algorithm?", ref altSearch, "Uses an additional search algorithm to try and find potential stuffable items from other mods that may not inherit from base game.");
            }
        }

        public virtual void GetSettingsHeaderErrorMessage(Listing_Standard listingStandard)
        {
            if (!IsSettingsValid())
            {
                listingStandard.GapLine();
                listingStandard.Label("Please select at least one category.".Colorize(Color.red));
                listingStandard.GapLine();
            }
        }

        public void UseDefaultCostInsteadCheckbox(Listing_Standard listingStandard)
        {
            listingStandard.CheckboxLabeled("Enable to use default stuff cost instead of being auto-calculated?", ref useDefaultCostInstead, height: 35, tooltip: "Enable to use default stuff cost instead of being auto-calculated.");
        }

        public void DisplayCheckboxes(Listing_Standard listingStandard)
        {
            if (!stuffCategoriesSetting.NullOrEmpty())
            {
                int count = stuffCategoriesSetting.Keys.Count;
                for (int i = 0; i < count; i++)
                {
                    string key = stuffCategoriesSetting.ElementAt(i).Key;
                    bool state = stuffCategoriesSetting.ElementAt(i).Value;
                    string desc = stuffCategoriesDescription.ElementAt(i).Value;
                    string fromMod = stuffCategoriesModName.ElementAt(i).Value;
                    listingStandard.CheckboxLabeled(key, ref state, "{0} from mod {1}".Formatted(desc, fromMod).CapitalizeFirst());
                    stuffCategoriesSetting.SetOrAdd(key, state);
                }
            }
        }

        public virtual void ClearSettings()
        {
            stuffCategoriesSetting.Clear();
            stuffCategoriesDescription.Clear();
            stuffCategoriesModName.Clear();
        }

        public virtual void SetStuffDefaults(List<StuffCategoryDef> stuffs)
        {
            foreach (StuffCategoryDef stuff in stuffs)
                if (!stuffCategoriesSetting.ContainsKey(stuff.defName))
                    SetSettings(stuff.defName, stuff.label, stuff.modContentPack.Name);

            int count = stuffCategoriesSetting.Count;
            for(int i = count - 1; i >= 0; --i)
            {
                var kvp = stuffCategoriesSetting.ElementAt(i);
                string key = kvp.Key;
                bool flag = false;
                foreach (StuffCategoryDef stuff in stuffs)
                {
                    if (key.Equals(stuff.defName))
                        flag = true;
                }
                if (!flag)
                    RemoveSettings(key);
            }
        }

        public virtual void SetDefaultsForCategories(ThingDef thingDef)
        {
            if (thingDef.stuffCategories == null)
                thingDef.stuffCategories = new List<StuffCategoryDef>();
            else
                thingDef.stuffCategories.Clear();
        }

        public virtual void ApplyUpdate(IEnumerable<StuffCategoryDef> cacheStuffCategoryDef, ThingDef thingDef)
        {
            if (stuffCategoriesSetting == null)
                return;
            SetDefaultsForCategories(thingDef);
            foreach (StuffCategoryDef stuff in cacheStuffCategoryDef)
                if (stuffCategoriesSetting.TryGetValue(stuff.defName, out bool flag) && flag)
                    thingDef.stuffCategories.Add(stuff);
            UpdateThingDef(thingDef);
        }

        public virtual void UpdateThingDef(ThingDef thingDef)
        {
            if (thingDef.stuffProps == null)
                thingDef.stuffProps = new StuffProperties();
            if (thingDef.stuffProps.statFactors == null)
                thingDef.stuffProps.statFactors = new List<StatModifier>();
            if (usesAdditionalStuffMultiplierArmor)
            {
                if (thingDef.statBases == null)
                    thingDef.statBases = new List<StatModifier>();

                int index = thingDef.statBases.FindIndex(i => StatDefOf.StuffEffectMultiplierArmor.Equals(i.stat));
                if (index >= 0)
                    thingDef.statBases.RemoveAt(index);

                thingDef.statBases.Add(new StatModifier()
                {
                    stat = StatDefOf.StuffEffectMultiplierArmor,
                    value = additionalStuffMultiplierArmor
                });
            }
            CostManager.UpdateCost(thingDef, DefaultStuffCost, useDefaultCostInstead);
            ScenarioManager.UpdateScenarioItem(thingDef, this);
        }


        public override void ExposeData()
        {
            Scribe_Values.Look(ref enabled, "enabled", false);
            Scribe_Values.Look(ref usesAltSearch, "usesAltSearch", true);
            Scribe_Values.Look(ref altSearch, "altSearch", false);
            Scribe_Values.Look(ref defaultStuffCost, "defaultStuffCost", 0.25f);
            Scribe_Values.Look(ref additionalStuffMultiplierDamageSharp, "additionalStuffMultiplierDamageSharp", 0.1f);
            Scribe_Values.Look(ref useRawPlantForStuff, "useRawPlantForStuff", false);

            Scribe_Values.Look(ref rangedSettingsToggle, "rangedSettingsToggle", false);
            Scribe_Values.Look(ref meleeSettingsToggle, "meleeSettingsToggle", false);
            Scribe_Values.Look(ref otherWeaponSettingsToggle, "otherWeaponSettingsToggle", false);

            Scribe_Values.Look(ref clothingSettingsToggle, "clothingSettingsToggle", false);
            Scribe_Values.Look(ref armorSettingsToggle, "armorSettingsToggle", false);
            Scribe_Values.Look(ref otherClothingToggle, "otherClothingToggle", false);
            Scribe_Values.Look(ref usesAdditionalStuffMultiplierArmor, "usesAdditionalStuffMultiplierArmor", false);
            Scribe_Values.Look(ref additionalStuffMultiplierArmor, "additionalStuffMultiplier", 0.1f);
            Scribe_Values.Look(ref useDefaultCostInstead, "useDefaultCostInstead", false);

            Scribe_Collections.Look(ref stuffCategoriesSetting, "stuffCategoriesSetting");
            Scribe_Collections.Look(ref stuffCategoriesDescription, "stuffCategoriesDescription");
            Scribe_Collections.Look(ref stuffCategoriesModName, "stuffCategoriesModName");

            Scribe_Collections.Look(ref ingredientStateSetting, "ingredientStateSetting");
            Scribe_Collections.Look(ref ingredientDescription, "ingredientDescription");
        }
    }
}
