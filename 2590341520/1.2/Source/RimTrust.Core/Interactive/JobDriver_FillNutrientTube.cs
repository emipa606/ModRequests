using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
	public class JobDriver_FillNutrientTube : JobDriver
	{
		protected Building_NutrientTube Tube
		{
			get
			{
				return (Building_NutrientTube)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected Thing plantMatter
		{
			get
			{
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.Tube, this.job, 1, -1, null, errorOnFailed) && this.pawn.Reserve(this.plantMatter, this.job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			base.AddEndCondition(delegate
			{
				if (this.Tube.SpaceLeftForNutrient > 0)
				{
					return JobCondition.Ongoing;
				}
				return JobCondition.Succeeded;
			});
			yield return Toils_General.DoAtomic(delegate
			{
				this.job.count = this.Tube.SpaceLeftForNutrient;
			});
			Toil reservePlantMatter = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return reservePlantMatter;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reservePlantMatter, TargetIndex.B, TargetIndex.None, true, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(200, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = delegate ()
				{
					this.Tube.AddNutrition(this.plantMatter);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield break;
		}

		private const TargetIndex TubeInd = TargetIndex.A;

		private const TargetIndex plantMatterInd = TargetIndex.B;

		private const int Duration = 200;
	}
}

