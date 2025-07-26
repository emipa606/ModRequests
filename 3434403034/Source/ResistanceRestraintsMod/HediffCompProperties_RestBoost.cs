using RimWorld;
using Verse;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_RestBoost : HediffCompProperties
    {
        public float restGainPerTick = 0.01f; // Adjust as needed

        public HediffCompProperties_RestBoost()
        {
            this.compClass = typeof(HediffComp_RestBoost);
        }
    }

    public class HediffComp_RestBoost : HediffComp
    {
        public HediffCompProperties_RestBoost Props => (HediffCompProperties_RestBoost)this.props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn?.needs?.rest != null)
            {
                Pawn.needs.rest.CurLevel += Props.restGainPerTick / 60f; // Apply per tick
                if (Pawn.needs.rest.CurLevel > 1f)
                    Pawn.needs.rest.CurLevel = 1f; // Cap at max
            }
        }
    }
}
