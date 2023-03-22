using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RimTrust.Trade
{
    public class StorytellerCompProperties_InterestDrop : StorytellerCompProperties
    {
        public StorytellerCompProperties_InterestDrop()
        {
            this.compClass = typeof(StorytellerComp_InterestDrop);
        }
        
        public IncidentDef incident = new IncidentDef();

        public int delayTicks = 60;
    }
}