using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace PsychicAnimals
{
    class RecipeWorker_ApplyPsylink : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            foreach (IngredientCount ingredient in recipe.ingredients)
            {
                if (thing.Map.listerThings.ThingsOfDef(ingredient.filter.AnyAllowedDef).Any(x => x.Spawned))
                {
                    return true;
                }
            }
            return false;
        }
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate (BodyPartRecord record)
                {
                    int level = PawnUtility.GetPsylinkLevel(pawn);
                    if (level > 5)
                    {
                        return false;
                    }
                    int count = 0;
                    foreach (Bill bill in pawn.health.surgeryBills.Bills)
                    {
                        if (bill.recipe == DefOf.InstallPsyLinkOnAnimal)
                        {
                            count++;
                        }
                    }
                    if ((level + count) > 5)
                    {
                        return false;
                    }
                    return true;
                });
        }


        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff.def == recipe.addsHediff)
                {
                    Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(recipe.addsHediff);
                    ((Hediff_Level)firstHediffOfDef).ChangeLevel(1);
                }
            }
            pawn.health.AddHediff(recipe.addsHediff, part);
            return;

        }
    }


}
