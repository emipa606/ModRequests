using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace DTechprinting
{
    public static class ResearchProjectHelper
    {
		public static Dictionary<ResearchProjectDef, ResearchProjectDef> oldNewMap = new Dictionary<ResearchProjectDef, ResearchProjectDef>();
		public static List<ResearchProjectDef> added = new List<ResearchProjectDef>();
		public static Dictionary<string, int> shardCostAssignment = new Dictionary<string, int>();

		public static Dictionary<ThingDef, ResearchProjectDef> allThingDic = null;
		public static Dictionary<ResearchProjectDef, List<ThingDef>> allResearchDic = null;

		public static void AssociateAll()
        {
			allThingDic = new Dictionary<ThingDef, ResearchProjectDef>();
			allResearchDic = new Dictionary<ResearchProjectDef, List<ThingDef>>();

			foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefsListForReading)
			{
				ResearchProjectDef rpd = ThingDefHelper.GetBestRPDForRecipe(recipe, true);
				if (rpd != null && recipe.ProducedThingDef != null)
				{
					ThingDef producedThing = recipe.ProducedThingDef;
					allThingDic.SetOrAdd(producedThing, rpd);

					List<ThingDef> things;
					if (allResearchDic.TryGetValue(rpd, out things))
						things.Add(producedThing);
					else
						allResearchDic.Add(rpd, new List<ThingDef> { producedThing });
				}
			}

			if (TechprintingSettings.shardBuildings)
			{
				foreach (ThingDef building in DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Building || x.building != null))
				{
					if (allThingDic.ContainsKey(building))
						continue;
					ResearchProjectDef rpd = ThingDefHelper.GetBestRPDForBuilding(building, true);
					if (rpd != null)
					{
						allThingDic.SetOrAdd(building, rpd);

						List<ThingDef> things;
						if (allResearchDic.TryGetValue(rpd, out things))
							things.Add(building);
						else
							allResearchDic.Add(rpd, new List<ThingDef> { building });
					}
				}
			}
			GearAssigner.HardAssign(ref allThingDic, ref allResearchDic);
			GearAssigner.OverrideAssign(ref allThingDic, ref allResearchDic);
		}

		public static bool ProjectUnlocksShardable(ResearchProjectDef rpd)
		{
			if (allResearchDic == null)
				AssociateAll();
			return allResearchDic.ContainsKey(rpd);
		}

		public static bool ProjectHasWeaponApparel(ResearchProjectDef rpd)
		{
			if (allResearchDic == null)
				AssociateAll();
			List<ThingDef> unlocks;
			if (!allResearchDic.TryGetValue(rpd, out unlocks))
				return false;

			foreach(ThingDef td in unlocks)
            {
				if (td.IsApparel || td.IsWeapon)
					return true;
            }
			return false;
		}

			/*
			public static bool ProjectUnlocksShardable(ResearchProjectDef rpd)
			{
				if (GearAssigner.HasHardAssignment(rpd) || GearAssigner.HasOverrideAssignment(rpd))
					return true;
				foreach(RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs)
				{
					ResearchProjectDef best = ThingDefHelper.GetBestRPDForRecipe(recipe, true);
					if (best == rpd)
						return true;	
				}
				return false;
			}

			public static bool ProjectHasWeaponApparel(ResearchProjectDef rpd)
			{
				if (GearAssigner.HasHardAssignment(rpd) || GearAssigner.HasOverrideAssignment(rpd))
					return true;
				foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs)
				{
					if (recipe.ProducedThingDef == null || (!recipe.ProducedThingDef.IsApparel && !recipe.ProducedThingDef.IsWeapon))
					{
						continue;
					}
					ResearchProjectDef best = ThingDefHelper.GetBestRPDForRecipe(recipe, true);
					if (best == rpd)
						return true;
				}
				return false;
			}*/

			public static bool InTechRange(ResearchProjectDef rpd)
		{
			return rpd.techLevel >= (TechLevel)Mathf.RoundToInt(TechprintingSettings.techLevelToAddPrints);
		}

		public static void SetTechprintCost(ResearchProjectDef rpd, int cost)
        {
			rpd.techprintCount = cost;
			if (rpd.heldByFactionCategoryTags.NullOrEmpty())
			{
				rpd.heldByFactionCategoryTags = new List<string> { "None" };
			}
		}

		public static void SetHardTechprintReqs()
        {
			List<ResearchProjectDef> newAdd = new List<ResearchProjectDef>();
			foreach (string rpdString in shardCostAssignment.Keys)
			{

				ResearchProjectDef rpd = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(rpdString);
				if (rpd != null)
				{
					newAdd.Add(rpd);
					SetTechprintCost(rpd, shardCostAssignment[rpdString]);
				}
			}
			if (!newAdd.NullOrEmpty())
			{
				Log.Message("DTechprinting: Set shard requirements from SetShardCost for " + newAdd.ToStringSafeEnumerable());
				ResearchProjectHelper.added.AddRange(newAdd);
			}
		}

		public static void SetTechprintRequirements()
		{
			AssociateAll();
			if (!TechprintingSettings.addTechprintRequirements)
				return;
			List<ResearchProjectDef> newAdd = new List<ResearchProjectDef>();
			foreach (ResearchProjectDef rpd in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				if (ProjectUnlocksShardable(rpd) && rpd.techprintCount == 0 && TechprintingSettings.addTechprintRequirements && InTechRange(rpd)) 
				{
					if (TechprintingSettings.weaponsApparelOnly && !ProjectHasWeaponApparel(rpd))
						continue;
					newAdd.Add(rpd);
					SetTechprintCost(rpd, TechprintingSettings.numShardsToAdd);
				}
			}
			if (!newAdd.NullOrEmpty())
			{
				Log.Message("DTechprinting: Added shard requirements to " + newAdd.ToStringSafeEnumerable());
				ResearchProjectHelper.added.AddRange(newAdd);
			}
		}

		public static ResearchProjectDef GenerateProject(ResearchProjectDef prereq)
		{
			ResearchProjectDef newProject = new ResearchProjectDef();
			newProject.defName = prereq.defName + "Recipes";
			newProject.label = prereq.label + " recipes";
			newProject.description = "Recipes for " + prereq.LabelCap + ", unknowable without techshards or techprints.";
			newProject.modContentPack = prereq.modContentPack;
			newProject.generated = true;
			newProject.baseCost = 50f;
			newProject.prerequisites = new List<ResearchProjectDef> { prereq };
			newProject.techLevel = prereq.techLevel;
			newProject.requiredResearchBuilding = prereq.requiredResearchBuilding;
			if ((prereq.requiredResearchFacilities) != null)
				newProject.requiredResearchFacilities = new List<ThingDef> (prereq.requiredResearchFacilities);
			if ((prereq.tags) != null)
				newProject.tags = new List<ResearchProjectTagDef>(prereq.tags);
			newProject.tab = prereq.tab;
			newProject.researchViewX = prereq.researchViewX + 0.5f;
			newProject.researchViewY = prereq.researchViewY + 0.5f;
			newProject.techprintCount = prereq.techprintCount;
			newProject.techprintCommonality = prereq.techprintCommonality;
			newProject.techprintMarketValue = prereq.techprintMarketValue;
			if ((prereq.heldByFactionCategoryTags) != null)
				newProject.heldByFactionCategoryTags = new List<string>(prereq.heldByFactionCategoryTags);
			//var giveShortHash = AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash");
			//giveShortHash.Invoke(null, new object[] { newProject, typeof(ResearchProjectDef) });
			return newProject;
		}

		public static void SwapRecipe(RecipeDef recipe, ResearchProjectDef oldProj, ResearchProjectDef newProj)
		{
			recipe.researchPrerequisite = newProj;
			if (recipe.products == null)
				return;
			foreach (ThingDefCountClass product in recipe.products)
			{
				if (product.thingDef.recipeMaker != null)
					product.thingDef.recipeMaker.researchPrerequisite = newProj;
			}
		}

		public static bool ResearchProjectIsBestPrereqForRecipe(RecipeDef recipe, ResearchProjectDef proj)
		{
			/*
			if (recipe.ProducedThingDef == null)
			{
				return false;
			}
			ResearchProjectDef best = ThingDefHelper.GetBestResearchProject(recipe.ProducedThingDef);
			*/
			ResearchProjectDef best = ThingDefHelper.GetBestRPDForRecipe(recipe, getLowestTech: true);
			return (best == proj);
		}

		public static void SwapPrintables(ResearchProjectDef oldProj, ResearchProjectDef newProj)
		{
			foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs)
			{
				if (ResearchProjectIsBestPrereqForRecipe(recipe, oldProj))
				{
					SwapRecipe(recipe, oldProj, newProj);
					continue;
				}
			}
		}

		public static ResearchProjectDef DoSplit(ResearchProjectDef rpd)
		{
			ResearchProjectDef newProj = GenerateProject(rpd);
			SwapPrintables(rpd, newProj);
			rpd.techprintCount = 0;
			rpd.heldByFactionCategoryTags = null;
			return newProj;
		}

		public static void SplitProject(ResearchProjectDef rpd)
		{
			ResearchProjectDef newProject = DoSplit(rpd);
			oldNewMap.Add(rpd, newProject);
			SplitChain(rpd); // recursion is fun
		}

		public static void SplitChain(ResearchProjectDef rpd)
		{
			if (rpd.techprintCount > 0 || rpd.prerequisites == null)
				return;
			foreach (ResearchProjectDef prereq in rpd.prerequisites)
			{
				if (prereq.techprintCount > 0)
					SplitProject(prereq);
			}
		}

		public static void ResolveSplits()
		{
			if (ResearchProjectHelper.oldNewMap.Count == 0)
				return;

			var cache = AccessTools.Field(typeof(ResearchProjectDef), "cachedUnlockedDefs");

			foreach (ResearchProjectDef rpd in ResearchProjectHelper.oldNewMap.Keys)
			{
				ResearchProjectDef toResolve = oldNewMap[rpd];
				DefGenerator.AddImpliedDef<ResearchProjectDef>(toResolve);

				cache.SetValue(rpd, null);
			}
			ResearchProjectDef.GenerateNonOverlappingCoordinates();
		}

		public static void SplitAllProjects()
		{
			if (oldNewMap == null)
				oldNewMap = new Dictionary<ResearchProjectDef, ResearchProjectDef>();
			foreach (ResearchProjectDef rpd in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				SplitChain(rpd);
			}
			ResolveSplits();
			UpdateTechprints(ResearchProjectHelper.oldNewMap);
			Log.Message("DTechprinting: Created new research projects " + oldNewMap.Values.ToStringSafeEnumerable());
		}

		public static void UpdateTechprints(Dictionary<ResearchProjectDef, ResearchProjectDef> oldNewMap)
		{
			foreach (ThingDef techprint in ThingCategoryDefOf.Techprints.childThingDefs)
			{
				CompProperties_Techprint comp = techprint.GetCompProperties<CompProperties_Techprint>();
				ResearchProjectDef oldProj = comp.project;
				ResearchProjectDef newProj;
				if (oldNewMap.TryGetValue(oldProj, out newProj))
				{
					comp.project = newProj;
				}
			}
		}

	}
}
