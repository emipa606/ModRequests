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
    public static class Pawn_TrainingTracker_Patches
    {
        [HarmonyPatch(typeof(Pawn_TrainingTracker))]
        [HarmonyPatch("Train", MethodType.Normal)]
        public static class Pawn_TrainingTracker_Train
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn_TrainingTracker __instance, TrainableDef td, Pawn trainer, bool complete, Pawn ___pawn)
            {
                if (td == TrainableDefOf.Tameness && complete == true)
                    return;

                new PendingCallouts.Interaction.PendingCalloutEventAnimalInteraction(trainer, ___pawn, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Train_Success_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Train_Success_Received).AttemptCallout();
            }
        }
    }
}