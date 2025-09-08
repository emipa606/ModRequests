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
    public class PresetStorage : IExposable
    {
        public string name;
        public Difficulty difficulty;
        public int lastHashCode;
        public bool isDefault;
        public string DefName => "CPP_" + name.Replace(" ", string.Empty);
        public PresetStorage()
        {

        }
        public PresetStorage(Difficulty source)
        {
            this.difficulty = CopyOf(source);
        }

        public Difficulty CopyOf(Difficulty source)
        {
            var difficulty = new Difficulty();
            difficulty.threatScale = source.threatScale;
            difficulty.allowBigThreats = source.allowBigThreats;
            difficulty.allowIntroThreats = source.allowIntroThreats;
            difficulty.allowCaveHives = source.allowCaveHives;
            difficulty.peacefulTemples = source.peacefulTemples;
            difficulty.allowViolentQuests = source.allowViolentQuests;
            difficulty.predatorsHuntHumanlikes = source.predatorsHuntHumanlikes;
            difficulty.scariaRotChance = source.scariaRotChance;
            difficulty.colonistMoodOffset = source.colonistMoodOffset;
            difficulty.tradePriceFactorLoss = source.tradePriceFactorLoss;
            difficulty.cropYieldFactor = source.cropYieldFactor;
            difficulty.mineYieldFactor = source.mineYieldFactor;
            difficulty.butcherYieldFactor = source.butcherYieldFactor;
            difficulty.researchSpeedFactor = source.researchSpeedFactor;
            difficulty.diseaseIntervalFactor = source.diseaseIntervalFactor;
            difficulty.enemyReproductionRateFactor = source.enemyReproductionRateFactor;
            difficulty.playerPawnInfectionChanceFactor = source.playerPawnInfectionChanceFactor;
            difficulty.manhunterChanceOnDamageFactor = source.manhunterChanceOnDamageFactor;
            difficulty.deepDrillInfestationChanceFactor = source.deepDrillInfestationChanceFactor;
            difficulty.foodPoisonChanceFactor = source.foodPoisonChanceFactor;
            difficulty.maintenanceCostFactor = source.maintenanceCostFactor;
            difficulty.enemyDeathOnDownedChanceFactor = source.enemyDeathOnDownedChanceFactor;
            difficulty.adaptationGrowthRateFactorOverZero = source.adaptationGrowthRateFactorOverZero;
            difficulty.adaptationEffectFactor = source.adaptationEffectFactor;
            difficulty.questRewardValueFactor = source.questRewardValueFactor;
            difficulty.raidLootPointsFactor = source.raidLootPointsFactor;
            difficulty.allowTraps = source.allowTraps;
            difficulty.allowTurrets = source.allowTurrets;
            difficulty.allowMortars = source.allowMortars;
            difficulty.allowExtremeWeatherIncidents = source.allowExtremeWeatherIncidents;
            difficulty.fixedWealthMode = source.fixedWealthMode;
            difficulty.fixedWealthTimeFactor = source.fixedWealthTimeFactor;
            difficulty.friendlyFireChanceFactor = source.friendlyFireChanceFactor;
            difficulty.allowInstantKillChance = source.allowInstantKillChance;
            this.lastHashCode = source.GetHashCode();
            return difficulty;
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Deep.Look(ref difficulty, "difficulty");
            Scribe_Values.Look(ref isDefault, "isDefault");
        }
    }
    class CustomPlaystylePresetsSettings : ModSettings
    {
        public PresetStorage DefaultPreset => presets.FirstOrDefault(x => x.isDefault);
        public void SetDefaultPreset(PresetStorage preset)
        {
            if (!presets.Contains(preset))
            {
                presets.Add(preset);
            }
            foreach (var preset2 in presets)
            {
                if (preset != preset2)
                {
                    preset2.isDefault = false;
                    var def = DefDatabase<DifficultyDef>.GetNamedSilentFail(preset2.DefName);
                    if (def != null)
                    {
                        MethodInfo methodInfo = AccessTools.Method(typeof(DefDatabase<DifficultyDef>), "Remove", null, null);
                        methodInfo.Invoke(null, new object[]
                        {
                           def
                        });
                    }
                }
            }
            preset.isDefault = true;
            this.Write();
        }

        public HashSet<PresetStorage> presets;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref presets, "presets", LookMode.Deep);
        }
        public void AddNewDifficulty(PresetStorage presetStorage)
        {
            if (!presets.Contains(presetStorage))
            {
                presets.Add(presetStorage);
                this.Write();
            }
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Widgets.BeginScrollView(inRect, ref scrollPosition, inRect, true);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("CPP.RemovePresets".Translate());
            listingStandard.GapLine();
            foreach (var preset in presets)
            {
                if (listingStandard.ButtonTextLabeled(preset.name, "CPP.RemovePreset".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("CPP.DeleteConfirmation".Translate(preset.name), "Ok".Translate(), () =>
                    {
                        presets.Remove(preset);
                        if (presets.Any())
                        {
                            SetDefaultPreset(presets.First());
                        }
                    }, "CPP.Cancel".Translate()));
                }
            }
            listingStandard.End();
            Widgets.EndScrollView();
            base.Write();
        }
        private static Vector2 scrollPosition = Vector2.zero;

    }
}

