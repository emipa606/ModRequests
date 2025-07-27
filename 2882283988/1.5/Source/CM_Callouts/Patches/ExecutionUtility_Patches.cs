using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Callouts.Patches
{
    [StaticConstructorOnStartup]
    public static class ExecutionUtility_Patches
    {
        [HarmonyPatch(typeof(ExecutionUtility))]
        [HarmonyPatch("DoExecutionByCut", MethodType.Normal)]
        public static class ExecutionUtility_DoExecutionByCut
        {
            [HarmonyPrefix]
            public static void Prefix(Pawn executioner, Pawn victim)
            {
                if (!victim.AnimalOrWildMan())
                    return;

                new PendingCallouts.Interaction.PendingCalloutEventAnimalInteraction(executioner, victim, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Slaughter_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Slaughter_Received).AttemptCallout();
            }
        }
    }
}