using CombatExtended;
using CombatExtended.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;
using static UnityEngine.GraphicsBuffer;

namespace BillDoorsFramework
{
    public class ProjectileCE_IncidentFlare : ProjectileCE_Rotational
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
            GetComp<CompFlareGlowerCE>()?.UpdateLit(Map);
            base.Destroy(mode);
        }

        public override void Impact(Thing hitThing)
        {
            if (hitThing is Pawn)
            {
                var newPos = hitThing.DrawPos;
                newPos.y = ExactPosition.y;
                ExactPosition = newPos;
                Position = ExactPosition.ToIntVec3();
            }
            if (def.projectile.explosionDelay == 0)
            {
                GetComp<CompIncidentFlare>()?.DoEffect(launcher.Faction);
                Destroy();
                return;
            }
            landed = true;
            GetComp<CompFlareGlowerCE>()?.UpdateLit(Map);
            ticksToDetonation = def.projectile.explosionDelay * 2;
        }
    }

    public class CompFlareEffecterCE : ThingComp
    {
        private Effecter effecter;

        public CompProperties_FlareEffecter Props => (CompProperties_FlareEffecter)props;

        public override void CompTick()
        {
            base.CompTick();
            if (parent is ProjectileCE_Rotational flare && flare.landed)
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

    public class CompFlareGlowerCE : CompGlower
    {
        protected override bool ShouldBeLitNow => (parent is ProjectileCE_Rotational flare && flare.landed) && base.ShouldBeLitNow;
    }
}
