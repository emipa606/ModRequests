using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

// Code written by Taranchuk

namespace AMP
{
	public class CompTerrainPumpDryAdv : CompTerrainPumpDry
	{
		protected override void AffectCell(IntVec3 c)
		{
			AffectCell(parent.Map, c);
		}

		public static new void AffectCell(Map map, IntVec3 c)
		{
			TerrainDef terrain = c.GetTerrain(map);
			TerrainDef terrainToDryTo = GetTerrainToDryTo(map, terrain);
			if (terrainToDryTo != null)
			{
				map.terrainGrid.SetTerrain(c, terrainToDryTo);
			}
			TerrainDef terrainDef = map.terrainGrid.UnderTerrainAt(c);
			if (terrainDef != null)
			{
				TerrainDef terrainToDryTo2 = GetTerrainToDryTo(map, terrainDef);
				if (terrainToDryTo2 != null)
				{
					map.terrainGrid.SetUnderTerrain(c, terrainToDryTo2);
				}
			}
		}

		private static TerrainDef GetTerrainToDryTo(Map map, TerrainDef terrainDef)
		{
			if (terrainDef == AdvDefOf.WaterDeep 
				|| terrainDef == AdvDefOf.WaterMovingChestDeep
				|| terrainDef == AdvDefOf.WaterMovingShallow
				|| terrainDef == AdvDefOf.WaterOceanDeep)
			{
				return TerrainDefOf.Gravel;
			}
			if (terrainDef.driesTo == null)
			{
				return null;
			}
			if (map.Biome == BiomeDefOf.SeaIce)
			{
				return TerrainDefOf.Ice;
			}
			return terrainDef.driesTo;
		}
	}

	[DefOf]
	public static class AdvDefOf
    {
		public static TerrainDef WaterDeep;
		public static TerrainDef WaterMovingChestDeep;
		public static TerrainDef WaterMovingShallow;
		public static TerrainDef WaterOceanDeep;

	}
}
