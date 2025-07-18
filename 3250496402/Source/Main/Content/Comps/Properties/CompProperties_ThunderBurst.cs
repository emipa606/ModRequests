using RimWorld;

namespace Kraltech_Industries;

public class CompProperties_ThunderBurst : CompProperties_AbilityEffect
{
    public float radius = 7.9f;

    public CompProperties_ThunderBurst()
    {
        compClass = typeof(CompAbilityEffect_ThunderField);
    }
}