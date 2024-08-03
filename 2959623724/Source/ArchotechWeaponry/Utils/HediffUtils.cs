using Verse;

namespace ArchotechWeaponry.Utils
{
    public class HediffUtils
    {
        public static void AddOrUpdateHediffWithSeverity(Pawn pawn, HediffDef hediff, BodyPartRecord part = null, float severity = 0f)
        {
            Pawn_HealthTracker health = pawn.health;
            HediffSet hediffSet = health.hediffSet;
            if (hediffSet.GetFirstHediffOfDef(hediff) == null)
            {
                health.AddHediff(hediff, part);
                hediffSet.GetFirstHediffOfDef(hediff).Severity = severity;
            }
            else
            {
                hediffSet.GetFirstHediffOfDef(hediff).Severity += severity;
            }

        }
    }
}