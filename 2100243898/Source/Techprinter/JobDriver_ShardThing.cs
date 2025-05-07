using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
/*
namespace DTechprinting
{
    public class JobDriver_ShardThing : JobDriver
    {
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!this.pawn.Reserve(this.job.GetTarget(TargetIndex.C), this.job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return this.ReserveShardable();
			
			yield break;
		}


		public Toil ReserveShardable()
		{
			return new Toil
			{
				initAction = delegate ()
				{
					if (this.pawn.Faction == null)
					{
						return;
					}
					Thing thing = this.job.GetTarget(TargetIndex.A).Thing;
					if (this.pawn.carryTracker.CarriedThing == thing)
					{
						return;
					}

					int maxAmountToPickup = Base.IsStackAllowed(thing.def, ShardMaker.stackSize) ? ShardMaker.stackSize : Base.IsSingleAllowed(thing.def) ? 1 : 0;
					if (maxAmountToPickup == 0)
					{
						return;
					}
					if (!this.pawn.Reserve(thing, this.job, 10, maxAmountToPickup, null, true))
					{
						Log.Error(string.Concat(new object[]
						{
							"Pawn techprinting reservation for ",
							this.pawn,
							" on job ",
							this,
							" failed, because it could not register stack from ",
							thing,
							" - amount: ",
							maxAmountToPickup
						}), false);
						this.pawn.jobs.EndCurrentJob(JobCondition.Errored, true, true);
					}
					this.job.count = maxAmountToPickup;
				},
				defaultCompleteMode = ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}
	}
}
*/