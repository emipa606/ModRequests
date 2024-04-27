using RimWorld;
using Verse;
using Verse.AI;

namespace ArchoMentalStateReset
{
	public class CompTargetEffect_ArchotechMentalReset : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (pawn.Faction != Faction.OfMechanoids && !pawn.Dead && pawn.InMentalState)
			{
				pawn.MentalState.RecoverFromState();
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
	}
}
