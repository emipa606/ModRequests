using System;
using HarmonyLib;
using Verse;

namespace Necro
{
    
    [HarmonyPatch(typeof(HediffGiver_Birthday))]
    [HarmonyPatch("TryApplyAndSimulateSeverityChange")]
    public class HediffGiver_Birthday_NecronoidPatch
    {
        
        private static bool Prefix(HediffGiver_Birthday __instance, Pawn pawn, float gotAtAge, bool tryNotToKillPawn)
        {
            return !(__instance is HediffGiver_Necronoid) || !pawn.Faction.IsPlayer;
        }

        
        public HediffGiver_Birthday_NecronoidPatch()
        {
        }
    }
}
