using RimWorld;
using Verse;
using System.Collections.Generic; // For List<>

namespace SacrificeYourself
{
    public class RecipeWorker_RecycleItem : RecipeWorker
    {
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            // Clone the ingredient instead of destroying it
            Thing recycledItem = ThingMaker.MakeThing(ingredient.def, ingredient.Stuff);
            recycledItem.stackCount = ingredient.stackCount;

            // Copy the quality of the original item, if it has one
            if (ingredient.TryGetComp<CompQuality>() is CompQuality originalQuality &&
                recycledItem.TryGetComp<CompQuality>() is CompQuality recycledQuality)
            {
                recycledQuality.SetQuality(originalQuality.Quality, ArtGenerationContext.Colony);
            }

            // Spawn the cloned item at the original ingredient's location
            GenPlace.TryPlaceThing(recycledItem, ingredient.Position, map, ThingPlaceMode.Near);

            // Destroy the original ingredient
            ingredient.Destroy();
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            foreach (Thing ingredient in ingredients)
            {
                // Ensure that the recycled item is properly processed
                Log.Message($"Recycled {ingredient.def.label} with quality {ingredient.TryGetComp<CompQuality>()?.Quality} back to the bill.");
            }
        }
    }
}
