using Verse;

namespace CryptoExpansion
{
  public class CompAssociated : ThingComp
  {
    public override void PostExposeData()
    {
      Scribe_References.Look(ref pawn, "pawn");
    }

    public Pawn pawn;
  }
}
