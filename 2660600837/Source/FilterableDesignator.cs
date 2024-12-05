using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace FilterableDesignator
{
	class FilterableDesignator : Mod
	{
//		static public ModContentPack currentMod;
//		static public Mod currentModInst;

		public FilterableDesignator(ModContentPack pack) : base(pack)
		{
//			GetSettings<Settings>();
//			currentMod = pack;
//			currentModInst = this;

			var harmony = new Harmony("denev.FilterableDesignator");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

/*
		public override void DoSettingsWindowContents(Rect inRect)
		{
			Settings.DoSettingsWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "FilterableDesignator.Title".Translate();
		}
*/
	}
}
