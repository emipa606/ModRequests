using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.Settings
{
    public abstract class BaseSettings : IExposable
    {
        private string settingsLabel = "";

        public string SettingsLabel { get => settingsLabel; set => settingsLabel = value; }

        public virtual void GetSettingsHeaderSimple(Listing_Standard listing_Standard)
        {

        }

        public virtual void GetSettingsHeader(Listing_Standard listing_Standard)
        {
            if(!"".Equals(settingsLabel))
                listing_Standard.Label("{0}".Formatted(SettingsLabel));
            listing_Standard.Label("Please Restart Game For Changes To Take Effect");
            listing_Standard.Gap();
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref settingsLabel, "settingsLabel");
        }
    }
}
