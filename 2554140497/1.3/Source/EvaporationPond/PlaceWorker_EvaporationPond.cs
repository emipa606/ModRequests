using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace EvaporationPond
{
    public class PlaceWorker_EvaporationPond : PlaceWorker
    {
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			var groundCells = GroundCells(checkingDef, loc, out var waterCells);
			if (waterCells is null || waterCells.Any(x => !IsShallowOceanWater(x, map)))
			{
				return new AcceptanceReport("EP.MustBeOnShallowOceanWater".Translate());
			}
			else if (groundCells is null || groundCells.Any(x => IsWater(x, Find.CurrentMap)))
			{
				return new AcceptanceReport("EP.MustBeOnNonWaterGround".Translate());
			}
			return true;
		}
		private bool IsWater(IntVec3 loc, Map map)
		{
			return map.terrainGrid.TerrainAt(loc).IsWater;
		}
		private bool IsShallowOceanWater(IntVec3 loc, Map map)
		{
			return map.terrainGrid.TerrainAt(loc) == TerrainDefOf.WaterOceanShallow;
		}
		public List<IntVec3> GroundCells(BuildableDef checkingDef, IntVec3 loc, out List<IntVec3> waterCells)
		{
			IEnumerable<Rot4> RotationsToUse()
			{
				yield return new Rot4(0);
				yield return new Rot4(1);
				yield return new Rot4(2);
				yield return new Rot4(3);
			}
			var baseRect = GenAdj.OccupiedRect(loc, checkingDef.defaultPlacingRot, checkingDef.Size);
			foreach (var rot4 in RotationsToUse())
            {
				bool foundWater = true;
				waterCells = baseRect.GetEdgeCells(rot4).ToList();
				if (waterCells.Any(x => !IsShallowOceanWater(x, Find.CurrentMap)))
                {
					foundWater = false;
				}
				if (foundWater)
                {
					var copy = waterCells.ListFullCopy();
					var groundCells = baseRect.Cells.Where(x => !copy.Contains(x));
					return groundCells.ToList();
				}
			}
			waterCells = null;
			return null;
		}
		public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			var groundCells = GroundCells(def, loc, out var waterCells);
			if (groundCells != null)
            {
				GenDraw.DrawFieldEdges(groundCells.ToList(), Designator_Place.CanPlaceColor.ToOpaque());
				GenDraw.DrawFieldEdges(waterCells, Designator_Place.CanPlaceColor.ToOpaque());
            }
		}
	}
}
