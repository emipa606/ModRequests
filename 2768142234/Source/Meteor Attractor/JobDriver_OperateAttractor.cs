using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MeteorAttractor
{
	// Token: 0x0200073B RID: 1851
	public class JobDriver_OperateAttractor : JobDriver
	{
		// Token: 0x060033CE RID: 13262 RVA: 0x00101242 File Offset: 0x000FF442
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
		}

		// Token: 0x060033CF RID: 13263 RVA: 0x0012A4CD File Offset: 0x001286CD
		protected override IEnumerable<Toil> MakeNewToils()
		{
			CompScanner scannerComp = this.job.targetA.Thing.TryGetComp<CompScanner>();
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			this.FailOn(() => !scannerComp.CanUseNow);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil work = new Toil();
			work.tickAction = delegate()
			{
				Pawn actor = work.actor;
				Building building = (Building)actor.CurJob.targetA.Thing;
				scannerComp.Used(actor);
				actor.skills.Learn(SkillDefOf.Intellectual, 0.09f, false);
				actor.GainComfortFromCellIfPossible(true);
			};
			work.AddFailCondition(() => !scannerComp.CanUseNow);
			work.defaultCompleteMode = ToilCompleteMode.Never;
			work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			work.activeSkill = (() => SkillDefOf.Intellectual);
			yield return work;
			yield break;
		}

		// Token: 0x060033D0 RID: 13264 RVA: 0x000161C7 File Offset: 0x000143C7
		public JobDriver_OperateAttractor()
		{
		}
	}
}
