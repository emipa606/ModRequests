using GasNetwork;
using Verse;

namespace AnaerobicDigester
{
    public class CompWaterPoweredGasTrader : CompGasTrader
    {
        public override bool WantsToBeOn => base.WantsToBeOn && IsFueled();

        public override bool GasOn
        {
            get => base.GasOn && IsFueled();
            set => base.GasOn = value;
        }

        private CompWaterConsumer _compWaterConsumer;

        private void ResolveRefuelable()
        {
            if (_compWaterConsumer == null)
            {
                _compWaterConsumer = this.parent.TryGetComp<CompWaterConsumer>();
            }
        }

        private bool IsFueled() => _compWaterConsumer == null || _compWaterConsumer.HasEnoughWater();

        public override void PostPostMake()
        {
            base.PostPostMake();
            this.ResolveRefuelable();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            this.ResolveRefuelable();
        }
    }
}