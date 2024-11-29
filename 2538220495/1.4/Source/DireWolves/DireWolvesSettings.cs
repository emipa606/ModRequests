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

namespace DireWolves
{
    class DireWolvesSettings : ModSettings
    {
        public float packSizeBonus = 0.1f;

        public int howlingCooldownTicks = 2500;
        public bool howlingEnabled = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref packSizeBonus, "packSizeBonus", 0.1f);
            Scribe_Values.Look(ref howlingCooldownTicks, "howlingCooldownTicks", 2500);
            Scribe_Values.Look(ref howlingEnabled, "howlingEnabled", true);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("PackSizeBonus".Translate() + ": " + packSizeBonus);
            packSizeBonus = listingStandard.Slider(packSizeBonus, 0, 100);
            listingStandard.CheckboxLabeled("DW.EnableHowling".Translate(), ref howlingEnabled);
            if (howlingEnabled)
            {
                listingStandard.Label("HowlingCooldownTicks".Translate() + ": " + howlingCooldownTicks);
                howlingCooldownTicks = (int)listingStandard.Slider(howlingCooldownTicks, 0, 10000);
            }
            listingStandard.End();
            base.Write();
        }
        private static Vector2 scrollPosition = Vector2.zero;
    }
}

