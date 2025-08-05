using Verse;
using RimWorld;
using System;
using System.Reflection;

namespace BDsPlasmaWeaponVanilla
{
    public class CompLizionPopperExplosion : CompExplosive
    {
        public override void CompTick()
        {
            base.CompTick();
            if (wickStarted && wickTicksLeft <= 0)
            {
                CompProximityLizionPopper compProximityLizionPopper = parent.TryGetComp<CompProximityLizionPopper>();
                if (compProximityLizionPopper != null)
                {
                    OverlayHandle? OverlayHandle = base.GetType().GetField("overlayBurningWick").GetValue(new CompExplosive()) as OverlayHandle?;
                    parent.Map.overlayDrawer.Disable(parent, ref OverlayHandle);
                    compProximityLizionPopper.Pop();
                    StopWick();
                }
            }
        }

        public void DoDetonation()
        {
            CompProximityLizionPopper compProximityLizionPopper = parent.TryGetComp<CompProximityLizionPopper>();
            if (compProximityLizionPopper != null)
            {
                compProximityLizionPopper.Pop();
            }
            Detonate(parent.MapHeld);
        }
    }
}
