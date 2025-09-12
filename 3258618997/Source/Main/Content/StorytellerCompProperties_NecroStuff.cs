using System;
using RimWorld;

namespace Necro
{
    
    public class StorytellerCompProperties_NecroStuff : StorytellerCompProperties
    {
        
        public StorytellerCompProperties_NecroStuff()
        {
            this.compClass = typeof(StorytellerComp_NecroStuff);
        }

        
        public IncidentDef incident;

        
        public float baseMtbDays = 5f;
    }
}
