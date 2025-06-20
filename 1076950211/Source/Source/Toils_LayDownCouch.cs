using RimWorld;
using Verse;
using Verse.AI;

namespace Therapy
{
    public static class Toils_LayDownCouch
    {
        private const int GetUpOrStartJobWhileOnCouchCheckInterval = 211;
        private const int MinLayDownTicks = 8000;

        public static Toil LayDown(TargetIndex couchIndex, bool lookForOtherJobs)
        {
            Toil layDown = new Toil();
            layDown.initAction = delegate 
            {
                Pawn actor = layDown.actor;
                actor.pather.StopDead();
                JobDriver curDriver = actor.jobs.curDriver;

                {
                    Building_Couch t = (Building_Couch) actor.CurJob.GetTarget(couchIndex).Thing;
                    if (!t.OccupiedRect().Contains(actor.Position))
                    {
                        Log.Error("Can't start LayDown toil because pawn is not on the couch. pawn=" + actor);
                        actor.jobs.EndCurrentJob(JobCondition.Errored);
                        return;
                    }
                    actor.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
                }
                curDriver.asleep = false;
                //actor.mindState.awokeVoluntarily = false;
            };
            layDown.tickAction = delegate {
                Pawn actor = layDown.actor;
                actor.GainComfortFromCellIfPossible();

                var driver = actor.jobs.curDriver;
                if (driver is JobDriver_ReceiveTherapy receive && receive.CurrentTreatedMemory != null) return; // being treated

                if (Find.TickManager.TicksGame - actor.CurJob.startTick > MinLayDownTicks)
                    if (lookForOtherJobs && actor.IsHashIntervalTick(GetUpOrStartJobWhileOnCouchCheckInterval))
                    {
                        //actor.mindState.awokeVoluntarily = true;
                        actor.jobs.CheckForJobOverride();
                        //actor.mindState.awokeVoluntarily = false;
                    }
            };
            layDown.defaultCompleteMode = ToilCompleteMode.Never;
            
            layDown.FailOnCouchNoLongerUsable(couchIndex);
            
            layDown.AddFinishAction(delegate 
            {
                Pawn actor = layDown.actor;
                JobDriver curDriver = actor.jobs.curDriver;
                //if (!actor.mindState.awokeVoluntarily && curDriver.asleep && !actor.Dead && actor.needs.rest != null
                //    && actor.needs.rest.CurLevel < RestUtility.FallAsleepMaxLevel(actor) && actor.needs.mood != null)
                //{
                //    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleepDisturbed, null);
                //}
                curDriver.asleep = false;
            });
            return layDown;
        }
    }
}