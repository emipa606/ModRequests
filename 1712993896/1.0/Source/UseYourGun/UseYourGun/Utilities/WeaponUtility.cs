using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

/// <summary>
/// Thank you to RunAndGun author "roolo" on Steam for allowing me to use his code!
/// Anything below this point is his code merely slightly changed to work with my assembly!
/// </summary>

namespace UseYourGun.Utilities
{
    static class WeaponUtility
    {
        public static List<ThingDef> getAllWeapons()
        {
            List<ThingDef> allWeapons = new List<ThingDef>();

            Predicate<ThingDef> isWeapon = (ThingDef td) => td.equipmentType == EquipmentType.Primary && !td.weaponTags.NullOrEmpty<string>() && !td.destroyOnDrop;
            foreach (ThingDef thingDef in from td in DefDatabase<ThingDef>.AllDefs
                                          where isWeapon(td)
                                          select td)
            {
                allWeapons.Add(thingDef);
            }
            return allWeapons;
        }
    }
}
