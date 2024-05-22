using HarmonyLib;
using RimWorld;
using Verse;

[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), nameof(InteractionWorker_MarriageProposal.RandomSelectionWeight))]
public static class MarriageProposalPatch
{
    [HarmonyPrefix]
    public static bool RandomSelectionWeightPrefix(ref float __result, Pawn initiator, Pawn recipient)
    {
        // Remove the gender-based weight adjustment for the initiator
        __result = 0.4f;

        return true;
    }
}
