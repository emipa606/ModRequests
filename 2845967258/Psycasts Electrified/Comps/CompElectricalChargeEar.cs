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
    public class CompElectricalChargeEar : ThingComp
    {
        public CompProperties_ElectricalChargeEar properties => (CompProperties_ElectricalChargeEar)this.props;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            foreach (FloatMenuOption option in base.CompFloatMenuOptions(pawn))
            {
                yield return option;
            }

            if (!pawn.HasPsylink)
            {
                yield break;
            }

            if (!Helper.HasChargingEar(pawn))
            {
                yield break;
            }

            Building building = this.parent as Building;
            CompPowerTrader power = building?.PowerComp as CompPowerTrader;

            FloatMenuOption chargeEarOption = ChargeEarOption(pawn, power);

            if (chargeEarOption != null)
            {
                yield return chargeEarOption;
                yield break;
            }

            Action charge = delegate ()
            {
                Job job = JobMaker.MakeJob(ModDefOf.ChargeEar, this.parent);
                pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
            };

            yield return new FloatMenuOption("PEM_Charge_Ears".Translate(), charge, MenuOptionPriority.Default);
        }

        public FloatMenuOption ChargeEarOption(Pawn pawn, CompPowerTrader power)
        {
            if (!power.PowerOn)
            {
                return new FloatMenuOption("PEM_No_Power".Translate(), null, MenuOptionPriority.Default);
            }

            IEnumerable<HediffComp> ears = Helper.GetChargingEars(pawn);

            if (ears.EnumerableNullOrEmpty())
            {
                return null;
            }

            bool bothEarsFull = true;

            foreach (HediffComp comp in ears)
            {
                CompChargingEar ear = comp as CompChargingEar;
                if (ear.charge < Settings.earMaxCharge)
                {
                    bothEarsFull = false;
                }
            }

            if (bothEarsFull)
            {
                return new FloatMenuOption("PEM_Ears_Charged".Translate(), null, MenuOptionPriority.Default);
            }

            return null;
        }
    }
}
