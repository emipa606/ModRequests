using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DTechprinting
{
    class JobDriver_ApplyTechshards : JobDriver_ApplyTechprint
    {

		protected CompTechprint TechshardComp
		{
			get
			{
				return this.Techprint.TryGetComp<CompTechshard>();
			}
		}

		protected Thing ShardsUsed
		{
			get
			{
				return this.job.targetB.Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (!this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			int num = this.pawn.Map.reservationManager.CanReserveStack(this.pawn, this.ShardsUsed, 10, null, false);
			if (num <= 0 || !this.pawn.Reserve(this.ShardsUsed, this.job, 1, -1, null, errorOnFailed))
				return false;
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
        {
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			int num = this.pawn.Map.reservationManager.CanReserveStack(this.pawn, this.ShardsUsed, 1, null, false);
			Toil toil = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return toil;
			Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return getToHaulTarget;
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDestroyedOrNull(TargetIndex.B);
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, false, false);
			yield return Toils_General.Wait(600, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = delegate ()
				{
					int numShards = this.pawn.CurJob.targetB.Thing.stackCount;
					ShardApplier.ApplyTechshards(this.TechshardComp.Props.project, this.pawn, numShards);
					this.Techprint.Destroy(DestroyMode.Vanish);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield return Toils_Reserve.Release(TargetIndex.A);
			yield break;
		}
	}
}
