using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace Gumball
{
    public class GumballMachineSettings : ModSettings
    {
        public static int silverCost = 5;

        public static bool extraordinaryOutcomes = true;
        public static bool goodOutcomes = true;
        public static bool badOutcomes = true;
        public static bool catastrophicOutcomes = false;

        public static float gumballSeverity = 1f;
        public static float neutralOutcomeChance = 0.3f;
        public static float extraordinaryOutcomeChance = 0.1f;
        public static float badOutcomeChance = 0.3f;
        public static float catastrophicOutcomeChance = 0.05f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref silverCost, "silverCost", 5);

            Scribe_Values.Look(ref extraordinaryOutcomes, "extraordinaryOutcomes", true);
            Scribe_Values.Look(ref goodOutcomes, "goodOutcomes", true);
            Scribe_Values.Look(ref badOutcomes, "badOutcomes", true);
            Scribe_Values.Look(ref catastrophicOutcomes, "catastrophicOutcomes", false);

            Scribe_Values.Look(ref gumballSeverity, "gumballSeverity", 1f);
            Scribe_Values.Look(ref neutralOutcomeChance, "neutralOutcomeChance", 0.3f);
            Scribe_Values.Look(ref extraordinaryOutcomeChance, "extraordinaryOutcomeChance", 0.1f);
            Scribe_Values.Look(ref badOutcomeChance, "badOutcomeChance", 0.3f);
            Scribe_Values.Look(ref catastrophicOutcomeChance, "catastrophicOutcomeChance", 0.05f);

            base.ExposeData();
        }

        private static float height_modifier = 100f;

        public static void DoWindowContents(Rect inRect)
        {
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height + height_modifier);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.maxOneColumn = true;
            listingStandard.ColumnWidth = viewRect.width;
            listingStandard.Begin(inRect);

            listingStandard.Label("Silver Cost: " + silverCost, -1f, "The cost of a gumball.");
            silverCost = (int)listingStandard.Slider((float)silverCost, 0f, 100f);

            listingStandard.Label("Gumball Effect Length: " + (int)(gumballSeverity * 100) + "%", -1f, "How long the effects of a gumball last in percentage of a day.");
            gumballSeverity = listingStandard.Slider(gumballSeverity, 0f, 1f);

            Text.Font = GameFont.Medium;
            listingStandard.Label("Allowed Outcomes:");
            Text.Font = GameFont.Small;
            listingStandard.Gap(4f);

            listingStandard.CheckboxLabeled("Good Outcomes", ref goodOutcomes, "Enables/Disables good outcomes.");

            if (goodOutcomes)
            {
                listingStandard.CheckboxLabeled("Extraordinary Outcomes", ref extraordinaryOutcomes, "Enables/Disables extraordinary outcomes.");
            }

            listingStandard.CheckboxLabeled("Bad Outcomes", ref badOutcomes, "Enables/Disables bad outcomes.");
            if (badOutcomes)
            {
                listingStandard.CheckboxLabeled("Catastrophic Outcomes", ref catastrophicOutcomes, "Enables/Disables catastrophic outcomes.");
            }
            

            listingStandard.Gap(10f);
            Text.Font = GameFont.Medium;
            listingStandard.Label("Outcome Chances:");
            Text.Font = GameFont.Small;
            listingStandard.Gap(4f);

            listingStandard.Label("Neutral Outcome Chance: " + (int)(neutralOutcomeChance * 100) + "%", -1f, "Chance of neutral outcome when not a bad outcome.");
            neutralOutcomeChance = listingStandard.Slider(neutralOutcomeChance, 0f, 1f);

            if (extraordinaryOutcomes)
            {
                listingStandard.Label("Extraordinary Outcome Chance: " + (int)(extraordinaryOutcomeChance * 100) + "%", -1f, "Chance of extrodinary outcome on good outcome.");
                extraordinaryOutcomeChance = listingStandard.Slider(extraordinaryOutcomeChance, 0f, 1f);
            }

            if (badOutcomes)
            {
                listingStandard.Label("Bad Outcome Chance: " + (int)(badOutcomeChance * 100) + "%", -1f, "Chance of bad outcome.");
                badOutcomeChance = listingStandard.Slider(badOutcomeChance, 0f, 1f);
            }
            if (badOutcomes && catastrophicOutcomes)
            {
                listingStandard.Label("Catastrophic Outcome Chance: " + (int)(catastrophicOutcomeChance * 100) + "%", -1f, "Chance of catastrophic outcome on bad outcome.");
                catastrophicOutcomeChance = listingStandard.Slider(catastrophicOutcomeChance, 0f, 1f);
            }


            listingStandard.End();
        }


    }

    public class GumballMachine : Mod
    {
        GumballMachineSettings settings;

        public GumballMachine(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<GumballMachineSettings>();
        }

        public override string SettingsCategory()
        {
            return "Gumball Machine";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            GumballMachineSettings.DoWindowContents(inRect);
        }

    }
}
