using Verse;
using RimWorld;

namespace GiveMeMeat
{
  public class StatPart_MeatAmount : StatPart
  {
    float meatMultiplier = GiveMeMeatSettings.MeatAmountToMultiply;

    public override void TransformValue(StatRequest req, ref float val)
    {
      val *= meatMultiplier;
    }

    public override string ExplanationPart(StatRequest req)
    {
      return "GMMstatDescription".Translate() + ": x" + meatMultiplier.ToStringPercent();
    }
  }
}
