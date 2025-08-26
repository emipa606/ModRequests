using System;
using Verse;
using HarmonyLib;
using RimWorld;
using Verse.AI;
using Settings = Tacticowl.ModSettings_Tacticowl;

namespace Tacticowl
{
    [HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
    static class Patch_TryStartMentalState
    {
        static bool Prepare()
		{
			return Settings.runAndGunEnabled;
		}
        //Used by MP
        static void Postfix(MentalStateHandler __instance, MentalStateDef stateDef)
        {
            if (stateDef == MentalStateDefOf.PanicFlee && Settings.enableForAI && Verse.Rand.Range(1, 100) <= Settings.enableForFleeChance)
                __instance.pawn.SetRunsAndGuns(Settings.CanRunAndGunAI(__instance.pawn));
            
        }
    }
}
