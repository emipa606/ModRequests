using RimWorld;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class Projectile_ExtinguishCaster : Projectile_Explosive
    {
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            if (Launcher.IsBurning())
            {
                Thing fire = Launcher.GetAttachment(ThingDefOf.Fire);
                fire?.Destroy();
            }
            Impact(null);
        }
    }
}
