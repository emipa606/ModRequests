using Verse;
using RimWorld;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_RemoveIfNotInBed : HediffCompProperties
    {
        public HediffCompProperties_RemoveIfNotInBed()
        {
            this.compClass = typeof(HediffComp_RemoveIfNotInBed);
        }
    }

    public class HediffComp_RemoveIfNotInBed : HediffComp
    {
        public HediffCompProperties_RemoveIfNotInBed Props => (HediffCompProperties_RemoveIfNotInBed)this.props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            // Only check once every 30 ticks (half a second)
            if (Pawn.IsHashIntervalTick(30))
            {
                // If the pawn is not in a bed, remove the hediff
                if (Pawn.CurrentBed() == null)
                {
                    Pawn.health.RemoveHediff(parent);
                }
            }
        }
    }
}
