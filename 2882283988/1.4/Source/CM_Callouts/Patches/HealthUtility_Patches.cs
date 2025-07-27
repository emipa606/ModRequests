using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

using CM_Callouts.PendingCallouts;

namespace CM_Callouts
{
    [StaticConstructorOnStartup]
    public static class HealthUtility_Patches
    {
        [HarmonyPatch(typeof(HealthUtility))]
        [HarmonyPatch("AdjustSeverity", MethodType.Normal)]
        public static class HealthUtility_AdjustSeverity
        {
            private static Hediff initialFoundHediff = null;
            private static int startingStageIndex = -1;

            [HarmonyPrefix]
            public static void Prefix(Pawn pawn, HediffDef hdDef, float sevOffset)
            { 
                if (sevOffset != 0f && pawn != null && hdDef?.lethalSeverity > 0.0f)
                {
                    initialFoundHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hdDef);
                    if (initialFoundHediff != null)
                        startingStageIndex = initialFoundHediff.CurStageIndex;
                }
                else
                {
                    initialFoundHediff = null;
                    startingStageIndex = -1;
                }
            }

            [HarmonyPostfix]
            public static void Postfix(Pawn pawn, HediffDef hdDef, float sevOffset)
            {
                if (sevOffset == 0f || pawn == null || hdDef?.lethalSeverity <= 0.0f)
                {
                    // Resetting even though it seems unnecessary, in case another call somehow circumvented the prefix (probably not possible :P)
                    initialFoundHediff = null;
                    startingStageIndex = -1;
                    return;
                }

                Hediff foundHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hdDef);

                if (foundHediff != null)
                {
                    int currentStageIndex = foundHediff.CurStageIndex;

                    if (initialFoundHediff == null || currentStageIndex > startingStageIndex)
                    {
                        HediffStage currentStage = foundHediff.CurStage;
                        ShowWoundLevel showWoundLevel = CalloutMod.settings.showWoundLevel;

                        // If the pawn is spawned and the current stage should be visible
                        if (pawn.SpawnedOrAnyParentSpawned && currentStage != null && currentStage.becomeVisible)
                        {
                            // Do a callout
                            new PendingCalloutEventLethalHediffProgression(pawn, foundHediff).AttemptCallout();

                            int stagesAwayFromDeath = foundHediff.def.stages.Count - currentStageIndex;
                            // Throw text if wound is serious enough according to our settings
                            if (showWoundLevel != ShowWoundLevel.None && (showWoundLevel == ShowWoundLevel.All || stagesAwayFromDeath <= (int)showWoundLevel))
                            {
                                Vector3 thingVector3 = pawn.SpawnedParentOrMe.DrawPos;
                                Map thingMap = pawn.SpawnedParentOrMe.Map;

                                Color moteColor = HealthUtility.SlightlyImpairedColor;
                                if (stagesAwayFromDeath == 1)
                                    moteColor = Color.magenta;
                                else if (stagesAwayFromDeath == 2)
                                    moteColor = HealthUtility.RedColor;
                                else if (stagesAwayFromDeath == 3)
                                    moteColor = HealthUtility.ImpairedColor;

                                CalloutTracker.CreateWoundTextMote(thingVector3, thingMap, foundHediff.Label, moteColor);
                            }
                        }
                    }
                }

                initialFoundHediff = null;
                startingStageIndex = -1;
            }
        }
    }
}
