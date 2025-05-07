using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Noise;
using HarmonyLib;
using Verse.AI;
using UnityEngine;
using System.Reflection;
using DTechprinting.Comps;

namespace DTechprinting
{
	[StaticConstructorOnStartup]
	public static class Base

	{
		public static Dictionary<ThingDef, ResearchProjectDef> thingDic = null;
		public static Dictionary<ResearchProjectDef, List<ThingDef>> researchDic = null;

		public static bool initialized = false;

		[RimWorld.DefOf]
		public static class DefOf
		{
			public static RecipeDef DTechprintingRecipe;
			public static RecipeDef DTechprintingStackRecipe;
			public static JobDef ApplyTechshards;
			public static JobDef ShardBuildingJob;
			public static ThingCategoryDef DTechshards;
			public static WorkTypeDef DTechprinting;
			public static ThingDef DTechprinter;
			public static DesignationDef ShardBuilding;
		}


		static Base()
		{
			if (!TechprintingSettings.lateLoad)
				Initialize();
		}

		public static void UpdateAll()
		{
			//ResearchProjectHelper.SetTechprintRequirements();
			if (TechprintingSettings.lateLoad)
				GenerateAllShards();
			MakeThingDictionaries();
			SetTechshardPrices();
			UpdateTechshardRecipes();
		}

		public static void Initialize()
		{
			TechprintingSettings.ValidateSettings();

			if (TechprintingSettings.addTechprintRequirements)
				ResearchProjectHelper.SetTechprintRequirements();
			if (TechprintingSettings.splitProjects)
				ResearchProjectHelper.SplitAllProjects();

			ResearchProjectHelper.SetHardTechprintReqs();

			GenerateAllShards();

			foreach (ResearchProjectDef rp in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				if (rp.techprintCount > 0 && rp.techprintCount < 100 && !ResearchProjectHelper.added.Contains(rp) && !ResearchProjectHelper.oldNewMap.Values.Contains(rp))
				{
					rp.techprintCount *= 100;
				}
			}

			MakeThingDictionaries();
			SetTechshardPrices();
		}

		public static void GenerateAllShards()
		{
			foreach (ThingDef def in ThingDefGenerator_Techshards.ImpliedTechshardDefs() ?? Enumerable.Empty<ThingDef>())
			{
				DefGenerator.AddImpliedDef<ThingDef>(def);
				def.ResolveReferences();
			}

			ResourceCounter.ResetDefs();
			DefDatabase<ThingCategoryDef>.ResolveAllReferences(true, false);
			DefDatabase<RecipeDef>.ResolveAllReferences(true, true);
		}

		public static void SetTechshardPrices()
		{
			foreach (ResearchProjectDef rpd in researchDic.Keys.ToList())
			{
				ThingDef shardDef = ShardMaker.ShardForProject(rpd);
				if (shardDef == null)
				{
					Log.Message("No techshard found for " + rpd.defName + " in SetTechshardPrices");
					continue;
				}
				var comp = shardDef.GetCompProperties<CompProperties_Techshard>();
				if (comp.project.techprintCount == 0 && !TechprintingSettings.printAllItems)
					continue;
				if (TechprintingSettings.profitableShards)
				{
					
					shardDef.SetStatBaseValue(StatDefOf.MarketValue, ShardMaker.SellValueOfOneShard(comp.project));
				}
				else
				{
					shardDef.SetStatBaseValue(StatDefOf.MarketValue, 0.01f);
				}
				shardDef.SetStatBaseValue(StatDefOf.SellPriceFactor, 1f);
			}
			
		}
		 
		
		public static void MakeThingDictionaries()
		{
			thingDic = new Dictionary<ThingDef, ResearchProjectDef>();
			researchDic = new Dictionary<ResearchProjectDef, List<ThingDef>>();

			foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefsListForReading)
			{
				ResearchProjectDef rpd = ThingDefHelper.GetBestRPDForRecipe(recipe);
				if (rpd != null && recipe.ProducedThingDef != null)
				{
					if (ShardMaker.ShardForProject(rpd) != null && (rpd.techprintCount > 0 || TechprintingSettings.printAllItems))
					{
						ThingDef producedThing = recipe.ProducedThingDef;
						if (producedThing.GetCompProperties<CompProperties_Shardable>() == null)
							producedThing.comps.Add(new CompProperties_Shardable());

						thingDic.SetOrAdd(producedThing, rpd);

						List<ThingDef> things;
						if (researchDic.TryGetValue(rpd, out things))
							things.Add(producedThing);
						else
							researchDic.Add(rpd, new List<ThingDef> { producedThing });
					}
				}
			}

			if (TechprintingSettings.shardBuildings)
            {
				foreach (ThingDef building in DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Building || x.building != null))
                {
					if (thingDic.ContainsKey(building))
						continue;
					ResearchProjectDef rpd = ThingDefHelper.GetBestRPDForBuilding(building);
					if (rpd != null)
					{
						if (ShardMaker.ShardForProject(rpd) != null && (rpd.techprintCount > 0 || TechprintingSettings.printAllItems))
						{
							if (building.GetCompProperties<CompProperties_Shardable>() == null)
								building.comps.Add(new CompProperties_Shardable());

							thingDic.SetOrAdd(building, rpd);

							List<ThingDef> things;
							if (researchDic.TryGetValue(rpd, out things))
								things.Add(building);
							else
								researchDic.Add(rpd, new List<ThingDef> { building });
						}
					}
				}
            }

