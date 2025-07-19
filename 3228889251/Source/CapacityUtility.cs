// Karel Kroeze
// CapacityUtility.cs
// 2017-05-17

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace Fluffy
{
    // todo; consolidation and clean up of various helpers.
    // todo; lobby for xml implementation of capacity tags on bodyparts so we can get rid of the dictionary.
    [StaticConstructorOnStartup]
    public static class CapacityUtility
    {
        public static Dictionary<PawnCapacityDef, HashSet<BodyPartTagDef>> CapacityTags =
            new Dictionary<PawnCapacityDef, HashSet<BodyPartTagDef>>();

        private static MethodInfo _generateSurgeryOptionMethodInfo;

        static CapacityUtility()
        {
            HashSet<BodyPartTagDef> filtrationTags = new HashSet<BodyPartTagDef> {
                BodyPartTagDefOf.BloodFiltrationKidney,
                BodyPartTagDefOf.BloodFiltrationLiver,
                BodyPartTagDefOf.BloodFiltrationSource
            };
            CapacityTags.Add(PawnCapacityDefOf.BloodFiltration, filtrationTags);

            HashSet<BodyPartTagDef> pumpingTags = new HashSet<BodyPartTagDef> {
                BodyPartTagDefOf.BloodPumpingSource
            };
            CapacityTags.Add(PawnCapacityDefOf.BloodPumping, pumpingTags);

            HashSet<BodyPartTagDef> breathingTags = new HashSet<BodyPartTagDef> {
                BodyPartTagDefOf.BreathingPathway,
                BodyPartTagDefOf.BreathingSource,
                BodyPartTagDefOf.BreathingSourceCage
            };
            CapacityTags.Add(PawnCapacityDefOf.Breathing, breathingTags);

            HashSet<BodyPartTagDef> consciousnessTags = new HashSet<BodyPartTagDef> {
                BodyPartTagDefOf.ConsciousnessSource
            };
            CapacityTags.Add(PawnCapacityDefOf.Consciousness, consciousnessTags);

            HashSet<BodyPartTagDef> hearingTags = new HashSet<BodyPartTagDef> {
                BodyPartTagDefOf.HearingSource
            };
            CapacityTags.Add(PawnCapacityDefOf.Hearing, hearingTags);

            HashSet<BodyPartTagDef> manipulationTags = new HashSet<BodyPartTagDef> {
                BodyPartTagDefOf.ManipulationLimbCore,
                BodyPartTagDefOf.ManipulationLimbDigit,
                BodyPartTagDefOf.ManipulationLimbSegment
            };
            CapacityTags.Add(PawnCapacityDefOf.Manipulation, manipulationTags);

            HashSet<BodyPartTagDef> movingTags = new HashSet<BodyPartTagDef> {
                BodyPartTagDefOf.MovingLimbCore,
                BodyPartTagDefOf.MovingLimbDigit,
                BodyPartTagDefOf.MovingLimbSegment,
                BodyPartTagDefOf.Pelvis,
                BodyPartTagDefOf.Spine
            };
            CapacityTags.Add(PawnCapacityDefOf.Moving, movingTags);

            HashSet<BodyPartTagDef> sightTags = new HashSet<BodyPartTagDef> {
                BodyPartTagDefOf.SightSource
            };
            CapacityTags.Add(PawnCapacityDefOf.Sight, sightTags);

            HashSet<BodyPartTagDef> talkingTags = new HashSet<BodyPartTagDef> {
                BodyPartTagDefOf.TalkingPathway,
                BodyPartTagDefOf.TalkingSource,
                BodyPartTagDefOf.Tongue
            };
            CapacityTags.Add(PawnCapacityDefOf.Talking, talkingTags);

            HashSet<BodyPartTagDef> eatingTags = new HashSet<BodyPartTagDef> {
                BodyPartTagDefOf.EatingPathway,
                BodyPartTagDefOf.EatingSource
            };
            // CapacityTags.Add(PawnCapacityDefOf.Eating, eatingTags);

            HashSet<BodyPartTagDef> metabolismTags = new HashSet<BodyPartTagDef> {
                BodyPartTagDefOf.MetabolismSource
            };
            // CapacityTags.Add(PawnCapacityDefOf.Metabolism, metabolismTags);

            // try and make an educated guess for any other capacity added by mods
            foreach (PawnCapacityDef capacityDef in DefDatabase<PawnCapacityDef>.AllDefsListForReading)
            {

                if (CapacityTags.ContainsKey(capacityDef))
                {
                    continue;
                }

                // in Rimworld 1.5.4069 Eating and Metabolism have been removed from BodyPartTagDefOf for some reason...
                if (capacityDef.defName.Equals("Eating"))
                {
                    CapacityTags.Add(capacityDef, eatingTags);
                    continue;
                }

                if (capacityDef.defName.Equals("Metabolism"))
                {
                    CapacityTags.Add(capacityDef, metabolismTags);
                    continue;
                }

                HashSet<BodyPartTagDef> tags = new HashSet<BodyPartTagDef>();
                foreach (BodyPartTagDef tag in DefDatabase<BodyPartTagDef>.AllDefsListForReading.Where(
                    td => td.defName.Contains(capacityDef.defName)
                ))
                {
                    Log.Message(
                        $"Medical Tab :: Adding {tag.defName} to the list of required capacities for {capacityDef.defName}.");
                    tags.Add(tag);
                }

                if (tags.Count == 0)
                {
                    Log.Warning(
                        $"Medical Tab :: Capacity {capacityDef.defName} does not have any bodyPartTags associated with it. This may be intentional. For modders; if the tag defName contains the capacity defName the two will be linked.");
                }

                CapacityTags.Add(capacityDef, tags);
            }

            // spawn a message about orphan tags
            foreach (BodyPartTagDef tag in DefDatabase<BodyPartTagDef>.AllDefsListForReading)
            {
                bool used = false;
                foreach (HashSet<BodyPartTagDef> tagset in CapacityTags.Values)
                {
                    if (tagset.Contains(tag))
                    {
                        used = true;
                        break;
                    }
                }

                if (!used)
                {
                    Log.Warning(
                        $"Medical Tab :: Tag {tag.defName} is not associated with any pawnCapacity. This may be intentional. For modders; if the tag defName contains the capacity defName the two will be linked.");
                }
            }
        }


        public static IEnumerable<Hediff> GetDiseases(this Pawn pawn)
        {
            return GetPotentiallyLethalHediffs(pawn).Where(h => h.IsDisease());
        }

        public static bool IsDisease(this Hediff hediff)
        {
            return hediff.TryGetComp<HediffComp_Immunizable>() != null &&
                   hediff.def.PossibleToDevelopImmunityNaturally();
        }

        public static IEnumerable<Hediff> GetPotentiallyLethalHediffs(this Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                       .Where(h => h.Visible &&
                                   h.def.lethalSeverity > 0);
        }

        public static bool IsHealthy(this Pawn pawn)
        {
            if (pawn.health.State != PawnHealthState.Mobile ||
                 pawn.health.summaryHealth.SummaryHealthPercent < 1f ||
                 pawn.health.hediffSet.BleedRateTotal > 0f ||
                 pawn.health.hediffSet.PainTotal > 0f ||
                 pawn.GetDiseases().Any() ||
                 DefDatabase<PawnCapacityDef>.AllDefsListForReading
                                             .Any(cap => pawn.health.capacities.GetLevel(cap) < 1f))
            {
                return false;
            }

            return true;
        }

        public static List<FloatMenuOption> AddedPartOptionsThatAffect(this RecipeDef r, PawnCapacityDef capacity,
                                                                        Pawn pawn, bool negative = false)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            if (!r?.addsHediff?.IsAddedPart() ?? true)
            {
                return options;
            }

            if (!NotMissingVitalIngredient(pawn, r))
            {
                return options;
            }

            float after = r.addsHediff.addedPartProps.partEfficiency;

            IEnumerable<BodyPartRecord> parts = r.Worker.GetPartsToApplyOn(pawn, r)
                         .Where(p => p.Affects(capacity) &&
                                      !pawn.health.hediffSet.AncestorHasDirectlyAddedParts(p));

            foreach (BodyPartRecord part in parts)
            {
                float current = PawnCapacityUtility.CalculatePartEfficiency(pawn.health.hediffSet, part);
                if ((after < current) == negative)
                {
                    options.Add(GenerateSurgeryOption(pawn, pawn, r,
                                                        r.PotentiallyMissingIngredients(null, pawn.Map),
                                                        part));
                }
            }

            return options;
        }

        public static bool AddsHediffThatAffects(this RecipeDef r, PawnCapacityDef capacity, float current,
                                                  bool negative = false)
        {
            return r.addsHediff.IsHediffThatAffects(capacity, current, negative);
        }

        public static bool Affects(this Bill_Medical bill, PawnCapacityDef capacity)
        {
            if (bill?.recipe == null)
            {
                return false;
            }

            return bill.recipe.AddsHediffThatAffects(capacity, -1) ||
                   bill.recipe.AdministersDrugThatAffects(capacity, -1) ||
                   (bill.recipe.addsHediff.IsAddedPart() && bill.Part.Affects(capacity));
        }

        public static bool AddsHediffThatReducesPain(this RecipeDef r)
        {
            return r.addsHediff.IsHediffThatReducesPain();
        }

        public static bool AdministersDrugThatAffects(this RecipeDef r, PawnCapacityDef capacity, float current,
                                                       bool negative = false)
        {
            if (r.ingredients.NullOrEmpty())
            {
                return false;
            }

            return r.ingredients[0].filter.BestThingRequest.singleDef.AffectsCapacityOnIngestion(capacity, current,
                                                                                                  negative);
        }

        public static bool AdministersDrugThatReducesPain(this RecipeDef r)
        {
            if (r.ingredients.NullOrEmpty())
            {
                return false;
            }

            return r.ingredients[0].filter.BestThingRequest.singleDef.ReducesPainOnIngestion();
        }

        public static bool Affects(this BodyPartRecord part, PawnCapacityDef capacity)
        {
            return CapacityTags[capacity].Any(tag => part.ThisOrAnyChildHasTag(tag));
        }

        public static bool AffectsCapacityOnIngestion(this ThingDef def, PawnCapacityDef capacity, float current,
                                                       bool negative = false)
        {
            return
                def?.ingestible?.outcomeDoers?.OfType<IngestionOutcomeDoer_GiveHediff>()
                    .Any(od => od.hediffDef.IsHediffThatAffects(capacity, current, negative)) ?? false;
        }

        public static FloatMenuOption GenerateSurgeryOption(Pawn pawn, Thing thingForMedBills, RecipeDef recipe,
                                                             IEnumerable<ThingDef> missingIngredients, BodyPartRecord part = null)
        {
            if (_generateSurgeryOptionMethodInfo == null)
            {
                _generateSurgeryOptionMethodInfo = typeof(HealthCardUtility).GetMethod("GenerateSurgeryOption",
                                                                                          BindingFlags.NonPublic |
                                                                                          BindingFlags.Static);
                if (_generateSurgeryOptionMethodInfo == null)
                {
                    throw new NullReferenceException("GenerateSurgeryOption method info not found!");
                }
            }

            var option = _generateSurgeryOptionMethodInfo.Invoke(null,
                                                         new object[]
                                                         {
                                                             pawn, thingForMedBills, recipe, missingIngredients, AcceptanceReport.WasAccepted, 0, part
                                                         })
                    as FloatMenuOption;
            option.mouseoverGuiAction = null;
            return option;
        }

        public static bool IsAddedPart(this HediffDef hediff)
        {
            return hediff?.addedPartProps?.partEfficiency != null;
        }

        public static bool IsHediffThatAffects(this HediffDef hediffDef, PawnCapacityDef capacity, float current,
                                                bool negative = false)
        {
            if (hediffDef?.stages.NullOrEmpty() ?? true)
            {
                return false;
            }

            foreach (HediffStage stage in hediffDef.stages)
            {
                if (stage.capMods.NullOrEmpty())
                {
                    continue;
                }

                foreach (PawnCapacityModifier capMod in stage.capMods)
                {
                    if (capMod.capacity == capacity)
                    {
                        float after = Mathf.Min((current + capMod.offset) * capMod.postFactor, capMod.setMax);
                        return (after < current) == negative;
                    }
                }
            }

            return false;
        }

        public static bool IsHediffThatReducesPain(this HediffDef hediffDef)
        {
            if (hediffDef?.stages.NullOrEmpty() ?? true)
            {
                return false;
            }

            return hediffDef.stages?.Any(hs => hs.painFactor < 1f || hs.painOffset < 0f) ?? false;
        }

        public static bool NotMissingVitalIngredient(Pawn pawn, RecipeDef r)
        {
            return !r.PotentiallyMissingIngredients(null, pawn.Map).Any();
        }

        public static bool ReducesPainOnIngestion(this ThingDef def)
        {
            return
                def?.ingestible?.outcomeDoers?.OfType<IngestionOutcomeDoer_GiveHediff>()
                    .Any(od => od.hediffDef.IsHediffThatReducesPain()) ?? false;
        }

        public static bool ThisOrAnyChildHasTag(this BodyPartRecord part, BodyPartTagDef tag)
        {
            if (part?.def?.tags == null)
            {
                return false;
            }

            if (part.def.tags.Contains(tag))
            {
                return true;
            }

            if (part.parts.NullOrEmpty())
            {
                return false;
            }

            return part.parts.Any(p => p.ThisOrAnyChildHasTag(tag));
        }

        public struct DiseaseProgress
        {
            #region Fields

            public float immunity;
            public string label;
            public float severity;
            public bool tended;
            public int tillTendTicks;
            public Color color;

            #endregion Fields

            #region Methods

            public static explicit operator DiseaseProgress(Hediff hediff)
            {
                HediffComp_Immunizable immunizable = hediff.TryGetComp<HediffComp_Immunizable>();
                HediffComp_TendDuration tendable = hediff.TryGetComp<HediffComp_TendDuration>();


                Color.RGBToHSV(hediff.LabelColor, out float hue, out _, out _);
                Color color = Color.HSVToRGB(hue, 1f, 1f);

                return new DiseaseProgress
                {
                    label = hediff.Label,
                    immunity = immunizable?.Immunity ?? 0,
                    severity = hediff.Severity,
                    tended = !hediff.TendableNow(),
                    tillTendTicks = tendable?.tendTicksLeft ?? 0,
                    color = color
                };
            }

            #endregion Methods
        }
    }
}
