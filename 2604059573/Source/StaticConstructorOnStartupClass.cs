using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace PsychicAnimals
{
    public class StaticConstructorOnStartupClass
    {

        [StaticConstructorOnStartup]
        public static class StartUp
        {
            static StartUp()
            {
                var harmony = new Harmony("PsychicAnimals.patch");
                harmony.PatchAll();
                DefOf.InstallPsyLinkOnAnimal.recipeUsers = ChangeRecipeDef();
                foreach (RecipeDef item in RecipeDefGenerator_Neurotrainer.ImpliedRecipeDefs())
                {
                    DefGenerator.AddImpliedDef(item);
                }
                ChangeNeurotrainerThingDefs();
            }

            public static List<ThingDef> ChangeRecipeDef()
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
                return recipeusers;
            }
            static void ChangeNeurotrainerThingDefs()
            {
                foreach (ThingDef thing in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    CompProperties_Neurotrainer comp = thing.GetCompProperties<CompProperties_Neurotrainer>();
                    if (comp != null && comp.ability != null && DefDatabase<RecipeDef>.GetNamed("Install_" + thing.defName, false) != null)
                    {
                        thing.descriptionHyperlinks.Add(new DefHyperlink { def = DefDatabase<RecipeDef>.GetNamed("Install_" + thing.defName) });
                    }
                }
            }
        }
    }
    [RimWorld.DefOf]
    public static class DefOf
    {
        public static RecipeDef InstallPsyLinkOnAnimal;
        public static AbilityCategoryDef WordOf;

        static DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOf));
        }
    }

    public class HelperClass
    {
        public static bool CanBeAssignedToTrain(TrainableDef td, ThingDef animal)
        {
            if (animal.race.trainability != TrainabilityDefOf.Advanced)
            {
                return false;
            }
            if (animal.race.untrainableTags != null)
            {
                for (int i = 0; i < animal.race.untrainableTags.Count; i++)
                {
                    if (td.MatchesTag(animal.race.untrainableTags[i]))
                    {
                        return false;
                    }
                }
            }
            if (animal.race.trainableTags != null)
            {
                for (int j = 0; j < animal.race.trainableTags.Count; j++)
                {
                    if (td.MatchesTag(animal.race.trainableTags[j]))
                    {
                        if (animal.race.baseBodySize < td.minBodySize)
                        {
                            return false;
                        }
                        return true;
                    }
                }
            }
            if (!td.defaultTrainable)
            {
                return false;
            }
            if (animal.race.trainability.intelligenceOrder < td.requiredTrainability.intelligenceOrder)
            {
                return false;
            }
            return true;

        }
    }
}
