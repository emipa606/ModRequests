using HarmonyLib;
using System;
using Verse;

namespace BillDoorsFramework
{
    /*
    [StaticConstructorOnStartup]
    public class StoreProjectiles
    {
        [HarmonyPatch(typeof(ThingWithComps), "SpawnSetup", new Type[] { typeof(Map), typeof(bool) })]
        static class ProjectilesSpawn_PostFix
        {
            [HarmonyPostfix]
            static void Postfix(ThingWithComps __instance)
            {
                if (ProjectilesStore.IsProjectile(__instance))
                {
                    ProjectilesStore.projectiles.Add(__instance);
                }
            }
        }
        [HarmonyPatch(typeof(ThingWithComps), "DeSpawn", new Type[] { typeof(DestroyMode) })]
        static class ProjectilesDeSpawn_PostFix
        {
            [HarmonyPostfix]
            static void Postfix(ThingWithComps __instance)
            {
                if (ProjectilesStore.IsProjectile(__instance))
                {
                    ProjectilesStore.projectiles.Remove(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
        static class TryCastShot_PostFix
        {
            [HarmonyPostfix]
            static void Postfix(bool __result, Verb_LaunchProjectile __instance)
            {
                if (__result)
                {
                    if ((__instance.caster as ThingWithComps)?.GetComp<CompPowerTrader_ForTurretGun>() is CompPowerTrader_ForTurretGun comp)
                    {
                        comp.shotCount += 1;
                        comp.shotTime = Find.TickManager.TicksGame + __instance.verbProps.ticksBetweenBurstShots + 1;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
        static class PreFix
        {
            [HarmonyPostfix]
            static void PostFix(Verb __instance)
            {
                if (__instance.EquipmentSource != null)
                {
                    if (__instance.EquipmentSource.TryGetComp<CompDurability>() is CompDurability comp)
                    {
                        if (__instance.IsMeleeAttack)
                        {
                            if (!comp.Props.melee) return;
                        }
                        else
                        {
                            if (!comp.Props.range) return;
                        }
                        int num = 0;
                        int amount = 0;
                        while (num < comp.Props.time)
                        {
                            num++;
                            if (Rand.Chance(comp.Chance))
                            {
                                amount++;
                            }
                        }
                        __instance.EquipmentSource.TakeDamage(new DamageInfo(RimWorld.DamageDefOf.Deterioration, amount));
                    }
                }
            }
        }
    }
    */
}
