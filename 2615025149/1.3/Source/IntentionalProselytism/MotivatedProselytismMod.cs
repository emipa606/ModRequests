using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RimWorld;
using HarmonyLib;
using Verse;
using UnityEngine;

namespace IntentionalProselytism {

	public class IntentionalProselytismMod : Mod {

		public static IntentionalProselytismSettings settings;
		internal static DataStorage _datastorage;

		public IntentionalProselytismMod(ModContentPack content) : base(content) {
			settings = GetSettings<IntentionalProselytismSettings>();
			LongEventHandler.ExecuteWhenFinished(() => {
			});
		}

		public override void DoSettingsWindowContents(Rect canvas) {
			var rect = new Rect(canvas.x, canvas.y, 200, Text.LineHeight);
			IntentionalProselytismSettings.certaintyReduceFactor = Widgets.HorizontalSlider(rect, IntentionalProselytismSettings.certaintyReduceFactor, 0.05f, 0.5f, true, "IntentionalProselytism.CertaintyReduceFactor.Desc");
			rect = new Rect(canvas.x + 220, canvas.y, 50, Text.LineHeight);
			Widgets.Label(rect, IntentionalProselytismSettings.certaintyReduceFactor.ToString());
			rect = new Rect(canvas.x, canvas.y + Text.LineHeight, 100, Text.LineHeight);
			if(Widgets.ButtonText(rect, "IntentionalProselytism.ResetToDefault".Translate()))
				IntentionalProselytismSettings.certaintyReduceFactor = 0.2f;
		}

		public override string SettingsCategory() {
			return TranslationKeys.Setting.Translate();
		}

	}

}
