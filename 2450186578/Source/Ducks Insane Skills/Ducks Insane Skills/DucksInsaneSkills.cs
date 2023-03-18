using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using System.Reflection;
using Verse;
using Verse.AI;
using UnityEngine;
using System;

namespace DucksInsaneSkills
{
    public class DucksInsaneSkillsMod : Mod
    {
        public static DucksInsaneSkillsSettings settings;
        public DucksInsaneSkillsMod(ModContentPack content) : base(content)
        {
            DucksInsaneSkillsMod.settings = GetSettings<DucksInsaneSkillsSettings>();
            var harmony = new Harmony("net.ducks.rimworld.mod.ducksskills");
            harmony.PatchAll();
        }

        public override string SettingsCategory() => "Ducks' Insane Skills";
        public override void DoSettingsWindowContents(Rect canvas) { settings.DoWindowContents(canvas); }
    }

    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch("XpRequiredToLevelUpFrom")]
    static class DucksSkills_XpRequiredToLevelUpFrom
    {
        public static bool Prefix(ref float __result, int startingLevel)
        {
            float currentlevel = startingLevel;
            float calc = (currentlevel + 1f) / 10f * ((currentlevel + 1f) / 10f + 1f) * 5f * 1000f;
            __result = calc;
            return false;
        }
    }

    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch("Interval")]
    static class DucksSkills_Interval
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch("Learn")]
    static class DucksSkills_Learn
    {
        public static FieldInfo _pawn;

        public static Pawn GetPawn(this SkillRecord _this)
        {
            if (_pawn == null)
            {
                _pawn = typeof(SkillRecord).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (_pawn == null)
            {
                Log.ErrorOnce("Unable to reflect SkillRecord.pawn!", 305432421);
            }
            return (Pawn)_pawn.GetValue(_this);
        }

        public static bool Prefix(SkillRecord __instance, float xp, bool direct = false)
        {
            //bool flag = false;
            if (xp > 0f)
            {
                xp *= __instance.LearnRateFactor(direct);
            }
            __instance.xpSinceLastLevel += xp;
            if (!direct)
            {
                __instance.xpSinceMidnight += xp;
            }
            if (DucksInsaneSkillsMod.settings.MaxLevel > 0 && __instance.levelInt == DucksInsaneSkillsMod.settings.MaxLevel && __instance.xpSinceLastLevel > __instance.XpRequiredForLevelUp - 1f)
            {
                __instance.xpSinceLastLevel = __instance.XpRequiredForLevelUp - 1f;
            }
            while (__instance.xpSinceLastLevel >= __instance.XpRequiredForLevelUp)
            {
                __instance.xpSinceLastLevel -= __instance.XpRequiredForLevelUp;
                __instance.levelInt++;
                //flag = true;
                if (__instance.levelInt == 14)
                {
                    if (__instance.passion == Passion.None)
                    {
                        TaleRecorder.RecordTale(TaleDefOf.GainedMasterSkillWithoutPassion, new object[]
                        {
                            GetPawn(__instance),
                            __instance.def
                        });
                    }
                    else
                    {
                        TaleRecorder.RecordTale(TaleDefOf.GainedMasterSkillWithPassion, new object[]
                        {
                            GetPawn(__instance),
                            __instance.def
                        });
                    }
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch("Level", MethodType.Setter)]
    static class DucksSkills_Level
    {
        public static bool Prefix(SkillRecord __instance, int value)
        {
            if (DucksInsaneSkillsMod.settings.MaxLevel > 0)
            {
                __instance.levelInt = Math.Min(value, DucksInsaneSkillsMod.settings.MaxLevel);
            }
            else
            {
                __instance.levelInt = value;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch("LearnRateFactor")]
    static class DucksSkills_LearnRateFactor
    {
        public static FieldInfo _pawn;

        public static Pawn GetPawn(this SkillRecord _this)
        {
            if (_pawn == null)
            {
                _pawn = typeof(SkillRecord).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            if (_pawn == null)
            {
                Log.ErrorOnce("Unable to reflect SkillRecord.pawn!", 305432421);
            }
            return (Pawn)_pawn.GetValue(_this);
        }

        public static bool Prefix(SkillRecord __instance, ref float __result)
        {
            float passion_mod = 1f;
            switch (__instance.passion)
            {
                case Passion.None:
                    passion_mod = 0.5f;
                    break;
                case Passion.Minor:
                    passion_mod = 1f;
                    break;
                case Passion.Major:
                    passion_mod = 1.5f;
                    break;
                    //default:
                    //throw new NotImplementedException("Passion level " + passion);
            }

            float saturation_mod = 1;

            if (DucksInsaneSkillsMod.settings.ValueSkillCap > 0f)
            {
                saturation_mod = Math.Min((1f / (Math.Max(__instance.xpSinceMidnight, 0) / passion_mod)) * DucksInsaneSkillsMod.settings.ValueSkillCap, 1);
            }

            if (DebugSettings.fastLearning)
            {
                saturation_mod = 200f;
            }

            __result = passion_mod * saturation_mod * GetPawn(__instance).GetStatValue(StatDefOf.GlobalLearningFactor, true);
            return false;
        }
    }

    [HarmonyPatch(typeof(SkillUI))]
    [HarmonyPatch("GetSkillDescription")]
    static class DucksSkills_GetSkillDescription
    {
        public static bool Prefix(ref string __result, ref SkillRecord sk)
        {

            StringBuilder stringBuilder = new StringBuilder();
            if (sk.TotallyDisabled)
            {
                stringBuilder.Append("DisabledLower".Translate().CapitalizeFirst());
            }
            else
            {
                stringBuilder.AppendLine(string.Concat(new object[]
                {
                    "Level".Translate() + " ",
                    sk.Level,
                    ": ",
                    sk.LevelDescriptor
                }));
                if (Current.ProgramState == ProgramState.Playing)
                {
                    string text = (sk.Level == 20) ? "Experience".Translate() : "ProgressToNextLevel".Translate();
                    stringBuilder.AppendLine(string.Concat(new object[]
                    {
                        text,
                        ": ",
                        sk.xpSinceLastLevel.ToString("F0"),
                        " / ",
                        sk.XpRequiredForLevelUp
                    }));
                }
                stringBuilder.Append("Passion".Translate() + ": ");
                switch (sk.passion)
                {
                    case Passion.None:
                        stringBuilder.Append("PassionNone".Translate(0.35f.ToStringPercent("F0")));
                        break;
                    case Passion.Minor:
                        stringBuilder.Append("PassionMinor".Translate(1f.ToStringPercent("F0")));
                        break;
                    case Passion.Major:
                        stringBuilder.Append("PassionMajor".Translate(1.5f.ToStringPercent("F0")));
                        break;
                }

                float passion_mod = 1f;
                switch (sk.passion)
                {
                    case Passion.None:
                        passion_mod = 0.5f;
                        break;
                    case Passion.Minor:
                        passion_mod = 1f;
                        break;
                    case Passion.Major:
                        passion_mod = 1.5f;
                        break;
                    default:
                        throw new NotImplementedException("Passion level " + sk.passion);
                }

                float saturation_mod = 1;

                if (DucksInsaneSkillsMod.settings.ValueSkillCap > 0f)
                {
                    saturation_mod = Math.Min((1f / (Math.Max(sk.xpSinceMidnight, 0) / passion_mod)) * DucksInsaneSkillsMod.settings.ValueSkillCap, 1);
                }

                if (saturation_mod < 1)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append("LearnedMaxToday".Translate(new object[]
                    {
                            Math.Floor(sk.xpSinceMidnight),
                            DucksInsaneSkillsMod.settings.ValueSkillCap*passion_mod,
                            saturation_mod.ToStringPercent("F0")
                    }));
                }
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append(sk.def.description);
            __result = stringBuilder.ToString();
            return false;
        }
    }

    [HarmonyPatch(typeof(QualityUtility))]
    [HarmonyPatch(nameof(QualityUtility.GenerateQualityCreatedByPawn))]
    [HarmonyPatch(new Type[]{ typeof(int), typeof(bool) })]
    static class DucksSkills_GenerateQualityCreatedByPawn
    {
        public static bool Prefix(ref QualityCategory __result, int relevantSkillLevel, bool inspired)
        {
            System.Random rng = new System.Random();

            float relevantSkillLevelFloat = relevantSkillLevel;

            float modifier = relevantSkillLevelFloat;
            if (inspired)
            {
                modifier = (relevantSkillLevelFloat * 2f) + 10f;
            }

            float bySkill = -(1f / (0.009f * modifier + 0.155f)) + 7f; // for a very long time i forgot about the negative sign in my equation. goodbye 4 hours of my life

            float rngRoll = Math.Max(Rand.GaussianAsymmetric(bySkill, 0.5f, 0.5f), 0f);
            float rngLeftOver = rngRoll % 1;
            if (Rand.Value < rngLeftOver)
            {
                rngRoll = (int)Math.Floor(rngRoll) + 1;
            }
            else
            {
                rngRoll = (int)Math.Floor(rngRoll);
            }
            rngRoll = Math.Min(Math.Max(rngRoll, 0f), 6f);

            __result = (QualityCategory)rngRoll;
            return false;
        }
    }

    [HarmonyPatch(typeof(QualityUtility))]
    [HarmonyPatch(nameof(QualityUtility.SendCraftNotification))]
    static class DucksSkills_SendCraftNotification
    {
        public static bool Prefix(Thing thing, Pawn worker)
        {
            if (DucksInsaneSkillsMod.settings.MuteCraftNotifications)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Bill))]
    [HarmonyPatch("PawnAllowedToStartAnew")]
    static class DucksSkills_PawnAllowedToStartAnew
    {
        public static bool Prefix(Bill __instance, ref bool __result, Pawn p)
        {

            // === 1.2 patch ===
            //if (__instance.pawnRestriction != null)
            //{
            //    __result = __instance.pawnRestriction == p;
            //    return false;
            //}

            // === 1.3 patch ===
            if (__instance.PawnRestriction != null)
            {
                __result = __instance.PawnRestriction == p;
                return false;
            }
            if (__instance.SlavesOnly)
            {
                __result = p.IsSlave;
                return false;
            }

            if (__instance.recipe.workSkill != null)
            {
                int level = p.skills.GetSkill(__instance.recipe.workSkill).Level;
                if (level < __instance.allowedSkillRange.min)
                {
                    JobFailReason.Is("UnderAllowedSkill".Translate(__instance.allowedSkillRange.min), __instance.Label);
                    __result = false;
                    return false;
                }
                if (__instance.allowedSkillRange.max != 20 & level > __instance.allowedSkillRange.max)
                {
                    JobFailReason.Is("AboveAllowedSkill".Translate(__instance.allowedSkillRange.max), __instance.Label);
                    __result = false;
                    return false;
                }
            }
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(TradeUtility))]
    [HarmonyPatch("GetPricePlayerSell")]
    static class DucksSkills_GetPricePlayerSell
    {
        public static bool Prefix(ref float __result, Thing thing, float priceFactorSell_TraderPriceType, float priceGain_PlayerNegotiator, float priceGain_FactionBase, TradeCurrency currency = TradeCurrency.Silver)
        {
            if (currency == TradeCurrency.Favor)
            {
                __result = thing.RoyalFavorValue;
                return false;
            }
            float statValue = thing.GetStatValue(StatDefOf.SellPriceFactor, true);
            float num = 0.6f * priceFactorSell_TraderPriceType * statValue * (1f - Find.Storyteller.difficultyValues.tradePriceFactorLoss);
            num *= 1f + priceGain_PlayerNegotiator + priceGain_FactionBase;
            num = Mathf.Clamp(num, 0.01f,1f);
            num = thing.MarketValue * num;
            if (num > 99.5f)
            {
                num = Mathf.Round(num);
            }
           __result = num;
            return false;
        }
    }

    [HarmonyPatch(typeof(TradeUtility))]
    [HarmonyPatch("GetPricePlayerBuy")]
    static class DucksSkills_GetPricePlayerBuy
    {
        public static bool Prefix(ref float __result, Thing thing, float priceFactorBuy_TraderPriceType, float priceGain_PlayerNegotiator, float priceGain_FactionBase)
        {
            float num = 1.4f * priceFactorBuy_TraderPriceType * (1f + Find.Storyteller.difficultyValues.tradePriceFactorLoss);
            num *= 1f - priceGain_PlayerNegotiator - priceGain_FactionBase;
            num = Mathf.Max(num, 1f);
            num = thing.MarketValue * num;
            if (num > 99.5f)
            {
                num = Mathf.Round(num);
            }
            __result = num;
            return false;
        }
    }
}

        