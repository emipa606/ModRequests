using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace StayPut
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("StayPut.Mod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Pawn_MindState), "StartFleeingBecauseOfPawnAction")]
    public static class Pawn_MindState_StartFleeingBecauseOfPawnAction_Patch_Patch
    {
        public static bool startFleeing;
        public static void Prefix(Pawn_MindState __instance)
        {
            if (__instance.pawn.RaceProps.Animal)
            {
                startFleeing = true;
            }
        }
        public static void Postfix(Pawn_MindState __instance)
        {
            startFleeing = false;
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    public class StartJobPatch
    {
        private static void Prefix(Pawn_JobTracker __instance, Pawn ___pawn, ref Job newJob, JobTag? tag)
        {
            if (newJob?.def == JobDefOf.Flee)
            {
                if (newJob.exitMapOnArrival && Pawn_MindState_StartFleeingBecauseOfPawnAction_Patch_Patch.startFleeing)
                {
                    newJob.exitMapOnArrival = false;
                }
            }
        }
    }
}
