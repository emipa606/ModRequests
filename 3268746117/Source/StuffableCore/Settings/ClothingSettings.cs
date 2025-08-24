using RimWorld;
using StuffableCore.SCCaching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.Settings
{
    public class ClothingSettings : StuffableCategorySettings
    {
        public ClothingSettings()
        {
            SettingsLabel = "Bulk clothing settings";
            usesAdditionalStuffMultiplierArmor = true;
        }

        public override bool ApplySearch(ThingDef item)
        {
            bool flag = item.IsApparel
                && item.apparel.tags.NotNullAndContains(SCConstants.StuffableClothing)
                && !item.apparel.tags.NotNullAndContains(SCConstants.StuffableArmor);
            if (flag)
                CacheUtil.AddToCache(item.defName, item, StuffTagCache.ClothingArmorCache);
            return flag;
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            bool flag = item.IsApparel
                && !item.apparel.tags.NotNullAndContains(SCConstants.StuffableClothing)
                && !item.apparel.tags.NotNullAndContains(SCConstants.StuffableArmor)
                && (!(item.tradeTags.NotNullAndContains("HiTechArmor") || item.tradeTags.NotNullAndContains("Armor"))
                    && (item.tradeTags.NotNullAndContains("BasicClothing") || item.tradeTags.NotNullAndContains("Clothing"))
                        || (item.thingCategories.NotNullAndContains(SCDefOf.ApparelMisc) && !item.thingCategories.NotNullAndContains(ThingCategoryDefOf.Apparel)));
            if (flag)
                CacheUtil.AddToCache(item.defName, item, StuffTagCache.ClothingArmorCache);
            return flag;
        }
    }

    public class ArmorSettings : StuffableCategorySettings
    {
        public ArmorSettings()
        {
            SettingsLabel = "Bulk armor settings";
            usesAdditionalStuffMultiplierArmor = true;
        }

        public override bool ApplySearch(ThingDef item)
        {
            bool flag = item.IsApparel && item.apparel.tags.NotNullAndContains(SCConstants.StuffableArmor);
            if (flag)
                CacheUtil.AddToCache(item.defName, item, StuffTagCache.ClothingArmorCache);
            return flag;
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            bool flag = item.IsApparel
                && !item.apparel.tags.NotNullAndContains(SCConstants.StuffableArmor)
                && ((item.tradeTags.NotNullAndContains("HiTechArmor") || item.tradeTags.NotNullAndContains("Armor"))
                    || item.thingCategories.NotNullAndContains(ThingCategoryDefOf.ApparelArmor));
            if (flag)
                CacheUtil.AddToCache(item.defName, item, StuffTagCache.ClothingArmorCache);
            return flag;
        }
    }

    public class ClothingAndArmorSettings : StuffableCategorySettings
    {

        public ClothingAndArmorSettings()
        {
            SettingsLabel = "Catch-all bulk clothing/armor settings";
            usesAdditionalStuffMultiplierArmor = true;
        }

        public override bool ApplySearch(ThingDef item)
        {
            return false;
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            bool flag = item.IsApparel && !HasStuffableTag(item);
            if(flag)
                CacheUtil.AddToCache(item.defName, item, StuffTagCache.ClothingArmorCache);
            return flag;
        }

        private static bool HasStuffableTag(ThingDef item)
        {
            bool? contains = item.apparel?.tags?.NotNullAndContains(SCConstants.stuffableTag);
            bool flag = false;
            if (contains.HasValue)
                flag = true;
            return flag;
        }
    }
}
