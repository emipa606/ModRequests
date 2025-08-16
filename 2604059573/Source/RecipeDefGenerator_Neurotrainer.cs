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
using Verse.Sound;

namespace PsychicAnimals
{
    public class RecipeDefGenerator_Neurotrainer
    {
        public static string PsytrainerDefPrefix = "Install";
        public static IEnumerable<RecipeDef> ImpliedRecipeDefs()
        {
            List<ThingDef> allthings = DefDatabase<ThingDef>.AllDefsListForReading;
            List<ThingDef> recipeusers = new List<ThingDef> { };
            foreach (ThingDef thing in allthings)
            {

                if (thing.race != null && thing.race.Animal && HelperClass.CanBeAssignedToTrain(TrainableDefOf.Release, thing))
                {
                    recipeusers.Add(thing);
                }
            }
            foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (allDef.comps.Any(x => x is CompProperties_Neurotrainer n && n.ability != null && PsychicAnimalsMod.Selectpsycasts.AllowedAbilities().Any(y => y.Key == n.ability && y.Value == true )))
                {
                    RecipeDef recipeDef = BaseNeurotrainer();
                    recipeDef.defName = PsytrainerDefPrefix + "_" + allDef.defName;
                    recipeDef.label = "InstallPsycastNeurotrainerLabel".Translate(allDef.label);
                    recipeDef.description = "InstallPsycastNeurotrainerDescription".Translate();
                    IngredientCount ingredientCount = new IngredientCount();
                    ingredientCount.filter.SetAllow(allDef, allow: true);
                    ingredientCount.SetBaseCount(1f);
                    recipeDef.ingredients.Add(ingredientCount);
                    recipeDef.fixedIngredientFilter.SetAllow(allDef, allow: true);
                    recipeDef.recipeUsers = recipeusers;
                    recipeDef.descriptionHyperlinks = new List<DefHyperlink> { new DefHyperlink { def = allDef }, new DefHyperlink { def = allDef.GetCompProperties<CompProperties_Neurotrainer>().ability } };
                    recipeDef.targetsBodyPart = false;
                    recipeDef.modContentPack = allDef.modContentPack;
                    recipeDef.ResolveReferences();
                    yield return recipeDef;
                };
            }
        }
        private static RecipeDef BaseNeurotrainer()
        {
            return new RecipeDef
            {
                workerClass = typeof(RecipeWorker_ApplyNeurotrainer),
                jobString = "Applying Neurotrainer.",
                anesthetize = false,
                workAmount = 500f,
                surgerySuccessChanceFactor = 99999f
        };
    }
}
}

