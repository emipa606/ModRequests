using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Renamon
{
	[StaticConstructorOnStartup]
	public static class ModCompatibility
	{
		public static Color GetSkinColorFirst(Pawn pawn)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				return alienComp.GetChannel("skin").first;
			}
			else
			{
				return Color.white;
			}
		}

		public static Color GetSkinColorSecond(Pawn pawn)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				return alienComp.GetChannel("skin").second;
			}
			else
			{
				return Color.white;
			}
		}
		public static void SetSkinColorFirst(Pawn pawn, Color color)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.OverwriteColorChannel("skin", color, null);
			}
		}
		public static void SetSkinColorSecond(Pawn pawn, Color color)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.OverwriteColorChannel("skin", null, color);
			}
		}


		public static Color GetHairColorFirst(Pawn pawn)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				return alienComp.GetChannel("hair").first;
			}
			else
			{
				return Color.white;
			}
		}

		public static Color GetHairColorSecond(Pawn pawn)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				return alienComp.GetChannel("hair").second;
			}
			else
			{
				return Color.white;
			}
		}
		public static void SetHairColorFirst(Pawn pawn, Color color)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.OverwriteColorChannel("hair", color, null);
				pawn.story.hairColor = color;
			}
		}
		public static void SetHairColorSecond(Pawn pawn, Color color)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.OverwriteColorChannel("hair", null, color);
			}
		}
		public static void SetAlienHead(Pawn pawn, string head)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.crownType = head;
			}
		}

		public static string GetAlienHead(Pawn pawn)
		{
			string sRet = "(unknown)";
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				sRet = alienComp.crownType;
			}
			return sRet;
		}
		public static List<string> GetAlienHeadPaths(Pawn pawn)
        {
			var alienDef = pawn.def as AlienRace.ThingDef_AlienRace;
			return alienDef.alienRace.generalSettings.alienPartGenerator.aliencrowntypes;
		}

		public static void CopyBodyAddons(Pawn source, Pawn to)
		{
			var sourceComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(source);
			if (sourceComp != null && sourceComp.addonGraphics != null && sourceComp.addonVariants != null)
            {
				var toComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(to);
				if (toComp != null)
                {
					toComp.addonGraphics = sourceComp.addonGraphics.ListFullCopy();
					toComp.addonVariants = sourceComp.addonVariants.ListFullCopy();
                }
			}
		}
	}
}
