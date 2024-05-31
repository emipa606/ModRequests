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

namespace ProximityFuze {

	public class ProximityFuzeMod : Mod {

		public static ProximityFuzeSettings settings;

		private static Dictionary<RecipeDef, SkillDef> defaultSkills = new Dictionary<RecipeDef, SkillDef>();

		private static List<ThingDef> allDefs;

		private const float gap = 8;
		private static float maxSize, textHeight;
		private Vector2 scrollPos = Vector2.zero;

		public ProximityFuzeMod(ModContentPack content) : base(content) {
			settings = GetSettings<ProximityFuzeSettings>();
			LongEventHandler.ExecuteWhenFinished(() => {
				allDefs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.thingClass == typeof(Building_TrapExplosiveProximityFuze));
				foreach(var def in allDefs) {
					var size = Text.CalcSize(def.defName);
					textHeight = size.y + gap;
					maxSize = Mathf.Max(size.x, maxSize);
					if(!ProximityFuzeSettings.radiusFactors.ContainsKey(def.defName)) ProximityFuzeSettings.radiusFactors[def.defName] = ProximityFuzeSettings.defaultFactor;
				}
			});
		}

		private static FieldInfo field__Building_Trap__autoRearm = AccessTools.DeclaredField(typeof(Building_Trap), "autoRearm");

		public override void DoSettingsWindowContents(Rect canvas) {
			GUI.BeginGroup(canvas);
			var size = Text.CalcSize("ProximityFuze.RadiusFactor".Translate());
			var rect = new Rect(0, 0, size.x, size.y);
			Widgets.Label(rect, "ProximityFuze.RadiusFactor".Translate());
			TooltipHandler.TipRegion(rect, "ProximityFuze.RadiusFactor.Desc".Translate());
			rect = new Rect(rect.x + rect.width + 30, rect.y, 200, rect.height);
			ProximityFuzeSettings.defaultFactor = Widgets.HorizontalSlider(rect, ProximityFuzeSettings.defaultFactor, 0, 2, true);
			rect = new Rect(rect.x + rect.width + 30, rect.y, 100, rect.height);
			Widgets.Label(rect, ProximityFuzeSettings.defaultFactor.ToString());
			rect = new Rect(rect.x + rect.width + 30, rect.y, 150, rect.height);
			if(Widgets.ButtonText(rect, "ProximityFuze.SetToAll".Translate())) {
				Dictionary<string, float> temp = new Dictionary<string, float>();
				foreach(var rf in ProximityFuzeSettings.radiusFactors) {
					temp[rf.Key] = ProximityFuzeSettings.defaultFactor;
					if(Current.ProgramState == ProgramState.Playing)
						foreach(var map in Current.Game.Maps)
							foreach(var thing in map.spawnedThings) {
								var thing2 = thing as Building_TrapExplosiveProximityFuze;
								if(thing2 != null) thing2.radiusFactor = ProximityFuzeSettings.defaultFactor;
							}
				}
				ProximityFuzeSettings.radiusFactors = temp;
			}
			if(Current.ProgramState == ProgramState.Playing) {
				rect = new Rect(rect.x + rect.width + 30, rect.y, 150, rect.height);
				TooltipHandler.TipRegion(rect, "ProximityFuze.Register.Desc".Translate());
				if(Widgets.ButtonText(rect, "ProximityFuze.Register".Translate())) {
					foreach(var map in Current.Game.Maps)
						foreach(var thing in map.spawnedThings)
							if(thing.GetType() == typeof(Building_TrapExplosive)) {
								var posi = thing.Position;
								var mapi = thing.Map;
								var autoRearm = field__Building_Trap__autoRearm.GetValue(thing);
								var thing2 = ThingMaker.MakeThing(thing.def) as Building_TrapExplosiveProximityFuze;
								field__Building_Trap__autoRearm.SetValue(thing2, autoRearm);
								thing2.radiusFactor = ProximityFuzeSettings.radiusFactors[thing.def.defName];
								thing2.SetFactionDirect(thing.Faction);
								thing.Destroy();
								//thing2.SpawnSetup(thing.Map, false);
								GenPlace.TryPlaceThing(thing2, posi, mapi, ThingPlaceMode.Direct);
							} else if(thing.GetType() == typeof(Building_TrapExplosiveProximityFuze))
								(thing as Building_TrapExplosiveProximityFuze).radiusFactor = ProximityFuzeSettings.radiusFactors[thing.def.defName];
				}
			}
			var pos = new Vector2(0, 0);
			rect = new Rect(0, rect.y + 30f, canvas.width, canvas.height - rect.y - 30f);
			var viewRect = new Rect(0, 0, canvas.width - 30, (textHeight + gap) * allDefs.Count);
			if(Current.ProgramState == ProgramState.Playing) viewRect.height += textHeight + gap;
			Widgets.BeginScrollView(rect, ref scrollPos, viewRect);
			foreach(var def in allDefs) {
				if(pos.y + gap + textHeight < scrollPos.y || pos.y > scrollPos.y + rect.height + gap + textHeight) break;
				pos.y += DrawRow(pos, def) + gap;
			}
			if(Current.ProgramState == ProgramState.Playing) {
				rect = new Rect(0, pos.y, 150, textHeight);
				if(Widgets.ButtonText(rect, "ProximityFuze.Remove".Translate())) {
					foreach(var map in Current.Game.Maps)
						foreach(var thing in map.spawnedThings)
							if(thing.GetType() == typeof(Building_TrapExplosiveProximityFuze)) {
								var posi = thing.Position;
								var mapi = thing.Map;
								var autoRearm = field__Building_Trap__autoRearm.GetValue(thing);
								thing.def.thingClass = typeof(Building_TrapExplosive);
								var thing2 = ThingMaker.MakeThing(thing.def) as Building_TrapExplosive;
								field__Building_Trap__autoRearm.SetValue(thing2, autoRearm);
								thing2.SetFactionDirect(thing.Faction);
								thing.Destroy();
								//thing2.SpawnSetup(thing.Map, false);
								GenPlace.TryPlaceThing(thing2, posi, mapi, ThingPlaceMode.Direct);
							}
				}
			}
			Widgets.EndScrollView();
			GUI.EndGroup();
		}

		private float DrawRow(Vector2 pos, ThingDef def) {
			var rect = new Rect(pos.x, pos.y, maxSize, textHeight);
			Widgets.Label(rect, def.LabelCap);
			rect = new Rect(rect.x + maxSize + 30f, rect.y, 200f, rect.height);
			ProximityFuzeSettings.radiusFactors[def.defName] = Widgets.HorizontalSlider(rect, ProximityFuzeSettings.radiusFactors[def.defName], 0, 2, true);
			rect = new Rect(rect.x + rect.width + 30f, rect.y, 500f, rect.height);
			Widgets.Label(rect, ProximityFuzeSettings.radiusFactors[def.defName].ToString());
			return rect.height;
		}

		public override string SettingsCategory() {
			return "ProximityFuze.Setting".Translate();
		}
	}

}
