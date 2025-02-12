﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;

namespace XenobionicPatcher {
    public class ExtraHediffStats {
        static StatCategoryDef category;

        public static IEnumerable<StatDrawEntry> SpecialDisplayStats(HediffDef hediff, StatRequest req) {
            category = DefDatabase<StatCategoryDef>.GetNamed("Basics");

            // TODO: Breakdown of comps / HediffGivers

            yield return HediffCategoryStat(hediff);

            if (hediff.isBad) {
                yield return new StatDrawEntry(
                    category:    category,
                    label:       "Stat_Hediff_Tendable_Name".Translate(),
                    reportText:  "Stat_Hediff_Tendable_Desc".Translate(),
                    valueString: hediff.tendable.ToStringYesNo(),
                    displayPriorityWithinCategory: 4975
                );

                yield return new StatDrawEntry(
                    category:    category,
                    label:       "Stat_Hediff_Immunizable_Name".Translate(),
                    reportText:  "Stat_Hediff_Immunizable_Desc".Translate(),
                    valueString: hediff.PossibleToDevelopImmunityNaturally().ToStringYesNo(),
                    displayPriorityWithinCategory: 4970
                );

                bool canBeLethal = hediff.lethalSeverity > 0 || (hediff.stages != null && hediff.stages.Any( s => s.lifeThreatening ));

                yield return new StatDrawEntry(
                    category:    category,
                    label:       "Stat_Hediff_Lethal_Name".Translate(),
                    reportText:  "Stat_Hediff_Lethal_Desc".Translate(),
                    valueString: canBeLethal.ToStringYesNo(),
                    displayPriorityWithinCategory: 4965
                );

                yield return new StatDrawEntry(
                    category:    category,
                    label:       "Stat_Hediff_Curable_Name".Translate(),
                    reportText:  "Stat_Hediff_Curable_Desc".Translate(),
                    valueString: 
                        hediff.everCurableByItem && hediff.cureAllAtOnceIfCuredByItem ?
                        "Stat_Hediff_CurableAllAtOnce".Translate().ToString() :
                        hediff.everCurableByItem.ToStringYesNo()
                    ,
                    displayPriorityWithinCategory: 4960
                );
            }

            if ((!hediff.stages.NullOrEmpty() && hediff.stages.Count > 1) || hediff.maxSeverity < float.MaxValue) {
                if (hediff.minSeverity > 0) yield return new StatDrawEntry(
                    category:    category,
                    label:       "Stat_Hediff_MinSeverity_Name".Translate(),
                    reportText:  "Stat_Hediff_MinSeverity_Desc".Translate(),
                    valueString: hediff.minSeverity.ToStringPercent(),
                    displayPriorityWithinCategory: 4955
                );

                if (hediff.initialSeverity != 0.5f) yield return new StatDrawEntry(
                    category:    category,
                    label:       "Stat_Hediff_InitialSeverity_Name".Translate(),
                    reportText:  "Stat_Hediff_InitialSeverity_Desc".Translate(),
                    valueString: hediff.initialSeverity.ToStringPercent(),
                    displayPriorityWithinCategory: 4950
                );

                if (hediff.lethalSeverity > 0) yield return new StatDrawEntry(
                    category:    category,
                    label:       "Stat_Hediff_LethalSeverity_Name".Translate(),
                    reportText:  "Stat_Hediff_LethalSeverity_Desc".Translate(),
                    valueString: hediff.lethalSeverity.ToStringPercent(),
                    displayPriorityWithinCategory: 4945
                );
                else if (hediff.maxSeverity < float.MaxValue) yield return new StatDrawEntry(
                    category:    category,
                    label:       "Stat_Hediff_MaxSeverity_Name".Translate(),
                    reportText:  "Stat_Hediff_MaxSeverity_Desc".Translate(),
                    valueString: hediff.maxSeverity.ToStringPercent(),
                    displayPriorityWithinCategory: 4945
                );
            }

            if (hediff.chanceToCauseNoPain > 0) yield return new StatDrawEntry(
                category:    category,
                label:       "Stat_Hediff_ChanceToCauseNoPain_Name".Translate(),
                reportText:  "Stat_Hediff_ChanceToCauseNoPain_Desc".Translate(),
                valueString: hediff.chanceToCauseNoPain.ToStringPercent(),
                displayPriorityWithinCategory: 4940
            );

            if (hediff.causesNeed != null) yield return new StatDrawEntry(
                category:    category,
                label:       "CreatesNeed".Translate(),
                reportText:  "Stat_Hediff_CausesNeed_Desc".Translate(),
                valueString: hediff.causesNeed.LabelCap,
                hyperlinks:  new[] { new Dialog_InfoCard.Hyperlink(hediff.causesNeed) },
                displayPriorityWithinCategory: 4935
            );

            if (!hediff.disablesNeeds.NullOrEmpty()) yield return new StatDrawEntry(
                category:    category,
                label:       "Stat_Hediff_DisablesNeeds_Name".Translate(),
                reportText:  "Stat_Hediff_DisablesNeeds_Desc".Translate(),
                valueString: GenText.ToCommaList( hediff.disablesNeeds.Select(nd => nd.LabelCap.ToString()) ),
                hyperlinks:  hediff.disablesNeeds.Select(nd => new Dialog_InfoCard.Hyperlink(nd)).ToArray(),
                displayPriorityWithinCategory: 4930
            );

            if (hediff.addedPartProps != null) {
                StatCategoryDef implantCategory = DefDatabase<StatCategoryDef>.GetNamed("Implant");
                var props = hediff.addedPartProps;

                if (hediff.spawnThingOnRemoved != null) yield return new StatDrawEntry(
                    category:    implantCategory,
                    label:       "Stat_Hediff_RelatedPart_Name".Translate(),
                    reportText:  "Stat_Hediff_RelatedPart_Desc".Translate(),
                    valueString: hediff.spawnThingOnRemoved.LabelCap,
                    hyperlinks:  new[] { new Dialog_InfoCard.Hyperlink(hediff.spawnThingOnRemoved) },
                    displayPriorityWithinCategory: 5000
                );
                yield return new StatDrawEntry(
                    category:    implantCategory,
                    label:       "PartEfficiency".Translate(),
                    reportText:  "Stat_Thing_BodyPartEfficiency_Desc".Translate(),
                    valueString: props.partEfficiency.ToStringPercent(),
                    displayPriorityWithinCategory: 4950
                );
                yield return new StatDrawEntry(
                    category:    implantCategory,
                    label:       "Stat_Hediff_BetterThanNatural_Name".Translate(),
                    reportText:  "Stat_Hediff_BetterThanNatural_Desc".Translate(),
                    valueString: props.betterThanNatural.ToStringYesNo(),
                    displayPriorityWithinCategory: 4920
                );
                yield return new StatDrawEntry(
                    category:    implantCategory,
                    label:       "Stat_Hediff_Solid_Name".Translate(),
                    reportText:  "Stat_Hediff_Solid_Desc".Translate(),
                    valueString: props.solid.ToStringYesNo(),
                    displayPriorityWithinCategory: 4900
                );
            }

            if (hediff.priceImpact || hediff.priceOffset != 0) {
                float priceOffset = hediff.priceOffset;
                if (priceOffset == 0 && hediff.spawnThingOnRemoved != null)
                    priceOffset = hediff.spawnThingOnRemoved.BaseMarketValue;
                
                if (priceOffset >= 1.0 || priceOffset <= -1.0) yield return new StatDrawEntry(
                    category:    category,
                    label:       "Stat_Hediff_PriceOffset_Name".Translate(),
                    reportText:  "Stat_Hediff_PriceOffset_Desc".Translate(),
                    valueString: priceOffset.ToStringMoneyOffset(),
                    displayPriorityWithinCategory: 5500
                );
            }

            /* TODO: New stats:
             * 
             * Breakdown of HediffGivers
             * socialFightChanceFactor
             * mentalBreakMtbDays
             * mentalStateGivers
             * destroyPart
             * 
             * TODO: Move these to HediffStatsUtility.SpecialDisplayStats
             */


            // NOTE: Core RimWorld already adds the effects of single stage Hediffs

            // Multi-stage stats
            if (!hediff.stages.NullOrEmpty() && hediff.stages.Count > 1) {
                
                for (int i = 0; i < hediff.stages.Count; i++) {
                    HediffStage stage = hediff.stages[i];
                    
                    // If a stage wants to be invisible, keep it that way
                    if (!stage.becomeVisible) continue;

                    string stageLabelCap   = stage.label?.CapitalizeFirst() ?? "";
                    string categoryDefName = hediff.defName + "-" + stageLabelCap + i;
                    string minSeverityPer  = stage.minSeverity.ToStringPercent();

                    // List each stage as its own StatCategoryDef
                    var categoryDef = DefDatabase<StatCategoryDef>.GetNamed(categoryDefName, false);
                    if (categoryDef == null) {
                        categoryDef = new StatCategoryDef {
                            defName        = categoryDefName,
                            displayOrder   = 201 + i,
                            modContentPack = XenobionicPatcher.Base.Instance.ModContentPack,
                        };

                        // We need to make sure these are distinct strings, even if the defName is unique.
                        // XXX: This is kind of dirty from an I18N POV.
                        string label     = StatCategoryDefOf.CapacityEffects.label;
                        string minPerStr = " (" + minSeverityPer + ")";

                        if (stageLabelCap.Length > 0) label += " - " + stageLabelCap;
                        label += minPerStr;
                        categoryDef.label = label;

                        DefGenerator.AddImpliedDef(categoryDef);
                    }

                    // Blend stage stats into the category
                    foreach (StatDrawEntry value in stage.SpecialDisplayStats()) {
                        value.category = categoryDef;
                        yield return value;
                    }
                }
            }
        }

