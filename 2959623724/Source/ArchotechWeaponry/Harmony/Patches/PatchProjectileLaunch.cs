using System;
using ArchotechWeaponry.Comps;
using ArchotechWeaponry.Utils;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ArchotechWeaponry.Harmony.Patches
{
[HarmonyPatch(typeof(Projectile))]
    [HarmonyPatch("Launch")]
    [HarmonyPatch(new Type[] {typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef)})]
    public class PatchProjectileLaunch
    {
        [HarmonyPostfix]
        public static void Postfix(Projectile __instance,
            Thing launcher,
            Vector3 origin,
            LocalTargetInfo usedTarget,
            LocalTargetInfo intendedTarget,
            bool preventFriendlyFire,
            Thing equipment,
            ThingDef targetCoverDef)
        {
            if (__instance.TryGetComp<Comp_HomingProjectile>() is Comp_HomingProjectile comp)
            {
                float chanceToRedirect = comp.Props.homingChance;
                if (Rand.Chance(chanceToRedirect))
                {
                    if (usedTarget != intendedTarget)
                    {
                        Vector3 newDest = intendedTarget.Cell.ToVector3Shifted();
                        AccessTools.Field(typeof(Projectile), "destination")
                            .SetValue(__instance, newDest);
                        AccessTools.Field(typeof(Projectile), "ticksToImpact").SetValue(__instance, Mathf.CeilToInt(ProjectileUtils.RecalculateNewTicksToImpact(__instance, newDest)));
                        __instance.HitFlags = ProjectileHitFlags.IntendedTarget | ProjectileHitFlags.NonTargetPawns;
                        __instance.usedTarget = intendedTarget;
                    }

                    if (intendedTarget.HasThing && !intendedTarget.ThingDestroyed)
                    {
                        comp?.SetHoming(true);
                        comp?.SetLastCell(intendedTarget.Thing.Position);
                    }
                }    
            }    
        }
    }
}