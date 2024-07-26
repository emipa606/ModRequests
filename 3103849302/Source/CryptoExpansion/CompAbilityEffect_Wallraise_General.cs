using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CryptoExpansion
{
  class CompAbilityEffect_Wallraise_General : CompAbilityEffect_Wallraise
  {
    public new CompProperties_AbilityWallraise_General Props
    {
      get { return (CompProperties_AbilityWallraise_General)props; }
    }

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
      base.Apply(target, dest);
      Map map = parent.pawn.Map;
      HashSet<Thing> list = AffectedCells(target, map).SelectMany((IntVec3 c) => from t in c.GetThingList(map)
        where t.def.category == ThingCategory.Item
        select t).ToHashSet();

      foreach (var item in list) item.DeSpawn(DestroyMode.Vanish); // despawns items on cell where walls spawn

      foreach (var cell in AffectedCells(target, map))
      {
        Thing wall = GenSpawn.Spawn(Props.wallDef, cell, map, WipeMode.Vanish);
        FleckMaker.ThrowDustPuffThick(cell.ToVector3Shifted(), map, Rand.Range(1.5f, 3f), Props.puffColor);
        CompAbilityEffect_Teleport.SendSkipUsedSignal(cell, parent.pawn);
      }

      foreach (var item in list) // for each despawned item, respawn or move it to a nearby cell
      {
        IntVec3 destCell = IntVec3.Invalid;
        for (int i = 0; i < 9; i++)
        {
          // look for a open cell radially- if found, set destination cell
          IntVec3 radialCell = item.Position + GenRadial.RadialPattern[i];
          if (radialCell.InBounds(map) && radialCell.Walkable(map) && map.thingGrid.ThingsListAtFast(radialCell).Count <= 0)
          {
            destCell = radialCell;
            break;
          }
        }

        if (destCell != IntVec3.Invalid) GenSpawn.Spawn(item, destCell, map, WipeMode.Vanish); // if cell isn't invalid, respawn
        else GenPlace.TryPlaceThing(item, item.Position, map, ThingPlaceMode.Near, null, null, default(Rot4)); // else place new thing on original cell
      }
    }

    public IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map)
    {
      foreach (IntVec2 intVec in Props.pattern)
      {
        IntVec3 finalCell = target.Cell + new IntVec3(intVec.x, 0, intVec.z);
        if (finalCell.InBounds(map)) yield return finalCell;
      }

      yield break;
    }
  }
}
