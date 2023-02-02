using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace NoQuestsWithoutComms
{
    public class NQWCSettings : ModSettings
    {
        public bool allowVFEMTournaments;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref allowVFEMTournaments, "allowVFEMTournaments");
            base.ExposeData();
        }
    }

    public class NQWCMod : Mod
    {
        NQWCSettings settings;

        public NQWCMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<NQWCSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("allowVFEMTournamentsExplanation", ref settings.allowVFEMTournaments, "allowVFEMTournamentsToolTip");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "No Quests Without Comms".Translate();
        }
    }
}