using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace BillDoorsFramework
{
    public class JobDriver_BombDisarm : JobDriver
    {
        public CompProximityExplosive CompBomb
        {
            get
            {
                if (compBombInt == null) compBombInt = TargetA.Thing?.TryGetComp<CompProximityExplosive>();
                return compBombInt;
            }
        }

        private CompProximityExplosive compBombInt;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, this.job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {

            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            var disarming = Toils_General.Wait((int)(CompBomb.Props.disarmTime * 60), TargetIndex.None).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);

            yield return disarming;
            yield return DisarmBomb();
            yield break;
        }

        public Toil DisarmBomb()
        {
            return new Toil()
            {
                actor = pawn,
                initAction = delegate ()
                {
                    CompBomb.TryDisarming(pawn);
                }
            };
        }
    }

    public class WorkGiver_BombDisarm : WorkGiver_Scanner
    {

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return CompProximityExplosive.GetCachedMine(pawn.Map);
        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!forced) return false;
            CompProximityExplosive compExplosive = t.TryGetComp<CompProximityExplosive>();
            if (compExplosive == null) return false;
            if (!compExplosive.Props.disarmEnabled) return false;
            //if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Research)) return false;
            //Log.Message("Success rate:" + compExplosive.GetDisarmSuccessRate(pawn).ToString());
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(JobDefOf.BDsBombDisarm, t);
        }
    }
}
