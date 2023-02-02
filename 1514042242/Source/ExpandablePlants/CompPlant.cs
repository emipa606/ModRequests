using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace ExpandablePlants
{
    class CompPlant : ThingComp
    {
        public CompProperties_Plant Props => props as CompProperties_Plant;

        public virtual float MinOptimalGrowthTemperature => Props.minOptimalGrowthTemperature;
        public virtual float MinGrowthTemperature => Props.minGrowthTemperature;
        public virtual float MaxOptimalGrowthTemperature => Props.maxOptimalGrowthTemperature;
        public virtual float MaxGrowthTemperature => Props.maxGrowthTemperature;
        public virtual float MaxLeaflessTemperature => Props.maxLeaflessTemperature;
        public virtual float MinLeaflessTemperature => Props.minLeaflessTemperature;

        public virtual float MinDieOfHeatTemperature => Props.minDieOfHeatTemperature;
        public virtual float MaxDieOfHeatTemperature => Props.maxDieOfHeatTemperature;

        public virtual float RestBegins => Props.restBegins;
        public virtual float RestEnds => Props.restEnds;

        public virtual bool CanDieOfHeat => Props.canDieOfHeat;
    }
}
