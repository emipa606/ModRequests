using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimWorld
{
    public class Building_MechGestatorStable_GAOTCE_Mech : Building_MechGestator
    {
        private int gestationSpeedMultiplier = 16;

        public override void Tick()
        {
            for (int i = 0; i < gestationSpeedMultiplier - 1; i++)
                if (activeBill != null && PoweredOn && BoundPawnStateAllowsForming)
                    activeBill.BillTick();

            base.Tick();
        }
    }
}
