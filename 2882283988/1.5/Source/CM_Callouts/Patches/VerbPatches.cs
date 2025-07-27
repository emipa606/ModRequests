using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

using CM_Callouts.PendingCallouts.Combat;

namespace CM_Callouts
{
    [StaticConstructorOnStartup]
    public static class Verb_Patches
    {
        [HarmonyPatch(typeof(Verb))]
        [HarmonyPatch("TryStartCastOn", new Type[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
        public static class Verb_TryStartCastOn
        {
            [HarmonyPostfix]
            public static void Postfix(Verb __instance)
            {
                if (__instance as Verb_MeleeAttack == null || __instance.CasterPawn == null)
                    return;

                if (CalloutUtility.CanCalloutNow(__instance.CasterPawn) && CalloutUtility.CanCalloutAtTarget(__instance.CurrentTarget.Thing))
                {
                    new PendingCalloutEventMeleeAttempt(__instance.CasterPawn, __instance.CurrentTarget.Thing as Pawn, __instance).AttemptCallout();
                }
            }
        }
    }
}
