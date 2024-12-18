using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ResurrectArchotech : JobDriver
	{
		private const TargetIndex CorpseInd = TargetIndex.A;

		private const TargetIndex ItemInd = TargetIndex.B;

		private const int DurationTicks = 600;

		private Corpse Corpse => (Corpse)job.GetTarget(TargetIndex.A).Thing;

		private Thing Item => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Corpse, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);
		}

        public override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			Toil toil = Toils_General.Wait(600);
			toil.WithProgressBarToilDelay(TargetIndex.A);
			toil.FailOnDespawnedOrNull(TargetIndex.A);
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return toil;
			yield return Toils_General.Do(ResurrectArchotech);
		}

		private void ResurrectArchotech()
		{
			Pawn innerPawn = Corpse.InnerPawn;
			ResurrectionUtilityArchotech.ResurrectWithSideEffects(innerPawn);
			Messages.Message("MessagePawnResurrected".Translate(innerPawn), innerPawn, MessageTypeDefOf.PositiveEvent);
			Item.SplitOff(1).Destroy();
		}
	}
}
