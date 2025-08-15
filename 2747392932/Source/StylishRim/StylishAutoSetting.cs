using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace StylishRim
{
	internal class StylishAutoSetting
	{
		public static bool gameLoaded = true;
		public static bool factionBlender = false;
		private static string MOD = "[Stylish Rim] ";
		private static string MOD_NAME_FACTION_BLENDER = "Faction Blender";
		public float totalFactor = 18000;
		public ThingDef def;
		public float weightCPU;
		public float weightMemory;
		public float calcTotal = 1;
		public float calcTarget = 0;
		public float weightResult;

		public int cacheResult;
		public int cacheBefore;
		public int resolutionResult;
		public int resolutionBefore;
		public bool disableCache;
		public bool disableCacheBefore;
		public bool leaveCache;
		public int AtlasScale => (def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.atlasScale ?? 1;
		public float BorderScale => (def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.borderScale ?? 1;
		public StylishAutoSetting(ThingDef def)
		{
			this.def = def;
			weightCPU = StylishRimSettings.weightCPU;
			weightMemory = StylishRimSettings.weightMemory;
			Calculate();
		}
		public void Calculate()
		{
			CalculateMemoryFactor();
			CalculateCPUFactor();
		}
		private void CalculateCPUFactor()
		{
			if (weightCPU == 4)
			{
				disableCache = true;
			}
			else if (weightCPU >= 3)
			{
				if (BorderScale > 1)
				{
					disableCache = true;
				}
				else if (cacheResult < 4 || resolutionResult > 6)
				{
					disableCache = true;
				}
				else
				{
					disableCache = false;
				}
			}
			else if (weightCPU >= 2)
			{
				if (BorderScale > 1 && AtlasScale > 4)
				{
					disableCache = true;
				}
				else if (cacheResult < 3)
				{
					disableCache = true;
				}
				else
				{
					disableCache = false;
				}
			}
			else
			{
				disableCache = false;
			}
		}
		private void CalculateMemoryFactor()
		{
			List<FactionDef> containsFaction = new List<FactionDef>();
			foreach (FactionDef f in gameLoaded ? Find.FactionManager.AllFactions.Select(x => x.def) : DefDatabase<FactionDef>.AllDefs)
			{
				if (f?.pawnGroupMakers == null) continue;
				bool contains = false;
				foreach (PawnGroupMaker pgm in f.pawnGroupMakers)
				{
					if (pgm?.options == null) continue;
					foreach (PawnGenOption pgo in pgm.options.Where(x => x.kind.race.defName == def.defName))
					{
						if (pgo?.kind?.race == null) continue;
						contains = true;
						break;
					}
					if (contains) break;
				}
				if (contains) containsFaction.Add(f);
			}
			foreach (FactionDef f in containsFaction)
			{
				if (f.modContentPack?.Name == null || f.modContentPack.Name != MOD_NAME_FACTION_BLENDER)
				{
					foreach (PawnGroupMaker pgm in f.pawnGroupMakers)
					{
						if (pgm?.options == null) continue;
						foreach (PawnGenOption pgo in pgm.options)
						{
							if (pgo?.kind?.race == null) continue;
							if (pgo.kind.race.defName == def.defName)
							{
								calcTarget += pgo.Cost / pgo.kind.combatPower;
							}
							calcTotal += pgo.Cost / pgo.kind.combatPower;
						}
					}
				}
			}
			weightResult = totalFactor / def.BaseMarketValue * (float)(Math.Sqrt(calcTarget) / Math.Sqrt(calcTotal)) * (float)Math.Max(Math.Sqrt(weightMemory), 1);
			weightResult += factionBlender ? 1 : 0;
			cacheResult = (int)Math.Max(Math.Min(Math.Round(weightResult, 0) + (weightCPU == 0 ? 1 : 0), 6), 1);

			if (weightMemory == 0)
			{
				resolutionResult = 1;
			}
			else
			{
				resolutionResult = (int)Math.Min(Math.Max(Math.Round(AtlasScale * (0.5f + weightMemory * 0.25), 0), Math.Max(weightMemory, 2)), Math.Min(128 * weightMemory / Math.Pow(2, cacheResult) * BorderScale, 8));
			}
			leaveCache = resolutionResult * resolutionResult * Math.Pow(2, cacheResult - 1) <= 16 * Math.Pow(2, 4 - weightCPU / 2 + weightMemory * 1.5);
		}
		public void Apply(PawnStyleSet pss)
		{
			disableCacheBefore = pss.disableCache;
			resolutionBefore = pss.resolution;
			cacheBefore = pss.cacheSize;

			pss.disableCache = disableCache;
			pss.resolution = resolutionResult;
			pss.cacheSize = cacheResult;
			pss.leaveOneCache = leaveCache;
		}
		public string Send()
		{
			if (StylishRimSettings.DEBUG)
			{
				Log.Warning($"{MOD}Race: {def.defName}, wc:{weightCPU}, wm:{weightMemory}. calc: {totalFactor} / {def.BaseMarketValue} * ({Math.Sqrt(calcTarget)} / {Math.Sqrt(calcTotal)}) * {(float)Math.Max(Math.Sqrt(weightMemory), 1)} = {weightResult}");
			}
			if (disableCache)
			{
				return $"{MOD}Race: {def.defName}, CacheOff.";
			}
			return $"{MOD}Race: {def.defName}, Pawn resolution: [{GetResolutionString(resolutionBefore)} to {GetResolutionString(resolutionResult)}], Pawn capacity: [{GetCacheString(cacheBefore)} to {GetCacheString(cacheResult)}]{(leaveCache ? ", Leave one cache" : "")}.";
		}
		public static string GetResolutionString(int resolution)
		{
			return $"{resolution * 128} * {resolution * 128}";
		}
		public static string GetCacheString(int cache)
		{
			int count = StylishTextureAtlas.IgnoreHeadOnly ? 64 : 32;
			for (int i = 0; i < 6 - cache; i++)
			{
				count /= 2;
			}
			return count.ToString();
		}
	}
}
