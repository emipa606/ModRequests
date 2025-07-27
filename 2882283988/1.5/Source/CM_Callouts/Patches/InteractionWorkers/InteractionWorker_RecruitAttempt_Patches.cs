using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Callouts.Patches.InteractionWorkers
{
    [StaticConstructorOnStartup]
    public static class InteractionWorker_RecruitAttempt_Patches
    {
        //[HarmonyPatch(typeof(InteractionWorker_RecruitAttempt))]
        //[HarmonyPatch("DoRecruit", new Type[] { typeof(Pawn), typeof(Pawn), typeof(float), typeof(string), typeof(string), typeof(bool), typeof(bool) })]
        public static class InteractionWorker_RecruitAttempt_DoRecruit
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn recruiter, Pawn recruitee)
            {
                if (!recruitee.AnimalOrWildMan())
                    return;

                new PendingCallouts.Interaction.PendingCalloutEventAnimalInteraction(recruiter, recruitee, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Tame_Success_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Tame_Success_Received).AttemptCallout();
            }

            public static void Patch(Harmony harmony)
            {
                MethodInfo doRecruit = AccessTools.Method(typeof(InteractionWorker_RecruitAttempt), "DoRecruit", new Type[] { typeof(Pawn), typeof(Pawn), typeof(float), typeof(string).MakeByRefType(), typeof(string).MakeByRefType(), typeof(bool), typeof(bool) });
                if (doRecruit != null)
                {
                    HarmonyMethod postFix = new HarmonyMethod(SymbolExtensions.GetMethodInfo(() => Postfix(null, null)));
                    harmony.Patch(doRecruit, null, postFix);
                }
            }
        }
    }
}