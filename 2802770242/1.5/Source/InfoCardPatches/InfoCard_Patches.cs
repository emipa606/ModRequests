using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace InfoCardPatches
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("InfoCardPatches.Mod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.ShouldShowFor))]
    public static class StatWorker_ShouldShowFor_Patch
    {
        public static bool Prefix(ref bool __result, StatWorker __instance, StatRequest req)
        {
            if (Current.Game?.World?.factionManager is null)
            {
                __result = ShouldShowFor(__instance, req);
                return false;
            }
            return true;
        }

        public static bool ShouldShowFor(StatWorker __instance, StatRequest req)
        {
            if (__instance.stat.alwaysHide)
            {
                return false;
            }
            var def = req.Def;
            if (!__instance.stat.showIfUndefined && !req.StatBases.StatListContains(__instance.stat))
            {
                return false;
            }
            if (!__instance.stat.CanShowWithLoadedMods())
            {
                return false;
            }
            if (req.Thing is Pawn pawn)
            {
                if (pawn.health != null && !__instance.stat.showIfHediffsPresent.NullOrEmpty())
                {
                    for (int i = 0; i < __instance.stat.showIfHediffsPresent.Count; i++)
                    {
                        if (!pawn.health.hediffSet.HasHediff(__instance.stat.showIfHediffsPresent[i]))
                        {
                            return false;
                        }
                    }
                }
                if (__instance.stat.showOnSlavesOnly && !pawn.IsSlave)
                {
                    return false;
                }
            }
            if (__instance.stat == StatDefOf.MaxHitPoints && req.HasThing)
            {
                return false;
            }
            if (!__instance.stat.showOnUntradeables && !StatWorker.DisplayTradeStats(req))
            {
                return false;
            }
            var thingDef = def as ThingDef;
            if (thingDef != null)
            {
                if (thingDef.category == ThingCategory.Pawn)
                {
                    if (!__instance.stat.showOnPawns)
                    {
                        return false;
                    }
                    if (!__instance.stat.showOnHumanlikes && thingDef.race.Humanlike)
                    {
                        return false;
                    }
                    if (!__instance.stat.showOnNonWildManHumanlikes && thingDef.race.Humanlike && !((req.Thing as Pawn)?.IsWildMan() ?? false))
                    {
                        return false;
                    }
                    if (!__instance.stat.showOnAnimals && thingDef.race.Animal)
                    {
                        return false;
                    }
                    if (!__instance.stat.showOnMechanoids && thingDef.race.IsMechanoid)
                    {
                        return false;
                    }
                    if (req.Thing is Pawn pawn2 && !__instance.stat.showDevelopmentalStageFilter.Has(pawn2.DevelopmentalStage))
                    {
                        return false;
                    }
                }
                if (!__instance.stat.showOnUnhaulables && !thingDef.EverHaulable && !thingDef.Minifiable)
                {
                    return false;
                }
            }
            if (__instance.stat.category == StatCategoryDefOf.BasicsPawn || __instance.stat.category == StatCategoryDefOf.BasicsPawnImportant
                || __instance.stat.category == StatCategoryDefOf.PawnCombat)
            {
                if (thingDef != null)
                {
                    return thingDef.category == ThingCategory.Pawn;
                }
                return false;
            }
            if (__instance.stat.category == StatCategoryDefOf.PawnMisc || __instance.stat.category == StatCategoryDefOf.PawnSocial || __instance.stat.category == StatCategoryDefOf.PawnWork)
            {
                if (thingDef == null || thingDef.category != ThingCategory.Pawn)
                {
                    return false;
                }
                if (req.Thing is Pawn pawn3)
                {
                    if (pawn3.IsColonyMech && __instance.stat.showOnPlayerMechanoids)
                    {
                        return true;
                    }
                    if (__instance.stat.showOnPawnKind.NotNullAndContains(pawn3.kindDef))
                    {
                        return true;
                    }
                }
                return thingDef.race.Humanlike;
            }
            if (__instance.stat.category == StatCategoryDefOf.Building)
            {
                if (thingDef == null)
                {
                    return false;
                }
                if (__instance.stat == StatDefOf.DoorOpenSpeed)
                {
                    return thingDef.IsDoor;
                }
                if (!__instance.stat.showOnNonWorkTables && !thingDef.IsWorkTable)
                {
                    return false;
                }
                return thingDef.category == ThingCategory.Building;
            }
            if (__instance.stat.category == StatCategoryDefOf.Apparel)
            {
                if (thingDef != null)
                {
                    if (!thingDef.IsApparel)
                    {
                        return thingDef.category == ThingCategory.Pawn;
                    }
                    return true;
                }
                return false;
            }
            if (__instance.stat.category == StatCategoryDefOf.Weapon)
            {
                if (thingDef != null)
                {
                    if (!thingDef.IsMeleeWeapon)
                    {
                        return thingDef.IsRangedWeapon;
                    }
                    return true;
                }
                return false;
            }
            if (__instance.stat.category == StatCategoryDefOf.Weapon_Ranged)
            {
                return thingDef?.IsRangedWeapon ?? false;
            }
            if (__instance.stat.category == StatCategoryDefOf.Weapon_Melee)
            {
                return thingDef?.IsMeleeWeapon ?? false;
            }
            if (__instance.stat.category == StatCategoryDefOf.BasicsNonPawn || __instance.stat.category == StatCategoryDefOf.BasicsNonPawnImportant)
            {
                if (thingDef == null || thingDef.category != ThingCategory.Pawn)
                {
                    return !req.ForAbility;
                }
                return false;
            }
            if (__instance.stat.category == StatCategoryDefOf.Terrain)
            {
                return def is TerrainDef;
            }
            if (req.ForAbility)
            {
                return __instance.stat.category == StatCategoryDefOf.Ability;
            }
            if (__instance.stat.category.displayAllByDefault)
            {
                return true;
            }
            Log.Error(string.Concat("Unhandled case: ", __instance.stat, ", ", def));
            return false;
        }
    }

    [HarmonyPatch(typeof(PlantProperties), "SpecialDisplayStats")]
    public static class PlantProperties_SpecialDisplayStats_Patch
    {
        public static bool Prefix(ref IEnumerable<StatDrawEntry> __result, PlantProperties __instance)
        {
            if (Current.Game?.World?.factionManager is null)
            {
                __result = SpecialDisplayStats(__instance).ToList();
                return false;
            }
            return true;
        }

        private static IEnumerable<StatDrawEntry> SpecialDisplayStats(PlantProperties __instance)
        {
            if (__instance.sowMinSkill > 0)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MinGrowingSkillToSow".Translate(), __instance.sowMinSkill.ToString(),
                    "Stat_Thing_Plant_MinGrowingSkillToSow_Desc".Translate(), 4151);
            }
            string attributes = "";
            if (__instance.Harvestable)
            {
                string text = "Harvestable".Translate();
                if (!attributes.NullOrEmpty())
                {
                    attributes += ", ";
                    text = text.UncapitalizeFirst();
                }
                attributes += text;
            }
            if (__instance.LimitedLifespan)
            {
                string text2 = "LimitedLifespan".Translate();
                if (!attributes.NullOrEmpty())
                {
                    attributes += ", ";
                    text2 = text2.UncapitalizeFirst();
                }
                attributes += text2;
            }
            if (!__instance.isStump)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "GrowingTime".Translate(), __instance.growDays.ToString("0.##") + " " + "Days".Translate(), "GrowingTimeDesc".Translate(), 4158);
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "FertilityRequirement".Translate(), __instance.fertilityMin.ToStringPercent(), "Stat_Thing_Plant_FertilityRequirement_Desc".Translate(), 4156);
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "FertilitySensitivity".Translate(), __instance.fertilitySensitivity.ToStringPercent(), "Stat_Thing_Plant_FertilitySensitivity_Desc".Translate(), 4155);
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "LightRequirement".Translate(), __instance.growMinGlow.ToStringPercent(), "Stat_Thing_Plant_LightRequirement_Desc".Translate(), 4154);
            }
            if (!attributes.NullOrEmpty())
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Attributes".Translate(), attributes, "Stat_Thing_Plant_Attributes_Desc".Translate(), 4157);
            }
            if (__instance.LimitedLifespan)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "LifeSpan".Translate(), __instance.LifespanDays.ToString("0.##") + " " + "Days".Translate(), "Stat_Thing_Plant_LifeSpan_Desc".Translate(), 4150);
            }
            if (__instance.harvestYield > 0f)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Stat_Thing_Plant_HarvestYield_Desc".Translate());
                stringBuilder.AppendLine();
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "HarvestYield".Translate(), Mathf.CeilToInt(__instance.harvestYield).ToString("F0"), stringBuilder.ToString(), 4150, null, __instance.GetHarvestYieldHyperlinks());
            }
            if (!__instance.isStump)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MinGrowthTemperature".Translate(), 0f.ToStringTemperature(), "Stat_Thing_Plant_MinGrowthTemperature_Desc".Translate(), 4152);
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MaxGrowthTemperature".Translate(), 58f.ToStringTemperature(), "Stat_Thing_Plant_MaxGrowthTemperature_Desc".Translate(), 4153);
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetExplanationFinalizePart))]
    public static class StatWorker_GetExplanationFinalizePart_Patch
    {
        public static bool Prefix(ref string __result, StatWorker __instance, StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            if (Current.Game?.World?.factionManager is null)
            {
                __result = GetExplanationFinalizePart(__instance, req, numberSense, finalVal);
                return false;
            }
            return true;
        }

        public static string GetExplanationFinalizePart(StatWorker __instance, StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            var stringBuilder = new StringBuilder();
            if (__instance.stat.parts != null)
            {
                for (int i = 0; i < __instance.stat.parts.Count; i++)
                {
                    string text = __instance.stat.parts[i].ExplanationPart(req);
                    if (!text.NullOrEmpty())
                    {
                        stringBuilder.AppendLine(text);
                    }
                }
            }
            if (__instance.stat.postProcessCurve != null)
            {
                float value = __instance.GetValue(req, applyPostProcess: false);
                float num = __instance.stat.postProcessCurve.Evaluate(value);
                if (!Mathf.Approximately(value, num))
                {
                    string text2 = __instance.ValueToString(value, finalized: false);
                    string text3 = __instance.stat.ValueToString(num, numberSense);
                    stringBuilder.AppendLine("StatsReport_PostProcessed".Translate() + ": " + text2 + " => " + text3);
                }
            }
            if (__instance.stat.postProcessStatFactors != null)
            {
                stringBuilder.AppendLine("StatsReport_OtherStats".Translate());
                for (int j = 0; j < __instance.stat.postProcessStatFactors.Count; j++)
                {
                    var statDef = __instance.stat.postProcessStatFactors[j];
                    stringBuilder.AppendLine($"    {statDef.LabelCap}: x{statDef.Worker.GetValue(req).ToStringPercent()}");
                }
            }
            stringBuilder.Append("StatsReport_FinalValue".Translate() + ": " + __instance.stat.ValueToString(finalVal, __instance.stat.toStringNumberSense));
            return stringBuilder.ToString();
        }
    }

    [HarmonyPatch(typeof(RaceProperties), nameof(RaceProperties.SpecialDisplayStats))]
    public static class RaceProperties_SpecialDisplayStats_Patch
    {
        public static bool Prefix(ref IEnumerable<StatDrawEntry> __result, RaceProperties __instance, ThingDef parentDef, StatRequest req)
        {
            if (Current.Game?.World?.factionManager is null)
            {
                __result = SpecialDisplayStats(__instance, parentDef, req).ToList();
                return false;
            }
            return true;
        }

        public static IEnumerable<StatDrawEntry> SpecialDisplayStats(RaceProperties __instance, ThingDef parentDef, StatRequest req)
        {
            var pawn = req.Pawn ?? (req.Thing as Pawn);
            if (!ModsConfig.BiotechActive || !__instance.Humanlike || pawn?.genes == null || pawn.genes.Xenotype == XenotypeDefOf.Baseliner)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Race".Translate(), parentDef.LabelCap, parentDef.description, 2100);
            }
            if (!parentDef.race.IsMechanoid)
            {
                string text = __instance.foodType.ToHumanString().CapitalizeFirst();
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Diet".Translate(), text, "Stat_Race_Diet_Desc".Translate(text), 1500);
            }
            if (req.Thing is Pawn pawn2 && pawn2.needs?.food != null)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "FoodConsumption".Translate(),
                    RaceProperties.NutritionEatenPerDay(pawn2), RaceProperties.NutritionEatenPerDayExplanation(pawn2), 1600);
            }
            if (parentDef.race.leatherDef != null)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "LeatherType".Translate(), parentDef.race.leatherDef.LabelCap, "Stat_Race_LeatherType_Desc".Translate(), 3550, null, new Dialog_InfoCard.Hyperlink[1]
                {
                    new Dialog_InfoCard.Hyperlink(parentDef.race.leatherDef)
                });
            }
            if (parentDef.race.Animal || __instance.wildness > 0f)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Wildness".Translate(), __instance.wildness.ToStringPercent(), TrainableUtility.GetWildnessExplanation(parentDef), 2050);
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "HarmedRevengeChance".Translate(), parentDef.race.manhunterOnDamageChance.ToStringPercent(), "HarmedRevengeChanceExplanation".Translate(), 510);
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "TameFailedRevengeChance".Translate(), parentDef.race.manhunterOnTameFailChance.ToStringPercent(), "Stat_Race_Animal_TameFailedRevengeChance_Desc".Translate(), 511);
            }
            if ((int)__instance.intelligence < 2 && __instance.trainability != null)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Trainability".Translate(), __instance.trainability.LabelCap, "Stat_Race_Trainability_Desc".Translate(), 2500);
            }
            yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "StatsReport_LifeExpectancy".Translate(), __instance.lifeExpectancy.ToStringByStyle(ToStringStyle.Integer), "Stat_Race_LifeExpectancy_Desc".Translate(), 2000);
            if (parentDef.race.Animal || __instance.FenceBlocked)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "StatsReport_BlockedByFences".Translate(), __instance.FenceBlocked ? "Yes".Translate() : "No".Translate(), "Stat_Race_BlockedByFences_Desc".Translate(), 2040);
            }
            if (parentDef.race.Animal)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "PackAnimal".Translate(), __instance.packAnimal ? "Yes".Translate() : "No".Translate(), "PackAnimalExplanation".Translate(), 2202);
                if (req.Thing is Pawn pawn3 && pawn3.gender != 0)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Sex".Translate(), pawn3.gender.GetLabel(animal: true).CapitalizeFirst(), pawn3.gender.GetLabel(animal: true).CapitalizeFirst(), 2208);
                }
                if (parentDef.race.nuzzleMtbHours > 0f)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnSocial, "NuzzleInterval".Translate(), Mathf.RoundToInt(parentDef.race.nuzzleMtbHours * 2500f).ToStringTicksToPeriod(), "NuzzleIntervalExplanation".Translate(), 500);
                }
                if (parentDef.race.roamMtbDays.HasValue)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "StatsReport_RoamInterval".Translate(), Mathf.RoundToInt(parentDef.race.roamMtbDays.Value * 60000f).ToStringTicksToPeriod(), "Stat_Race_RoamInterval_Desc".Translate(), 2030);
                }
                foreach (var item in AnimalProductionUtility.AnimalProductionStats(parentDef))
                {
                    yield return item;
                }
            }
            if (!ModsConfig.BiotechActive || !__instance.IsMechanoid)
            {
                yield break;
            }
            yield return new StatDrawEntry(StatCategoryDefOf.Mechanoid, "MechWeightClass".Translate(), __instance.mechWeightClass.ToStringHuman().CapitalizeFirst(), "MechWeightClassExplanation".Translate(), 500);
            var thingDef = MechanitorUtility.RechargerForMech(parentDef);
            if (thingDef != null)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Mechanoid, "StatsReport_RechargerNeeded".Translate(), thingDef.LabelCap, "StatsReport_RechargerNeeded_Desc".Translate(), 503, null, new Dialog_InfoCard.Hyperlink[1]
                {
                    new Dialog_InfoCard.Hyperlink(thingDef)
                });
            }
            foreach (var item2 in MechWorkUtility.SpecialDisplayStats(parentDef, req))
            {
                yield return item2;
            }
        }
    }

    [HarmonyPatch(typeof(BuildingProperties), nameof(BuildingProperties.SpecialDisplayStats))]
    public static class BuildingProperties_SpecialDisplayStats_Patch
    {
        private static List<string> tmpFenceBlockedAnimals = new List<string>();
        public static bool Prefix(ref IEnumerable<StatDrawEntry> __result, BuildingProperties __instance, ThingDef parentDef, StatRequest req)
        {
            if (Current.Game?.World?.factionManager is null)
            {
                __result = SpecialDisplayStats(__instance, parentDef, req).ToList();
                return false;
            }
            return true;
        }

        public static IEnumerable<StatDrawEntry> SpecialDisplayStats(BuildingProperties __instance, ThingDef parentDef, StatRequest req)
        {
            if (__instance.joyKind != null)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("Stat_RecreationType_Desc".Translate());
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("Stat_JoyKind_AllTypes".Translate() + ":");
                foreach (var allDef in DefDatabase<JoyKindDef>.AllDefs)
                {
                    stringBuilder.AppendLine("  - " + allDef.LabelCap);
                }
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_JoyKind".Translate(), __instance.joyKind.LabelCap, stringBuilder.ToString(), 4750, __instance.joyKind.LabelCap);
            }
            if (parentDef.Minifiable)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_WorkToUninstall".Translate(), __instance.uninstallWork.ToStringWorkAmount(), "Stat_Thing_WorkToUninstall_Desc".Translate(), 1102);
            }
            if (typeof(Building_TrapDamager).IsAssignableFrom(parentDef.thingClass))
            {
                float f = StatDefOf.TrapMeleeDamage.Worker.GetValue(req) * 0.015f;
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "TrapArmorPenetration".Translate(), f.ToStringPercent(), "ArmorPenetrationExplanation".Translate(), 3000);
            }
            if (__instance.isFence)
            {
                var taggedString = "Stat_Thing_Fence_Desc".Translate();
                tmpFenceBlockedAnimals.Clear();
                tmpFenceBlockedAnimals.AddRange(from k in DefDatabase<PawnKindDef>.AllDefs
                                                where k.RaceProps.Animal && k.RaceProps.FenceBlocked
                                                select k into g
                                                select g.LabelCap.Resolve() into s
                                                orderby s
                                                select s);
                taggedString += ":\n\n";
                taggedString += tmpFenceBlockedAnimals.ToLineList("- ");
                yield return new StatDrawEntry(StatCategoryDefOf.Building, "StatsReport_Fence".Translate(), "Yes".Translate(), taggedString, 4800);
                tmpFenceBlockedAnimals.Clear();
            }
            if (!ModsConfig.IdeologyActive)
            {
                yield break;
            }
        }
    }

    [HarmonyPatch(typeof(StatsReportUtility), "Reset")]
    public static class StatsReportUtility_Reset_Patch
    {
        public static bool Prefix()
        {
            if (Current.Game?.World?.factionManager is null)
            {
                Reset();
                return false;
            }
            return true;
        }

        public static void Reset()
        {
            StatsReportUtility.scrollPosition = default;
            StatsReportUtility.scrollPositionRightPanel = default;
            StatsReportUtility.selectedEntry = null;
            StatsReportUtility.scrollPositioner.Arm(armed: false);
            StatsReportUtility.mousedOverEntry = null;
            StatsReportUtility.cachedDrawEntries.Clear();
            StatsReportUtility.quickSearchWidget.Reset();
            PermitsCardUtility.selectedPermit = null;
            PermitsCardUtility.selectedFaction = null;
        }
    }
}
