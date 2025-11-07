using RimWorld;
using Verse;

namespace BillDoorsFramework
{
    public class Projectile_CompExplosive : Bullet
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (hitThing != null)
            {
                this.Position = hitThing.TrueCenter().ToIntVec3();
            }
            var comps = this.GetComps<CompExplosiveExposed>();
            foreach (var comp in comps)
            {
                comp.DoDetonate(this.Map);
            }
            base.Impact(hitThing, blockedByShield);
        }
    }

    public class CompExplosiveExposed : CompExplosive
    {
        public void DoDetonate(Map map, bool ignoreUnspawned = false)
        {
            Detonate(map, ignoreUnspawned);
        }
    }
}
