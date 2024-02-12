using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace ArtifactsExpanded
{
    public class ArtifactsExpandedMod : Mod
    {
        //gets the settings and assigns it to a field
        public static ArtifactsExpandedModSettings modSettings;
        public ArtifactsExpandedMod(ModContentPack modContent) : base(modContent)
        {
            modSettings = GetSettings<ArtifactsExpandedModSettings>();
        }

        //UI for the settings window
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard settingsWindow = new Listing_Standard();
            settingsWindow.Begin(inRect);
            settingsWindow.CheckboxLabeled("ArchotechSacrificerBeltAnimalSettingLabel".Translate(), ref modSettings.sacrificerBeltCountAnimals, "ArchotechSacrificerBeltAnimalSettingTooltip".Translate());
            settingsWindow.CheckboxLabeled("ArchotechSacrificerBeltPrisonerSettingLabel".Translate(), ref modSettings.sacrificerBeltCountPrisoners, "ArchotechSacrificerBeltPrisonerSettingTooltip".Translate());
            settingsWindow.CheckboxLabeled("ArchotechSacrificerBeltThoughtSettingLabel".Translate(), ref modSettings.sacrificerBeltGivesThought, "ArchotechSacrificerBeltThoughtSettingTooltip".Translate());
            settingsWindow.Gap(12f);
            settingsWindow.Label("ArchotechNeurotrainerSettingLabel".Translate(), -1, "ArchotechNeurotrainerSettingTooltip".Translate());
            modSettings.archotechNeurotrainerExpFactor = settingsWindow.Slider(modSettings.archotechNeurotrainerExpFactor, 0.1f, 10f);
            settingsWindow.Gap(12f);
            settingsWindow.Label("GrowerMechSerumSettingLabel".Translate(), -1, "GrowerMechSerumSettingTooltip".Translate());
            modSettings.growerMechSerumGrowFactor = settingsWindow.Slider(modSettings.growerMechSerumGrowFactor, 0.1f, 10f);
            settingsWindow.Gap(12f);
            settingsWindow.Label("RevitalizerMechSerumSettingLabel".Translate(), -1, "RevitalizerMechSerumSettingTooltip".Translate());
            modSettings.revitalizerMechSerumFactor = settingsWindow.Slider(modSettings.revitalizerMechSerumFactor, 1f, 10f);
            settingsWindow.Gap(12f);
            settingsWindow.CheckboxLabeled("AntiPsychicPulserAffectsAllSettingLabel".Translate(), ref modSettings.antiPsychicPulserAffectsAll, "AntiPsychicPulserAffectsAllSettingTooltip".Translate());
            settingsWindow.Gap(12f);
            settingsWindow.Label("MindSuppressorStrengthSettingLabel".Translate(), -1, "MindSuppressorStrengthSettingTooltip".Translate());
            modSettings.mindSuppressorStrength = settingsWindow.Slider(modSettings.mindSuppressorStrength, 0.1f, 1f);
            settingsWindow.Gap(12f);
            settingsWindow.Label("RewardStandardHighFreqWeightFactorSettingLabel".Translate(), -1, "RewardStandardHighFreqWeightFactorSettingTooltip".Translate());
            modSettings.rewardStandardHighFreqWeightFactor = settingsWindow.Slider(modSettings.rewardStandardHighFreqWeightFactor, 1f, 2.5f);
            settingsWindow.End();

            base.DoSettingsWindowContents(inRect);
        }

        //the display name in the mod settings menu
        public override string SettingsCategory()
        {
            return "ArtifactsExpandedSettingsName".Translate();
        }
    }
}