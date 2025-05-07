using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace DTechprinting
{
	public static class ThingDefGenerator_Techshards
	{


		public static IEnumerable<ThingDef> ImpliedTechshardDefs()
		{
			ResearchProjectHelper.AssociateAll();
			foreach (ResearchProjectDef researchProjectDef in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
			{
				bool unlocks = ResearchProjectHelper.ProjectUnlocksShardable(researchProjectDef);
				if (ShardMaker.ShardForProject(researchProjectDef) == null && (researchProjectDef.techprintCount > 0 || unlocks))
				{
					ThingDef thingDef = new ThingDef();
					thingDef.category = ThingCategory.Item;
					thingDef.thingClass = typeof(ThingWithComps);
					thingDef.thingCategories = new List<ThingCategoryDef>();
					thingDef.thingCategories.Add(Base.DefOf.DTechshards);
					thingDef.useHitPoints = true;
					thingDef.selectable = true;
					thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, 100f);
					thingDef.SetStatBaseValue(StatDefOf.Flammability, 1f);
					thingDef.SetStatBaseValue(StatDefOf.MarketValue, 0.01f);
					thingDef.SetStatBaseValue(StatDefOf.Mass, 0.01f);
					thingDef.SetStatBaseValue(StatDefOf.SellPriceFactor, 1f);
					thingDef.altitudeLayer = AltitudeLayer.Item;
					thingDef.comps.Add(new CompProperties_Forbiddable());
					thingDef.comps.Add(new CompProperties_Techshard
					{
						project = researchProjectDef
					});
					thingDef.tickerType = TickerType.Never;
					thingDef.alwaysHaulable = true;
					thingDef.rotatable = false;
					thingDef.pathCost = 15;
					thingDef.drawGUIOverlay = true;
					thingDef.stackLimit = 100;
					thingDef.modContentPack = researchProjectDef.modContentPack;
					thingDef.description = "TechshardDesc".Translate(researchProjectDef.Named("PROJECT")) + "\n\n" + researchProjectDef.LabelCap + "\n\n" + researchProjectDef.description;
					thingDef.graphicData = new GraphicData();
					thingDef.graphicData.texPath = "Things/Item/Special/Techshard";
					thingDef.graphicData.graphicClass = typeof(Graphic_StackCount);
					thingDef.defName = "Techshard_" + researchProjectDef.defName;
					thingDef.label = "TechshardLabel".Translate(researchProjectDef.Named("PROJECT"));
					thingDef.smallVolume = true;
					thingDef.resourceReadoutPriority = ResourceCountPriority.Middle;
					Base.DefOf.DTechshards.childThingDefs.Add(thingDef);

					var giveShortHash = AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash");
					giveShortHash.Invoke(null, new object[] { thingDef, typeof(ThingDef) });
					yield return thingDef;
				}
			}
			yield break;
		}

		public const string Tag = "Techshard";
	}
}
