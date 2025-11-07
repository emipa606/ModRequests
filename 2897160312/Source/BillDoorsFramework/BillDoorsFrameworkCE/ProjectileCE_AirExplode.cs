using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class ProjectileCE_AirExplode : ProjectileCE_Explosive
    {

        public override Vector3 DrawPos
        {
            get
            {
                Vector3 pos = base.DrawPos;
                pos.z += AirExplodeHeight * (startingTicksToImpact - ticksToImpact) / startingTicksToImpact;
                return pos;
            }
        }

        private float AirExplodeHeight
        {
            get
            {
                if (airExplodeHeight < 0)
                {
                    if (this.def.GetModExtension<ModExtension_AirExplode>() is ModExtension_AirExplode extension)
                    {
                        airExplodeHeight = extension.airExplodeHeight;
                    }
                    else
                    {
                        airExplodeHeight = 7;
                    }
                }
                return airExplodeHeight;
            }
        }

        private float airExplodeHeight = -1;

    }

    public class ProjectileCE_FromAirExplode : BulletCE
    {
        public override Vector3 DrawPos
        {
            get
            {
                Vector3 pos = base.DrawPos;
                pos.z += AirExplodeHeight * ticksToImpact / startingTicksToImpact;
                return pos;
            }
        }

        private float AirExplodeHeight
        {
            get
            {
                if (airExplodeHeight < 0)
                {
                    if (this.def.GetModExtension<ModExtension_AirExplode>() is ModExtension_AirExplode extension)
                    {
                        airExplodeHeight = extension.airExplodeHeight;
                    }
                    else
                    {
                        airExplodeHeight = 7;
                    }
                }
                return airExplodeHeight;
            }
        }

        private float airExplodeHeight = -1;
    }

    public class ModExtension_AirExplode : DefModExtension
    {
        public float airExplodeHeight = 7f;
    }
}
