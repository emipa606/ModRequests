using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnsAreDrowning
{
    class PawnsAreDrowningSettings : ModSettings
    {
        public Dictionary<string, float> drowningStates = new Dictionary<string, float>
        {
            {"Marsh", 0.01f},
            {"WaterDeep", 0.05f},
            {"WaterMovingChestDeep", 0.05f},
            {"WaterMovingShallow", 0.01f},
            {"WaterOceanDeep", 0.05f},
            {"WaterOceanShallow", 0.01f},
            {"WaterShallow", 0.01f},
        };

        public Dictionary<string, bool> raceStates = new Dictionary<string, bool>();
        public Dictionary<string, bool> apparelStates = new Dictionary<string, bool>();

        private List<string> traitKeys;
        private List<float> floatValues;

        private List<string> raceKeys;
        private List<bool> raceValues;

        private List<string> apparelKeys;
        private List<bool> apparelValues;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref drowningStates, "drowningStates", LookMode.Value, LookMode.Value, ref traitKeys, ref floatValues);
            Scribe_Collections.Look(ref raceStates, "raceStates", LookMode.Value, LookMode.Value, ref raceKeys, ref raceValues);
            Scribe_Collections.Look(ref apparelStates, "apparelStates", LookMode.Value, LookMode.Value, ref apparelKeys, ref apparelValues);
        }



        public void DoSettingsWindowContents(Rect inRect)
        {
            var drowningSettingsKeys = drowningStates.Keys.ToList().OrderByDescending(x => x).ToList();
            var racesSettingsKeys = raceStates.Keys.ToList().OrderByDescending(x => x).ToList();
            var apparelSettingsKeys = apparelStates.Keys.ToList().OrderByDescending(x => x).ToList();

            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, (drowningSettingsKeys.Count * 24) + (racesSettingsKeys.Count * 26) + (apparelSettingsKeys.Count * 26));
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect2);
            listingStandard.Label("Race settings:");
            for (int num = racesSettingsKeys.Count - 1; num >= 0; num--)
            {
                bool raceState = raceStates[racesSettingsKeys[num]];
                listingStandard.CheckboxLabeled("Enable drowning for " + racesSettingsKeys[num] + ": ", ref raceState);
                raceStates[racesSettingsKeys[num]] = raceState;
            }
            if (listingStandard.ButtonTextLabeled("Reset to default: ", "Reset"))
            {
                raceStates = new Dictionary<string, bool>();
                DrowningMod.ResetRaceSettings();
            }
            listingStandard.GapLine();

            listingStandard.Label("Apparel settings:");
            for (int num = apparelSettingsKeys.Count - 1; num >= 0; num--)
            {
                bool apparelState = apparelStates[apparelSettingsKeys[num]];
                listingStandard.CheckboxLabeled("Prevents drowning: " + apparelSettingsKeys[num], ref apparelState);
                apparelStates[apparelSettingsKeys[num]] = apparelState;
            }
            if (listingStandard.ButtonTextLabeled("Reset to default: ", "Reset"))
            {
                apparelStates = new Dictionary<string, bool>();
                DrowningMod.ResetApparelSettings();
            }
            listingStandard.GapLine();

            listingStandard.Label("Water terrain settings:");
            for (int num = drowningSettingsKeys.Count - 1; num >= 0; num--)
            {
                var test = drowningStates[drowningSettingsKeys[num]];
                listingStandard.SliderLabeled(drowningSettingsKeys[num] + " - Drowning severity: ", ref test, test.ToString(), 0f, 1f);
                drowningStates[drowningSettingsKeys[num]] = test;
            }
            if (listingStandard.ButtonTextLabeled("Reset to default: ", "Reset"))
            {
                drowningStates = new Dictionary<string, float>
                {
                    {"Marsh", 0.01f},
                    {"WaterDeep", 0.05f},
                    {"WaterMovingChestDeep", 0.05f},
                    {"WaterMovingShallow", 0.01f},
                    {"WaterOceanDeep", 0.05f},
                    {"WaterOceanShallow", 0.01f},
                    {"WaterShallow", 0.01f},
                };
                DrowningMod.ResetTerrainSettings();
            }
            listingStandard.End();
            Widgets.EndScrollView();
            base.Write();
        }

        private static Vector2 scrollPosition = Vector2.zero;
    }
}
