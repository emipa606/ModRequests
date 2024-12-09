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
    public class CompElectricalOverload : ThingComp
    {
        public CompProperties_ElectricalOverload properties => (CompProperties_ElectricalOverload)this.props;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            foreach (FloatMenuOption option in base.CompFloatMenuOptions(pawn)) { yield return option; }

            if (!Helper.HasChargingEar(pawn)) { yield break; }

            Building building = this.parent as Building;
            CompPowerTrader power = building?.PowerComp as CompPowerTrader;
            if (power == null) { yield break; }

            //Check for power requirements for superload
            FloatMenuOption cannotSuperload = ElectricalSuperload(pawn, power);

            if (cannotSuperload != null) { yield return cannotSuperload; }

            /*
             * For if a pawn has no psylink yet
             */
            if (!pawn.HasPsylink && cannotSuperload == null)
            {
                Action superload = delegate ()
                {
                    Job job = JobMaker.MakeJob(ModDefOf.ElectricalSuperload, this.parent);
                    pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
                };
                yield return new FloatMenuOption("PEM_Electrically_Superload".Translate(Helper.GetElectricalOverloadCost(pawn)), superload, MenuOptionPriority.Default);
                yield break;
            }


            /*
             * For leveling up the pawns psylink
             */
            if (pawn.GetPsylinkLevel() < 6 && cannotSuperload == null)
            {
                Action superload = delegate ()
                {
                    Job job = JobMaker.MakeJob(ModDefOf.ElectricalSuperload, this.parent);
                    pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
                };
                yield return new FloatMenuOption("PEM_Electrically_Superload".Translate(Helper.GetElectricalSuperloadCost(pawn)), superload, MenuOptionPriority.Default);
            }


            /*
             * Refill a pawns psyfocus in exchange for power
             */
            FloatMenuOption cannotOverload = ElectricalOverload(pawn, power);

            if (cannotOverload != null)
            {
                yield return cannotOverload;
                yield break;
            }

            Action overload = delegate ()
            {
                Job job = JobMaker.MakeJob(ModDefOf.ElectricalOverload, this.parent);
                pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
            };

            yield return new FloatMenuOption("PEM_Electrically_Overload".Translate(Helper.GetElectricalOverloadCost(pawn)), overload, MenuOptionPriority.Default);
        }

        public FloatMenuOption ElectricalOverload(Pawn pawn, CompPowerTrader power)
        {
            if (!pawn.psychicEntropy.NeedsPsyfocus)
            {
                return new FloatMenuOption("PEM_Psyfocus_Disallowed".Translate(), null, MenuOptionPriority.Default);
            }

            if (pawn.psychicEntropy.CurrentPsyfocus == 1f)
            {
                return new FloatMenuOption("PEM_Psyfocus_Full".Translate(), null, MenuOptionPriority.Default); ;
            }

            if (!power.PowerOn)
            {
                return new FloatMenuOption("PEM_No_Power".Translate(), null, MenuOptionPriority.Default);
            }

            if (power.PowerNet.CurrentStoredEnergy() < Helper.GetElectricalOverloadCost(pawn))
            {
                return new FloatMenuOption("PEM_Need_Energy".Translate(Helper.GetElectricalOverloadCost(pawn) - power.PowerNet.CurrentStoredEnergy(), "Overload"), null, MenuOptionPriority.Default);
            }
            return null;
        }

        public static FloatMenuOption ElectricalSuperload(Pawn pawn, CompPowerTrader power)
        {
            if (!power.PowerOn)
            {
                return new FloatMenuOption("PEM_No_Power".Translate(), null, MenuOptionPriority.Default);
            }

            if (power.PowerNet.CurrentStoredEnergy() < Helper.GetElectricalSuperloadCost(pawn))
            {
                return new FloatMenuOption("PEM_Need_Energy".Translate(Helper.GetElectricalSuperloadCost(pawn) - power.PowerNet.CurrentStoredEnergy(), "Superload"), null, MenuOptionPriority.Default);
            }
            return null;
        }
    }
}
