using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimTrust.Trade
{
    [HarmonyPatch(typeof(RimWorld.PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike", new Type[] { typeof(Pawn), typeof(DamageInfo?), typeof(PawnDiedOrDownedThoughtsKind), typeof(List<IndividualThoughtToAdd>), typeof(List<ThoughtToAddToAll>) })]
    sealed class PawnDiedOrDownedThoughtsUtility
    {
        public static bool Prefix(PawnDiedOrDownedThoughtsUtility __instance, ref Pawn victim, ref DamageInfo? dinfo, ref PawnDiedOrDownedThoughtsKind thoughtsKind, ref List<IndividualThoughtToAdd> outIndividualThoughts, ref List<ThoughtToAddToAll> outAllColonistsThoughts)
        {
            var NeuralImplantOnVictim = victim.health?.hediffSet?.GetFirstHediffOfDef(HediffDef_Neural.NeuralImplant);

            if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
            {
                foreach (Pawn pawn2 in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
                {
                    if (pawn2 != victim && pawn2.needs != null && pawn2.needs.mood != null && (pawn2.MentalStateDef != MentalStateDefOf.SocialFighting || ((MentalState_SocialFighting)pawn2.MentalState).otherPawn != victim))
                    {
                        if (victim.Faction == Faction.OfPlayer && victim.Faction == pawn2.Faction && victim.HostFaction != pawn2.Faction && !victim.IsQuestLodger() && NeuralImplantOnVictim != null)
                        {
                            Log.Message("before Prefix");
                            outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.Ascension, pawn2, victim, 1f, 1f));
                            Log.Message("Pawn :" + pawn2.Name);
                            Log.Message("Victim :" + victim.Name);
                            Log.Message("after Pretfix");
                        }
                    }
                }
                return false;
            }
            return true;
        }
    }
}