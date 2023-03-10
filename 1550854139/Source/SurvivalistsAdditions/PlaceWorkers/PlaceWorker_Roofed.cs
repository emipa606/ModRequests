using Verse;

namespace SurvivalistsAdditions {

  public class PlaceWorker_Roofed : PlaceWorker {

		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null) {
      foreach (IntVec3 current in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size)) {
        if (!map.roofGrid.Roofed(current)) {
          return new AcceptanceReport ("SRV_NeedsRoof".Translate(new object[] { checkingDef.LabelCap }));
        }
      }
      return true;
    }
  }
}
