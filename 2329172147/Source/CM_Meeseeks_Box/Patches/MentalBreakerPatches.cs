using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class MentalBreakerPatches
    {
        //TaggedString label, TaggedString text, LetterDef textLetterDef, LookTargets lookTargets, Faction relatedFaction = null, Quest quest = null, List<ThingDef> hyperlinkThingDefs = null, string debugInfo = null
        [HarmonyPatch(typeof(MentalBreaker))]
        [HarmonyPatch("TestMoodMentalBreak", MethodType.Normal)]
        public static class MentalBreaker_TestMoodMentalBreak
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, Pawn ___pawn, int ___ticksBelowMinor)
            {
                if (__result == true)
                    return;

                // Normal chance + if it fails, try again at the extreme break frequency
                if (___ticksBelowMinor > 2000)
                {
                    CompMeeseeksMemory compMeeseeksMemory = ___pawn.GetComp<CompMeeseeksMemory>();

                    if (compMeeseeksMemory != null)
                    {
                        __result = Rand.MTBEventOccurs(0.5f, 60000f, 150f);
                    }
                }
            }
        }
    }
}
