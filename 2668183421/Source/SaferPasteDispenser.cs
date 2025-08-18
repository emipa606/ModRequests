using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SaferPasteDispenser
{
	public class SaferPasteDispenser : Mod
	{
		public SaferPasteDispenser(ModContentPack pack) : base(pack)
		{
			GetSettings<Settings>();

			var harmony = new Harmony("denev.SaferPasteDispenser");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			HarmonyPatches.TakeMealFromDispenser_Patch.Patch(harmony);
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Settings.DoSettingsWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "SaferPasteDispenser.Title".Translate();
		}
	}
}
