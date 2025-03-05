using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using UnityEngine;

namespace Krelinos_BiocodeIt
{

    // Just so you know, I basically Copy & Pasted these two classes from the Mod Settings section of the Rimworld Wiki Modding Tutorials
    public class BiocodeIt_Settings : ModSettings
    {
        public static int biocodedStealChancePercent = 0;
        public static int biocodedMarketValuePercent = 0;

        public static bool notifyPlayerOfSpite;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref biocodedMarketValuePercent, "biocodedMarketValuePercent", 35);
            Scribe_Values.Look(ref biocodedStealChancePercent, "biocodedStealChancePercent", 2);

            Scribe_Values.Look(ref notifyPlayerOfSpite, "notifyPlayerOfSpite", true);
            base.ExposeData();
        }
    }

    public class BiocodeIt : Mod
    {
        BiocodeIt_Settings settings;
        public BiocodeIt(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<BiocodeIt_Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label(String.Format("BiocodedMarketValuePercentLabel".Translate(), BiocodeIt_Settings.biocodedMarketValuePercent));
            BiocodeIt_Settings.biocodedMarketValuePercent = (int)listingStandard.Slider(BiocodeIt_Settings.biocodedMarketValuePercent, 0f, 100f);
            
            listingStandard.Label(String.Format("BiocodedStealChancePercentLabel".Translate(), BiocodeIt_Settings.biocodedStealChancePercent));
            BiocodeIt_Settings.biocodedStealChancePercent = (int)listingStandard.Slider(BiocodeIt_Settings.biocodedStealChancePercent, 0f, 100f);

            listingStandard.Label(String.Format("BiocodedStealChancePercentExplanation".Translate(),
                Math.Round(BiocodeIt_Settings.biocodedStealChancePercent / 100f, 2),
                Math.Round(
                    (1 - Math.Pow((100 - BiocodeIt_Settings.biocodedStealChancePercent) / 100f, 6)) * 100,
                1)
            ));

            listingStandard.Gap();

            // 2 Nov 2022
            // I've used dnSpy for 4 hours trying to find out why you cannot biocode melee, apparel, medieval, neolthic.
            // I found nothiiiing! Ugh.

            // 12 Nov 2022
            // KasumiTakeshi on Steam is incredible. As it turns out, BaseApparel and BaseWeapon need the "Comp_Biocodable" component.
            // Only BaseHumanMakeableGun (industrial and up) had the Comp_Biocodable. This is now changed in an XML patch.


            listingStandard.CheckboxLabeled("BiocodedNotifyPlayerOfSpiteLabel".Translate(), ref BiocodeIt_Settings.notifyPlayerOfSpite);

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "BiocodeIt_SettingsCategory".Translate();
        }
    }

}
