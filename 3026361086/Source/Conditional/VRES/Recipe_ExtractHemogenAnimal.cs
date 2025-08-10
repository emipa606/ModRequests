using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Komishne.SanguophageTweaks.Conditional.VRES
{
    public class Recipe_ExtractHemogenAnimal : Recipe_Surgery
    {
        protected const float BloodlossSeverityIndustrial = 0.45f;
        protected const float BloodlossSeverityHerbal = 0.65f;

        protected ThingDef hemogenPackProducedThingDef = VanillaRacesExpandedSanguophage.InternalDefOf.VRE_HemogenPack_Animal;

        public ThingDef HemogenPackProducedThingDef
        {
            get
            {
                return hemogenPackProducedThingDef;
            }
        }

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (!base.AvailableOnNow(thing, part))
                return false;

            var pawn = thing as Pawn;
            if (pawn == null)
                return true;
            if ((pawn.genes != null && pawn.genes.HasGene(GeneDefOf.Hemogenic)) ||
                (pawn.RaceProps != null && !pawn.RaceProps.Animal))
                return false;

            return true;
        }

        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            if (!(thing is Pawn pawn))
                return base.AvailableReport(thing, part);

            return pawn.RaceProps == null || !pawn.RaceProps.Animal ? "Must be an animal." :
                pawn.DevelopmentalStage.Baby() ? "TooSmall".Translate() : base.AvailableReport(thing, part);
        }

        public override bool CompletableEver(Pawn surgeryTarget) =>
            base.CompletableEver(surgeryTarget) && PawnHasEnoughBloodForExtractionWorstCase(surgeryTarget);

        public override void CheckForWarnings(Pawn medPawn)
        {
            base.CheckForWarnings(medPawn);
            if (PawnHasEnoughBloodForExtractionWorstCase(medPawn))
                return;
            Messages.Message("MessageCannotStartHemogenExtraction".Translate(medPawn.Named("PAWN")),
                /*lookTargets=*/medPawn, MessageTypeDefOf.NeutralEvent, /*historical=*/false);
        }

        public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
        {
            if (pawn is null)
                return recipe.label;
            float maximumBloodLoss = Utility.BloodlossSeverityMultiplierFromBodySize(pawn) * BloodlossSeverityHerbal;
            return $"{recipe.label} ({maximumBloodLoss:P0} at most blood loss)";
        }

        // TODO: This surgery does not actually use any ingredients, so the blood loss severity is always equal to the
        // value for industrial medicine. Double-check this is what the vanilla version does, and remove this logic
        // (or add needing medicine).
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients,
            Bill bill)
        {
            float bloodlossSeverity = BloodlossSeverityIndustrial;
            for (int index = 0; index < ingredients.Count; ++index)
            {
                if (ingredients[index].GetStatValue(StatDefOf.MedicalPotency) < 1.0f)
                {
                    bloodlossSeverity = BloodlossSeverityHerbal;
                    break;
                }
            }
            bloodlossSeverity *= Utility.BloodlossSeverityMultiplierFromBodySize(pawn);

            if (SanguophageUtility.WouldDieFromAdditionalBloodLoss(pawn, bloodlossSeverity))
            {
                Messages.Message("MessagePawnHadNotEnoughBloodToProduceHemogenPack".Translate(pawn.Named("PAWN")),
                    /*lookTargets=*/pawn, MessageTypeDefOf.NeutralEvent);
                return;
            }

            Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn);
            hediff.Severity = bloodlossSeverity;
            pawn.health.AddHediff(hediff);
            OnSurgerySuccess(pawn, part, billDoer, ingredients, bill);
            if (!IsViolationOnPawn(pawn, part, Faction.OfPlayer))
                return;
            ReportViolation(pawn, billDoer, pawn.HomeFaction, -1, HistoryEventDefOf.ExtractedHemogenPack);
        }

        protected override void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn billDoer,
            List<Thing> ingredients, Bill bill)
        {
            if (GenPlace.TryPlaceThing(ThingMaker.MakeThing(HemogenPackProducedThingDef), pawn.PositionHeld,
                pawn.MapHeld, ThingPlaceMode.Near))
                return;
            Log.Error($"Could not drop animal hemogen pack near {pawn.PositionHeld}");
        }

        protected bool PawnHasEnoughBloodForExtractionWorstCase(Pawn pawn)
        {
            return !SanguophageUtility.WouldDieFromAdditionalBloodLoss(
                pawn, Utility.BloodlossSeverityMultiplierFromBodySize(pawn) * BloodlossSeverityHerbal);
        }
    }
}
