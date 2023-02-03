using DubsBadHygiene;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DesalinationPlant
{
    public class CompProperties_DesalinationPlant : CompProperties
    {
        public float waterPerTick;
        public CompProperties_DesalinationPlant()
        {
            this.compClass = typeof(CompDesalinationPlant);
        }
    }
    public class CompDesalinationPlant : ThingComp
    {
        private CompPipe compPipe;

        private CompPowerTrader compPower;
        public CompProperties_DesalinationPlant Props => base.props as CompProperties_DesalinationPlant;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compPipe = this.parent.GetComp<CompPipe>();
            compPower = this.parent.GetComp<CompPowerTrader>();
        }
        public override void CompTick()
        {
            base.CompTick();
            if (compPower.PowerOn)
            {
                compPipe.pipeNet.PushWater(Props.waterPerTick);
            }
        }
    }
}
