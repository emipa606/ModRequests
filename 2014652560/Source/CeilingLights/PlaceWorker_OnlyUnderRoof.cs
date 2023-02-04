using Verse;

namespace CeilingLights
{
  public class PlaceWorker_OnlyUnderRoof : PlaceWorker
  {
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
      if (!CeilingLightsSettings.ForceRoofRequired)
      {
        return true;
      }
      return !map.roofGrid.Roofed(loc) ? new AcceptanceReport((string)"MustPlaceUnroofed".Translate()) : (AcceptanceReport)true;
    }
  }
}
