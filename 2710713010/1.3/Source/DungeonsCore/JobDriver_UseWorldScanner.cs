using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace DungeonsCore
{
    public class JobDriver_UseWorldScanner : JobDriver
	{
		public Thing WorldScanner => this.pawn.CurJob.GetTarget(TargetIndex.A).Thing;
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
		}
		public override IEnumerable<Toil> MakeNewToils()
		{
			var scannerComp = WorldScanner.TryGetComp<CompWorldScanner>();
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil work = new Toil();
			work.tickAction = delegate
			{
				Pawn actor = work.actor;
				_ = (Building)actor.CurJob.targetA.Thing;
				scannerComp.Used(actor);
				actor.skills.Learn(SkillDefOf.Intellectual, 0.035f);
				actor.GainComfortFromCellIfPossible(chairsOnly: true);
			};
			work.AddFailCondition(() => !WorkGiver_PerformPingingOnWorldScanner.CanUseScanner(pawn, WorldScanner));
			work.defaultCompleteMode = ToilCompleteMode.Never;
			work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			work.activeSkill = () => SkillDefOf.Intellectual;
			yield return work;
		}
	}
}
