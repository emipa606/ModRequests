using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SimpleSlaveryCollars
{
    public class CompTargetable_ColonyPawn : CompTargetable
    {
        protected override bool PlayerChoosesTarget => true;

        protected override TargetingParameters GetTargetingParameters() => new TargetingParameters()
        {
            canTargetPawns = true,
            onlyTargetColonistsOrPrisonersOrSlaves = true,
            canTargetBuildings = false,
            validator = (Predicate<TargetInfo>)(x => this.BaseTargetValidator(x.Thing))
        };
        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
        }
    }
    public class CompTargetEffect_SetSlaveCollar : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            if (!user.IsColonistPlayerControlled || !user.CanReserveAndReach((LocalTargetInfo)target, PathEndMode.Touch, Danger.Deadly))
                return;
            Job job = JobMaker.MakeJob(SS_JobDefOf.SetSlaveCollar, (LocalTargetInfo)target, (LocalTargetInfo)(Thing)this.parent);
            job.count = 1;
            user.jobs.TryTakeOrderedJob(job);
        }
    }
}
