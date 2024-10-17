using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Metro
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("Metro2033.Mod").PatchAll();
        }
    }

    //[HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    //public class StartJobPatch
    //{
    //    private static void Postfix(Pawn_JobTracker __instance, Pawn ___pawn, Job newJob, JobTag? tag)
    //    {
    //        if (___pawn is Mutant)
    //        {
    //            try
    //            {
    //                Log.Message(___pawn + " starts " + newJob);
    //            }
    //            catch
    //            {
    //                Log.Message(___pawn + " starts " + newJob.def);
    //            }
    //        }
    //    }
    //}
    //
    //
    //[HarmonyPatch(typeof(Pawn_JobTracker), "EndCurrentJob")]
    //public class EndCurrentJobPatch
    //{
    //    private static void Prefix(Pawn_JobTracker __instance, Pawn ___pawn, JobCondition condition, ref bool startNewJob, bool canReturnToPool = true)
    //    {
    //
    //        if (___pawn is Mutant)
    //        {
    //            try
    //            {
    //                Log.Message(___pawn + " ends " + __instance.curJob + " - " + startNewJob);
    //            }
    //            catch
    //            {
    //
    //            }
    //
    //        }
    //    }
    //}
}