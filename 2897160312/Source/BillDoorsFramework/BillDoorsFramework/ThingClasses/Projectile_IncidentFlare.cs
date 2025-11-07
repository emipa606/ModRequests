using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class Projectile_IncidentFlare : Projectile_Rotational
    {
        private int ticksToDetonation;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToDetonation, "ticksToDetonation", 0, false);
        }
        public override void Tick()
        {
            base.Tick();
            if (ticksToDetonation > 0)
            {
                ticksToDetonation--;
                if (ticksToDetonation == def.projectile.explosionDelay / 2)
                {
                    GetComp<CompIncidentFlare>()?.DoEffect(launcher.Faction);
                    return;
                }
                if (ticksToDetonation <= 0)
                {
                    Destroy();
                    return;
                }
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            GetComp<CompFlareGlower>()?.UpdateLit(Map);
            base.Destroy(mode);
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (def.projectile.explosionDelay == 0)
            {
                GetComp<CompIncidentFlare>()?.DoEffect(launcher.Faction);
                Destroy();
                return;
            }
            landed = true;
            GetComp<CompFlareGlower>()?.UpdateLit(Map);
            ticksToDetonation = def.projectile.explosionDelay * 2;
        }
    }

    public class CompFlareEffecter : ThingComp
    {
        private Effecter effecter;

        public CompProperties_FlareEffecter Props => (CompProperties_FlareEffecter)props;

        public override void CompTick()
        {
            base.CompTick();
            if (parent is Projectile_Rotational flare && flare.Landed)
            {
                if (effecter == null)
                {
                    effecter = Props.effecterDef.Spawn(flare, flare.MapHeld, Props.offset.RotatedBy(-flare.rotationAngle));
                }
                effecter?.EffectTick(parent, parent);
            }
            else
            {
                effecter?.Cleanup();
                effecter = null;
            }
        }
    }
    public class CompProperties_FlareEffecter : CompProperties
    {
        public EffecterDef effecterDef;

        public Vector2 offset;

        public CompProperties_FlareEffecter()
        {
            compClass = typeof(CompFlareEffecter);
        }
    }

    public class CompFlareGlower : CompGlower
    {
        protected override bool ShouldBeLitNow => (parent is Projectile_Rotational flare && flare.Landed) && base.ShouldBeLitNow;
    }
}
