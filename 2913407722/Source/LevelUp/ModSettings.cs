using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace LevelUp
{
    public class LevelUpModSettings : ModSettings
    {
        public float LevelUpRate = 0.075f;
        public float LevelUpHealthRate = 0.1f;
        public float MaxRandomLevel = 10f;
        public float BaseXP = 75f;
        public const string defaultLevellingMode = "Compound Levelling";
        public const string defaultHealthScalingMode = "Compounding Health";
        public string LevellingType = defaultLevellingMode;
        public string HealthScalingType = defaultHealthScalingMode;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref LevelUpRate, "LevellingRate", 0.075f);
            Scribe_Values.Look(ref LevelUpHealthRate, "HealthPerLevelUp", 0.1f);
            Scribe_Values.Look(ref MaxRandomLevel, "MaxRandomLevel", 10f);
            Scribe_Values.Look(ref BaseXP, "BaseXPForLevelling", 75f);
            Scribe_Values.Look(ref LevellingType, "LevellingMode", "Compound Levelling");
            Scribe_Values.Look(ref HealthScalingType, "HealthScalingMode", "Compounding Health");
            base.ExposeData();
        }
    }
    public class LevelUpMod : Mod
    {
        public static string[] LevellingMode = { "Compound Levelling", "Simple Levelling" };
        public static string[] HealthScalingMode = { "Compounding Health", "Simple Health" };
        LevelUpModSettings settings;
        public LevelUpMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<LevelUpModSettings>();
        }
        public override string SettingsCategory() => "Level This!";
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.AddLabeledRadioList($"Levelling Mode:", labels: LevellingMode, val: ref settings.LevellingType);
            listingStandard.GapLine(3f);
            listingStandard.AddLabeledRadioList($"Health Scaling Mode:", labels: HealthScalingMode, val: ref settings.HealthScalingType);
            listingStandard.GapLine(3f);
            listingStandard.Label($"Levelling Rate ({settings.LevelUpRate.ToStringPercent()})");
            settings.LevelUpRate = listingStandard.Slider(settings.LevelUpRate, 0.001f, 1f);
            listingStandard.Label($"Health Per Level Rate ({settings.LevelUpHealthRate.ToStringPercent()})");
            settings.LevelUpHealthRate = listingStandard.Slider(settings.LevelUpHealthRate, 0.001f, 1f);
            listingStandard.AddLabeledNumericalTextField("Maximum Random Level", ref settings.MaxRandomLevel, minValue: 1f, maxValue: 1000f);
            listingStandard.AddLabeledNumericalTextField("Base XP Required", ref settings.BaseXP, minValue: 1f, maxValue: 100000f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}