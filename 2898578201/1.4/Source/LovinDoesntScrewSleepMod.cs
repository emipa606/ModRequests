using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace LovinDoesntScrewSleep
{
    public class LovinDoesntScrewSleepMod : Mod
    {
        public LovinDoesntScrewSleepMod(ModContentPack pack) : base(pack)
        {
            new Harmony("LovinDoesntScrewSleep.Mod").PatchAll();
        }
    }

    [DefOf]
    public static class LDSS_DefOf
    {
        public static HediffDef LDSS_LovinSleep;
    }

    [HarmonyPatch(typeof(RestUtility), "CanFallAsleep")]
    public static class RestUtility_CanFallAsleep_Patch
    {
        public static List<Pawn> shouldFallAsleep = new List<Pawn>();
        public static void Postfix(ref bool __result, Pawn pawn)
        {
            if (shouldFallAsleep.Contains(pawn))
            {
                __result = true;
                shouldFallAsleep.Remove(pawn);
            }
        }
    }
    [HarmonyPatch(typeof(Pawn_JobTracker), "CheckForJobOverride")]
    public static class Pawn_JobTracker_CheckForJobOverride_Patch
    {
        public static bool Prefix(Pawn_JobTracker __instance)
        {
            var hediff = __instance.pawn.health.hediffSet.GetFirstHediffOfDef(LDSS_DefOf.LDSS_LovinSleep);
            if (__instance.pawn.RaceProps.Humanlike && __instance.pawn.CurJobDef == JobDefOf.LayDown)
            {
                if (hediff != null)
                {
                    if (__instance.pawn.needs.rest.CurLevelPercentage < 0.99f)
                    {
                        return false;
                    }
                    else
                    {
                        __instance.pawn.health.RemoveHediff(hediff);
                    }
                }
            }
            else if (hediff != null)
            {
                __instance.pawn.health.RemoveHediff(hediff);
            }
            return true;
        }
    }

    [HarmonyPatch]
    public static class JobDriver_Lovin_FinishAction
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.GetDeclaredMethods(typeof(JobDriver_Lovin)).LastOrDefault(x => x.Name.Contains("<MakeNewToils>") && x.ReturnType == typeof(void));
        }
        public static void Postfix(JobDriver __instance)
        {
            __instance.pawn.health.AddHediff(LDSS_DefOf.LDSS_LovinSleep);
            RestUtility_CanFallAsleep_Patch.shouldFallAsleep.Add(__instance.pawn);
        }
    }
}
