using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace VoidEvents
{
    class VoidSettings : ModSettings
    {
        public static bool EnableVoidExpansion = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref EnableVoidExpansion, "EnableVoidExpansion", true);
        }

        // Draw the actual settings window that shows up after selecting Z-Levels in the list
        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Void.EnableVoidExpansion".Translate(), ref EnableVoidExpansion);
            listingStandard.End();
        }
    }
}