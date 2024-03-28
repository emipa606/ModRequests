using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColonyNeedAmmunitionMod
{
    public class DepletionOfMine : Thing
    {
        private float depletion = 0.0f;
        private int lastRecoveryTick = -1;

        private const float RECOVERY_DEPLETION_PER_DAY = 0.0037f;

        public float Depletion {
            get {
                return depletion;
            }
        }

        public override void TickLong()
        {
            if (Find.TickManager.TicksGame - lastRecoveryTick > 2500 * 24) {
                recovery();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<float>(ref this.depletion, "depletion", 0.0f, false);
            Scribe_Values.LookValue<int>(ref this.lastRecoveryTick, "lastRecoveryTick", -1, false);
        }

        private void recovery() {
            lastRecoveryTick = Find.TickManager.TicksGame;
            depletion -= RECOVERY_DEPLETION_PER_DAY;
            if (depletion <= 0) this.Destroy();
        }

        public void addDepletion(float val) {
            depletion += val;
        }
    }
}
