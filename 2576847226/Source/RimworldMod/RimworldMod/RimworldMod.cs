using HugsLib;
using System;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;
using Harmony;

namespace RimworldMod
{
	public class OreMod : ModBase
	{
		public override string ModIdentifier
		{
			get
			{
				return "ColorDeepOres";
			}
		}
	}

	[HarmonyPatch(typeof(DeepResourceGrid), "GetCellExtraColor", null)]
	public static class ColorTheDeepOres
	{
		[HarmonyPostfix]
		public static void GimmeColors(ref Color __result, int index, DeepResourceGrid __instance)
		{
			IntVec3 c = ((Map)typeof(DeepResourceGrid).GetField("map", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)).cellIndices.IndexToCell(index);
			ThingDef theOre = __instance.ThingDefAt(c);
			if (theOre == ThingDefOf.Plasteel)
			{
				__result = Color.cyan;
			}
			else if (theOre == ThingDefOf.Steel)
			{
				__result = Color.gray;
			}
			else if (theOre == ThingDefOf.Gold)
			{
				__result = Color.yellow;
			}
			else if (theOre == ThingDefOf.Silver)
			{
				__result = Color.white;
			}
			else if (theOre == ThingDef.Named("Jade"))
			{
				__result = Color.green;
			}
			else if (theOre == ThingDefOf.Chemfuel)
			{
				__result = Color.red;
			}
			else if (theOre == ThingDefOf.Uranium)
			{
				__result = Color.magenta;
			}
			else
			{
				__result = Color.black;
			}
			__result.a = 0.25f;
		}
	}

	//Temporary fix
	[HarmonyPatch(typeof(StockGenerator_Animals),"HandlesThingDef")]
	public static class unfuckTheMerchants
	{
		[HarmonyPrefix]
		public static bool stopIt()
		{
			return false;
		}

		[HarmonyPostfix]
		public static void fuck(ref bool __result)
		{
			__result = true;
		}
	}
}