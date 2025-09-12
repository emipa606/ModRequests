using System;
using Verse;

namespace Necro
{
    
    public class CompProperties_NecronoidStuffSpawner : CompProperties
    {
        
        public CompProperties_NecronoidStuffSpawner()
        {
            this.compClass = typeof(NecronoidStuffSpawner);
        }

        
        public ThingDef thingToSpawn;

        
        public int spawnCount = 1;

        
        public IntRange spawnIntervalRange = new IntRange(100, 100);

        
        public int spawnMaxAdjacent = -1;

        
        public bool spawnForbidden;

        
        public bool requiresPower;

        
        public bool writeTimeLeftToSpawn;

        
        public bool showMessageIfOwned;

        
        public string saveKeysPrefix;

        
        public bool inheritFaction;
    }
}
