using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicPopulation
{
    internal class DynamicPopulationSettings : ModSettings
    {
        public int popCriticalLow;
        public int popLow;
        public int popDesired;
        public int popHigh;
        public int popCriticalHigh;
        public int popMax;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref popCriticalLow, "popCriticalLow", 0);
            Scribe_Values.Look(ref popLow, "popLow", 1);
            Scribe_Values.Look(ref popDesired, "popDesired", 4);
            Scribe_Values.Look(ref popHigh, "popHigh", 7);
            Scribe_Values.Look(ref popCriticalHigh, "popCriticalHigh", 11);
            Scribe_Values.Look(ref popMax, "popMax", 20);
            base.ExposeData();
        }
    }

    public class DynamicPopulation : Mod
    {
        private static DynamicPopulationSettings Settings;

        public DynamicPopulation(ModContentPack content) : base(content)
        {
            Settings = GetSettings<DynamicPopulationSettings>();
            this.ChangeDefs();
        }

        public void ChangeDefs()
        {
            List<StorytellerDef> storyTellerDefs = DefDatabase<StorytellerDef>.AllDefsListForReading;
            foreach (var storyTeller in storyTellerDefs)
            {
                storyTeller.populationIntentFactorFromPopCurve = new SimpleCurve
                {
                    {
                        new CurvePoint(Settings.popCriticalLow, 8.0f)
                    },
                    {
                        new CurvePoint(Settings.popLow, 2.0f)
                    },
                    {
                        new CurvePoint(Settings.popDesired, 1.0f)
                    },
                    {
                        new CurvePoint(Settings.popHigh, 0.4f)
                    },
                    {
                        new CurvePoint(Settings.popCriticalHigh, 0.0f)
                    },
                    {
                        new CurvePoint(Settings.popMax, -1.0f)
                    },
                };
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            int _popCriticalLow = Settings.popCriticalLow;
            int _popLow = Settings.popLow;
            int _popDesired = Settings.popDesired;
            int _popHigh = Settings.popHigh;
            int _popCriticalHigh = Settings.popCriticalHigh;
            int _popMax = Settings.popMax;
            Listing_Standard listing_standard = new Listing_Standard
            {
                ColumnWidth = (inRect.width - 34f) / 2.1f
            };
            listing_standard.Begin(inRect);
            listing_standard.GapLine();

            Listing_Standard firstSection = listing_standard.BeginSection(350);

            string popCriticalLowBuffer = Settings.popCriticalLow.ToString();
            firstSection.Label("Critically Low Population", -1, "8x Chance To Get Colonist/Prisoner Events");
            firstSection.TextFieldNumeric<int>(ref Settings.popCriticalLow, ref popCriticalLowBuffer, 0, 99);
            firstSection.Gap();
            string popLowBuffer = Settings.popLow.ToString();
            firstSection.Label("Low Population", -1, "2x Chance To Get Colonist/Prisoner Eventsr");
            firstSection.TextFieldNumeric<int>(ref Settings.popLow, ref popLowBuffer, 0, 99);
            firstSection.Gap();
            string popDesiredBuffer = Settings.popDesired.ToString();
            firstSection.Label("Desired Population", -1, "1x Chance To Get Colonist/Prisoner Events");
            firstSection.TextFieldNumeric<int>(ref Settings.popDesired, ref popDesiredBuffer, 0, 99);
            firstSection.Gap();
            string popHighBuffer = Settings.popHigh.ToString();
            firstSection.Label("High Population", -1, "0.35x Chance To Get Colonist/Prisoner Events");
            firstSection.TextFieldNumeric<int>(ref Settings.popHigh, ref popHighBuffer, 0, 99);
            firstSection.Gap();
            string popCriticalHighBuffer = Settings.popCriticalHigh.ToString();
            firstSection.Label("Critically High Population", -1, "(Practically) 0% Chance To Get Colonist/Prisoner Events");
            firstSection.TextFieldNumeric<int>(ref Settings.popCriticalHigh, ref popCriticalHighBuffer, 0, 99);
            firstSection.Gap();
            string popMaxBuffer = Settings.popMax.ToString();
            firstSection.Label("Max Population", -1, "-100% Chance To Get Colonist/Prisoner Events");
            firstSection.TextFieldNumeric<int>(ref Settings.popMax, ref popMaxBuffer, 0, 99);
            firstSection.Gap();

            listing_standard.EndSection(firstSection);
            listing_standard.Gap();
            Listing_Standard buttonSection = listing_standard.BeginSection(30);
            buttonSection.ColumnWidth = (inRect.width - 34f) / 7f;
            if (buttonSection.ButtonText("Default"))
            {
                Settings.popCriticalLow = 0;
                Settings.popLow = 1;
                Settings.popDesired = 4;
                Settings.popHigh = 7;
                Settings.popCriticalHigh = 11;
                Settings.popMax = 20;
            }
            if (buttonSection.ButtonText("Hermit"))
            {
                Settings.popCriticalLow = 0;
                Settings.popLow = 1;
                Settings.popDesired = 1;
                Settings.popHigh = 1;
                Settings.popCriticalHigh = 1;
                Settings.popMax = 1;
            }
            if (buttonSection.ButtonText("Double"))
            {
                Settings.popCriticalLow = 1;
                Settings.popLow = 2;
                Settings.popDesired = 8;
                Settings.popHigh = 14;
                Settings.popCriticalHigh = 22;
                Settings.popMax = 40;
            }
            listing_standard.EndSection(buttonSection);
            listing_standard.End();
            if (_popCriticalLow != Settings.popCriticalLow ||
                _popLow != Settings.popLow ||
                _popDesired != Settings.popDesired ||
                _popCriticalHigh != Settings.popCriticalHigh ||
                _popMax != Settings.popMax || _popHigh != Settings.popMax)
            {
                this.ChangeDefs();
            }
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Dynamic Population";
        }
    }
}
