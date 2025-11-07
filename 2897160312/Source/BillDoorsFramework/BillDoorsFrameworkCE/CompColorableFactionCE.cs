using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BillDoorsFramework
{
    public class CompColorableFactionCE : CompColorableFaction
    {
        protected override Faction GetFaction()
        {
            if (parent is ProjectileCE projectile)
            {
                return projectile.launcher?.Faction;
            }
            return base.GetFaction();
        }
    }
}
