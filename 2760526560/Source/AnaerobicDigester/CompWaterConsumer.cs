using System.Text;
using DubsBadHygiene;
using GasNetwork;
using RimWorld;
using Verse;

namespace AnaerobicDigester
{
    //Adapted from https://github.com/joeyjoejoeshabidoo/Alternative-Power-Solutions-for-RW/blob/main/1.3/Source/AlternativePowerSolutions/CompWaterConsumer.cs
    public class CompProperties_WaterConsumer : CompProperties
    {
        public float waterPerTick;
        public CompProperties_WaterConsumer()
        {
            this.compClass = typeof(CompWaterConsumer);
        }
    }
    public class CompWaterConsumer : ThingComp
    {
        private CompPipe compPipe;
        
        private CompGasTrader compGas;
        public CompProperties_WaterConsumer Props => base.props as CompProperties_WaterConsumer;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compPipe = this.parent.GetComp<CompPipe>();
            compGas = this.parent.GetComp<CompGasTrader>();
        }
        public override void CompTick()
        {
            base.CompTick();
            if (compGas != null && (compGas.GasOn || compGas.WantsToBeOn))
            {
                compPipe.pipeNet.PullWater(Props.waterPerTick, out _);
            }
        }

        public override string CompInspectStringExtra()
        {
            return $"Water consumption per second : {Props.waterPerTick * 60} L";
        }

        public bool HasEnoughWater()
        {
            return compPipe.pipeNet.WaterStorage >= Props.waterPerTick;
        }
    }
}