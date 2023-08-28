using UnityEngine;
using Verse;

namespace NMeijer.OptionPresets
{
	public class Dialog_SaveOptionsPreset : Window
	{
		#region Variables

		private string _presetName;

		#endregion

		#region Properties

		public override Vector2 InitialSize => new Vector2(400f, 115f);

		#endregion

		public Dialog_SaveOptionsPreset()
		{
			absorbInputAroundWindow = true;
			doCloseX = true;
		}

		#region Public Methods

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.Begin(inRect);
			listingStandard.Label("OptionPresetsSaveAs".Translate());
			_presetName = listingStandard.TextEntry(_presetName);
			if(listingStandard.ButtonText("SaveGameButton".Translate()))
			{
				if(OptionPresets.PresetExists(_presetName))
				{
					Dialog_MessageBox confirmation = Dialog_MessageBox.CreateConfirmation("OptionPresetsOverrideConfirm".Translate(_presetName), SavePreset);
					Find.WindowStack.Add(confirmation);
				}
				else
				{
					SavePreset();
				}
			}
			listingStandard.End();
		}

		#endregion

		#region Private Methods

		private void SavePreset()
		{
			OptionPresets.SaveCurrentActivePreset(_presetName);
			Close();
		}

		#endregion
	}
}