using ArchotechWeaponry.Defs;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ArchotechWeaponry.Harmony.Patches
{
    [HarmonyPatch(typeof(CompBladelinkWeapon))]
    [HarmonyPatch("CanAddTrait")]
    public class PatchCanAddTrait
    {
        public static void Postfix(WeaponTraitDef trait, CompBiocodable __instance, ref bool __result)
        {
            if (__result)
            {
                ThingWithComps weapon = __instance.parent;
                if (trait.GetModExtension<ArchotechTraitExtension>() is ArchotechTraitExtension traitExtension)
                {
                    if (weapon.def.HasModExtension<ArchotechDamageExtension>())
                    {
                        __result = weapon.def.weaponTags.Any(tag => traitExtension.allowedTags.Contains(tag));
                    }
                    else
                    {
                        __result = false;
                    }
                }
                else
                {
                    if (weapon.def.HasModExtension<ArchotechDamageExtension>())
                    {
                        __result = false;
                    }
                }
            }
        }
    }
}