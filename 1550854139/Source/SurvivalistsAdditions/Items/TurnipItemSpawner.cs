using Verse;

namespace SurvivalistsAdditions {
  
  internal class TurnipItemSpawner : ItemSpawner {

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
      base.SpawnSetup(map, respawningAfterLoad);
      SpawnRandomQuantity(SrvDefOf.SRV_Turnip, 1, 2, map);
      SpawnRandomQuantity(SrvDefOf.SRV_Turnip_Green, 1, 4, map);
      Destroy();
    }
  }
}
