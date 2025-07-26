using Verse;
using RimWorld;
using UnityEngine;
using System.Linq; // Required for .Any() extension method

namespace ResistanceRestraintsMod
{
    public class CompReduceResistanceInBed : ThingComp
    {
        public CompProperties_ReduceResistanceInBed Props => (CompProperties_ReduceResistanceInBed)props;

        public override void CompTick()
        {
            base.CompTick();

            if (parent is Building_Bed bed && bed.CurOccupants.Any()) // Correct way to check if pawns are in the bed
            {
                foreach (Pawn pawn in bed.CurOccupants) // Correct way to get pawns in the bed
                {
                    if (pawn?.guest != null && pawn.IsPrisonerOfColony)
                    {
                        float reductionAmount = Props.baseReduction;

                        // Additional reduction if pawn is immobilized
                        if (pawn.health.hediffSet.HasHediff(HediffDef.Named("SilkCircuit_Immobile")))
                        {
                            reductionAmount *= Props.immobileMultiplier;
                        }

                        // Apply resistance reduction (ensuring it never goes below 0)
                        pawn.guest.resistance = Mathf.Max(pawn.guest.resistance - reductionAmount, 0f);
                    }
                }
            }
        }
    }

    public class CompProperties_ReduceResistanceInBed : CompProperties
    {
        public float baseReduction = 0.05f; // Default resistance reduction per tick
        public float immobileMultiplier = 2f; // Multiplier if the pawn is immobilized

        public CompProperties_ReduceResistanceInBed()
        {
            compClass = typeof(CompReduceResistanceInBed);
        }
    }
}
