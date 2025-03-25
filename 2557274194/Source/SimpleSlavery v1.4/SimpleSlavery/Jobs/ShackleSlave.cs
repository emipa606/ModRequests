using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace SimpleSlaveryCollars.Jobs
{
    public class WorkGiver_Warden_ShackleSlave : WorkGiver_Warden
    {
        //
        // Properties
        //
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        //
        // Methods
        //
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var slave = (Pawn)t;

            if (pawn == slave)
            {
                return null;
            }

            if (!slave.IsSlaveOfColony || !slave.health.hediffSet.HasHediff(SS_HediffDefOf.Enslaved) || !pawn.CanReserve(slave) || SlaveUtility.GetEnslavedHediff(slave).shackledGoal == SlaveUtility.GetEnslavedHediff(slave).shackled || slave.InAggroMentalState)
                return null;
            return JobMaker.MakeJob(SS_JobDefOf.ShackleSlave, slave);
        }
    }

    internal class JobDriver_ShackleSlave : JobDriver
    {
        const int shackleDuration = 300;

        private Pawn Victim
        {
            get
            {
                return (Pawn)job.GetTarget(TargetIndex.A).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed) //Z- () -> bool errorOnFailed
        {
            return pawn.Reserve(Victim, job, 1, -1, null);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !Victim.IsSlaveOfColony);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.WaitWith(TargetIndex.A, shackleDuration, true);
            yield return new Toil
            {
                initAction = delegate
                {
                    SlaveUtility.GetEnslavedHediff(Victim).shackled = SlaveUtility.GetEnslavedHediff(Victim).shackledGoal;
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
