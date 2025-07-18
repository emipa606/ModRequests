using Verse;

namespace Kraltech_Industries;

public class Projectile_ExplosiveAccurateBullet : Projectile_Explosive
{
    public override void Tick()
    {
        if (intendedTarget.Thing != null) destination = intendedTarget.Thing.DrawPos;
        base.Tick();
    }
}