using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace StrayBullets
{
    [StaticConstructorOnStartup]
    public class HarmonyPatch
    {
        static MethodInfo _impact = typeof(Projectile).GetMethod("Impact", BindingFlags.Instance | BindingFlags.NonPublic);

        static HarmonyPatch()
        {
            var harmony = new Harmony("nkm.strayBullets");
            harmony.Patch(AccessTools.Method(typeof(Projectile), "CheckForFreeInterceptBetween"), null, new HarmonyMethod(typeof(HarmonyPatch).GetMethod("PostFix_CheckForFreeInterceptBetween")));
        }
        [HarmonyPriority(401)]
        public static void PostFix_CheckForFreeInterceptBetween(Projectile __instance, Vector3 lastExactPos, Vector3 newExactPos, ref bool __result, Vector3 ___origin)
        {
            if (__result)
                return;
            if (!(__instance is Bullet))
                return;
            if (__instance.HitFlags.HasFlag(ProjectileHitFlags.IntendedTarget)) // Activate only if it's missed shot
                return;
            if (lastExactPos.ToIntVec3() == newExactPos.ToIntVec3()) // Prevent duplicate hits
                return;
            if (Compatibility.VWE_Coilguns && __instance.EquipmentDef.modContentPack.Name == "Vanilla Weapons Expanded - Coilguns") // Compatibility patch
                return;

            float factorFromDistance = VerbUtility.InterceptChanceFactorFromDistance(___origin, newExactPos.ToIntVec3());
            List<Thing> things = __instance.Position.GetThingList(__instance.Map).Where(x => x is Pawn && x != __instance.Launcher && x != __instance.intendedTarget.Thing || x.BaseBlockChance() > 0f).ToList();
            foreach (var thing in things)
            {
                float hitChance = 0f;
                if(thing is Pawn) // Pawn
                {
                    Pawn pawn = thing as Pawn;
                    float factorFromPosture = pawn.GetPosture() == PawnPosture.Standing ? 1f : 0.1f; // 10% chance to hit downed pawn
                    float factorFromTargetSize = Mathf.Clamp(pawn.BodySize, 0f, 2f);
                    float passBodyChance = factorFromPosture * factorFromTargetSize;
                    float passCoverChance = 1f - CoverUtility.CalculateOverallBlockChance(pawn, __instance.Launcher.Position, __instance.Map);
                    float friendlyFireChance = 1f;

                    if (__instance.Launcher?.Faction != null && pawn.Faction != null && !pawn.HostileTo(__instance.Launcher.Faction)) // isFriendly?
                    {
                        if (factorFromDistance <= 0f) // too close
                            continue;
                        else
                            friendlyFireChance = Settings.FriendlyFireProbability;
                    }

                    hitChance = Settings.BaseHitProbability * passCoverChance * passBodyChance * friendlyFireChance;
                }
                else // Covers
                {
                    hitChance = thing.BaseBlockChance() * factorFromDistance;
                }

                if (Rand.Chance(hitChance))
                {
                    Main.LogMessage($"[{string.Format("{0:00}%", hitChance * 100f)}] {__instance.Launcher} hit {thing} with {__instance.EquipmentDef}");
                    _impact.Invoke(__instance, new object[] { thing, false });
                    __result = true;
                    break;
                }
            }
        }
    }
}