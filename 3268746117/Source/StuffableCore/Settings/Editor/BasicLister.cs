using HarmonyLib;
using RimWorld;
using StuffableCore.SCCaching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace StuffableCore.Settings.Editor
{
    public class CategoryLister : BasicLister
    {

        public CategoryLister(StuffableCategorySettings selected) : base(selected) { }

        public CategoryLister(StuffableCategorySettings selected, int windowListSize) : base(selected, windowListSize) 
        {

        }

        public override void ChangeIndex(int index)
        {
            boundsMin = index * windowListSize;
        }

        public override int MaxFilterSize()
        {
            return Selected.stuffCategoriesSetting.Count;
        }

        public override void GetSettings(Listing_Standard listing_Standard)
        {
            base.GetSettings(listing_Standard);
            if (!Selected.stuffCategoriesSetting.NullOrEmpty())
            {
                if (Selected.filter == null)
                    Selected.filter = new Dictionary<string, bool>();
                int listSize = Selected.stuffCategoriesSetting.Count;
                boundsMax = Math.Min(boundsMin + windowListSize, listSize);
                for (int i = boundsMin; i < boundsMax; i++)
                {
                    string key = Selected.stuffCategoriesSetting.ElementAt(i).Key;
                    bool value = Selected.stuffCategoriesSetting.ElementAt(i).Value;
                    Selected.stuffCategoriesDescription.TryGetValue(key, out string label);
                    listing_Standard.CheckboxLabeled(label, ref value, tooltip: "Keep enabled for recipe?");
                    Selected.stuffCategoriesSetting.SetOrAdd(key, value);
                    bool flag = CacheUtil.GetFromCache(key, out List<ThingDef> cacheValue, IngredientSelectionCache.IngredientCache);

                    if (flag)
                    {
                        if (value)
                        {
                            foreach (ThingDef k in cacheValue.Where(h => !Selected.filter.ContainsKey(h.defName)))
                            {
                                bool oldValue = true;
                                if (Selected.ingredientStateSetting.TryGetValue(k.defName, out bool newValue))
                                    oldValue = newValue;
                                Selected.filter.SetOrAdd(k.defName, oldValue);
                                Selected.ingredientDescription.SetOrAdd(k.defName, k.label);
                                Selected.ingredientStateSetting.SetOrAdd(k.defName, oldValue);
                            }
                        }
                        else
                        {
                            foreach (ThingDef k in cacheValue.Where(t => Selected.filter.ContainsKey(t.defName)))
                                Selected.filter.Remove(k.defName);
                        }
                    }
                }
            }
        }

        public override bool Search(string search, out int newIndex)
        {
            int count = Selected.stuffCategoriesSetting.Count;
            newIndex = 0;
            bool flag = false;
            for (int i = 0; i < count; i++)
            {
                var item = Selected.stuffCategoriesSetting.ElementAt(i);
                string key = item.Key;
                if (key.ToLower().Contains(search.ToLower()))
                {
                    newIndex = i / windowListSize;
                    index = newIndex;
                    flag = true;
                    break;
                }
            }
            return flag;
        }

    }

    public class IngredientLister : BasicLister
    {
        public IngredientLister(StuffableCategorySettings selected) : base(selected) { }

        public IngredientLister(StuffableCategorySettings selected, int windowListSize) : base(selected, windowListSize) { }

        public override void ChangeIndex(int index)
        {
            boundsMin = index * windowListSize;
        }

        public override int MaxFilterSize()
        {
            return Selected.FilterSize;
        }

        public override void GetSettings(Listing_Standard listing_Standard)
        {
            if (!Selected.filter.NullOrEmpty())
            {
                int listSize = Selected.FilterSize;
                boundsMax = Math.Min(boundsMin + windowListSize, listSize);
                for (int i = boundsMin; i < boundsMax; i++)
                {
                    string key = Selected.filter.ElementAt(i).Key;
                    bool value = Selected.filter.ElementAt(i).Value;
                    Selected.ingredientDescription.TryGetValue(key, out string label);
                    listing_Standard.CheckboxLabeled(label, ref value, tooltip: "Keep enabled for recipe?");
                    Selected.filter.SetOrAdd(key, value);
                    Selected.ingredientStateSetting.SetOrAdd(key, value);
                }
            }
        }

        public override bool Search(string search, out int newIndex)
        {
            int count = Selected.FilterSize;
            newIndex = 0;
            bool flag = false;
            for (int i = 0; i < count; i++)
            {
                var item = Selected.filter.ElementAt(i);
                string key = item.Key;
                if (key.ToLower().Contains(search.ToLower()))
                {
                    newIndex = i / windowListSize;
                    index = newIndex;
                    flag = true;
                    break;
                }
            }
            return flag;
        }
    }

    public class BasicLister : BaseModule, ICarousel
    {
        public int index = 0;
        public int windowListSize = 0;

        public int boundsMin = 0;
        public int boundsMax = 0;

        public BasicLister(StuffableCategorySettings selected) : base(selected)
        {
        }

        public BasicLister(StuffableCategorySettings selected, int windowListSize) : this(selected)
        {
            this.windowListSize = windowListSize;
        }

        public virtual void ChangeIndex(int index)
        {

        }

        public virtual int MaxFilterSize()
        {
            return 0;
        }

        public virtual bool Search(string search, out int index)
        {
            index = -1;
            return false;
        }
    }
}
