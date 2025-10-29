using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ResumableJobProgress
{
	public class ResumableJobProgress : Mod
	{
		public ResumableJobProgress(ModContentPack pack) : base(pack)
		{
			GetSettings<Settings>();

			var harmony = new Harmony("denev.ResumableJobProgress");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Settings.DoSettingsWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "ResumableJobProgress.Title".Translate();
		}
	}
}
