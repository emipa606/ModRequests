using UnityEngine;
using Verse;

namespace Vampire
{
	public class Dialog_Improved_VampireSetup : Window
	{
		private Listing_Standard selectedVampireInfoListing = new Listing_Standard();
		private string header = "Caitiff Settings";


		public override void DoWindowContents(Rect inRect)
		{
			//Title
			Text.Font = GameFont.Medium;
			Widgets.Label(
				new Rect((inRect.width * 0.5f) - (header.GetWidthCached() * 0.5f), 0f, inRect.width, 45f
				), header);
			Text.Font = GameFont.Small;


			//GUI.BeginGroup(inRect);

			Rect radioRect = new Rect(0f, 40f, inRect.width, inRect.height - 36);
			selectedVampireInfoListing.Begin(radioRect);

			if (selectedVampireInfoListing.RadioButton("ROMVC_GameMode_Default".Translate(),
				Improved_VampireSettings.Get.caitiffmode == CaitiffMode.Default, 0f, "ROMVC_GameMode_DefaultDesc".Translate()))
			{
				Improved_VampireSettings.Get.caitiffmode = CaitiffMode.Default;
				VampireSettings.Get.ApplySettings();
			}

			selectedVampireInfoListing.Gap(6f);

			if (selectedVampireInfoListing.RadioButton("ROMV_GameMode_Custom".Translate(),
				Improved_VampireSettings.Get.caitiffmode == CaitiffMode.Custom, 0f, "ROMVC_GameMode_CustomDesc".Translate()))
			{
				Improved_VampireSettings.Get.caitiffmode = CaitiffMode.Custom;
				Improved_VampireSettings.Get.ApplySettings();
			}

			if (Improved_VampireSettings.Get.caitiffmode == CaitiffMode.Custom)
			{

				selectedVampireInfoListing.Gap(24f);
				var caitiffxp = selectedVampireInfoListing.GetRect(22f);
				Improved_VampireSettings.Get.CaitiffXP = //(int) selectedVampireInfoListing.Slider(1f, 4f, 13f);
					(int)Widgets.HorizontalSlider(caitiffxp,
						Improved_VampireSettings.Get.CaitiffXP, 1f, 15f, false,
						"ROMVC_Slider_CaitiffXP".Translate() + ": " + Improved_VampireSettings.Get.CaitiffXP,
						1f.ToString(),
						15f.ToString(), 1f);
				TooltipHandler.TipRegion(caitiffxp, () => "ROMVC_Slider_CaitiffXPTooltip".Translate(), 66666666);
			}

			selectedVampireInfoListing.End();

			//if (selectedVampireInfoListing.)

			Rect acceptButton = new Rect((inRect.width * 0.5f), inRect.height - (38f * 2), 150, 38);
			if (Widgets.ButtonText(acceptButton, "AcceptButton".Translate()))
			{
				Improved_VampireSettings.Get.ApplySettings();
				this.Close(false);
			}

			//GUI.EndGroup();
		}
	}
}
