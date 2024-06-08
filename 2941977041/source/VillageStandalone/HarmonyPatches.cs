using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace VillageStandalone
{
    [StaticConstructorOnStartup]
    public class MainHarmonyInstance : Mod
    {
        public MainHarmonyInstance(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.otters.rimworld.mod.VillageStandalones");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            CompatibilityPatches.ExecuteCompatibilityPatches(harmony);
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(RotDrawMode), typeof(PawnRenderFlags) })]
    public class PawnRenderer_RenderPawnInternal
    {
        public static bool Prefix(Vector3 rootLoc, bool renderBody, Pawn ___pawn)
        {
            if (renderBody || ___pawn?.Map == null || ___pawn?.RaceProps?.Humanlike != true) return true;
            return !(___pawn?.CurrentBed()?.def?.HasModExtension<VillageStandaloneModExtension>() == true);
        }
    } 

    [HarmonyPatch(typeof(Pawn_MindState), "MindStateTick")]
    public class Pawn_MindState_MindStateTick
    {
        public static void Postfix(Pawn_MindState __instance)
        {
            if (Find.TickManager.TicksGame % 123 == 0 && __instance.pawn.Spawned && __instance.pawn.RaceProps.IsFlesh && __instance.pawn.needs.mood != null)
            {
                var currBed = __instance.pawn.CurrentBed();
                if (currBed == null) return;
                var modExt = currBed.def.GetModExtension<VillageStandaloneModExtension>();
                if (modExt == null || !modExt.RemoveSoakingWet) return;

                WeatherDef curWeatherLerped = __instance.pawn.Map.weatherManager.CurWeatherLerped;
                if (curWeatherLerped.exposedThought != null && curWeatherLerped.exposedThought == ThoughtDef.Named("SoakingWet") && !__instance.pawn.Position.Roofed(__instance.pawn.Map))
                {
                    __instance.pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(curWeatherLerped.exposedThought);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Toils_LayDown), "ApplyBedThoughts", new Type[] { typeof(Pawn), typeof(Building_Bed) })]
    public class Toils_LayDown_ApplyBedThoughts
    {
        public static void Postfix(Pawn actor)
        {
            Building_Bed building_Bed = actor.CurrentBed();
            if (building_Bed == null) return;
            var modExt = building_Bed.def.GetModExtension<VillageStandaloneModExtension>();
            if (modExt == null) return;

            var effect = ModSettings.effects.FirstOrDefault(x => x?.villagestandaloneDefName == building_Bed.def.defName);
            if (effect == null) return;
            if (effect.RemoveSleptOutside) actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
            if (effect.RemoveSleptInCold) actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
            if (effect.RemoveSleptInHeat) actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
            if (effect.RemoveSleptInBarracks) actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
            if (effect.RemoveSleepDisturbed) actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleepDisturbed);
            if (effect.RemoveSleptInBarracks) actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);

            //if (effect.RemoveSunlightSensitivity_Mild) actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SunlightSensitivity_Mild);

        }
    }

    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), "IdeoligionForbids")]
    public class CompAssignableToPawn_Bed_IdeoligionForbids
    {
        public static void Postfix(CompAssignableToPawn_Bed __instance, ref bool __result, Pawn pawn)
        {
            if (__instance?.parent == null) return;
            var modExt = __instance.parent.def.GetModExtension<VillageStandaloneModExtension>();
            if (modExt == null) return;

            var effect = ModSettings.effects.FirstOrDefault(x => x?.villagestandaloneDefName == __instance.parent.def.defName);
            if (effect == null) return;
            if (effect.ideologyVillageStandaloneAssignmentAllowed) __result = false;
        }
    }
}   
