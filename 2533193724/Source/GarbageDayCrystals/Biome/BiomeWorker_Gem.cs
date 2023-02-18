using System;
using Crystosentient.Settings;
using RimWorld;
using RimWorld.Planet;

namespace Crystosentient.Biome
{
	// Token: 0x02000012 RID: 18
	public class BiomeWorker_Gem : BiomeWorker
	{
		// Token: 0x0600006B RID: 107 RVA: 0x0000242C File Offset: 0x0000062C
		public override float GetScore(Tile tile, int tileID)
		{

			if (!Crystosentient.Settings.Settings.SpawnGemGarden)
			{
				return -100f;
			}
			if (tile.elevation <= 0f)
			{
				return -100f;
			}
			if (tile.hilliness == Hilliness.Flat)
			{
				return 12f + (tile.temperature - 20f) * 1.9f;
			}
			else
			{
				return 12f + (tile.temperature - 20f) * 2f;
			}

		}
		
	}
}

