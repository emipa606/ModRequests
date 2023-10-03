using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using RimWorld;

namespace ExecutionModByYuno
{
    public class JobDriver_PrisonerWait: JobDriver
    {

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedOrNull(TargetIndex.A);
            Toil work = new Toil();
            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);

            work.tickAction = delegate ()
            {
                Job job = new Job(RimWorld.JobDefOf.Wait, TargetA);
                pawn.jobs.StartJob(job);
            };
            yield return work;
            yield break;
        }

    }
}
