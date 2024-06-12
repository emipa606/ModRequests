// Nightvision NightVision Storage_Combat.cs
// 
// 24 10 2018
// 
// 24 10 2018

#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using NightVision.Harmony;
using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace NightVision
{
    public class Storage_Combat
    {
        public Storage_Combat()
        {
            LoadDefaultSettings();
        }

        public SettingOption<bool> CombatFeaturesEnabled;

        public SettingOption_WithPatch MeleeHitEffectsEnabled;

        public SettingOption<float> SurpriseAttackMultiplier;

        public SettingOption_WithPatch RangedHitEffectsEnabled;

        public SettingOption_WithPatch RangedCooldownEffectsEnabled;

        public SettingOption<bool> RangedCooldownLinkedToCaps;
        public SettingOption<IntRange> RangedCooldownMinAndMax;

        public SettingOption<int> HitCurviness;

        public SettingOption<int> DodgeCurviness;

        public void LoadSaveCommit()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Values.Look(ref CombatFeaturesEnabled.TempValue, "CombatFeaturesEnabled", true);

                Scribe_Values.Look(ref MeleeHitEffectsEnabled.TempValue, "MeleeHitEffectsEnabled", true);

                Scribe_Values.Look(ref SurpriseAttackMultiplier.TempValue, "SurpriseAttackMultiplier", 0.5f);

                Scribe_Values.Look(ref RangedHitEffectsEnabled.TempValue, "RangedHitEffectsEnabled", true);

                Scribe_Values.Look(ref RangedCooldownEffectsEnabled.TempValue, "RangedCooldownEffectEnabled", true);

                Scribe_Values.Look(ref RangedCooldownLinkedToCaps.TempValue, "RangedCooldownLinkedToCaps", true);

                Scribe_Values.Look(ref RangedCooldownMinAndMax.TempValue, "RangedCooldownMinMax",
                    RangedCooldownMinAndMax.StoredValue);

                Scribe_Values.Look(ref HitCurviness.TempValue, "HitCurviness", 2);

                Scribe_Values.Look(ref DodgeCurviness.TempValue, "DodgeCurviness", 3);
            }

            CombatFeaturesEnabled.Commit();
            MeleeHitEffectsEnabled.Commit();
            SurpriseAttackMultiplier.Commit();
            RangedHitEffectsEnabled.Commit();
            RangedCooldownEffectsEnabled.Commit();
            RangedCooldownLinkedToCaps.Commit();
            RangedCooldownMinAndMax.Commit();
            HitCurviness.Commit();
            DodgeCurviness.Commit();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Scribe_Values.Look(ref CombatFeaturesEnabled.StoredValue, "CombatFeaturesEnabled", true);

                Scribe_Values.Look(ref MeleeHitEffectsEnabled.StoredValue, "MeleeHitEffectsEnabled", true);

                Scribe_Values.Look(ref SurpriseAttackMultiplier.StoredValue, "SurpriseAttackMultiplier", 0.5f);

                Scribe_Values.Look(ref RangedHitEffectsEnabled.StoredValue, "RangedHitEffectsEnabled", true);

                Scribe_Values.Look(ref RangedCooldownEffectsEnabled.StoredValue, "RangedCooldownEffectEnabled", true);

                Scribe_Values.Look(ref RangedCooldownLinkedToCaps.StoredValue, "RangedCooldownLinkedToCaps", true);

                Scribe_Values.Look(ref RangedCooldownMinAndMax.StoredValue, "RangedCooldownMinMax",
                    LinkedRangedCooldownMinAndMax);

                Scribe_Values.Look(ref HitCurviness.StoredValue, "HitCurviness", 2);

                Scribe_Values.Look(ref DodgeCurviness.StoredValue, "DodgeCurviness", 3);
            }
        }

        public void LoadDefaultSettings()
        {
            CombatFeaturesEnabled = new SettingOption<bool>(Str_CombatSettings.CombatEnabled, true, null);

            MeleeHitEffectsEnabled = new SettingOption_WithPatch(
                Str_CombatSettings.MeleeHitEnabled,
                true,
                Str_CombatSettings.MeleeHitTip,
                new[]
                {
                    AccessTools.Method(typeof(Verb_MeleeAttack), "GetNonMissChance"),
                    AccessTools.Method(typeof(Verb_MeleeAttack), "GetDodgeChance")
                },
                new[] { HarmonyPatchType.Postfix, HarmonyPatchType.Postfix },
                new[]
                {
                    AccessTools.Method(typeof(VerbMeleeAttack_GetNonMissChance),
                        nameof(VerbMeleeAttack_GetNonMissChance.GetNonMissChance_Postfix)),
                    AccessTools.Method(typeof(VerbMeleeAttack_GetDodgeChance),
                        nameof(VerbMeleeAttack_GetDodgeChance.GetDodgeChance_Postfix))
                }
            );

            SurpriseAttackMultiplier = new SettingOption<float>(Str_CombatSettings.SAtkChance, 0.5f,
                tooltip: Str_CombatSettings.SAtkTip);

            RangedHitEffectsEnabled = new SettingOption_WithPatch(
                Str_CombatSettings.RangedHitEffectsEnabled,
                true,
                Str_CombatSettings.RangedHitTip,
                new[]
                {
                    AccessTools.Method(typeof(ShotReport), nameof(ShotReport.HitReportFor)),
                    AccessTools.Property(typeof(ShotReport), nameof(ShotReport.AimOnTargetChance_StandardTarget))
                        .GetGetMethod(),
                    AccessTools.Method(typeof(TooltipUtility), nameof(TooltipUtility.ShotCalculationTipString)),
                    AccessTools.Method(typeof(CastPositionFinder), "CastPositionPreference")
                },
                new[]
                {
                    HarmonyPatchType.Postfix, HarmonyPatchType.Postfix, HarmonyPatchType.Transpiler,
                    HarmonyPatchType.Transpiler
                },
                new[]
                {
                    AccessTools.Method(typeof(ShotReport_HitReportFor),
                        nameof(ShotReport_HitReportFor.HitReportFor_Postfix)),
                    AccessTools.Method(typeof(ShotReport_AimOnTargetChanceStandardTarget),
                        nameof(ShotReport_AimOnTargetChanceStandardTarget.Postfix)),
                    AccessTools.Method(
                        typeof(TooltipUtility_ShotCalculationTipString),
                        nameof(TooltipUtility_ShotCalculationTipString.ShotCalculationTipString_Transpiler)
                    ),
                    AccessTools.Method(typeof(CastPositionFinder_CastPositionPreference),
                        nameof(CastPositionFinder_CastPositionPreference.Transpiler))
                }
            );
            RangedCooldownEffectsEnabled = new SettingOption_WithPatch(
                Str_CombatSettings.RangedCDEnabled,
                true,
                Str_CombatSettings.RangedCDTip,
                AccessTools.Method(
                    typeof(VerbProperties),
                    nameof(VerbProperties.AdjustedCooldown),
                    new Type[] { typeof(Tool), typeof(Pawn), typeof(Thing) }
                ),
                HarmonyPatchType.Transpiler,
                AccessTools.Method(typeof(VerbProperties_AdjustedCooldown),
                    nameof(VerbProperties_AdjustedCooldown.Transpiler))
            );
            RangedCooldownLinkedToCaps = new SettingOption<bool>(Str_CombatSettings.RangedCDToCaps, true,
                tooltip: Str_CombatSettings.RangedCDToCapsTip);

            RangedCooldownMinAndMax = new SettingOption<IntRange>(
                Str_CombatSettings.RangedCDMinMax,
                tooltip: Str_CombatSettings.RangedCDMinMaxTip,
                value: LinkedRangedCooldownMinAndMax,
                callBack:
                () =>
                {
                    if (RangedCooldownLinkedToCaps.Value)
                    {
                        RangedCooldownMinAndMax.Value = LinkedRangedCooldownMinAndMax;
                    }
                }
            );

            HitCurviness = new SettingOption<int>(Str_CombatSettings.HitCurve, 2
                , tooltip: Str_CombatSettings.HitCurveTip
            );
            DodgeCurviness = new SettingOption<int>(Str_CombatSettings.DodgeCurve, 3,
                tooltip: Str_CombatSettings.DodgeCurveTip);
        }


        private static IntRange LinkedRangedCooldownMinAndMax => new IntRange((int)(100 / Settings.Store.MultiplierCaps.max),
            (int)(100 / Settings.Store.MultiplierCaps.min));
    }
}