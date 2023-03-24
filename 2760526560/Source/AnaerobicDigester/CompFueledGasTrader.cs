using GasNetwork;
using RimWorld;
using Verse;

namespace AnaerobicDigester
{
    public class CompFueledGasTrader : CompGasTrader
    {
        public override bool WantsToBeOn => base.WantsToBeOn && IsFueled();

        public override bool GasOn
        {
            get => base.GasOn && IsFueled();
            set => base.GasOn = value;
        }

        private CompRefuelable _compRefuelable;

        private void ResolveRefuelable()
        {
            if (_compRefuelable == null)
            {
                _compRefuelable = this.parent.TryGetComp<CompRefuelable>();
            }
        }

        private bool IsFueled() => _compRefuelable == null || _compRefuelable.HasFuel;

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