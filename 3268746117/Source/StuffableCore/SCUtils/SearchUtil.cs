using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace StuffableCore.SCUtils
{
    public static class SearchUtil
    {

        private static readonly List<string> InvalidDeffNames = new List<string> { "DankPyon_RawWood", "DankPyon_RawDarkWood", "WoodLog" };
        private static readonly ThingCategoryDef RawWoodCategory = DefDatabase<ThingCategoryDef>.GetNamed("DankPyon_RawWood");
        private static bool IsInvalidItem(ThingDef def)
        {
            if (def.IsWithinCategory(ThingCategoryDefOf.ResourcesRaw)) return true;
            if (RawWoodCategory != null && def.IsWithinCategory(RawWoodCategory)) return true;
            if (InvalidDeffNames.Contains(def.defName.ToLower())) return true;
            if (def.virtualDefParent != null && IsInvalidItem(def.virtualDefParent)) return true;
            return false;
        }
        
        public static bool IsSCWeapon(ThingDef item)
        {
            return !IsInvalidItem(item) && (item.weaponTags.NotNullAndContains(SCConstants.StuffableWeapon) || item.weaponTags.NotNullAndContains(SCConstants.StuffableWeaponMelee) || item.weaponTags.NotNullAndContains(SCConstants.StuffableWeaponRanged));
        }

        public static bool IsNotSCWeapon(ThingDef item)
        {
            return !IsSCWeapon(item);
        }

        public static bool IsTurret(ThingDef item)
        {
            return item.weaponTags.NotNullAndContains("TurretGun");
        }

        public static bool IsNotTurret(ThingDef item)
        {
            return !IsTurret(item);
        }

        public static bool IsMechWeapon(ThingDef item)
        {
            bool flag = false;
            List<string> search = item.weaponTags;
            if (search.NullOrEmpty())
                return flag;
            foreach (string key in search)
            {
                string v0 = key.ToLower();
                if (v0.Contains("MechanoidGun".ToLower()) 
                    || v0.Contains("Hellsphere".ToLower()) 
                    || v0.Contains("BeamGraser".ToLower()) 
                    || v0.Contains("InfernoCannonGun".ToLower())
                    || v0.Contains("ChargeBlaster".ToLower()))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }
    }
}
