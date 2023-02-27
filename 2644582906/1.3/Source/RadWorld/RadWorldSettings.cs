using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RadWorld
{
    class RadWorldSettings : ModSettings
    {
        public bool disableCustomRadiationSystem;
        public bool disableNonRadiatedAnimalRemoval;
        public bool disableGasMaskFilters;
        public bool disableRadiationGoingIndoors;
        public float cavernSurfaceCoverage = 0.80f;
        public float cavernBarrenCoverage = 0.60f;
        public float cavernInfestedCoverage = 0.70f;
        public float cavernSickCoverage = 0.46f;
        public float cavernLushCoverage = 0.54f;
        public float cavernCollapsedCoverage = 0.73f;
        public float cavernRegularCoverage = 0.25f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref disableCustomRadiationSystem, "disableCustomRadiationSystem");
            Scribe_Values.Look(ref disableNonRadiatedAnimalRemoval, "disableNonRadiatedAnimalRemoval");
            Scribe_Values.Look(ref disableGasMaskFilters, "disableGasMaskFilters");
            Scribe_Values.Look(ref disableRadiationGoingIndoors, "disableRadiationGoingIndoors");
            Scribe_Values.Look(ref cavernSurfaceCoverage, "cavernSurfaceCoverage", 0.80f);
            Scribe_Values.Look(ref cavernBarrenCoverage, "cavernBarrenCoverage", 0.60f);
            Scribe_Values.Look(ref cavernInfestedCoverage, "cavernInfestedCoverage", 0.70f);
            Scribe_Values.Look(ref cavernSickCoverage, "cavernSickCoverage", 0.46f);
            Scribe_Values.Look(ref cavernLushCoverage, "cavernLushCoverage", 0.54f);
            Scribe_Values.Look(ref cavernCollapsedCoverage, "cavernCollapsedCoverage", 0.73f);
            Scribe_Values.Look(ref cavernRegularCoverage, "cavernRegularCoverage", 0.25f);
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("RW.DisableCustomRadiationSystem".Translate(), ref disableCustomRadiationSystem);
            listingStandard.CheckboxLabeled("RW.disableNonRadiatedAnimalRemoval".Translate(), ref disableNonRadiatedAnimalRemoval);
            listingStandard.CheckboxLabeled("RW.disableGasMaskFilters".Translate(), ref disableGasMaskFilters);
            listingStandard.CheckboxLabeled("RW.disableRadiationGoingIndoors".Translate(), ref disableRadiationGoingIndoors);
            listingStandard.Label("cavernSurfaceCoverage".Translate() + ": " + (1f - cavernSurfaceCoverage).ToStringDecimalIfSmall());
            cavernSurfaceCoverage = 1f - (listingStandard.Slider(1f - cavernSurfaceCoverage, 0f, 1f));
            listingStandard.Label("cavernBarrenCoverage".Translate() + ": " + (1f - cavernBarrenCoverage).ToStringDecimalIfSmall());
            cavernBarrenCoverage = 1f - (listingStandard.Slider(1f - cavernBarrenCoverage, 0f, 1f)); 
            listingStandard.Label("cavernInfestedCoverage".Translate() + ": " + (1f - cavernInfestedCoverage).ToStringDecimalIfSmall());
            cavernInfestedCoverage = 1f - (listingStandard.Slider(1f - cavernInfestedCoverage, 0f, 1f)); 
            listingStandard.Label("cavernSickCoverage".Translate() + ": " + (1f - cavernSickCoverage).ToStringDecimalIfSmall());
            cavernSickCoverage = 1f - (listingStandard.Slider(1f - cavernSickCoverage, 0f, 1f)); 
            listingStandard.Label("cavernLushCoverage".Translate() + ": " + (1f - cavernLushCoverage).ToStringDecimalIfSmall());
            cavernLushCoverage = 1f - (listingStandard.Slider(1f - cavernLushCoverage, 0f, 1f)); 
            listingStandard.Label("cavernCollapsedCoverage".Translate() + ": " + (1f - cavernCollapsedCoverage).ToStringDecimalIfSmall());
            cavernCollapsedCoverage = 1f - (listingStandard.Slider(1f - cavernCollapsedCoverage, 0f, 1f)); 
            listingStandard.Label("cavernRegularCoverage".Translate() + ": " + (1f - cavernRegularCoverage).ToStringDecimalIfSmall());
            cavernRegularCoverage = 1f - (listingStandard.Slider(1f - cavernRegularCoverage, 0f, 1f)); 
            listingStandard.End();
        }
    }
}

