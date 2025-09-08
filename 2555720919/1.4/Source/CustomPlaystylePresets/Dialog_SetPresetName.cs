using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CustomPlaystylePresets
{
	public class Dialog_SetPresetName : Dialog_Rename
	{
		private PresetStorage presetStorage;
		public Dialog_SetPresetName(PresetStorage presetStorage)
		{
			this.presetStorage = presetStorage;
			curName = presetStorage.name;
		}

		public override AcceptanceReport NameIsValid(string name)
		{
			AcceptanceReport result = base.NameIsValid(name);
			if (!result.Accepted)
			{
				return result;
			}
			if (CustomPlaystylePresetsMod.settings.presets?.Any((PresetStorage a) => a != presetStorage && a.name == name) ?? false)
			{
				return "NameIsInUse".Translate();
			}
			return true;
		}
        public override void SetName(string name)
		{
			presetStorage.name = name;
			CustomPlaystylePresetsMod.settings.SetDefaultPreset(presetStorage);
			var customDifficultyDef = CustomPlaystylePresetsMod.CreateNewDifficultyDef(presetStorage);
			DefDatabase<DifficultyDef>.Add(customDifficultyDef);
			CustomPlaystylePresetsMod.settings.AddNewDifficulty(presetStorage);
			DrawStorytellerSelectionInterface_Patch.customStorage = presetStorage;
			DrawStorytellerSelectionInterface_Patch.customDifficultyDef = customDifficultyDef;

		}
	}
}