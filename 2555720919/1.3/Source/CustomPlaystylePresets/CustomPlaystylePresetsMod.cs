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

namespace CustomPlaystylePresets
{
    class CustomPlaystylePresetsMod : Mod
    {
        public static CustomPlaystylePresetsSettings settings;
        public CustomPlaystylePresetsMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<CustomPlaystylePresetsSettings>();
            if (settings.presets is null)
            {
                settings.presets = new HashSet<PresetStorage>();
            }
            new Harmony("CustomPlaystylePresets.Mod").PatchAll();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        public static void AddNewDifficultyDef(PresetStorage presetStorage)
        {
            var customDifficultyDef = CreateNewDifficultyDef(presetStorage);
            DefDatabase<DifficultyDef>.Add(customDifficultyDef);
        }
        public static DifficultyDef CreateNewDifficultyDef(PresetStorage src)
        {
            var newDef = new DifficultyDef { defName = "CPP_" + src.name.Replace(" ", string.Empty), label = src.name, description = src.name };
            newDef.threatScale = src.difficulty.threatScale;
            newDef.allowBigThreats = src.difficulty.allowBigThreats;
            newDef.allowIntroThreats = src.difficulty.allowIntroThreats;
            newDef.allowCaveHives = src.difficulty.allowCaveHives;
            newDef.peacefulTemples = src.difficulty.peacefulTemples;
            newDef.allowViolentQuests = src.difficulty.allowViolentQuests;
            newDef.predatorsHuntHumanlikes = src.difficulty.predatorsHuntHumanlikes;
            newDef.scariaRotChance = src.difficulty.scariaRotChance;
            newDef.colonistMoodOffset = src.difficulty.colonistMoodOffset;
            newDef.tradePriceFactorLoss = src.difficulty.tradePriceFactorLoss;
            newDef.cropYieldFactor = src.difficulty.cropYieldFactor;
            newDef.mineYieldFactor = src.difficulty.mineYieldFactor;
            newDef.butcherYieldFactor = src.difficulty.butcherYieldFactor;
            newDef.researchSpeedFactor = src.difficulty.researchSpeedFactor;
            newDef.diseaseIntervalFactor = src.difficulty.diseaseIntervalFactor;
            newDef.enemyReproductionRateFactor = src.difficulty.enemyReproductionRateFactor;
            newDef.playerPawnInfectionChanceFactor = src.difficulty.playerPawnInfectionChanceFactor;
            newDef.manhunterChanceOnDamageFactor = src.difficulty.manhunterChanceOnDamageFactor;
            newDef.deepDrillInfestationChanceFactor = src.difficulty.deepDrillInfestationChanceFactor;
            newDef.foodPoisonChanceFactor = src.difficulty.foodPoisonChanceFactor;
            newDef.maintenanceCostFactor = src.difficulty.maintenanceCostFactor;
            newDef.enemyDeathOnDownedChanceFactor = src.difficulty.enemyDeathOnDownedChanceFactor;
            newDef.adaptationGrowthRateFactorOverZero = src.difficulty.adaptationGrowthRateFactorOverZero;
            newDef.adaptationEffectFactor = src.difficulty.adaptationEffectFactor;
            newDef.questRewardValueFactor = src.difficulty.questRewardValueFactor;
            newDef.raidLootPointsFactor = src.difficulty.raidLootPointsFactor;
            newDef.allowTraps = src.difficulty.allowTraps;
            newDef.allowTurrets = src.difficulty.allowTurrets;
            newDef.allowMortars = src.difficulty.allowMortars;
            newDef.allowExtremeWeatherIncidents = src.difficulty.allowExtremeWeatherIncidents;
            newDef.fixedWealthMode = src.difficulty.fixedWealthMode;
            newDef.isCustom = true;
            return newDef;
        }
        public override string SettingsCategory()
        {
            return "Custom Playstyle Presets";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }
    }

    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            if (CustomPlaystylePresetsMod.settings.DefaultPreset != null)
            {
                CustomPlaystylePresetsMod.AddNewDifficultyDef(CustomPlaystylePresetsMod.settings.DefaultPreset);
            }
        }
    }
}