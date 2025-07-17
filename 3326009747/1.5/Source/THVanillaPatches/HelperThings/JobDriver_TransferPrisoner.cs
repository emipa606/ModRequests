using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace THVanillaPatches.HelperThings
{
	public class JobDriver_TransferPrisoner: JobDriver
	{
      private const TargetIndex DestBedIndex = TargetIndex.A;
      private const TargetIndex PrisonerIndex = TargetIndex.B;

      private Pawn Prisoner => job.GetTarget(PrisonerIndex).Thing as Pawn;

      private Building_Bed DestBed => job.GetTarget(DestBedIndex).Thing as Building_Bed;

      public override bool TryMakePreToilReservations(bool errorOnFailed)
      {
        return pawn.Reserve((LocalTargetInfo) Prisoner, job, errorOnFailed: errorOnFailed) && pawn.Reserve((LocalTargetInfo) Prisoner, job, errorOnFailed: errorOnFailed) && pawn.Reserve((LocalTargetInfo) DestBed, job, errorOnFailed: errorOnFailed);
      }

      protected override IEnumerable<Toil> MakeNewToils()
      {
	    JobDriver_TransferPrisoner f = this;
        f.FailOnDestroyedOrNull(PrisonerIndex);
        f.FailOnDespawnedNullOrForbidden(DestBedIndex);
        yield return Toils_Goto.GotoThing(PrisonerIndex, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(PrisonerIndex);
        
        job.count = 1;
        
        yield return Toils_Haul.StartCarryThing(PrisonerIndex);

        yield return Toils_Haul.CarryHauledThingToCell(DestBedIndex);
        
        yield return Toils_Haul.DropCarriedThing();
        
        yield return Toils_General.Do(() =>
        {
	        Pawn prisoner = Prisoner;
        
	        prisoner.ownership.ClaimBedIfNonMedical(DestBed); 
        });
      }
	}
}