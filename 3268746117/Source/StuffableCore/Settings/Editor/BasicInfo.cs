using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace StuffableCore.Settings.Editor
{
    public class BasicInfo : BaseModule
    {

        public BasicInfo(StuffableCategorySettings selected) : base(selected) { }

        public override void GetSettings(Listing_Standard listingStandard)
        {
            listingStandard.Label("Basic Informationinator".Colorize(Color.green), tooltip: "Basic info tab.");
            listingStandard.GapLine();
            listingStandard.Label("Currently editing settings for :".Formatted(Selected.SettingsLabel), tooltip: "Currently editing settings for : {0}".Formatted(Selected.SettingsLabel.Colorize(Color.green)));
            listingStandard.Label("{0}".Formatted(Selected.SettingsLabel).Colorize(Color.green), tooltip: "Tooltip? No this is Patrick.");
            listingStandard.GapLine();
            Selected.GetSettingsHeaderEnabled(listingStandard);
            Selected.GetSettingsHeaderAltSearchEnabled(listingStandard);
            listingStandard.GapLine();
            Selected.UseDefaultCostInsteadCheckbox(listingStandard);
            Selected.GetDefaultCostSlider(listingStandard);
            listingStandard.GapLine();
            Selected.GetAdditionalStuffMultiplierArmorSlider(listingStandard);
        }
    }
}


