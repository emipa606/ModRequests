using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace ResistanceRestraintsMod
{
    public class CompProperties_HediffOnExit : CompProperties
    {
        public HediffDef hediffToApplyOnExit; // The hediff to apply
        public List<HediffDef> hediffsToCheck = new List<HediffDef>(); // List of hediffs to check
        public float severity = 0f; // Severity of the hediff
        public bool onlyApplyToHumans = true; // Optional restriction to humans only

        public CompProperties_HediffOnExit()
        {
            this.compClass = typeof(CompHediffOnExit);
        }
    }

    public class CompHediffOnExit : ThingComp
    {
        private Building_Bed bed => this.parent as Building_Bed;
        private HashSet<Pawn> trackedPawns = new HashSet<Pawn>();

        public CompProperties_HediffOnExit Props => (CompProperties_HediffOnExit)props;

        public override void CompTick()
        {
            base.CompTick();
            if (bed == null) return;

            List<Pawn> currentOccupants = bed.CurOccupants?.ToList() ?? new List<Pawn>();

            // Add new occupants to tracking
            foreach (Pawn pawn in currentOccupants)
            {
                if (pawn == null || pawn.Dead) continue;
                trackedPawns.Add(pawn);
            }

            // Handle pawns who left
            foreach (Pawn pawn in trackedPawns.ToList()) // Copy to avoid modification issues
            {
                if (!currentOccupants.Contains(pawn))
                {
                    OnPawnExit(pawn);
                    trackedPawns.Remove(pawn);
                }
            }
        }

        private void OnPawnExit(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.guest.Recruitable) return;

            // Check if pawn should be excluded based on race
            if (Props.onlyApplyToHumans && pawn.RaceProps?.Humanlike == false) return;

            // Check if the pawn has any of the defined hediffs
            bool hasExcludedHediff = Props.hediffsToCheck.Any(h => pawn.health.hediffSet.HasHediff(h));

            if (!hasExcludedHediff && Props.hediffToApplyOnExit != null)
            {
                // Apply the hediff if they do NOT have any of the exclusionary hediffs
                Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffToApplyOnExit);
                if (existingHediff == null)
                {
                    Hediff newHediff = HediffMaker.MakeHediff(Props.hediffToApplyOnExit, pawn);
                    newHediff.Severity = Props.severity;
                    pawn.health.AddHediff(newHediff);
                }
            }
        }
    }
}
