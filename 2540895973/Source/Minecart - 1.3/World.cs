using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Minecart
{
    public class WorldObject_Minecart : WorldObject
    {
        public float direction = 0f;
        public List<Building_Minecart> minecarts;
        public int NextTile;

        private float tileProgress = 0;

        public override void PostAdd()
        {
            List<int> neighbors = new List<int>();
            Find.WorldGrid.GetTileNeighbors(Tile, neighbors);
            neighbors.RemoveAll((t) => Find.WorldGrid.GetRoadDef(Tile, t) == null);
            if (neighbors.Count > 0)
            {
                NextTile = neighbors.MinBy((t) => 180 - Math.Abs(Math.Abs(direction - Find.WorldGrid.GetHeadingFromTo(Tile, t)) - 180));
                direction = Find.WorldGrid.GetHeadingFromTo(Tile, NextTile);
            }
            else
            {
                NextTile = Tile;
            }
        }

        public override Vector3 DrawPos => Vector3.Lerp(
                Find.WorldGrid.GetTileCenter(Tile),
                Find.WorldGrid.GetTileCenter(NextTile),
                tileProgress
            );

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Speed: ");
            sb.Append(minecarts[0].Speed.ToStringWithSign());
            sb.Append("c");

            return sb.ToString();
        }

        public override void Tick()
        {
            if (minecarts.Count > 0)
                tileProgress += minecarts[0].Speed / CaravanTicksPerMoveUtility.CellToTilesConversionRatio;
            else
                tileProgress += 0.05f;
            if (tileProgress >= 1)
            {
                Tile = NextTile;
                MapParent mapParent = Find.WorldObjects.MapParentAt(Tile);
                if (mapParent != null && mapParent.HasMap)
                {
                    Map map = mapParent.Map;
                    foreach (IntVec3 c in CellRect.WholeMap(map).ContractedBy(GenGrid.NoBuildEdgeWidth).EdgeCells) {
                        if (c.GetTerrain(map).defName == "Rail")
                        {
                            foreach(Building_Minecart minecart in minecarts)
                            {
                                minecart.SpawnSetup(map, false);
                                minecart.Position = c;
                            }
                        }
                    }
                }


                List<int> neighbors = new List<int>();
                Find.WorldGrid.GetTileNeighbors(Tile, neighbors);
                neighbors.RemoveAll((t) => Find.WorldGrid.GetRoadDef(Tile, t) == null);
                if (neighbors.Count > 0)
                {
                    NextTile = neighbors.MinBy((t) => 180 - Math.Abs(Math.Abs(direction - Find.WorldGrid.GetHeadingFromTo(Tile, t)) - 180));
                    direction = Find.WorldGrid.GetHeadingFromTo(Tile, NextTile);
                }
                tileProgress--;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref direction, "direction");
            Scribe_Values.Look(ref NextTile, "nextTile");
            Scribe_Values.Look(ref tileProgress, "subtile");
            Scribe_Deep.Look(ref minecarts, "minecarts");
        }
    }

    [DefOf]
    public static class WorldObjectDefOf
    {
        public static WorldObjectDef MinecartCaravan;
    }
}
