using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

namespace customYear
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("customYear");
            harmony.PatchAll(Assembly.GetExecutingAssembly());


        }

    }

    [HarmonyPatch(typeof(GenDate), "Year")]
    [HarmonyPatch(new Type[] { typeof(long), typeof(float) })]
    internal class patch_GenDate_Year
    {
        [HarmonyPrefix]
        static bool Prefix(ref int __result, long absTicks, float longitude)
        {
            long num = absTicks + (long)GenDate.TimeZoneAt(longitude) * 2500L;
            __result = modBase.startYear + Mathf.FloorToInt((float)num / 3600000f) * modBase.unitYear;
            return false;
        }
    }

    [HarmonyPatch(typeof(Verse.Pawn_AgeTracker), "AgeTickMothballed")]
    public static class patch_Pawn_AgeTracker_AgeTickMothballed
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn_AgeTracker __instance)
        {
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay == 0)
            {
                AccessTools.Method(__instance.GetType(), "RecalculateLifeStageIndex", null, null).Invoke(__instance, null);
            }
        }
    }


    /*
    // pawn aging
    [HarmonyPatch(typeof(Verse.Pawn), "Tick")]
    public static class patch_pawn_tick
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance)
        {
            if (!__instance.IsColonist) return;

            int multiplier = 1;

            

            if (__instance.RaceProps.Humanlike)
            {
                multiplier = modBase.humanAging;
            }
            else
            {
                multiplier = modBase.animalAging;
            }

            int additionalTick = multiplier - 1;

            if (additionalTick <= 0) return;

            //__instance.ageTracker.AgeTickMothballed(additionalTick);
            //__instance.TickMothballed(additionalTick);

            Log.Message($"--- {Find.TickManager.TicksAbs.ToString()}");

            Log.Message(__instance.ageTracker.AgeBiologicalTicks.ToString());


        }
    }
    */




}
