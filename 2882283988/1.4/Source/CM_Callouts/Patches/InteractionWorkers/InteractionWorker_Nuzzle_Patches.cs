using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Callouts.Patches.InteractionWorkers
{
    [StaticConstructorOnStartup]
    public static class InteractionWorker_Nuzzle_Patches
    {
        [HarmonyPatch(typeof(InteractionWorker_Nuzzle))]
        [HarmonyPatch("Interacted", MethodType.Normal)]
        public static class InteractionWorker_Nuzzle_Interacted
        {
            [HarmonyPostfix]
            public static void Postfix(InteractionWorker_Nuzzle __instance, Pawn initiator, Pawn recipient)
            {
                if (recipient.needs.mood == null)
                    return;

                // Recipient and initiator are deliberately reversed here so that the nuzzle callout can share logic with other animal interaction
                new PendingCallouts.Interaction.PendingCalloutEventAnimalInteraction(recipient, initiator, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Nuzzle_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Nuzzle_Received).AttemptCallout();
            }
        }
    }
}