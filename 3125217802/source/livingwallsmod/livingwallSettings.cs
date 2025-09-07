using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace valkyrim.livingwalls
{
    public class LivingWallSettings : ModSettings
    {
        public bool doLivingWallsDecay = true;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref doLivingWallsDecay, "doLivingWallsDecay");
            base.ExposeData();
        }
    }
    public class LivingwallMod : Mod
    {

        LivingWallSettings settings;

        /// <summary>
        /// A mandatory constructor which resolves the reference to our settings.
        /// </summary>
        /// <param name="content"></param>
        public LivingwallMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<LivingWallSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Do unfueled living walls decay", ref settings.doLivingWallsDecay, "toggles walls health depleting to 10% thier base value when unfueled");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "LivingwallMod".Translate();
        }
    }
}
