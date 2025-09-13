using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using VREAndroids;

namespace VAGUE
{
    internal class Gene_CoagulatoryMechanites : Gene
    {
        public int ticksPerSec = 60;
        public float secsPerTick = 1 / 60;
        public IEnumerable<Hediff> bleeders;
        private List<Hediff> sortedBleeders;

        public override void Tick()
        {
            if (this.pawn.IsHashIntervalTick(10 * ticksPerSec) && this.Active && this.pawn.health.hediffSet.BleedRateTotal > 0)
            {
                bleeders = GetApplicableHediffs();

                if (bleeders.Count() > 0) {
                    sortedBleeders = bleeders.OrderByDescending(o => o.BleedRate).ToList();

                    sortedBleeders[0].Tended(1f, 1f);
                }
            }
        }

        private IEnumerable<Hediff> GetApplicableHediffs()
        {
            List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;

            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i].TendableNow() && hediffs[i].Bleeding && hediffs[i].ageTicks >= 15*ticksPerSec)
                {
                    yield return hediffs[i];
                }
            }
        }
    }
}
