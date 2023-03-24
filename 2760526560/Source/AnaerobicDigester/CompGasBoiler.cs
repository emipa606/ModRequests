using DubsBadHygiene;
using GasNetwork;

namespace AnaerobicDigester
{
    public class CompProperties_CompGasBoiler : CompProperties_CompBoiler
    {
        public CompProperties_CompGasBoiler()
        {
            this.compClass = typeof(CompGasBoiler);
        }
    }
    
    public class CompGasBoiler : CompBoiler
    {
        public CompGasTrader gasComp;

        public override bool WorkingNow
        {
            get
            {
                return base.WorkingNow && (this.gasComp == null ||
                                           (this.gasComp.GasOn && this.gasComp.GasConsumptionPerTick <= this.gasComp.Network.Stored));
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.gasComp = this.parent.GetComp<CompGasTrader>();
        }
    }
}