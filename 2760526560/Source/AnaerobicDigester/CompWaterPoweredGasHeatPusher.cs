using GasNetwork;

namespace AnaerobicDigester
{
    public class CompWaterPoweredGasHeatPusher : CompGasHeatPusher
    {
        protected CompWaterConsumer _waterConsumer;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this._waterConsumer = this.parent.GetComp<CompWaterConsumer>();
        }
        
        protected override bool ShouldPushHeatNow
        {
            get
            {
                return base.ShouldPushHeatNow && (this._waterConsumer == null || this._waterConsumer.HasEnoughWater());
            }
        }
    }
}