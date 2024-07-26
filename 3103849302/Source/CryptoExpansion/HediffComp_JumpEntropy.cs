using Verse;

namespace CryptoExpansion;

public class HediffComp_JumpEntropy : HediffComp
{
  public float entropy;
  public bool jumping;

  public float Entropy
  {
    get => entropy;
    set => entropy = value;
  }

  public HediffCompProperties_JumpEntropy Props => (HediffCompProperties_JumpEntropy)props;

  public override string CompDescriptionExtra => entropy > 0 ? $" - Jump Entropy: {entropy}" : null;

  public override void CompExposeData()
  {
    base.CompExposeData();
    Scribe_Values.Look<float>(ref entropy, "entropy");
    Scribe_Values.Look<bool>(ref jumping, "jumping");
  }
}
