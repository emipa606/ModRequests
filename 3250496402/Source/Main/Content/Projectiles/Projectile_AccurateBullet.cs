using RimWorld;

namespace Kraltech_Industries;

public class Projectile_AccurateBullet : Bullet
{
    public override void Tick()
    {
        if (intendedTarget.Thing != null) destination = intendedTarget.Thing.DrawPos;
        base.Tick();
    }
}