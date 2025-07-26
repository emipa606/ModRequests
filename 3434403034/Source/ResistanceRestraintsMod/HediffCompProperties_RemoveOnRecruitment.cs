using System;
using Verse;
using RimWorld;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_RemoveOnRecruitment : HediffCompProperties
    {
        public HediffCompProperties_RemoveOnRecruitment()
        {
            this.compClass = typeof(HediffComp_RemoveOnRecruitment);
        }
    }

    public class HediffComp_RemoveOnRecruitment : HediffComp
    {
        public HediffCompProperties_RemoveOnRecruitment Props => (HediffCompProperties_RemoveOnRecruitment)this.props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            // Ensure the pawn exists and is a valid character
            if (Pawn == null || Pawn.Faction == null || Pawn.guest == null)
                return;

            // Check if the pawn was a prisoner but is now recruited
            if (!Pawn.IsPrisoner && Pawn.Faction == Faction.OfPlayer)
            {
                // Remove this hediff
                Pawn.health.RemoveHediff(parent);
            }
        }
    }
}
