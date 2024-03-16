using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MSFS_Code {

	[StaticConstructorOnStartup]
	internal static class MSFS_Initializer {
        static MSFS_Initializer() => LongEventHandler.QueueLongEvent(Setup, "LibraryStartup", false, null);
        public static void Setup() {
			ThingDef thingDef;
			thingDef = ThingDef.Named("Steel");
			List<RecipeDef> RecipeDefs = DefDatabase<RecipeDef>.AllDefsListForReading;
			for (int i = 0; i < RecipeDefs.Count; i++) {
				if (RecipeDefs[i].defName == "ExtractMetalFromSlag") {
					RecipeDefs[i].workAmount = (int)Controller.Settings.workAmount;
					RecipeDefs[i].products.Clear();
					RecipeDefs[i].products.Add(new ThingDefCountClass(thingDef, (int)Controller.Settings.steelAmount));
					if (Controller.Settings.component.Equals(true)) {
						RecipeDefs[i].products.Add(new ThingDefCountClass(ThingDefOf.ComponentIndustrial, 1));
					}
					break;
				}
			}
		}
	}

	public class Controller : Mod {
		public static Settings Settings;
		public override string SettingsCategory()
        {
            return "MSFS.Name".Translate();
        }
        public override void DoSettingsWindowContents(Rect canvas)
        {
            Settings.DoWindowContents(canvas);
        }
		public Controller(ModContentPack content) : base(content) {
			Settings = GetSettings<Settings>();
		}
	}

	public class Settings : ModSettings {
		public float steelAmount = 20.0f;
		public float workAmount = 1600.0f;
		public bool component = false;
		public void DoWindowContents(Rect canvas)
        {
			Listing_Standard list = new Listing_Standard();
			list.ColumnWidth = canvas.width;
			list.Begin(canvas);
			list.Gap();
			list.Label("MSFS.SteelAmount".Translate()+"  "+(int)steelAmount);
			steelAmount = list.Slider(steelAmount, 10f, 300.99f);
			Text.Font = GameFont.Tiny;
			list.Label("          "+"MSFS.SteelAmountTip".Translate());
			Text.Font = GameFont.Small;
			list.Gap();
			list.Label("MSFS.WorkAmount".Translate()+"  "+(int)((workAmount/60)+.5));
			workAmount = list.Slider(workAmount, 575f, 2400f);
			Text.Font = GameFont.Tiny;
			list.Label("          "+"MSFS.WorkAmountTip".Translate());
			Text.Font = GameFont.Small;
			list.Gap();
			list.CheckboxLabeled( "MSFS.Component".Translate(), ref component);
			list.Gap();
			list.End();
		}
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look(ref steelAmount, "steelAmount", 20.0f);
			Scribe_Values.Look(ref workAmount, "workAmount", 1600.0f);
			Scribe_Values.Look(ref component, "component", false);
		}
	}

}
