using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using System.Reflection;
using UnityEngine;

namespace CPWeaponExt
{
    /*
     * 
     *  Harmony Classes
     *  ===============
     *  Harmony is a system developed by pardeike (aka Brrainz).
     *  It allows us to use pre/post method patches instead of using detours.
     * 
     */
    [StaticConstructorOnStartup]
    static class HarmonyRailgun
    {
        //Static Constructor
        /*
         * Contains 1 Harmony patch.
         * ===================
         * 
         * [PREFIX] MentalBreaker -> TryDoRandomMoodCausedMentalBreak
         * 
         */
        static HarmonyRailgun()
        {
            var harmony = new Harmony("rimworld.cpwallshot");
            var type = typeof(HarmonyRailgun);
            harmony.Patch(
                AccessTools.Method(typeof(Projectile),
                    "CanHit"), null,
                new HarmonyMethod(type, nameof(CanHit_PostFix)), null);
            harmony.Patch(
                AccessTools.Method(typeof(Verb),
                    "CanHitCellFromCellIgnoringRange"),
                new HarmonyMethod(type, nameof(CanHitCellFromCellIgnoringRange_Prefix)), null);
        }
        
        //Added B19, Oct 2019
        //ProjectileExtension check
        public static bool CanHitCellFromCellIgnoringRange_Prefix(Verb __instance, IntVec3 sourceSq, IntVec3 targetLoc, bool includeCorners, ref bool __result)
        {
            if (__instance?.EquipmentCompSource?.PrimaryVerb?.verbProps?.defaultProjectile is ThingDef proj &&
                proj?.HasModExtension<ProjectileExtension>() == true &&
                proj.GetModExtension<ProjectileExtension>() is ProjectileExtension ext)
            {
                if (ext.passesWalls || ext.passesRoofs)
                    __result = true;
                return false;
            }

            return true;
        }

        //Added B19, Oct 2019
        //ProjectileExtension check
        public static void CanHit_PostFix(Projectile __instance, Thing thing, ref bool __result)
        {
            if (!__result && __instance?.def?.HasModExtension<ProjectileExtension>() == true &&
                __instance.def.GetModExtension<ProjectileExtension>() is ProjectileExtension ext)
            {
                //Mods will often have their own walls, so we cannot do a def check for 
                //ThingDefOf.Wall
                //Most "walls" should either be in the structure category or be able to hold walls.
                if (thing?.def?.designationCategory == DesignationCategoryDefOf.Structure ||
                    thing?.def?.holdsRoof == true)
                {
                    if (ext.passesWalls)
                    {
                        __result = false;
                        return;
                    }
                }
            }
        }
    }
}