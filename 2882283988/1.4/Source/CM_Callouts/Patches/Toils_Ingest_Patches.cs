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
    public static class Toils_Ingest_Patches
    {
        [HarmonyPatch(typeof(Toils_Ingest))]
        [HarmonyPatch("ChewIngestible", MethodType.Normal)]
        public static class Toils_Ingest_ChewIngestible
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn chewer, TargetIndex ingestibleInd, Toil __result)
            {
                if (__result == null)
                    return;

                if (chewer == null || !chewer.AnimalOrWildMan())
                    return;

                __result.AddPreInitAction(delegate
                {
                    Pawn initiator = __result.GetActor();
                    if (initiator == chewer)
                        return;

                    Thing food = initiator.CurJob.GetTarget(ingestibleInd).Thing;
                    if (!food.IngestibleNow)
                    {
                        return;
                    }

                    if (food != null)
                    {
                        new PendingCalloutEventAnimalInteractionWithFood(initiator, chewer, food.def, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Feed_Initiated, CalloutDefOf.CM_Callouts_RulePack_Interaction_Animal_Feed_Received).AttemptCallout();
                    }
                });
            }
        }
    }
}