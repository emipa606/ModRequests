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
    class RecipeWorker_ApplyNeurotrainer : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            foreach (IngredientCount ingredient in recipe.ingredients)
            {
                if (thing.Map.listerThings.ThingsOfDef(ingredient.filter.AnyAllowedDef).Any())
                {
                    Pawn pawn = thing as Pawn;
                    if (pawn.abilities != null && !pawn.abilities.abilities.Any(x => x.def == ingredient.filter.AnyAllowedDef.GetCompProperties<CompProperties_Neurotrainer>().ability))
                    {
                        foreach (Bill bill in pawn.health.surgeryBills.Bills)
                        {
                            if (bill.recipe == recipe)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            foreach (Thing item in ingredients)
            {
                AbilityDef abi = item.TryGetComp<CompNeurotrainer>()?.ability;
                if (abi != null)
                {
                    if (pawn.abilities.abilities != null)
                    {
                        foreach (Ability ability in pawn.abilities.abilities)
                        {
                            if (ability.def == abi)
                            {
                                return;
                            }
                        }
                    }
                    pawn.abilities.GainAbility(abi);
                    return;
                }

            }
        }
    }


}
