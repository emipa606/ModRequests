using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

using CM_Callouts.PendingCallouts.Interaction;

namespace CM_Callouts.Patches
{
    [StaticConstructorOnStartup]
    public static class Toils_Tend_Patches
    {
        [HarmonyPatch(typeof(Toils_Tend))]
        [HarmonyPatch("FinalizeTend", MethodType.Normal)]
        public static class Toils_Tend_FinalizeTend
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn patient, Toil __result)
            {
                if (__result == null)
                    return;

                if (patient == null || !patient.AnimalOrWildMan())
                    return;

                __result.AddPreInitAction(delegate
                {
                    Pawn initiator = __result.GetActor();
                    new PendingCalloutEventAnimalInteraction(initiator, patient, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Tend_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Tend_Received).AttemptCallout();
                });
            }
        }
    }
}