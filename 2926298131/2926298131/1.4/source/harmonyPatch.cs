using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;
using LostTechnology;
using System.Reflection;
using System.Runtime.ExceptionServices;
using RimWorld.Planet;



//i use a more friendly and safe way to generate techprints by Harmony Patch
//it prefix the generator so the defs are passed to several processes which make they can be store in the stockpile correctly(in stockpile settings either) like the techprints in dlc
//the difference between this patch and the normal patch is that the generator is called in the early of the loading phrase
//so we can't use StaticConstructorOnStartup because it's so late which make the patch useless. Instead directly inherit from the Mod class.
[HarmonyPatch(typeof(ThingDefGenerator_Techprints), "ImpliedTechprintDefs")]
public class ThingDefGenerator_Techprints_ImpliedTechprintDefs_patch
{
    public static void Prefix(out List<ResearchProjectDef> __state)
    {
        //__state contains those projects with a number trailing name
        __state = LostTechnology.lostTechnologyWorld.patchDef();

    }

    public static IEnumerable<ThingDef> Postfix(IEnumerable<ThingDef> defs, List<ResearchProjectDef>__state)
    {
		// for those normal techprints, just yield
        foreach(ThingDef def in defs)
        {
            yield return def;
        }
		// deal with wired named projects
        if (__state.Count > 0)
        {
            foreach(ResearchProjectDef item in __state)
            {
				if (item.baseCost > 4050)
				{
					item.techprintCount = 2;
				}
				else
				{
					item.techprintCount = 1;
				}
				item.techprintMarketValue = item.baseCost * LostTechnologySettings.priceFactor / (float)item.techprintCount;

				// below is copied from the source
				ThingDef thingDef = new ThingDef();
				thingDef.resourceReadoutPriority = ResourceCountPriority.Middle;
				thingDef.category = ThingCategory.Item;
				thingDef.thingClass = typeof(ThingWithComps);
				//thingDef.thingCategories = new List<ThingCategoryDef>();
				//thingDef.thingCategories.Add(ThingCategoryDefOf.Techprints);
				thingDef.graphicData = new GraphicData();
				thingDef.graphicData.graphicClass = typeof(Graphic_Single);
				thingDef.useHitPoints = false;
				thingDef.destroyable = false;
				thingDef.selectable = true;
				thingDef.thingSetMakerTags = new List<string>();
				//thingDef.thingSetMakerTags.Add("Techprint");
				//thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, 100f);
				thingDef.SetStatBaseValue(StatDefOf.Flammability, 0f);
				thingDef.SetStatBaseValue(StatDefOf.MarketValue, item.techprintMarketValue);
				thingDef.SetStatBaseValue(StatDefOf.Mass, 0.03f);
				thingDef.SetStatBaseValue(StatDefOf.SellPriceFactor, 0.1f);
				thingDef.altitudeLayer = AltitudeLayer.Item;
				thingDef.comps.Add(new CompProperties_Forbiddable());
				thingDef.comps.Add(new CompProperties_Techprint
				{
					project = item
				});
				thingDef.tickerType = TickerType.Never;
				thingDef.alwaysHaulable = true;
				thingDef.rotatable = false;
				thingDef.pathCost = 15;
				thingDef.drawGUIOverlay = true;
				thingDef.modContentPack = item.modContentPack;
				thingDef.tradeTags = new List<string>();
				//thingDef.tradeTags.Add("Techprint");
				thingDef.tradeability = Tradeability.None;
				thingDef.description = "TechprintDesc".Translate(NamedArgumentUtility.Named(item, "PROJECT")) + "\n\n" + item.LabelCap + "\n\n" + item.description;
				thingDef.useHitPoints = true;
				if (thingDef.thingCategories == null)
				{
					thingDef.thingCategories = new List<ThingCategoryDef>();
				}
				thingDef.graphicData.texPath = "Things/Item/Special/TechprintUltratech";

				// Replace the trailing number to word to prevent the error
				thingDef.defName = "Techprint_" + System.Text.RegularExpressions.Regex.Replace(item.defName, @"[0-9]+\b", m => NumberToWord(int.Parse(m.Value)));
				thingDef.label = "TechprintLabel".Translate(NamedArgumentUtility.Named(item, "PROJECT"));
				yield return thingDef;
			}
        }


    }
	public static string NumberToWord(int number)
	{
		var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
		string number_string = "";
		bool flag = true;
		while(flag)
        {
			int first = number / 10;
			flag = first != 0;
			if (flag){
				number_string += unitsMap[first];
			}
            else
            {
				number_string += unitsMap[number%10];
			}
			number -= first * 10;
			
        }
		return number_string;
	}

}


[HarmonyPatch(typeof(ThingSetMaker_TraderStock), "Generate")]
public class ThingSetMaker_TraderStock_Generate
{
	public static bool npcGenerated;
	public static void Prefix()
	{
		npcGenerated = true;
	}
	public static void Postfix()
	{
		npcGenerated = false;
	}
}

[HarmonyPatch(typeof(StockGenerator_Techprints), "GenerateThings")]
public class StockGenerator_Techprints_Patch
{
    public static bool Prefix(StockGenerator_Techprints __instance, ref IEnumerable<Thing> __result, int forTile, Faction faction = null)
    {
		if (LostTechnologySettings.disallowNPCfactionFromSellingTechprints && ThingSetMaker_TraderStock_Generate.npcGenerated)
		{
            __result = new List<Thing>();
			return false;
		}
        List<CountChance> myCountChance = new List<CountChance>();
        // the number of vanilla's techproject is 89
        float countmultiplier = Mathf.Clamp(DefDatabase<ResearchProjectDef>.DefCount / 89, 1, 3);
        // slightly add the generation count of techprints when there're more projects
        myCountChance.Add(new CountChance { count = (int)Math.Round(2 * countmultiplier), chance = 0.5f });
        myCountChance.Add(new CountChance { count = (int)Math.Round(3 * countmultiplier), chance = 0.5f });
        // countChances is private, so needs a reflection
        var privateprop = __instance.GetType().GetField("countChances", BindingFlags.NonPublic | BindingFlags.Instance);
        privateprop.SetValue(__instance, myCountChance); // Set the property.
		return true;
    }
}













[HarmonyPatch(typeof(Site), "Destroy")]
internal class Patch_Site_Destroy
{

	[HarmonyPrefix]
	static bool Prefix(Site __instance)
	{
		if(__instance.parts.Find(a => a.def == LostTechnology.Def.SitePartDefOf.Techprint) != null)
        {
			SitePart sp = __instance.parts.Find(a => a.def == LostTechnology.Def.SitePartDefOf.ItemStash);
			sp.things = new ThingOwner<Thing>(sp, false);
			foreach(ThingDef td in dataUtility.GetData(__instance).ar_td)
            {
				sp.things.TryAdd(ThingMaker.MakeThing(td));
			}
			//__instance.SpawnSetup();
			return false;
        }
		return true;
	}

}


