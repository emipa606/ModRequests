using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RecreationForIdle
{
    public class RecreationForIdleMod : Mod
    {
        public RecreationForIdleMod(ModContentPack pack) : base(pack)
        {
            new Harmony("RecreationForIdleMod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Need_Joy), "GainJoy")]
    public static class Need_Joy_GainJoy_Patch
    {
        public static void Prefix(Need_Joy __instance, ref float amount, JoyKindDef joyKind)
        {
            if (__instance.CurLevelPercentage >= 0.90f && __instance.pawn.mindState?.lastJobTag == JobTag.Idle)
            {
                amount *= 0.075f;
            }
        }
    }

    [HarmonyPatch(typeof(Need_Joy), "FallPerInterval", MethodType.Getter)]
    public static class Need_Joy_FallPerInterval_Patch
    {
        public static void Postfix(Need_Joy __instance, ref float __result)
        {
            if (__instance.CurLevelPercentage >= 0.90f && __instance.pawn.mindState.IsIdle)
            {
                if (PatchHelper.JobIncludesPauses(__instance.pawn.CurJobDef))
                {
                    __result *= 1;
                }
                else
                {
                    __result *= 4;
                }
            }
        }
    }

    //Temp
    // Alternative to change boredom gain
    //[HarmonyPatch(typeof(Need_Joy), "GainJoy")]
    //public static class GainJoyPatch
    //{
    //    static bool Prefix(Need_Joy __instance, float amount, JoyKindDef joyKind)
    //    {
    //        if (amount <= 0f)
    //        {
    //            return false;
    //        }

    //        if (__instance.CurLevelPercentage >= 0.90f && __instance.pawn.mindState?.lastJobTag == JobTag.Idle)
    //        {
    //            amount *= 0.075f;
    //        }

    //        amount *= __instance.tolerances.JoyFactorFromTolerance(joyKind);
    //        amount = Mathf.Min(amount, 1f - __instance.CurLevel);
    //        __instance.curLevelInt += amount;

    //        if (joyKind != null)
    //        {
    //            __instance.tolerances.Notify_JoyGained(amount/* * 0.75f*/, joyKind);
    //        }

    //        __instance.lastGainTick = Find.TickManager.TicksGame;

    //        return false;
    //    }
    //}

    // Attempt to change boredom gain
    //[HarmonyPatch(typeof(JoyToleranceSet), "Notify_JoyGained")]
    //public static class JoyToleranceSet_Notify_JoyGained_Patch
    //{
    //    public static void Postfix(JoyToleranceSet __instance, float amount, JoyKindDef joyKind)
    //    {

    //        if (/*joyNeed.CurLevelPercentage >= 0.75f &&*/ pawn.mindState?.lastJobTag == JobTag.Idle)
    //        {

    //            __instance.tolerances[joyKind] -= amount * 0f;
    //        }
    //    }
    //} 

    public static class PatchHelper
    {
        public static void MultiplyJoyDuration(JobDriver jobDriver)
        {
            Pawn pawn = jobDriver.pawn;

            if (pawn.needs.joy != null && /*pawn.needs.joy.CurLevelPercentage >= 0.75f &&*/ pawn.mindState?.lastJobTag == JobTag.Idle)
            {
                Job job = jobDriver.job;

                if (job != null)
                {
                    job.def.joyDuration *= 10;
                }
            }
        }

        public static bool JobIncludesPauses(JobDef jobDef)
        {
            if (jobDef.defName == "Play_Billiards")
            {
                return true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(JobDriver_EatAtCannibalPlatter), "MakeNewToils")]
    public static class JobDriver_EatAtCannibalPlatter_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_EatAtCannibalPlatter __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_GoForWalk), "MakeNewToils")]
    public static class JobDriver_GoForWalk_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_GoForWalk __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_Meditate), "MakeNewToils")]
    public static class JobDriver_Meditate_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_Meditate __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_PlayBilliards), "MakeNewToils")]
    public static class JobDriver_PlayBilliards_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_PlayBilliards __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_PlayGoldenCube), "MakeNewToils")]
    public static class JobDriver_PlayGoldenCube_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_PlayGoldenCube __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_Reading), "MakeNewToils")]
    public static class JobDriver_Reading_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_Reading __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_Reign), "MakeNewToils")]
    public static class JobDriver_Reign_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_Reign __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_RelaxAlone), "MakeNewToils")]
    public static class JobDriver_RelaxAlone_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_RelaxAlone __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_SitFacingBuilding), "MakeNewToils")]
    public static class JobDriver_SitFacingBuilding_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_SitFacingBuilding __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_Skygaze), "MakeNewToils")]
    public static class JobDriver_Skygaze_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_Skygaze __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_SocialRelax), "MakeNewToils")]
    public static class JobDriver_SocialRelax_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_SocialRelax __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_VisitJoyThing), "MakeNewToils")]
    public static class JobDriver_VisitJoyThing_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_VisitJoyThing __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_VisitSickPawn), "MakeNewToils")]
    public static class JobDriver_VisitSickPawn_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_VisitSickPawn __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

    [HarmonyPatch(typeof(JobDriver_WatchBuilding), "MakeNewToils")]
    public static class JobDriver_WatchBuilding_MakeNewToils_Patch
    {
        public static void Prefix(JobDriver_WatchBuilding __instance)
        {
            PatchHelper.MultiplyJoyDuration(__instance);
        }
    }

}
