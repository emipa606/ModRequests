using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HugsLib;
using HugsLib.Settings;
using Verse;

namespace HalfDragons
{
    public static class SettingsAccess
    {
        public static int needIncreaseInterval;
        public static float needIncreaseValue;
        public static float injuryHealingPoints;
        public static float injuryHealingCost;
        public static float thresholdToBeConsideredDamaged;
        public static float thresholdToBeConsideredHealed;
        public static float dragonBloodGainFromDragonRage;
    }

    public class HalfDragonSettings : ModBase
    {
        public override string ModIdentifier
        {
            get { return "HalfDragons"; }
        }

        protected override bool HarmonyAutoPatch => false;

        private SettingHandle<int> needIncreaseInterval;
        private SettingHandle<float> needIncreaseValue;
        private SettingHandle<float> injuryHealingPoints;
        private SettingHandle<float> injuryHealingCost;
        private SettingHandle<float> thresholdToBeConsideredDamaged;
        private SettingHandle<float> thresholdToBeConsideredHealed;
        private SettingHandle<float> dragonBloodGainFromDragonRage;
        public override void DefsLoaded()
        {
            needIncreaseInterval = Settings.GetHandle<int>(
                "needIncreaseInterval",
                "needIncreaseInterval".Translate(),
                "needIncreaseInterval_tip".Translate(),
                600,
                Validators.IntRangeValidator(1, int.MaxValue));
            needIncreaseValue = Settings.GetHandle<float>(
                "needIncreaseValue",
                "needIncreaseValue".Translate(),
                "needIncreaseValue_tip".Translate(),
                0.005f,
                Validators.FloatRangeValidator(0, 1f));
            injuryHealingPoints = Settings.GetHandle<float>(
                "injuryHealingPoints",
                "injuryHealingPoints".Translate(),
                "injuryHealingPoints_tip".Translate(),
                5f,
                Validators.FloatRangeValidator(0, int.MaxValue));
            injuryHealingCost = Settings.GetHandle<float>(
                "injuryHealingCost",
                "injuryHealingCost".Translate(),
                "injuryHealingCost_tip".Translate(),
                0.02f,
                Validators.FloatRangeValidator(0, 1));
            thresholdToBeConsideredDamaged = Settings.GetHandle<float>(
                "thresholdToBeConsideredDamaged",
                "thresholdToBeConsideredDamaged".Translate(),
                "thresholdToBeConsideredDamaged_tip".Translate(),
                0.5f,
                Validators.FloatRangeValidator(0, 1));
            thresholdToBeConsideredHealed = Settings.GetHandle<float>(
                "thresholdToBeConsideredHealed",
                "thresholdToBeConsideredHealed".Translate(),
                "thresholdToBeConsideredHealed_tip".Translate(),
                1f,
                Validators.FloatRangeValidator(0, 1));
            dragonBloodGainFromDragonRage = Settings.GetHandle<float>(
                "dragonBloodGainFromDragonRage",
                "dragonBloodGainFromDragonRage".Translate(),
                "dragonBloodGainFromDragonRage_tip".Translate(),
                0.1f,
                Validators.FloatRangeValidator(-1, 1));
            SetSettings();
        }

        public override void SettingsChanged()
        {
            base.SettingsChanged();
            SetSettings();
        }

        private void SetSettings()
        {
            SettingsAccess.needIncreaseInterval = needIncreaseInterval.Value;
            SettingsAccess.needIncreaseValue = needIncreaseValue.Value;
            SettingsAccess.injuryHealingPoints = injuryHealingPoints.Value;
            SettingsAccess.injuryHealingCost = injuryHealingCost.Value;
            SettingsAccess.thresholdToBeConsideredDamaged = thresholdToBeConsideredDamaged.Value;
            SettingsAccess.thresholdToBeConsideredHealed = thresholdToBeConsideredHealed.Value;
            SettingsAccess.dragonBloodGainFromDragonRage = dragonBloodGainFromDragonRage.Value;
        }
    }
}
