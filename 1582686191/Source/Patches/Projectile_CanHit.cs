using HarmonyLib;
using RimWorld;
using Verse;

namespace FriendlyFireTweaks.Patches
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(Projectile), "CanHit", typeof(Thing))]
    public class Projectile_CanHit
    {
        public static bool Prefix(Projectile __instance, ref bool __result, Thing thing, Thing ___launcher, LocalTargetInfo ___intendedTarget)
        {
            Thing Shooter = ___launcher;
            Thing Target = thing;
            Thing IntendedTarget = ___intendedTarget.Thing;

            if (Shooter is null || Target is null || IntendedTarget is null)
            {
                return true;
            }

            if (Target == IntendedTarget)
            {
                return true;
            }
            
            if (Shooter.HostileTo(Target))
            {
                return true;
            }

            if (!Target.ShouldBeProtectedFrom(Shooter))
            {
                return true;
            }

            if (Rand.RangeInclusive(1, 100) <= Utility.GetHitChance(Shooter))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }
}