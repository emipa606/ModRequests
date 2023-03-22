using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
    public class JobDriver_UseNeuralChair_TII : JobDriver
    {
        private float workLeft;
        private const float BaseWorkAmount = 500f; //350-->500

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn((Toil to) => !((Building_NeuralChair_TII)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseNeuralChairNow_TII);
            Toil doWork = new Toil();
            doWork.initAction = delegate ()
            {
                this.workLeft = BaseWorkAmount;
            };
            doWork.tickAction = delegate ()
            {
                Pawn actor2 = base.CurToil.actor;
                if (((Building_NeuralChair_TII)actor2.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseNeuralChairNow_TII)
                {
                    float num = doWork.actor.GetStatValue(StatDefOf.WorkSpeedGlobal, true) * 1.7f;
                    this.workLeft -= num;
                    if (this.workLeft <= 0f)
                    {
                        this.ReadyForNextToil();
                        FloatMenuManagerNeuralChair_TII.currentAction(actor2);
                        return;
                    }
                }
            };
            doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            doWork.WithProgressBar(TargetIndex.A, () => 1f - this.workLeft / BaseWorkAmount, false, -0.5f);
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            yield return doWork;
            yield break;
        }
    }
}