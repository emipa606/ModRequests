using Verse;

namespace SurvivalistsAdditions {

  internal class ItemSpawner : Thing {

    protected void SpawnRandomQuantity(ThingDef tDef, int minToSpawn, int maxToSpawn, Map map) {
      int stack = Rand.RangeInclusive(minToSpawn, maxToSpawn);
      if (stack <= 0) {
        return;
      }
      Thing placedProduct = ThingMaker.MakeThing(tDef);
      placedProduct.stackCount = stack;
      GenPlace.TryPlaceThing(placedProduct, Position, map, ThingPlaceMode.Near);
    }
  }
}
