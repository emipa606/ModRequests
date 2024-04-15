using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CMS
{
    public class MapComponent_StoneGrid : MapComponent
    {
        public ThingDef[] StoneGrid;

        public MapComponent_StoneGrid(Map map) : base(map)
        {

        }

        public ThingDef StoneChunkAt(IntVec3 c)
        {
            return StoneTypeAt(c).building.mineableThing;
        }

        public TerrainDef StoneTerrainAt(IntVec3 c)
        {
            return StoneTypeAt(c).building.naturalTerrain;
        }

        public ThingDef StoneTypeAt(IntVec3 c)
        {
            if (StoneGrid == null)
                Initialize();
            return StoneGrid[map.cellIndices.CellToIndex(c)];
        }

        public override void MapGenerated()
        {
            Initialize();
        }

        public void Initialize()
        {
            RockNoises.Init(map);
            StoneGrid = new ThingDef[map.cellIndices.NumGridCells];
            for (int i = 0; i < map.cellIndices.NumGridCells; i++)
            {
                var st = GenStep_RocksFromGrid.RockDefAt(map.cellIndices.IndexToCell(i));
                StoneGrid[i] = st;
            }
            RockNoises.Reset();
        }

        public ThingDef ChunkFor(ThingDef rockDef)
        {
            Log.Message("Getting chunk name for " + rockDef.defName + ".");
            return DefDatabase<ThingDef>.GetNamed(rockDef.defName + "Chunk");
        }
    }
}