using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace NMeijer.OptionPresets
{
	[HarmonyPatch(typeof(Dialog_Options), nameof(Dialog_Options.DoWindowContents))]
	public static class Patch_Dialog_Options
	{
		#region Constants

		private const float Width = 280f;
		private const float Height = 150f;

		#endregion
		
		#region Private Methods

		private static void Postfix(ref Rect inRect)
		{
			Rect rect = new Rect(inRect.width - Width, inRect.height - Height - 40f, Width, Height);

			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.Begin(rect);

			Text.Font = GameFont.Medium;
			listingStandard.Label("OptionPresetsPresets".Translate());
			Text.Font = GameFont.Small;

			listingStandard.Gap();

			if(listingStandard.ButtonText("OptionPresetsLoadPreset".Translate()))
			{
				ShowFloatMenuOptionsForPresets(OptionPresets.LoadPreset);
			}
			if(listingStandard.ButtonText("OptionPresetsSaveAsPreset".Translate()))
			{
				Find.WindowStack.Add(new Dialog_SaveOptionsPreset());
			}

			listingStandard.Gap(6f);

			if(listingStandard.ButtonText("OptionPresetsDeletePreset".Translate()))
			{
				ShowFloatMenuOptionsForPresets(ConfirmPresetDeletion);
			}
			listingStandard.End();
		}

		private static void ConfirmPresetDeletion(string preset)
		{
			Dialog_MessageBox confirmation = Dialog_MessageBox.CreateConfirmation("OptionPresetsDeleteConfirm".Translate(preset), () => OptionPresets.DeletePreset(preset));
			Find.WindowStack.Add(confirmation);
		}

		private static void ShowFloatMenuOptionsForPresets(Action<string> onClick)
		{
			string[] availablePresets = OptionPresets.GetAvailablePresets();

			List<FloatMenuOption> options = new List<FloatMenuOption>();
			for(int i = 0, length = availablePresets.Length; i < length; i++)
			{
				string preset = availablePresets[i];
				options.Add(new FloatMenuOption(preset, () => onClick(preset)));
			}

			Find.WindowStack.Add(new FloatMenu(options));
		}

		#endregion
	}
}