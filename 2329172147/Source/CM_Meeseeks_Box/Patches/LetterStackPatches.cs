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
    public static class LetterStackPatches
    {
        //TaggedString label, TaggedString text, LetterDef textLetterDef, LookTargets lookTargets, Faction relatedFaction = null, Quest quest = null, List<ThingDef> hyperlinkThingDefs = null, string debugInfo = null
        [HarmonyPatch(typeof(LetterStack))]
        [HarmonyPatch("ReceiveLetter", new Type[] { typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(Quest), typeof(List<ThingDef>), typeof(string) })]//MethodType.Normal)]
        public static class LetterStack_ReceiveLetter_DontCryOverSpiltMeeseeks
        {
            [HarmonyPrefix]
            public static bool Prefix(LetterDef textLetterDef, LookTargets lookTargets)
            {
                if (textLetterDef == LetterDefOf.Death && lookTargets.PrimaryTarget.HasThing)
                {
                    Pawn deadPawn = lookTargets.PrimaryTarget.Thing as Pawn;

                    if (deadPawn != null && deadPawn.kindDef == MeeseeksDefOf.MeeseeksKind)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
