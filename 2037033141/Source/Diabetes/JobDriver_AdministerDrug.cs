using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Diabetes
{
	public class JobDriver_AdministerDrug : JobDriver
	{
		protected Thing Drug
		{
			get
			{
				return this.job.targetA.Thing;
			}
		}

		protected Pawn Deliveree
		{
			get
			{
				return (Pawn) this.job.targetB.Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!this.pawn.Reserve(this.Deliveree, this.job, errorOnFailed: errorOnFailed))
			{
				return false;
			}
			if (this.pawn.inventory == null || this.pawn.inventory.Contains(base.TargetThingA))
			{
				int maxAmountToPickup = DrugUtility.GetMaxAmountToPickup(this.Drug, this.pawn, this.job.count);
				if (!this.pawn.Reserve(this.Drug, this.job, 1, maxAmountToPickup, null, errorOnFailed))
				{
					return false;
				}
				this.job.count = maxAmountToPickup;
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
			if (this.pawn.inventory != null && this.pawn.inventory.Contains(base.TargetThingA))
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, TargetIndex.A);
			}
			else
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
				yield return Toils_Ingest.PickupIngestible(TargetIndex.A, this.Deliveree);
			}
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
			yield return Toils_Ingest.ChewIngestible(this.Deliveree, 1.5f, TargetIndex.A, TargetIndex.None).FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
			yield return Toils_DrugIngest.FinalizeIngest(this.Deliveree, TargetIndex.A);
			yield break;
		}
	}
}
