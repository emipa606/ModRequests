using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace HarderGearLooting
{
    public class HarderGearLootingSettings : ModSettings
    {
        // Existing fields
        public static bool expandBiocoding = true;
        public static float biocodeIndustrialWeaponsChance = 0f;
        public static float biocodeSpacerWeaponsChance = 1f;
        public static float biocodeSpacerArmorChance = 1f;

        public static bool useAcidifier = true;
        public static float acidifierRemovalSuccessFactor = 0.5f;
        public static float acidifierOnDownChance = 0f;

        // Dictionaries for faction-specific settings
        public static Dictionary<string, bool> biocodingFactions;
        public static Dictionary<string, bool> acidifierFactions;

       
        public override void ExposeData()
        {
            base.ExposeData();

            // Save/load existing top-level fields
            Scribe_Values.Look(ref expandBiocoding, "expandBiocoding", true);
            Scribe_Values.Look(ref biocodeIndustrialWeaponsChance, "biocodeIndustrialWeaponsChance", 1f);
            Scribe_Values.Look(ref biocodeSpacerWeaponsChance, "biocodeSpacerWeaponsChance", 1f);
            Scribe_Values.Look(ref biocodeSpacerArmorChance, "biocodeSpacerWeaponsChance", 1f);
            Scribe_Values.Look(ref useAcidifier, "useAcidifier", true);
            Scribe_Values.Look(ref acidifierRemovalSuccessFactor, "acidifierRemovalSuccessFactor", 0.5f);
            Scribe_Values.Look(ref acidifierRemovalSuccessFactor, "acidifierOnDownChance", 0f);
            // Load the dictionaries based on faction defNames
            Scribe_Collections.Look(ref biocodingFactions, "biocodingFactions", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref acidifierFactions, "acidifierFactions", LookMode.Value, LookMode.Value);            
        }
    }


    public class HarderGearLootingMod : Mod
    {
        private Vector2 scrollPosition;
        public static Harmony harmonyInstance;

        public HarderGearLootingMod(ModContentPack content) : base(content)
        {
            // Load settings
            this.GetSettings<HarderGearLootingSettings>();
            harmonyInstance = new Harmony("HGL.PreStartupPatches");
            harmonyInstance.PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            // Define a scrollable area
            Listing_Standard listingStandard = new Listing_Standard();
            Rect scrollContentRect = new Rect(0f, 0f, inRect.width - 32f, (float)((Text.LineHeight + listingStandard.verticalSpacing) * (15 + 2 * HarderGearLootingSettings.biocodingFactions.Count)));
            // Begin scrollable view
            Widgets.BeginScrollView(inRect, ref scrollPosition, scrollContentRect);

            listingStandard.Begin(scrollContentRect);

            // General settings
            listingStandard.Label("General Biocoding Settings:");
            listingStandard.CheckboxLabeled("Enable Expanded Biocoding", ref HarderGearLootingSettings.expandBiocoding, "Toggle all biocoding changes. If false, the vanilla biocoding logic is used.");
            GUI.enabled = HarderGearLootingSettings.expandBiocoding;
            listingStandard.Label($"Biocode Industrial Weapons Chance: {HarderGearLootingSettings.biocodeIndustrialWeaponsChance:P0}");
            HarderGearLootingSettings.biocodeIndustrialWeaponsChance = listingStandard.Slider(HarderGearLootingSettings.biocodeIndustrialWeaponsChance, 0f, 1f);
            listingStandard.Label($"Biocode Spacer Weapons Chance: {HarderGearLootingSettings.biocodeSpacerWeaponsChance:P0}");
            HarderGearLootingSettings.biocodeSpacerWeaponsChance = listingStandard.Slider(HarderGearLootingSettings.biocodeSpacerWeaponsChance, 0f, 1f);
            listingStandard.Label($"Biocode Spacer Armor Chance: {HarderGearLootingSettings.biocodeSpacerArmorChance:P0}");
            HarderGearLootingSettings.biocodeSpacerArmorChance = listingStandard.Slider(HarderGearLootingSettings.biocodeSpacerArmorChance, 0f, 1f);

            // Faction-specific biocoding settings
            listingStandard.Label("Biocoding Settings by Faction:");

            foreach (FactionDef factionDef in DefDatabase<FactionDef>.AllDefs)
            {
                if (!HarderGearLootingSettings.biocodingFactions.ContainsKey(factionDef.defName))
                {
                    continue;
                }

                bool biocodeValue = HarderGearLootingSettings.biocodingFactions[factionDef.defName];
                listingStandard.CheckboxLabeled($"Biocode {factionDef.label}", ref biocodeValue, $"Enable biocoding changes for {factionDef.label}.");
                HarderGearLootingSettings.biocodingFactions[factionDef.defName] = biocodeValue;
            }
            GUI.enabled = true;

            // General acidifier settings
            listingStandard.GapLine();
            listingStandard.Label("General Acidifier Settings:");
            listingStandard.CheckboxLabeled("Enable Gear Acidifiers", ref HarderGearLootingSettings.useAcidifier, "Enable or disable all acidifier options.");
            GUI.enabled = HarderGearLootingSettings.useAcidifier;

            listingStandard.Label($"Acidifier Removal Success Factor: {HarderGearLootingSettings.acidifierRemovalSuccessFactor:P0}");
            HarderGearLootingSettings.acidifierRemovalSuccessFactor = listingStandard.Slider(HarderGearLootingSettings.acidifierRemovalSuccessFactor, 0f, 2f);
            listingStandard.Label($"Acidifier Activation on Down Chance: {HarderGearLootingSettings.acidifierOnDownChance:P0}");
            HarderGearLootingSettings.acidifierOnDownChance = listingStandard.Slider(HarderGearLootingSettings.acidifierOnDownChance, 0f, 1f);

            // Faction-specific acidifier settings
            listingStandard.Label("Acidifier Settings by Faction:");

            foreach (FactionDef factionDef in DefDatabase<FactionDef>.AllDefs)
            {
                if (!HarderGearLootingSettings.acidifierFactions.ContainsKey(factionDef.defName))
                {
                    continue;
                }

                bool acidifierValue = HarderGearLootingSettings.acidifierFactions[factionDef.defName];
                listingStandard.CheckboxLabeled($"Acidifier {factionDef.label}", ref acidifierValue, $"Enable acidifiers for {factionDef.label}.");
                HarderGearLootingSettings.acidifierFactions[factionDef.defName] = acidifierValue;
            }

            listingStandard.End();
            Widgets.EndScrollView();
        }


        public override string SettingsCategory()
        {
            return "Harder Gear Looting";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            this.GetSettings<HarderGearLootingSettings>().Write(); // Ensure settings are saved
            HarderGearLooting.GenerateFactionSets();
        }
    }

}
