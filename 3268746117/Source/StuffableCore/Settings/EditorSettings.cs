using HarmonyLib;
using RimWorld;
using StuffableCore.SCCaching;
using StuffableCore.Settings.BulkEditor;
using StuffableCore.Settings.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using Verse;

namespace StuffableCore.Settings
{
    [HarmonyPatch(typeof(ThingMaker), "MakeThing")]
    public static class EditorPatch_2ElectricBoogaloo
    {
        public static bool Prefix(ThingDef def, ref ThingDef stuff)
        {
            if (def == null) return false; // simple sidearms is the only mod ive seen cause this but can be null???
            if (SCMod.settings != null && SCMod.settings.EditorSettings != null && SCMod.settings.EditorSettings.ThingDefSettingsCache != null && SCMod.settings.EditorSettings.ThingDefSettingsCache.TryGetValue(def.defName, out var scs) && scs != null && scs.enabled)
            {
                var v = scs.GetEnabledIngredientsForEnabledCategories();
                if (!v.NullOrEmpty())
                {
                    var element = v.RandomElement();
                    if (element != null)
                        stuff = element;
                }
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(RecipeDefGenerator), "SetIngredients")]
    public static class EditorPatch
    {
        public class PatchState
        {
            public StuffableCategorySettings scs;
        }

        public static bool Prefix(RecipeDef r, ThingDef def, int adjustedCount, out PatchState __state)
        {
            __state = new PatchState(); 
            if (SCMod.settings.EditorSettings.ThingDefSettingsCache.TryGetValue(def.defName, out StuffableCategorySettings scs) && scs.enabled)
            {
                __state.scs = scs;
                return false;
            }
            return true;
        }

        public static void Postfix(RecipeDef r, ThingDef def, int adjustedCount, PatchState __state)
        {
            if (__state.scs == null)
                return;
            r.ingredients.Clear();
            r.adjustedCount = adjustedCount;
            if (def.MadeFromStuff)
            {
                IngredientCount ingredientCount = new IngredientCount();
                ingredientCount.SetBaseCount((float)(def.CostStuffCount * adjustedCount));
                ingredientCount.filter.customSummary = def.stuffCategorySummary ?? def.stuffCategories.Select<StuffCategoryDef, string>((Func<StuffCategoryDef, string>)(category => category.noun)).ToCommaListOr();
                foreach (ThingDef thingDef in __state.scs.GetEnabledIngredientsForEnabledCategories())
                {
                    ingredientCount.filter.SetAllow(thingDef, true);
                    r.fixedIngredientFilter.SetAllow(thingDef, true);
                }
                r.ingredients.Add(ingredientCount);
                r.productHasIngredientStuff = true;
            }
            if (def.CostList == null)
                return;
            foreach (ThingDefCountClass cost in def.CostList)
            {
                IngredientCount ingredientCount = new IngredientCount();
                ingredientCount.SetBaseCount((float)(cost.count * adjustedCount));
                ingredientCount.filter.SetAllow(cost.thingDef, true);
                r.ingredients.Add(ingredientCount);
            }
        }
    }

    public class EditorSettings : StuffableCategorySettings
    {
        private static Dictionary<string, StuffableCategorySettings> thingDefSettingsCache;

        public Dictionary<string, StuffableCategorySettings> ThingDefSettingsCache { get => thingDefSettingsCache; set => thingDefSettingsCache = value; }

        public EditorSettings() : base() { }

        public override void Initialize()
        {
            base.Initialize();

            if (ThingDefSettingsCache == null)
                ThingDefSettingsCache = new Dictionary<string, StuffableCategorySettings>();
        }

        public override void SetStuffDefaults(List<StuffCategoryDef> stuffs)
        {
            StuffTagCache.AllCaches.ForEach((caches) => {
                foreach(ThingDef thingDef in caches.Values)
                {
                    string key = thingDef.defName;
                    if(!ThingDefSettingsCache.TryGetValue(key, out StuffableCategorySettings value))
                    {
                        value = new StuffableCategorySettings(thingDef.LabelCap)
                        {
                            enabled = false
                        };
                    }
                    if(value.SettingsLabel.NullOrEmpty())
                        value.SettingsLabel = thingDef.LabelCap;
                    value.SetStuffDefaults(stuffs);
                    ThingDefSettingsCache.SetOrAdd(thingDef.defName, value);
                }
            });
        }

        public override void ApplyUpdate(IEnumerable<StuffCategoryDef> cacheStuffCategoryDef, ThingDef thingDef)
        {
            if (!ThingDefSettingsCache.TryGetValue(thingDef.defName, out StuffableCategorySettings scs))
                return;
            if (!scs.enabled)
                return;
            scs.SetDefaultsForCategories(thingDef);
            foreach (StuffCategoryDef stuff in cacheStuffCategoryDef)
                if (scs.stuffCategoriesSetting.TryGetValue(stuff.defName, out bool flag) && flag)
                        thingDef.stuffCategories.Add(stuff);
            scs.UpdateThingDef(thingDef);
        }

        public override void SetSettings(string key, string description, string fromModName, bool enabled)
        {

        }

        public override void RemoveSettings(string key)
        {
            foreach (StuffableCategorySettings scs in ThingDefSettingsCache.Values)
                scs.RemoveSettings(key);
        }

        public override void UpdateThingDef(ThingDef thingDef)
        {
            if (ThingDefSettingsCache.TryGetValue(thingDef.defName, out StuffableCategorySettings scs) && scs.enabled)
                scs.UpdateThingDef(thingDef);
        }

        public override void ClearSettings()
        {
            foreach(StuffableCategorySettings scs in ThingDefSettingsCache.Values)
            {
                scs.ClearSettings();
                scs.ingredientStateSetting.Clear();
                scs.ingredientDescription.Clear();
            }
        }

        public override void GetSettingsHeaderCustom(Listing_Standard listing_Standard)
        {
            base.GetSettingsHeaderCustom(listing_Standard);
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("Enable all configured recipes to update next restart?", ref enabled, "Enabled all configured recipes to update next restart?");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref thingDefSettingsCache, "ThingDefSettingsCache", LookMode.Undefined, LookMode.Deep);
            if (thingDefSettingsCache == null)
                thingDefSettingsCache = new Dictionary<string, StuffableCategorySettings>();
        }
    }
}
