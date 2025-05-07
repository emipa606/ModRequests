using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using DTechprinting.Comps;

namespace DTechprinting
{
    public static class ShardMaker
    {

		public const int stackSize = 10;
		public const float forceSingleThreshold = 1500f;
		
		public static ThingDef ShardForProject(ResearchProjectDef rpd)
		{
			return DefDatabase<ThingDef>.AllDefs.FirstOrDefault(delegate (ThingDef x)
			{
				CompProperties_Techshard compProperties = x.GetCompProperties<CompProperties_Techshard>();
				return compProperties != null && compProperties.project == rpd;
			});
		}

		public static float GetWorstValue(ThingDef td)
		{
			float val = StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(td, null, QualityCategory.Awful), true);
			if (td.stackLimit == 1 || td.BaseMarketValue >= forceSingleThreshold)
				return val;
			return ShardMaker.stackSize * val;
		}

		public static float GetBestValue(ThingDef td)
		{
			float val = StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(td, null, QualityCategory.Legendary), true);
			if (td.stackLimit == 1 || td.BaseMarketValue >= forceSingleThreshold)
				return val;
			return ShardMaker.stackSize * val;
		}

		public static float GetAverageValue(ThingDef td)
		{
			if (td.stackLimit == 1 || td.BaseMarketValue >= forceSingleThreshold)
				return td.BaseMarketValue;
			else
			{
				return ShardMaker.stackSize * td.BaseMarketValue;
			}
		}

		public static float GetResearchProjectValue(ResearchProjectDef rpd)
		{
			List<ThingDef> things;
			if (!Base.researchDic.TryGetValue(rpd, out things))
				Log.Error("getresearchprojectvalue tried to get rpd not in dic");
			return things.Max(td => GetAverageValue(td));
		}

		public static float GetResearchProjectSellValue(ResearchProjectDef rpd)
		{
			List<ThingDef> things;
			if (!Base.researchDic.TryGetValue(rpd, out things))
				Log.Error("GetResearchProjectSellValue tried to get rpd not in dic");
			return things.Max(td => GetAverageValue(td) * td.GetStatValueAbstract(StatDefOf.SellPriceFactor));
		}

		public static float SellValueOfOneShard(ResearchProjectDef rpd)
		{
			
			return GetResearchProjectSellValue(rpd) / 100f; // 0.5 sale price factor * max sale value of an item / 50 shards for such an item 
		}

		public static float RequiredValueForOneShard(ResearchProjectDef rpd) // required value from techprinted item for one shard
		{
			return GetResearchProjectValue(rpd) * 2f / 100f; // representative item is worth 50 shards
		}

		public static int CalcNumShardsForRaw(Thing ingredient, ResearchProjectDef rpd)
		{
			float ingredientValue = ingredient.MarketValue * ingredient.stackCount;
			float shardValue = RequiredValueForOneShard(rpd);
			QualityCategory q;
			if (ingredient.TryGetQuality(out q))
				shardValue *= 0.75f; // one average item at 75% health only if it has quality
			return Mathf.RoundToInt(ingredientValue / shardValue);
		}

		public static int CalcNumShardsFor(Thing ingredient)
		{
			ResearchProjectDef rpd = Base.thingDic[ingredient.def];
			int numShardsRaw = CalcNumShardsForRaw(ingredient, rpd);
			numShardsRaw = Mathf.RoundToInt(numShardsRaw * TechprintingSettings.shardYieldRatio);
			QualityCategory q;
			if (ingredient.TryGetQuality(out q))
			{
				if (q == QualityCategory.Legendary || q == QualityCategory.Masterwork)
				{
					int clampMax = TechprintingSettings.printAllItems ? Mathf.RoundToInt(Mathf.Max(rpd.techprintCount, 100)) : Mathf.RoundToInt(rpd.techprintCount);
					return Mathf.Clamp(numShardsRaw, 1, clampMax);
				}
			}
			return Mathf.Clamp(numShardsRaw, 1, 100);
		}

		public static Thing MakeShards(Thing ingredient)
		{
			//ResearchProjectDef rpd = Base.thingDic[ingredient.def];
			//ThingDef shardDef = Techshard(rpd);
			CompShardable comp = ingredient.TryGetComp<CompShardable>();
			if (comp == null)
            {
				Log.Error("Tried to shard thing with no CompShardable");
				return null;
            }

			ThingDef shardDef = comp.shard;

			Thing thing = ThingMaker.MakeThing(shardDef);
			thing.stackCount = CalcNumShardsFor(ingredient);

			return thing;
		}
	}
}
