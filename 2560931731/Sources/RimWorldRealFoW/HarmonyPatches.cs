using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorldRealFoW
{
    internal class HarmonyPatches
    {
        //Pawn will not target thing that is hidden by the fog
        [HarmonyPrefix]
        public static bool CanSeePreFix(ref bool __result, Thing seer, Thing target, Func<IntVec3, bool> validator = null)
        {
            Pawn seerPawn = seer as Pawn;
            if (
                seerPawn != null && seer.Faction != null 
                && (RFOWSettings.aiSmart && seer.Faction != Faction.OfPlayer) 
                && seerPawn.RaceProps.Humanlike)
            {
                __result =  MapUtils.getMapComponentSeenFog(seer.Map).isShown(seer.Faction, target.Position);

                return __result;
            }
            else
                return true;
        }
        //For interaction bubbles
        [HarmonyPrefix]
        public static bool DrawBubblePrefix(Pawn pawn, bool isSelected, float scale)
        {
            if (!RFOWSettings.hideSpeakBubble)
                return true;
            if (pawn != null && !pawn.IsColonist && pawn.Map != null)
                return MapUtils.getMapComponentSeenFog(pawn.Map).isShown(Faction.OfPlayer, pawn.Position);
            return true;
        }
        //For suppressing letter
        [HarmonyPrefix]
        public static bool ReceiveLetterPrefix(ref Letter let)
        {

            if (let.def == LetterDefOf.NegativeEvent && RFOWSettings.hideEventNegative)
            {
                return false;
            }
            if (let.def == LetterDefOf.NeutralEvent && RFOWSettings.hideEventNeutral)
            {
                return false;
            }
            if (let.def == LetterDefOf.PositiveEvent && RFOWSettings.hideEventPositive)
            {
                return false;
            }
            if (let.def == LetterDefOf.ThreatBig && RFOWSettings.hideThreatBig)
            {
                return false;
            }
            if (let.def == LetterDefOf.ThreatSmall && RFOWSettings.hideThreatSmall)
            {
                return false;
            }

            return true;
        }
    }
}
