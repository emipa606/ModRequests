using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Psycasts_Electrified
{
    public class CompChargingEar : HediffComp
    {
        public float charge = 0f;

        /*
         * Charge the ear if pawn is meditating electrically, else just discharge the ear. Happens every 30 ticks/half a second at normal game speed
         */
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (this.Pawn.IsHashIntervalTick(30))
            {
                if (isMeditatingElectrically())
                {
                    float chargePerInterval = Settings.earChargeRate;
                    charge = Mathf.Clamp(charge + Settings.earChargeRate, 0f, Settings.earMaxCharge);
                    return;
                } else

                charge = Mathf.Clamp(charge - (Settings.earPowerUsage / 2), 0f, Settings.earMaxCharge);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<float>(ref charge, "currentCharge", 0f, false);
        }

        /*
         * Add % and maybe add info on the label
         */
        public override string CompTipStringExtra
        {
            get
            {
                return "PEM_Ear_Report".Translate() + charge.ToString();
            }
        }

        /*
         * Check what meditation type the pawn is doing
         */
        public bool isMeditatingElectrically()
        {
            if (!Pawn.psychicEntropy.IsCurrentlyMeditating)
            {
                return false;
            }

            JobDriver driver = Pawn.CurJob.GetCachedDriver(Pawn);
            if (driver != null && driver is JobDriver_Meditate)
            {
                CompMeditationFocus focus = ((JobDriver_Meditate)driver).Focus.Thing.TryGetComp<CompMeditationFocus>();

                if(focus != null)
                {
                    return focus.Props.focusTypes.Contains(ModDefOf.Electrical);
                }
            }

            return false;
        }

        public bool isCharged()
        {
            return charge >= Settings.earMaxCharge;
        }
    }
}
