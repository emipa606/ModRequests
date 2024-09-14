using System;
using System.Collections.Generic;
using Analyst;
using RimWorld;
using Verse;
using Verse.AI;

public class JobDriver_MechOperateScanner : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return ReservationUtility.Reserve(base.pawn, base.job.targetA, base.job, 1, -1, (ReservationLayerDef)null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		CompScanner scannerComp = ThingCompUtility.TryGetComp<CompScanner>(((LocalTargetInfo)(ref base.job.targetA)).Thing);
		ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_MechOperateScanner>(this, (TargetIndex)1);
		ToilFailConditions.FailOnBurningImmobile<JobDriver_MechOperateScanner>(this, (TargetIndex)1);
		ToilFailConditions.FailOn<JobDriver_MechOperateScanner>(this, (Func<bool>)(() => !scannerComp.CanUseNow));
		yield return Toils_Goto.GotoThing((TargetIndex)1, (PathEndMode)4);
		Toil work = ToilMaker.MakeToil("MakeNewToils");
		work.tickAction = delegate
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			Pawn actor = work.actor;
			_ = (Building)((LocalTargetInfo)(ref actor.CurJob.targetA)).Thing;
			scannerComp.Used(actor);
		};
		ToilEffects.PlaySustainerOrSound(work, scannerComp.Props.soundWorking, 1f);
		work.AddFailCondition((Func<bool>)(() => !scannerComp.CanUseNow));
		work.defaultCompleteMode = (ToilCompleteMode)5;
		ToilFailConditions.FailOnCannotTouch<Toil>(work, (TargetIndex)1, (PathEndMode)4);
		yield return work;
	}
}
