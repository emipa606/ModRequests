using HarmonyLib;
using RimWorld;
using StuffableCore.SCCaching;
using StuffableCore.SCUtils;
using StuffableCore.Settings.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace StuffableCore.Settings
{

    public class MeleeSettings : StuffableCategorySettings
    {
        public MeleeSettings()
        {
            SettingsLabel = "Bulk melee weapon settings";
        }

        public override bool ApplySearch(ThingDef item)
        {
            bool flag = item.weaponTags.NotNullAndContains(SCConstants.StuffableWeaponMelee) && !SearchUtil.IsMechWeapon(item);
            if (flag)
                CacheUtil.AddToCache(item.defName, item, StuffTagCache.WeaponsCache);
            return flag;
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            bool flag1 = !item.weaponTags.NotNullAndContains(SCConstants.StuffableWeaponMelee);
            bool flag2 = !item.thingCategories.NotNullAndContains(ThingCategoryDefOf.ResourcesRaw);
            bool flag3 = false;
            bool flag4 = !item.IsDrug;

            List<WeaponClassDef> weaponClasses = item.weaponClasses;
            if (!weaponClasses.NullOrEmpty())
            {
                weaponClasses.ForEach(i => {
                    string name = i.defName;
                    flag3 = name.Contains("Melee") || name.Contains("MeleePiercer") || name.Contains("MeleeBlunt");
                });
            }
            bool flag = item.IsMeleeWeapon && flag1 && flag2 && flag3 && flag4 && !SearchUtil.IsMechWeapon(item) && item.recipeMaker != null;
            if (flag)
                CacheUtil.AddToCache(item.defName, item, StuffTagCache.WeaponsCache);
            return flag;
        }
    }

    public class RangedSettings : StuffableCategorySettings
    {

        public RangedSettings()
        {
            SettingsLabel = "Bulk ranged weapon settings";
        }

        public override bool ApplySearch(ThingDef item)
        {
            bool flag = item.weaponTags.NotNullAndContains(SCConstants.StuffableWeaponRanged) && !SearchUtil.IsMechWeapon(item);
            if (flag)
                CacheUtil.AddToCache(item.defName, item, StuffTagCache.WeaponsCache);
            return flag;
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            
            bool flag1 = !item.weaponTags.NotNullAndContains(SCConstants.StuffableWeaponRanged);
            bool flag2 = !item.thingCategories.NotNullAndContains(ThingCategoryDefOf.ResourcesRaw);
            bool flag3 = false;
            bool flag4 = !item.IsDrug;

            List<WeaponClassDef> weaponClasses = item.weaponClasses;
            if (!weaponClasses.NullOrEmpty()){
                weaponClasses.ForEach(i => {
                    string name = i.defName;
                    flag3 = name.Contains("Ranged") || name.Contains("RangedHeavy") || name.Contains("RangedLight");
                });
            }
            bool flag = (item.IsRangedWeapon && flag1 && flag2 && flag4 || flag3) && SearchUtil.IsNotTurret(item) && !SearchUtil.IsMechWeapon(item) && item.recipeMaker != null;
            if (flag)
                CacheUtil.AddToCache(item.defName, item, StuffTagCache.WeaponsCache);
            return flag;
        }
    }

    public class WeaponSettings : StuffableCategorySettings
    {

        public WeaponSettings()
        {
            SettingsLabel = "Catch-all melee/ranged weapon settings";
        }

        public override bool ApplySearch(ThingDef item)
        {
            return false;
        }

        public override bool ApplyAltSearch(ThingDef item)
        {
            bool flag1 = SearchUtil.IsNotSCWeapon(item) && SearchUtil.IsNotTurret(item);
            bool flag2 = !item.thingCategories.NotNullAndContains(ThingCategoryDefOf.ResourcesRaw);
            bool flag3 = !item.IsDrug;
            bool flag4 = !SearchUtil.IsMechWeapon(item);
            bool flag = (item.IsRangedWeapon || item.IsMeleeWeapon) && flag1 && flag2 && flag3 && flag4 && item.recipeMaker != null;
            if (flag)
                CacheUtil.AddToCache(item.defName, item, StuffTagCache.WeaponsCache);
            return flag;
        }
    }
}
