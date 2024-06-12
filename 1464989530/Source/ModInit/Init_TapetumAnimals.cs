// Nightvision NightVision Init_TapetumAnimals.cs
// 
// 06 12 2018
// 
// 06 12 2018

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace NightVision
{
    public partial class Initialiser
    {
        private const int EXPECTED_NUM_ANIMALS = 4;


        //Try to dynamically inject tapetum into large predators ensuring coverage of as many biomes as possible
        //Fallsback to adding to the same animals as vanilla rimworld
        public void AddTapetumRecipeToAnimals()
        {
            var bestAnimals = new List<ThingDef>();
            var tapetumResearch = ResearchProjectDef.Named(defName: "TapetumImplant");
            var descAppendage = new StringBuilder();


            foreach (var biome in DefDatabase<BiomeDef>.AllDefs)
            {
                try
                {
                    if (!biome.AllWildAnimals.Any())
                    {
                        continue;
                    }

                    List<PawnKindDef> possibleAnimals = biome.AllWildAnimals
                                .Where(predicate: pkd => pkd.RaceProps.predator && pkd.RaceProps.baseBodySize > 1).ToList();

                    if (possibleAnimals.Count == 0)
                    {
                        continue;
                    }

                    var bestAnimal = possibleAnimals.Aggregate(
                        func: (best, next) => best.RaceProps.baseBodySize > next.RaceProps.baseBodySize ? best : next
                    ).race;

                    bestAnimals.AddDistinct(element: bestAnimal);
                }
                catch (ArgumentException)
                {
                    Log.Error(
                        text:
                        $"Biome {biome.defName} has duplicate animals in commonality list. Ensure that commonality is not defined in both the animal def and the biome def. Further errors will occur if playing in affected biome."
                    );
                }
            }

            //Comment: 4 is the expected number of animals in vanilla
            if (bestAnimals.Count < EXPECTED_NUM_ANIMALS)
            {
                var fallback = FallbackAnimals();
                foreach (var animal in fallback)
                {
                    bestAnimals.AddDistinct(animal);
                }
            }

            foreach (var animal in bestAnimals)
            {
                if (animal.recipes.NullOrEmpty())
                {
                    animal.recipes = new List<RecipeDef>();
                }

                animal.recipes.Add(item: Defs_NightVision.ExtractTapetumLucidum);
                descAppendage.Append(value: "\n - " + animal.LabelCap);
            }

            tapetumResearch.description += descAppendage.ToString();
        }

        private IEnumerable<ThingDef> FallbackAnimals()
        {
            return
                new List<ThingDef>
                {
                    ThingDef.Named("Bear_Grizzly"), ThingDef.Named("Bear_Polar"), ThingDef.Named("Cougar"),
                    ThingDef.Named("Panther")
                };
        }
    }
}