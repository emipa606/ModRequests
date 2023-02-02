using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;


namespace BoderlandsSettings
{
    [StaticConstructorOnStartup]
    static class SettingsImplementerExecutorInAConstructor
    {
        static SettingsImplementerExecutorInAConstructor()
        {
            List<PawnKindDef> allBullymongs = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(x => x.RaceProps.wildBiomes?.First() != null && x.RaceProps.wildBiomes.First().commonality != 0 && x.defName == "Bullymong");

            List<PawnKindDef> allRakks = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(x => x.RaceProps.wildBiomes?.First() != null && x.RaceProps.wildBiomes.First().commonality != 0 && x.defName == "Rakk");

            List<PawnKindDef> allRatches = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(x => x.RaceProps.wildBiomes?.First() != null && x.RaceProps.wildBiomes.First().commonality != 0 && x.defName == "Ratch");

            List<PawnKindDef> allSkags = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(x => x.RaceProps.wildBiomes?.First() != null && x.RaceProps.wildBiomes.First().commonality != 0 && (x.defName == "Skag" || x.defName == "SkagArctic" || x.defName == "SkagSpitter"));

            List<PawnKindDef> allSpiderants = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(x => x.RaceProps.wildBiomes?.First() != null && x.RaceProps.wildBiomes.First().commonality != 0 && x.defName == "Spiderant");

            List<PawnKindDef> allTyrants = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(x => x.RaceProps.wildBiomes?.First() != null && x.RaceProps.wildBiomes.First().commonality != 0 && x.defName == "Tyrant");

            foreach (PawnKindDef creature in allBullymongs)
            {
                for (int i = 0; i < creature.RaceProps.wildBiomes.Count; i++) { creature.RaceProps.wildBiomes[i].commonality = BorderlandsSettings.bullymongChance; }
            }

            foreach (PawnKindDef creature in allRatches)
            {
                for (int i = 0; i < creature.RaceProps.wildBiomes.Count; i++) { creature.RaceProps.wildBiomes[i].commonality = BorderlandsSettings.ratchChance; }
            }

            foreach (PawnKindDef creature in allRakks)
            {
                for (int i = 0; i < creature.RaceProps.wildBiomes.Count; i++) { creature.RaceProps.wildBiomes[i].commonality = BorderlandsSettings.rakkChance; }
            }

            foreach (PawnKindDef creature in allSkags)
            {
                for (int i = 0; i < creature.RaceProps.wildBiomes.Count; i++) { creature.RaceProps.wildBiomes[i].commonality = BorderlandsSettings.skagChance; }
            }

            foreach (PawnKindDef creature in allSpiderants)
            {
                for (int i = 0; i < creature.RaceProps.wildBiomes.Count; i++) { creature.RaceProps.wildBiomes[i].commonality = BorderlandsSettings.spiderantChance; }
            }

            foreach (PawnKindDef creature in allTyrants)
            {
                for (int i = 0; i < creature.RaceProps.wildBiomes.Count; i++) { creature.RaceProps.wildBiomes[i].commonality = BorderlandsSettings.tyrantChance; }
            }
        }
    }

    public class BorderlandsSettings : ModSettings
    {
        /// <summary>
        /// The three settings our mod has.
        /// </summary>
        /// 
        public static float bullymongChance = 0.2f;
        public static float rakkChance = 0.5f;
        public static float ratchChance = 0.3f;
        public static float skagChance = 0.4f;
        public static float spiderantChance = 0.4f;
        public static float tyrantChance = 0.1f;
        /*
        public float tyrantChanceAridShrubland = 0.1f;
        public float tyrantChanceBorealForest = 0.1f;
        public float tyrantChanceColdBog = 0.1f;
        public float tyrantChanceDesert = 0.1f;
        public float tyrantChanceExtremeDesert = 0.1f;
        public float tyrantChanceIceSheet = 0.0f;
        public float tyrantChanceSeaIce = 0.0f;
        public float tyrantChanceTemperateForest = 0.1f;
        public float tyrantChanceTemperateSwamp = 0.1f;
        public float tyrantChanceTropicalRainforest = 0.1f;
        public float tyrantChanceTropicalSwamp = 0.1f;
        public float tyrantChanceTundra = 0.0f;
        */

        /// <summary>
        /// The part that writes our settings to file. Note that saving is by ref.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref bullymongChance, "bullymongChance", 0.2f);
            Scribe_Values.Look(ref ratchChance, "ratchChance", 0.3f);
            Scribe_Values.Look(ref rakkChance, "rakkChance", 0.5f);
            Scribe_Values.Look(ref skagChance, "skagChance", 0.4f);
            Scribe_Values.Look(ref spiderantChance, "spiderantChance", 0.4f);
            Scribe_Values.Look(ref tyrantChance, "tyrantChance", 0.1f);
            base.ExposeData();
        }
    }

    public class BorderlandsMod : Mod
    {
        /// <summary>
        /// A reference to our settings.
        /// </summary>
        BorderlandsSettings settings;

        /// <summary>
        /// A mandatory constructor which resolves the reference to our settings.
        /// </summary>
        /// <param name="content"></param>
        public BorderlandsMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<BorderlandsSettings>();
        }

        /// <summary>
        /// The (optional) GUI part to set your settings.
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(inRect);


            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            listingStandard.Label("Changed settings need a game restart to take effect.");
            Text.Font = GameFont.Small;

            Text.Anchor = TextAnchor.UpperLeft;

            listingStandard.GapLine();
            listingStandard.Label("Bullymong spawn chance");
            listingStandard.Label(BorderlandsSettings.bullymongChance.ToString());
            BorderlandsSettings.bullymongChance = (float)Math.Round(listingStandard.Slider(BorderlandsSettings.bullymongChance, 0.0f, 1.0f), 2);

            listingStandard.GapLine();
            listingStandard.Label("Rakk spawn chance");
            listingStandard.Label(BorderlandsSettings.rakkChance.ToString());
            BorderlandsSettings.rakkChance = (float)Math.Round(listingStandard.Slider(BorderlandsSettings.rakkChance, 0.0f, 1.0f), 2);

            listingStandard.GapLine();
            listingStandard.Label("Ratch spawn chance");
            listingStandard.Label(BorderlandsSettings.ratchChance.ToString());
            BorderlandsSettings.ratchChance = (float)Math.Round(listingStandard.Slider(BorderlandsSettings.ratchChance, 0.0f, 1.0f), 2);

            listingStandard.GapLine();
            listingStandard.Label("Skag spawn chance");
            listingStandard.Label(BorderlandsSettings.skagChance.ToString());
            BorderlandsSettings.skagChance = (float)Math.Round(listingStandard.Slider(BorderlandsSettings.skagChance, 0.0f, 1.0f), 2);

            listingStandard.GapLine();
            listingStandard.Label("Spiderant spawn chance");
            listingStandard.Label(BorderlandsSettings.spiderantChance.ToString());
            BorderlandsSettings.spiderantChance = (float)Math.Round(listingStandard.Slider(BorderlandsSettings.spiderantChance, 0.0f, 1.0f), 2);

            listingStandard.GapLine();
            listingStandard.Label("Tyrant spawn chance");
            listingStandard.Label(BorderlandsSettings.tyrantChance.ToString());
            BorderlandsSettings.tyrantChance = (float)Math.Round(listingStandard.Slider(BorderlandsSettings.tyrantChance, 0.0f, 1.0f), 2);
            listingStandard.GapLine();


            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns>The (translated) mod name.</returns>
        public override string SettingsCategory()
        {
            return "Borderlands: The Rim";
        }
    }
}