			GearAssigner.HardAssign(ref thingDic, ref researchDic);
			GearAssigner.OverrideAssign(ref thingDic, ref researchDic);
		}
		

		public static void UpdateTechshardRecipes()
		{
			if (Current.Game == null)
				return;

			if (thingDic == null || researchDic == null)
				MakeThingDictionaries();

			List<ResearchProjectDef> techList = researchDic.Keys.ToList().FindAll(x => ProjectIsPrintable(x));
			/*
			List<ResearchProjectDef> techList = researchDic.Keys.ToList().FindAll(x => !x.IsFinished && x.techprintCount > 0 && !x.TechprintRequirementMet); 
			if (TechprintingSettings.printAllItems)
				techList = techList.Concat(researchDic.Keys.ToList().FindAll(x => !x.IsFinished && x.techprintCount <= 0)).ToList(); 
			
			if (TechprintingSettings.enableUnlockedTechPrinting)
				techList = techList.Concat(researchDic.Keys.ToList().FindAll(x => !x.IsFinished && x.techprintCount > 0 && x.TechprintRequirementMet)).ToList(); 
			
			if (TechprintingSettings.enableCompletedTechPrinting)
				techList = techList.Concat(researchDic.Keys.ToList().FindAll(x => x.IsFinished)).ToList(); // completed projects
			*/
			if (techList.NullOrEmpty())
				return;
			ThingFilter itemFilter = ClearFilter(DefOf.DTechprintingRecipe, 1);
			ThingFilter stackFilter = ClearFilter(DefOf.DTechprintingStackRecipe, ShardMaker.stackSize);
			foreach (ResearchProjectDef rpd in techList)
			{
				List<ThingDef> things;
				if (researchDic.TryGetValue(rpd, out things))
				{
					foreach (ThingDef td in things)
					{
						itemFilter.SetAllow(td, IsSingleAllowed(td));
						stackFilter.SetAllow(td, IsStackAllowed(td, ShardMaker.stackSize));
					}
				}
			}
		}

		public static bool ThingIsPrintable(ThingDef td)
        {
			ResearchProjectDef rpd;
			if (!thingDic.TryGetValue(td, out rpd))
				return false;
			return (IsSingleAllowed(td) || IsStackAllowed(td, ShardMaker.stackSize)) && ProjectIsPrintable(rpd);
        }

		public static bool ProjectIsPrintable(ResearchProjectDef rpd)
        {
			if (!researchDic.ContainsKey(rpd)) // must have a shard
				return false;
			// this is organized for readability, not efficiency
			if (!rpd.IsFinished && rpd.techprintCount > 0 && !rpd.TechprintRequirementMet) // base: projects with shard requirements unmet, always show
				return true;
			if (TechprintingSettings.printAllItems && (!rpd.IsFinished && rpd.techprintCount <= 0)) // projects with no shard requirement, only when printAllItems
				return true;
			if (TechprintingSettings.enableUnlockedTechPrinting && (!rpd.IsFinished && rpd.techprintCount > 0 && rpd.TechprintRequirementMet)) // projects with shards but requirement is met
				return true;
			if (TechprintingSettings.enableCompletedTechPrinting && (rpd.IsFinished)) // completed projects
				return true;

			return false;
		}

		public static bool IsSingleAllowed(ThingDef td)
		{
			ResearchProjectDef rpd;
			if (!thingDic.TryGetValue(td, out rpd))
				Log.Error("IsSingleAllowed tried to get td not in dic");
			return td.BaseMarketValue >= ShardMaker.RequiredValueForOneShard(rpd) || td.BaseMarketValue >= ShardMaker.forceSingleThreshold;
		}
		 
		public static bool IsStackAllowed(ThingDef td, int size)
		{
			if (td.stackLimit < size)
				return false;
			ResearchProjectDef rpd;
			if (!thingDic.TryGetValue(td, out rpd))
				Log.Error("IsSingleAllowed tried to get td not in dic");
			float shardValue = ShardMaker.RequiredValueForOneShard(rpd);
			return
					((td.BaseMarketValue < shardValue) 
						&& (td.BaseMarketValue < ShardMaker.forceSingleThreshold) 
						&& (td.BaseMarketValue * size >= shardValue)
					) 
					|| (td.BaseMarketValue * size < shardValue*100); // 1 item not allowed, but stack is OR stack is under 100 shards
		}

		public static ThingFilter ClearFilter(RecipeDef recipe, int count)
		{
			IngredientCount ic = new IngredientCount();
			ic.SetBaseCount(count);
			recipe.ingredients.Clear();
			recipe.ingredients.Add(ic);
			ThingFilter filter = ic.filter;
			recipe.defaultIngredientFilter = filter;
			filter.SetDisallowAll(null, null);
			recipe.fixedIngredientFilter = filter;
			return filter;
		}
		
	}
}
