using System;
using UnityEngine;
using Verse;

namespace dragonagemetals
{
	public class DAMetalMod : Mod
	{
		public override string SettingsCategory()
		{
			return "Dragon Age Metals".Flatten();
		}

		public override void DoSettingsWindowContents(Rect canvas)
		{
			DAMetalMod.Settings.DoWindowContents(canvas);
		}

		public DAMetalMod(ModContentPack content) : base(content)
		{
			DAMetalMod.Settings = base.GetSettings<DragonAgeMetalsSettings>();
		}

		public static DragonAgeMetalsSettings Settings;
	}
}
