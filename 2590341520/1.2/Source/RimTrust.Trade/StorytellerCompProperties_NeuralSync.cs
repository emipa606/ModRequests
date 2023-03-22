using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimTrust.Trade
{
    public class StorytellerCompProperties_NeuralSync : StorytellerCompProperties
    {
        public StorytellerCompProperties_NeuralSync()
        {
            this.compClass = typeof(StorytellerComp_NeuralSync);
        }
        
        public IncidentDef incident = new IncidentDef();

        public int delayTicks = 60;
    }
}