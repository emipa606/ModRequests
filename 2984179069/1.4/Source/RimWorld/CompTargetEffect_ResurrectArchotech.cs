using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompTargetEffect_ResurrectArchotech : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			if (user.IsColonistPlayerControlled && user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
			{
				Job job = JobMaker.MakeJob(MAG_JobDefOf.BetterResurrect, target, parent);
				job.count = 1;
				user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		}
	}
}
