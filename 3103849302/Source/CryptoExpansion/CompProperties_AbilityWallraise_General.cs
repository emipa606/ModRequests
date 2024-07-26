using RimWorld;
using UnityEngine;
using Verse;

namespace CryptoExpansion
{
  public class CompProperties_AbilityWallraise_General : CompProperties_AbilityWallraise
  {
    public ThingDef wallDef = null;

    public Color puffColor = Color.white;

    public ThingDef WallDef => wallDef ?? ThingDefOf.RaisedRocks;
  }
}
