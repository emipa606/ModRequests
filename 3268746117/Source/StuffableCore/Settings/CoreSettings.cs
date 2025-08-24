using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.Settings
{

    public class CoreSettings : BaseSettings, ISettings, IExposable
    {

        public bool modEnabled = true;
        public bool toggleAll = false;
        public bool toggleAllOff = false;
        public bool resetToDefaults = false;
        public bool enableRawFoodStuffs = false;

        public bool clearCache = false;

        public bool needsUpdate = false;

        public void GetSettings(Listing_Standard listing_Standard)
        {
            listing_Standard.Label("Welcome!");
            listing_Standard.CheckboxLabeled("Enable mod?", ref modEnabled, "Enables mod completely on next reload.");
            //listing_Standard.CheckboxLabeled("Toggle and lock everything on?", ref toggleAll, "Enable everything?");
            //listing_Standard.CheckboxLabeled("Toggle and lock everything off?", ref toggleAllOff, "Disable everything?");
            //listing_Standard.CheckboxLabeled("Reset stuff category settings on next reload?", ref resetToDefaults, "Enable to reset all settings on next reload.");
            listing_Standard.CheckboxLabeled("Enable raw food/plant matter to be used as stuff?", ref enableRawFoodStuffs, "Enable raw food/plant matter to be used as stuff?");
            //clearCache = listing_Standard.ButtonText("Reset settings cache.");
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref modEnabled, "modEnabled", true);
            Scribe_Values.Look(ref resetToDefaults, "resetToDefaults", false);
            Scribe_Values.Look(ref enableRawFoodStuffs, "enableRawFoodStuffs", false);
            Scribe_Values.Look(ref needsUpdate, "needsUpdate", true);
        }
    }
}
