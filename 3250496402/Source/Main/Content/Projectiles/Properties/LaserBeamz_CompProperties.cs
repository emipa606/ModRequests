using Verse;

namespace Kraltech_Industries;

public class LaserBeamz_CompProperties : CompProperties
{
    public float dbStartOffset;

    public ThingDef laserMote;

    public LaserBeamz_CompProperties()
    {
        compClass = typeof(CompLaserBeamz);
    }
}