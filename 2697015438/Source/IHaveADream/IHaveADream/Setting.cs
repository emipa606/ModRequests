using RimWorld;
using HarmonyLib;
using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace HDream
{
    public class SettingProperty : ModSettings
    {

        public float wishFrequencyFactor = 1f;
        public float wishPendingDebuffFactor = 1f;
        public float wishPendindStackMultiplierOffset = 0f;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref wishFrequencyFactor, "wishFrequencyFactor", 1f);
            Scribe_Values.Look(ref wishPendingDebuffFactor, "wishPendingFactor", 1f);
            Scribe_Values.Look(ref wishPendindStackMultiplierOffset, "wishPendindStackMultiplierOffset", 0f);
            base.ExposeData();
        }

    }



    public class SettingMenu : Mod
    {
        public static SettingProperty settings;

        public SettingMenu(ModContentPack content) : base(content)
        {
            settings = GetSettings<SettingProperty>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("\n");
            listingStandard.Label(TranslationKey.SETTING_DESC_WISH_FREQUENCY_FACTOR.Translate());
            settings.wishFrequencyFactor = Mathf.Round(listingStandard.Slider(settings.wishFrequencyFactor, 0.05f, 4f) * 100) / 100;
            listingStandard.Label(TranslationKey.SETTING_VALUE.Translate(settings.wishFrequencyFactor.ToString("F3"), "1"));
            listingStandard.Label("\n\n");
            listingStandard.Label(TranslationKey.SETTING_WARNING_WISH_PENDING.Translate());
            listingStandard.Label(TranslationKey.SETTING_DESC_WISH_PENDING_FACTOR.Translate());
            settings.wishPendingDebuffFactor = Mathf.Round(listingStandard.Slider(settings.wishPendingDebuffFactor, 0.2f, 2f) * 100) / 100;
            listingStandard.Label(TranslationKey.SETTING_VALUE.Translate(settings.wishPendingDebuffFactor.ToString("F3"), "1"));
            listingStandard.Label(TranslationKey.SETTING_DESC_WISH_PENDING_STACK_MULTIPLIER.Translate());
            settings.wishPendindStackMultiplierOffset = Mathf.Round(listingStandard.Slider(settings.wishPendindStackMultiplierOffset, -0.2f, 0.08f) * 1000) / 1000;
            listingStandard.Label(TranslationKey.SETTING_VALUE.Translate(settings.wishPendindStackMultiplierOffset.ToString("F3"), "0"));
            listingStandard.End();
            UpdatePendingDef();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Main.ModName;
        }


        private static float basePendindStackMultiplier = -1f;
        private static List<float> baseMoodDef = null;
        public static void UpdatePendingDef()
        {
            if (basePendindStackMultiplier == -1) basePendindStackMultiplier = HDThoughtDefOf.WishPending.stackedEffectMultiplier;
            if (baseMoodDef == null)
            {
                baseMoodDef = new List<float>();
                for (int i = 0; i < HDThoughtDefOf.WishPending.stages.Count; i++)
                {
                    baseMoodDef.Add(HDThoughtDefOf.WishPending.stages[i].baseMoodEffect);
                }
            }
            HDThoughtDefOf.WishPending.stackedEffectMultiplier = basePendindStackMultiplier + settings.wishPendindStackMultiplierOffset;
            for (int i = 0; i < baseMoodDef.Count; i++)
            {
                HDThoughtDefOf.WishPending.stages[i].baseMoodEffect = baseMoodDef[i] * settings.wishPendingDebuffFactor;
            }
        }
    }
}
