using HarmonyLib;
using RimWorld;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace LogAfterDefError {

	public class LogAfterDefErrorMod : Mod {

		public LogAfterDefErrorMod(ModContentPack content) : base(content) {
			GetSettings<LogAfterDefErrorModSettings>();
			RuntimeHelpers.RunClassConstructor(typeof(HarmonyPatches).TypeHandle);
		}

		public override void DoSettingsWindowContents(Rect inRect) {
			var listing = new Listing_Standard();
			listing.Begin(inRect);
			listing.CheckboxLabeled("LogAfterDefError.Settings.Enabled".Translate(), ref LogAfterDefErrorModSettings.enabled);
			listing.End();
		}

		public override string SettingsCategory() {
			return "LogAfterDefError.Settings".Translate();
		}
	}

	public class LogAfterDefErrorModSettings : ModSettings {

		public static bool enabled = true;

		public override void ExposeData() {
			Scribe_Values.Look(ref enabled, "enabled", true);
		}

	}

}
