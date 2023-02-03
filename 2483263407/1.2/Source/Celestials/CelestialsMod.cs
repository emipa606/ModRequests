using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

using O21Toolbox;

namespace Celestials
{
    public class CelestialsMod : Mod
    {
        public static CelestialsMod mod;

        public static CelestialsSettings settings;

        public override string SettingsCategory() => "Celestials";

        public CelestialsMod(ModContentPack content) : base(content)
        {
            mod = this;
            settings = GetSettings<CelestialsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            DoSettingsPage_General(listingStandard, inRect);

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public void DoSettingsPage_General(Listing_Standard listingStandard, Rect inRect)
        {
            listingStandard.CheckboxEnhanced("Resurrection Side Effects", "If true, resurrection has the same side-effects as resurrector mech serum.", ref settings.resurrectionSideEffects);
            listingStandard.CheckboxEnhanced("EMP Burst", "If true, resurrection generates a large EMP blast.", ref settings.empExplosion);
            listingStandard.GapLine();
            Rect rect = listingStandard.GetRect(18);
            Rect rect2 = rect.LeftHalf().Rounded();
            Rect rect3 = rect.RightHalf().Rounded();
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect2, "Chance of Celestial");
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.TextFieldPercent(rect3, ref settings.celestialChance, ref settings.chanceOfCelestial_buffer, 0f, 1f);
            listingStandard.Note("Getting the numbers after the decimal point to be 0 can be finnicky due to RimWorlds own code, you can paste in the value to get around that. \nDefault: 1%", GameFont.Tiny);
            listingStandard.GapLine();
            listingStandard.Label("Resurrection time");
            listingStandard.IntRange(ref settings.resurrectionTicks, 0, 300000);
            listingStandard.Note("This is in ticks, so 60000 = 1 day. \nDefaults: 30000 - 60000", GameFont.Tiny);
        }
    }
}
