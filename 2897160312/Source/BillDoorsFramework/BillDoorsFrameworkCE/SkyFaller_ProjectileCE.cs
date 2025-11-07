using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class SkyFaller_ProjectileCE : Skyfaller
    {
        protected override void Impact()
        {
            this.TryGetComp<CompExplosiveCE>()?.Explode(this, new Vector3(Position.x, 0, Position.z), Map);
            base.Impact();
        }
    }
}
