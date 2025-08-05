using System.Collections.Generic;
using PipeSystem;
using RimWorld;
using Verse;
using System;

namespace BDsPlasmaWeapon
{
    public class CompPrimitiveGeyserExtractor : CompResource
    {
        public CompProperties_PrimitiveGeyserExtractor Props
        {
            get
            {
                return (CompProperties_PrimitiveGeyserExtractor)props;
            }
        }

        Random random = new Random();

        protected CompPowerTrader powerComp;

        protected CompBreakdownable breakdownableComp;

        protected CompFlickable flickableComp;

        protected float currentGeneration = 0;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = parent.GetComp<CompPowerTrader>();
            flickableComp = parent.GetComp<CompFlickable>();
            breakdownableComp = parent.GetComp<CompBreakdownable>();
        }

        public override string CompInspectStringExtra()
        {
            if (Prefs.DevMode)
            {
                return currentGeneration.ToString();
            }
            return null;
        }

        private bool ShouldProduce
        {
            get
            {

                if ((powerComp != null && !powerComp.PowerOn) || (breakdownableComp != null && breakdownableComp.BrokenDown) || (flickableComp != null && !flickableComp.SwitchIsOn))
                {
                    return false;
                }
                return true;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(Props.generateInterval) && ShouldProduce)
            {
                CompResourceStorage compResourceStorage = parent.TryGetComp<CompResourceStorage>();
                float productionCache = production;
                if (compResourceStorage != null)
                {
                    float amountCanAccept = compResourceStorage.AmountCanAccept;
                    if (amountCanAccept > production)
                    {
                        compResourceStorage.AddResource(production);
                        productionCache = 0;
                    }
                    else if (amountCanAccept > 0)
                    {
                        compResourceStorage.AddResource(amountCanAccept);
                        productionCache = production - amountCanAccept;
                    }
                }
                if (productionCache > 0)
                {
                    PipeNet pipeNet = PipeNet;
                    pipeNet.DistributeAmongStorage(productionCache);
                }
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (ShouldProduce)
            {
                PipeNet pipeNet = PipeNet;
                pipeNet.DistributeAmongStorage(production * (250 / (float)Props.generateInterval));
            }
        }

        private float production
        {
            get
            {
                float randomFactor = (float)(random.NextDouble() * (Props.maxGeneration - Props.minGeneration));
                currentGeneration = randomFactor + Props.minGeneration;
                return currentGeneration;
            }
        }
    }

    public class CompProperties_PrimitiveGeyserExtractor : CompProperties_Resource
    {
        public int generateInterval = 60;
        public float maxGeneration = 1;
        public float minGeneration = 0;


        public CompProperties_PrimitiveGeyserExtractor()
        {
            compClass = typeof(CompPrimitiveGeyserExtractor);
        }
    }
}
