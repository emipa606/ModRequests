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
		internal const string Modname_VFEM = "Vanilla Factions Expanded - Mechanoids";
		internal static Type type_Building_IndoctrinationPod = AccessTools.TypeByName("VFEMech.Building_IndoctrinationPod");
		internal static FieldInfo field_ideoConversionTarget = AccessTools.Field(type_Building_IndoctrinationPod, "ideoConversionTarget");

		public IntentionalProselytismMod(ModContentPack content) : base(content) {
			settings = GetSettings<IntentionalProselytismSettings>();
				LongEventHandler.ExecuteWhenFinished(() => {
			});
		}

		public override void DoSettingsWindowContents(Rect canvas) {
			var label = TranslationKeys.CertaintyReduceFactor.Translate();
			var labelSize = Text.CalcSize(label);
			var rect = new Rect(canvas.x, canvas.y, labelSize.x + 10f, Text.LineHeight);
			IntentionalProselytismSettings.certaintyReduceFactor = Widgets.HorizontalSlider_NewTemp(rect, IntentionalProselytismSettings.certaintyReduceFactor, 0.05f, 0.5f, true, label);
			TooltipHandler.TipRegion(rect, TranslationKeys.CertaintyReduceFactor__Desc.Translate());
			rect = new Rect(canvas.x + 220, canvas.y, 50, Text.LineHeight);
			Widgets.Label(rect, IntentionalProselytismSettings.certaintyReduceFactor.ToString());
			rect = new Rect(canvas.x, canvas.y + Text.LineHeight, 100, Text.LineHeight);
			if(Widgets.ButtonText(rect, TranslationKeys.ResetToDefault.Translate()))
				IntentionalProselytismSettings.certaintyReduceFactor = 0.2f;
			label = TranslationKeys.DisableInterColonistProselytizing.Translate();
			rect = new Rect(canvas.x, rect.y + Text.LineHeight, Text.CalcSize(label).x + Text.LineHeight + 10f, Text.LineHeight);
			Widgets.CheckboxLabeled(rect, label, ref IntentionalProselytismSettings.disableInterColonistProselytizing);
			TooltipHandler.TipRegion(rect, TranslationKeys.DisableInterColonistProselytizing__Desc.Translate());
			if(ModLister.HasActiveModWithName(Modname_VFEM)) {
				label = TranslationKeys.UnlockVFEMIndoctrinationPod.Translate();
				rect = new Rect(canvas.x, rect.y + Text.LineHeight, Text.CalcSize(label).x + Text.LineHeight + 10f, Text.LineHeight);
				Widgets.CheckboxLabeled(rect, label, ref IntentionalProselytismSettings.unlockVFEMIndoctrinationPod);
				TooltipHandler.TipRegion(rect, TranslationKeys.UnlockVFEMIndoctrinationPod__Desc.Translate());
				if(!IntentionalProselytismSettings.unlockVFEMIndoctrinationPod && Current.ProgramState == ProgramState.Playing) {
					Find.Maps.ForEach(x => x.spawnedThings.Where(thing => type_Building_IndoctrinationPod.IsAssignableFrom(thing.def.thingClass)).ToList().ForEach(y => field_ideoConversionTarget.SetValue(y, Faction.OfPlayer.ideos.PrimaryIdeo)));
				}
			}
		}

		public override string SettingsCategory() {
			return TranslationKeys.Setting.Translate();
		}

	}

	[HarmonyPatch(typeof(AbilityUtility), nameof(AbilityUtility.ValidateNotSameIdeo))]
	internal static class RimWorld__AbilityUtility__ValidateNotSameIdeo {
		internal static void Prefix(Pawn targetPawn, ref bool showMessages) {
			if(DataStorage.GetIdeoStatic(targetPawn, targetPawn) != targetPawn.Ideo)
				showMessages = false;
		}
	}

}
