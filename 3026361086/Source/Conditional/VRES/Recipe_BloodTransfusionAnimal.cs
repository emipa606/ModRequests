using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Komishne.SanguophageTweaks.Conditional.VRES
{
    class Recipe_BloodTransfusionAnimal : Recipe_Surgery
    {
        public const float BloodlossHealedPerPack = 0.35f;
        protected ThingDef hemogenPackIngredientThingDef = VanillaRacesExpandedSanguophage.InternalDefOf.VRE_HemogenPack_Animal;

        public ThingDef HemogenPackIngredientThingDef
        {
            get
            {
                return hemogenPackIngredientThingDef;
            }
        }

        // TODO: Add cache.
        public float BloodLossHealedPerPackForPawn(Pawn pawn)
        {
            return BloodlossHealedPerPack * Utility.BloodlossSeverityMultiplierFromBodySize(pawn);
        }

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null) =>
            thing.MapHeld != null &&
            thing.MapHeld.listerThings.ThingsOfDef(HemogenPackIngredientThingDef).Count != 0 &&
            (!(thing is Pawn pawn) || pawn.health.hediffSet.HasHediff(HediffDefOf.BloodLoss)) &&
            (!(thing is Pawn animalPawn) || animalPawn.RaceProps is null || animalPawn.RaceProps.Animal) &&
            base.AvailableOnNow(thing, part);

        public override bool CompletableEver(Pawn surgeryTarget) =>
            surgeryTarget.health.hediffSet.HasHediff(HediffDefOf.BloodLoss) && base.CompletableEver(surgeryTarget);

        public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
        {
            if (pawn is null)
                return recipe.label;
            return $"{recipe.label} ({BloodLossHealedPerPackForPawn(pawn):P0} blood loss reduction per pack)";
        }

        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
        }

        public override void ApplyOnPawn(
            Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            int numPacksUsed = 0;
            for (int index = 0; index < ingredients.Count; ++index)
            {
                if (ingredients[index].def == HemogenPackIngredientThingDef)
                {
                    numPacksUsed += ingredients[index].stackCount;
                }
            }
            float amountOfBloodlossHealed = numPacksUsed * BloodLossHealedPerPackForPawn(pawn);  // final amount
            Utility.AdjustBloodLoss(pawn, -1 * amountOfBloodlossHealed);

            float amountOfHemogenAdded = numPacksUsed * JobGiver_GetHemogen.HemogenPackHemogenGain;  // pre-body-size
                                                                                                     // adjustment
            if (amountOfHemogenAdded > 0.0f && pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>() != null)
                GeneUtility.OffsetHemogen(pawn, amountOfHemogenAdded);

            for (int index = 0; index < ingredients.Count; ++index)
                ingredients[index].Destroy();
        }

        // TODO: This appears to be assuming the only ingredient is the hemogen pack. Add some checking?
        public override float GetIngredientCount(IngredientCount ing, Bill bill)
        {
            if (!(bill.billStack?.billGiver is Pawn billGiver))
                return base.GetIngredientCount(ing, bill);
            Hediff firstHediffOfDef = billGiver.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            return firstHediffOfDef == null ? base.GetIngredientCount(ing, bill) :
                // Use as many packs as needed to fully cure the blood loss hediff, if there are enough on the map and
                // they are not forbidden.
                Mathf.Min(
                    bill.Map.listerThings.ThingsOfDef(HemogenPackIngredientThingDef).Sum(
                        x => !x.IsForbidden(billGiver) ? x.stackCount : 0),
                    firstHediffOfDef.Severity / BloodLossHealedPerPackForPawn(billGiver));
        }
    }
}