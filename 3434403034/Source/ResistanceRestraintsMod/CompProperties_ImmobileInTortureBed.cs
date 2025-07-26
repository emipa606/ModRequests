using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace ResistanceRestraintsMod
{
    public class CompProperties_TortureBed : CompProperties
    {
        public CompProperties_TortureBed()
        {
            this.compClass = typeof(CompTortureBed);
        }
    }

    public class CompTortureBed : ThingComp
    {
        private Building_Bed bed => this.parent as Building_Bed;
        private CompRefuelable compRefuelable => this.parent.GetComp<CompRefuelable>();
        private CompPowerTrader compPowerTrader => this.parent.GetComp<CompPowerTrader>();
        private HashSet<Pawn> affectedPawns = new HashSet<Pawn>();

        public override void CompTick()
        {
            base.CompTick();
            if (bed == null) return;

            // Check if the building has fuel (if applicable)
            bool hasFuel = compRefuelable?.HasFuel ?? true; // Assume true if no refuelable component

            // Check if the building has power (if applicable)
            bool hasPower = compPowerTrader?.PowerOn ?? true; // Assume true if no power component

            // The bed must have both fuel and power to apply the hediff
            bool canApplyHediff = hasFuel && hasPower;

            List<Pawn> currentOccupants = bed.CurOccupants?.ToList() ?? new List<Pawn>();

            foreach (Pawn pawn in currentOccupants)
            {
                if (pawn == null || pawn.Dead || pawn.Downed) continue;

                Hediff immobilityHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf_SilkCircuit.SilkCircuit_Immobile);

                if (canApplyHediff)
                {
                    // Add the hediff if not already present
                    if (immobilityHediff == null)
                    {
                        pawn.health.AddHediff(HediffDefOf_SilkCircuit.SilkCircuit_Immobile);
                        affectedPawns.Add(pawn);
                    }
                }
                else
                {
                    // Remove the hediff if the bed is out of fuel or power
                    if (immobilityHediff != null)
                    {
                        pawn.health.RemoveHediff(immobilityHediff);
                        affectedPawns.Remove(pawn);
                    }
                }
            }

            // Remove the hediff from any previously affected pawns who are no longer in bed or if the bed is out of fuel/power
            foreach (Pawn pawn in affectedPawns.ToList()) // Copy to avoid modifying collection during iteration
            {
                if (!currentOccupants.Contains(pawn) || !canApplyHediff)
                {
                    Hediff immobilityHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf_SilkCircuit.SilkCircuit_Immobile);
                    if (immobilityHediff != null)
                    {
                        pawn.health.RemoveHediff(immobilityHediff);
                    }
                    affectedPawns.Remove(pawn);
                }
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            // Ensure any pawn still tracked gets the hediff removed
            foreach (Pawn pawn in affectedPawns)
            {
                Hediff immobilityHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf_SilkCircuit.SilkCircuit_Immobile);
                if (immobilityHediff != null)
                {
                    pawn.health.RemoveHediff(immobilityHediff);
                }
            }

            affectedPawns.Clear();
        }
    }

    [DefOf]
    public static class HediffDefOf_SilkCircuit
    {
        public static HediffDef SilkCircuit_Immobile;

        static HediffDefOf_SilkCircuit()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf_SilkCircuit));
        }
    }
}
