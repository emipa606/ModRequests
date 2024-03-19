using HarmonyLib;
using RimWorld;
using Roos_Satyr_Xenotype;
using Verse;

namespace Roos_Satyr_Xenotype
{
    public class InteractionWorker_RBSF_Vulgarity : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (initiator.story.traits.HasTrait(RBSF_DefOf.RBSF_Uncouth))
            {
                return 0.7f;
            }
            return 0f;
        }
    }

    public class ThoughtWorker_UncouthVsUncouth : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            if (!p.RaceProps.Humanlike)
                return false;

            if (!p.story.traits.HasTrait(RBSF_DefOf.RBSF_Uncouth))
                return false;

            if (!other.story.traits.HasTrait(RBSF_DefOf.RBSF_Uncouth))
                return false;

            if (other.def != p.def)
                return false;

            return true;
        }
    }

    public class ThoughtWorker_IsBeautyForUncouth : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn PawnAsker)
        {

            if (!PawnAsker.story.traits.HasTrait(RBSF_DefOf.RBSF_Uncouth))
                return ThoughtState.Inactive;

            float TotalBeauty = 0f;
            if (PawnAsker.Map == null)
                return ThoughtState.ActiveAtStage(0);

            foreach (Pawn pawn in PawnAsker.Map.mapPawns.AllPawnsSpawned)
            {
                if (pawn == null || !pawn.RaceProps.Humanlike) 
                    continue;

                if (pawn.Faction != PawnAsker.Faction) 
                    continue;

                if (pawn == PawnAsker)
                    continue;

                float beautyAmount = pawn.GetStatValue(StatDefOf.PawnBeauty);

                if (beautyAmount > 2.0f)
                    beautyAmount = 2.0f;

                if (beautyAmount > 0.0f)
                    //Log.Message("found beautiful pawn " + pawn.Name + "with a beauty value of: " + pawn.GetStatValue(StatDefOf.PawnBeauty) + ".");
                    TotalBeauty += beautyAmount;
                
            }
            //Log.Message("Total beauty was " + TotalBeauty+ ".");
            //Log.Message("");

            //Log.Message("Checked beauty for uncouth pawn " + PawnAsker.Name + " total beauty was " + TotalBeauty);
            if (TotalBeauty < 1f)
                return ThoughtState.ActiveAtStage(0);

            else if (TotalBeauty < 3f)
                return ThoughtState.ActiveAtStage(1);

            else if (TotalBeauty < 5f)
                return ThoughtState.ActiveAtStage(2);

            else if (TotalBeauty < 7f)
                return ThoughtState.ActiveAtStage(3);

            else if (TotalBeauty < 9f)
                return ThoughtState.ActiveAtStage(4);

            return ThoughtState.ActiveAtStage(5);
        }
    }
}

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt))]
[HarmonyPatch(nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight))]
static class InteractionWorker_RomanceAttempt_RandomSelectionWeight_Postfix
{
    static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
    {
        if (initiator.story.traits.HasTrait(RBSF_DefOf.RBSF_Uncouth))
        {
            __result *= 3f;
            return;
        }
    }
}