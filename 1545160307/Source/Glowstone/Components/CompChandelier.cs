using RimWorld;
using Verse;
using Verse.Sound;

namespace Glowstone {

  public class CompChandelier : ThingComp {


    public override void CompTickRare() {
      base.CompTickRare();

      // Minify this if the ceiling is missing
      int occCells = 0;
      int unroofedCells = 0;
      foreach (IntVec3 current in parent.OccupiedRect()) {
        occCells++;
        if (!parent.Map.roofGrid.Roofed(current)) {
          unroofedCells++;
        }
        if (current.GetEdifice(parent.Map) == null || current.GetEdifice(parent.Map).def == null) {
          continue;
        }
        if (current.GetEdifice(parent.Map).def.blockWind == true || current.GetEdifice(parent.Map).def.holdsRoof == true) {
          Minify();
        }
      }
      if (((float)(occCells - unroofedCells) / occCells) < 0.5f) {
        Minify();
      }
    }


    public virtual void Minify() {
      MinifiedThing package = parent.MakeMinified();
      GenPlace.TryPlaceThing(package, parent.Position, parent.Map, ThingPlaceMode.Near);
      SoundDef.Named("ThingUninstalled").PlayOneShot(new TargetInfo(parent.Position, parent.Map));
    }
  }
}
