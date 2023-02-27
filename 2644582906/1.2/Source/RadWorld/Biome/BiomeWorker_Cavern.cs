using System;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RadWorld
{
    public class BiomeWorker_Cavern : BiomeWorker
	{
		public virtual float CaveGeneratorValue => 0f;
		public float GetScore(Tile tile, int tileID, BiomeDef biomeDef)
		{
			if (tile.WaterCovered)
			{
				return -100f;
			}
			Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tileID);
			var value = CavernPerlin.GetNoiseFor(biomeDef).GetValue(tileCenter);
			if (value > CaveGeneratorValue)
			{
				var result = 100f + value;
				return result;
			}
			return -100f;
		}

        public override float GetScore(Tile tile, int tileID)
        {
            throw new NotImplementedException();
        }
    }

	public class BiomeWorker_CavernRegular : BiomeWorker_Cavern
	{
		public override float CaveGeneratorValue => RadWorldMod.settings.cavernRegularCoverage;
        public override float GetScore(Tile tile, int tileID)
        {
			return GetScore(tile, tileID, RW_DefOf.RW_Cavern);
        }
    }

	public class BiomeWorker_CavernCollapsed : BiomeWorker_Cavern
	{
		public override float CaveGeneratorValue => RadWorldMod.settings.cavernCollapsedCoverage;
		public override float GetScore(Tile tile, int tileID)
		{
			return GetScore(tile, tileID, RW_DefOf.RW_CollapsedCavern);
		}
	}

	public class BiomeWorker_CavernLush : BiomeWorker_Cavern
	{
		public override float CaveGeneratorValue => RadWorldMod.settings.cavernLushCoverage;
		public override float GetScore(Tile tile, int tileID)
		{
			return GetScore(tile, tileID, RW_DefOf.RW_LushCavern);
		}
	}

	public class BiomeWorker_CavernSick : BiomeWorker_Cavern
	{
		public override float CaveGeneratorValue => RadWorldMod.settings.cavernSickCoverage;
		public override float GetScore(Tile tile, int tileID)
		{
			return GetScore(tile, tileID, RW_DefOf.RW_SickCavern);
		}
	}

	public class BiomeWorker_CavernInfested : BiomeWorker_Cavern
	{
		public override float CaveGeneratorValue => RadWorldMod.settings.cavernInfestedCoverage;
		public override float GetScore(Tile tile, int tileID)
		{
			return GetScore(tile, tileID, RW_DefOf.RW_InfestedCavern);
		}
	}

	public class BiomeWorker_CavernBarren : BiomeWorker_Cavern
	{
		public override float CaveGeneratorValue => RadWorldMod.settings.cavernBarrenCoverage;
		public override float GetScore(Tile tile, int tileID)
		{
			return GetScore(tile, tileID, RW_DefOf.RW_BarrenCavern);
		}
	}

	public class BiomeWorker_CavernSurface : BiomeWorker_Cavern
	{
		public override float CaveGeneratorValue => RadWorldMod.settings.cavernSurfaceCoverage;
		public override float GetScore(Tile tile, int tileID)
		{
			return GetScore(tile, tileID, RW_DefOf.RW_SurfaceCavern);
		}
	}
}
