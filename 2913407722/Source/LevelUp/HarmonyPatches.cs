using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System;

namespace LevelUp
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("Jdalt.RimWorld.LevelUp.Main");
            harmony.Patch(AccessTools.Property(typeof(Pawn), nameof(Pawn.HealthScale)).GetGetMethod(), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(LevelUpHealthScale)));
        }
        public static void LevelUpHealthScale(Pawn __instance, ref float __result)
        {
            LevelUpModSettings settings = LoadedModManager.GetMod<LevelUpMod>().GetSettings<LevelUpModSettings>();
            if (__instance.health.hediffSet.HasHediff(LevellingHediffDefOf.HealthLevel))
            {
                Hediff HealthLevel = __instance.health.hediffSet.GetFirstHediffOfDef(LevellingHediffDefOf.HealthLevel);
                if (HealthLevel.Severity < 0)
                {
                    float Severity = Rand.Range(0f, settings.MaxRandomLevel);
                    float baseHealthScale = __instance.RaceProps.baseHealthScale;
                    HealthLevel.Severity = Severity / baseHealthScale;
                    Log.Message("Level was somehow negative? Please report to Level This! Author");
                }
                int Level = Mathf.FloorToInt(HealthLevel.Severity);
                float RoundedRate = (float)Math.Round(settings.LevelUpHealthRate, digits: 2);
                float LevelUpRate = RoundedRate + 1f;
                //Log.Message("Compounding Health Maths: " + __instance.RaceProps.baseHealthScale + "(1.1f)^" + Level + " = " + __instance.RaceProps.baseHealthScale * (Mathf.Pow(1.1f, Level)), true);
                if (settings.HealthScalingType == "Compounding Health")
                {
                    __result *= (1 * Mathf.Pow(LevelUpRate, Level));
                    return;
                }
                if (settings.HealthScalingType == "Simple Health")
                {
                    __result *= (1 + (RoundedRate * Level));
                    return;
                }
            }
        }
    }
}
