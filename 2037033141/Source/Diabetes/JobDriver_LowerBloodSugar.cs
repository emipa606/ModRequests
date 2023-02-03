using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Diabetes
{
	public class JobDriver_LowerBloodSugar : JobDriver
	{
		protected Thing Drug
		{
			get
			{
				return this.job.targetA.Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
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
			if (this.pawn.inventory != null && this.pawn.inventory.Contains(base.TargetThingA))
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, TargetIndex.A);
			}
			else
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
				yield return Toils_Ingest.PickupIngestible(TargetIndex.A, this.pawn);
			}
			yield return Toils_Ingest.ChewIngestible(this.pawn, 1f, TargetIndex.A);
			yield return Toils_DrugIngest.FinalizeIngest(this.pawn, TargetIndex.A);
			yield break;
		}
	}
}
