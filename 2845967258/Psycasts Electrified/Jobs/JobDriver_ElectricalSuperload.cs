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
    public class JobDriver_ElectricalSuperload : JobDriver
    {
        private const TargetIndex Bench = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(Bench), this.job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(Bench);

            yield return Toils_Goto.GotoThing(Bench, PathEndMode.InteractionCell);

            Toil superload = new Toil
            {
                initAction = () =>
                {
                    ApplyBurn();
                    DrainEnergy();
                }
            };

            superload.PlaySoundAtEnd(SoundDefOf.Crunch);

            yield return superload;
        }

        //make burns more severe
        private void ApplyBurn()
        {
            int numberOfBurns = Rand.Range(2,5);

            IEnumerable<BodyPartRecord> bodyParts = this.pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            Hediff burn = null;

            for (int i = 0; i < numberOfBurns; i++)
            {
                burn = HediffMaker.MakeHediff(HediffDefOf.Burn, this.pawn, bodyParts.RandomElementByWeight((BodyPartRecord x) => x.coverageAbsWithChildren));

                burn.Severity = Rand.Gaussian(6f, 2f);
                this.pawn.health.AddHediff(burn);
            }
        }

        private void DrainEnergy()
        {
            LocalTargetInfo electricalBench = this.job.GetTarget(Bench);

            CompPowerTrader power = electricalBench.Thing.TryGetComp<CompPowerTrader>();

            if (power != null)
            {
                float powerNeeded = Helper.GetElectricalSuperloadCost(this.pawn) / power.PowerNet.batteryComps.Count();

                foreach (CompPowerBattery battery in power.PowerNet.batteryComps)
                {
                    battery.DrawPower(powerNeeded);
                }
                this.pawn.ChangePsylinkLevel(1);
            }
        }
    }
}
