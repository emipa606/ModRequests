using RimWorld;
using SearchAndDestroy.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace SearchAndDestroy
{
    class JobGiver_AIFightEnemiesShortExp : JobGiver_AIFightEnemies
    {
		protected override Job TryGiveJob(Pawn pawn)
		{
			var job = base.TryGiveJob(pawn);
			if(job != null)
            {
				job.expiryInterval = 30;
            }
			return job;
		}
        protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            var targetPawn = target as Pawn;
            if(targetPawn == null)
            {
                return false;
            }
            if(targetPawn.NonHumanlikeOrWildMan() && !targetPawn.IsAttacking())
            {
                return false;
            }
            return base.ExtraTargetValidator(pawn, target);
        }
    }
}
