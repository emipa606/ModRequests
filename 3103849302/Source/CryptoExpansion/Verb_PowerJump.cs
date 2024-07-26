using RimWorld;
using Verse;

namespace CryptoExpansion
{
  public class Verb_PowerJump : Verb_Jump
  {
    protected override bool TryCastShot()
    {
      if (HediffCompSource.parent.TryGetComp<HediffComp_JumpEntropy>() is { } entropyHediff)
      {
        entropyHediff.Entropy += 1f;
        entropyHediff.jumping = true;
      }
      return JumpUtility.DoJump(CasterPawn, currentTarget, ReloadableCompSource, verbProps);
    }
  }
}