        public static StatDrawEntry HediffCategoryStat (HediffDef hediff) {
            string typeLangKey = null;
            foreach (System.Type categoryType in new[] {
                // Not all of the base types, but most of them that fit a broader category
                typeof(Hediff_Addiction), typeof(Hediff_High), typeof(Hediff_AddedPart), typeof(Hediff_Implant), typeof(Hediff_Injury), typeof(Hediff_MissingPart), typeof(Hediff_Psylink),
            } ) {
                if (Helpers.IsSupertypeOf(categoryType, hediff.hediffClass)) {
                    typeLangKey = categoryType.Name;
                    typeLangKey = Regex.Replace(typeLangKey, @"Hediff_", "");
                    typeLangKey = Regex.Replace(typeLangKey, @"_(\w)", m => m.Groups[1].Value.ToUpper());  // snake_case to CamelCase
                    break;
                }
            }

            if (typeLangKey == null) typeLangKey =
                hediff.countsAsAddedPartOrImplant || hediff.addedPartProps != null ? "BodyAugment" :
                hediff.displayWound     ? "Wound" :
                hediff.chronic          ? "ChronicDisease" :
                hediff.makesSickThought ? "Sickness" :
                hediff.isBad            ? "Affliction" :
                "Condition"
            ;
            
            // See comment on ExtraSurgeryStats.SurgeryCategoryStat
            string backupText = GenText.CapitalizeFirst( GenText.SplitCamelCase(typeLangKey).ToLower() );

            typeLangKey = "Stat_Hediff_HediffType_" + typeLangKey;
            string type = typeLangKey.CanTranslate() ? typeLangKey.Translate() : backupText.Translate();

            return new StatDrawEntry(
                category:    category,
                label:       "Stat_Hediff_HediffType_Name".Translate(),
                reportText:  "Stat_Hediff_HediffType_Desc".Translate(),
                valueString: type,
                displayPriorityWithinCategory: 4999
            );
        }
    }
}
