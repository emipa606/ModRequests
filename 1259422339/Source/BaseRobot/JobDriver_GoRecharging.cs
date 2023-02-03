using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace BaseRobot
{
    // Token: 0x02000006 RID: 6
    public class JobDriver_GoRecharging : JobDriver
	{
		// Token: 0x06000024 RID: 36 RVA: 0x000036F0 File Offset: 0x000018F0
		private Toil DespawnIntoContainer()
		{
			var toil = new Toil();
			toil.initAction = delegate()
			{
                if (toil.actor is ArcBaseRobot arcBaseRobot && arcBaseRobot.rechargeStation != null)
                {
                    arcBaseRobot.rechargeStation.AddRobotToContainer(arcBaseRobot);
                }
            };
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00003738 File Offset: 0x00001938
		public Toil GotoThing(IntVec3 cell, Map map, PathEndMode PathEndMode)
		{
			var toil = new Toil();
			var target = new LocalTargetInfo(cell);
			toil.initAction = delegate()
			{
				Pawn actor = toil.actor;
				actor.pather.StartPath(target, PathEndMode);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return toil;
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00003794 File Offset: 0x00001994
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return GotoThing(TargetA.Cell, Map, PathEndMode.OnCell).FailOnDespawnedOrNull(TargetIndex.A);
			yield return DespawnIntoContainer();
			yield break;
		}

		// Token: 0x06000027 RID: 39 RVA: 0x000037B7 File Offset: 0x000019B7
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA.Cell, job, 1, -1, null, true);
		}
	}
}
