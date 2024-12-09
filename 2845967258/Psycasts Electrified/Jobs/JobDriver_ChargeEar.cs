using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Psycasts_Electrified
{
    class JobDriver_ChargeEar : JobDriver
    {
        private const TargetIndex Bench = TargetIndex.A;
        private IEnumerable<CompChargingEar> ears;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(Bench), this.job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(Bench);


            yield return Toils_Goto.GotoThing(Bench, PathEndMode.InteractionCell);

            Toil charge = new Toil
            {
                initAction = () =>
                {
                    ears = Helper.GetChargingEars(this.pawn);
                },
                tickAction = () =>
                {
                    bool doneCharging = true;

                    foreach (CompChargingEar ear in ears)
                    {
                        if (!ear.isCharged())
                        {
                            ear.charge += Settings.earChargeRate;
                            doneCharging = false;
                        }
                    }

                    if (doneCharging)
                    {
                        this.ReadyForNextToil();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Never,
            };

            charge.WithEffect(this.job.GetTarget(Bench).Thing.def.repairEffect, TargetIndex.A, null);

            yield return charge;
        }
    }
}
