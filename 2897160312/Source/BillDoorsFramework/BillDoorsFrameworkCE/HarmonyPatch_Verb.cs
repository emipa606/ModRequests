using CombatExtended;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BillDoorsFramework
{
    [StaticConstructorOnStartup]
    public class VerbPatch
    {
        [HarmonyPatch(typeof(Verb_LaunchProjectileCE), "TryCastShot")]
        static class TryCastShot_PostFix
        {
            [HarmonyPostfix]
            static void Postfix(bool __result, Verb_LaunchProjectileCE __instance)
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
    }

    [StaticConstructorOnStartup]
    public class MortarPatch
    {
        [HarmonyPatch(typeof(Verb_ShootMortarCE), "TryCastGlobalShot")]
        static class TryCastShot_PostFix
        {
            [HarmonyPostfix]
            static void Postfix(bool __result, Verb_ShootMortarCE __instance)
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
    }


    [StaticConstructorOnStartup]
    public static class LGISdualModeNPCpatchCE
    {
        [HarmonyPatch(typeof(PawnGenerator), "PostProcessGeneratedGear", new Type[] { typeof(Thing), typeof(Pawn) })]
        static class ProjectilesSpawn_PostFix
        {
            [HarmonyPostfix]
            static void Postfix(Thing gear)
            {
                ModExtension_LGISdualModeNPCpatch extension = gear.def.GetModExtension<ModExtension_LGISdualModeNPCpatch>();
                if (extension != null && Rand.Chance(extension.chance))
                {
                    CompSecondaryVerbCE comp = gear.TryGetComp<CompSecondaryVerbCE>();
                    if (comp != null)
                    {
                        comp.InitData();
                        comp.SwitchToSecondary();
                    }
                    CompSecondaryAmmo comp2 = gear.TryGetComp<CompSecondaryAmmo>();
                    if (comp2 != null)
                    {
                        comp2.InitData();
                        comp2.SwitchToSecondary();
                    }
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    public class EquipFromInventoryPatch
    {
        [HarmonyPatch(typeof(Verb_ShootMortarCE), "TryCastGlobalShot")]
        static class TryCastShot_PostFix
        {
            [HarmonyPostfix]
            static void Postfix(bool __result, Verb_ShootMortarCE __instance)
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
    }
}
