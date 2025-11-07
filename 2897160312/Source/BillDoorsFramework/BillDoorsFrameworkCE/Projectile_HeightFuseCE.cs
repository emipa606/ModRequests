using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BillDoorsFramework
{
    internal class Projectile_HeightFuseCE : ProjectileCE
    {
        private bool armed;

        float detonationHeight;

        public bool detonateOnAscend
        {
            get
            {
                if (def.GetModExtension<DefModExtension_HeightFuse>() != null)
                {
                    return def.GetModExtension<DefModExtension_HeightFuse>().detonateOnAscend;
                }
                return !def.projectile.flyOverhead;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref armed, "armed", false);
        }

        public override void Tick()
        {
            base.Tick();
            if (detonationHeight <= 0)
            {
                detonationHeight = (def.projectile as ProjectilePropertiesCE).aimHeightOffset;
            }
            if (detonateOnAscend && Height >= detonationHeight)
            {
                base.Impact(null);
            }
            if (!armed && this.Height > detonationHeight)
            {
                armed = true;
            }
            if (armed && this.Height <= detonationHeight)
            {
                base.Impact(null);
            }
        }
    }

    internal class DefModExtension_HeightFuse : DefModExtension
    {
        public bool detonateOnAscend = false;
    }
}
