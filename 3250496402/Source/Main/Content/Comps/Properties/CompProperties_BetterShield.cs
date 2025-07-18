using Verse;

namespace Kraltech_Industries;

public class CompProperties_BetterShield : CompProperties
{
    public bool blocksRangedWeapons = true;

    public float energyLossPerDamage = 0.01f;

    public float energyOnReset = 40f;

    public float maxDrawSize = 1.55f;

    public float minDrawSize = 1.2f;

    public int startingTicksToReset = 3200;

    public CompProperties_BetterShield()
    {
        compClass = typeof(CompBetterShield);
    }
}