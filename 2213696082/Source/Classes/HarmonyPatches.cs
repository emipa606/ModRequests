using Verse;
using RimWorld;
using Verse.AI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using static Verse.PawnCapacityUtility;

namespace LifeSupport {
    [StaticConstructorOnStartup]
    public static class HarmonyPatches {
        internal static Type Hediff_DeathRattle = null;

        static HarmonyPatches() {
            //Harmony
            var harmony = new Harmony("LifeSupport");

            harmony.Patch(
                AccessTools.Method(typeof(Pawn_HealthTracker), "ShouldBeDeadFromRequiredCapacity"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_ShouldBeDeadFromRequiredCapacity)))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Toils_LayDown), "LayDown"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_LayDown)))
            );

            harmony.Patch(
                AccessTools.Method(typeof(PawnCapacityUtility), "CalculateLimbEfficiency"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_CalculateLimbEfficiency)))
            );

            Hediff_DeathRattle = AccessTools.TypeByName("DeathRattle.Hediff_DeathRattle");
            if (!(Hediff_DeathRattle is null)) {
                Log.Message("[LifeSupport] found DeathRattle");
            }
        }

        public static bool Patch_ShouldBeDeadFromRequiredCapacity(ref Pawn_HealthTracker __instance, ref PawnCapacityDef __result) {
            // Check if consciousness is there. If it is then its okay.

            Pawn_HealthTracker health = __instance;
            Pawn pawn = health.hediffSet.pawn;

            if (!health.hediffSet.HasHediff(LifeSupportDefOf.QE_LifeSupport)) {
                // not on life support
                return true;
            } else if (!pawn.ValidLifeSupportNearby()) {
                // life support is unpowered
                return true;
            } else if (!health.capacities.CapableOf(PawnCapacityDefOf.Consciousness)) {
                // no consciousness
                return true;
            }

            __result = null;
            return false;
        }

        public static void Patch_LayDown(ref Toil __result) {
            bool debug = false;
            if (debug) Log.Message("Patch_LayDown");
            Toil toil = __result;
            if (toil == null)
                return;

            toil.AddPreTickAction(delegate () {
                Pawn pawn = toil.actor;
                if (pawn is null || pawn.Dead) {
                    return;
                }

                pawn.SetHediffs();
            });
        }

        public static bool Patch_CalculateLimbEfficiency(ref float __result, HediffSet diffSet, BodyPartTagDef limbCoreTag, BodyPartTagDef limbSegmentTag,
                BodyPartTagDef limbDigitTag, float appendageWeight, out float functionalPercentage, List<CapacityImpactor> impactors) {
            functionalPercentage = 0f;

            if (limbCoreTag != BodyPartTagDefOf.MovingLimbCore) {
                return true;
            }

            var hediff = diffSet.GetFirstHediffOfDef(LifeSupportDefOf.QE_LifeSupport);
            if (hediff is null) {
                return true;
            }
            
            if (hediff.Severity < 1f) {
                return true;
            }

            __result = 0f;

            if (!(impactors is null)) {
                var capacityImpactor = new CapacityImpactorHediff();
                capacityImpactor.hediff = hediff;
                impactors.Add(capacityImpactor);
            }

            return false;
        }
    }
}
