using RimWorld;
using System;
using Verse;

namespace CCLGlower
{
    internal static class _CompGlower
    {
        internal static CompPowerTrader powerComp(this CompGlower obj)
        {
            return ThingCompUtility.TryGetComp<CompPowerTrader>(obj.parent);
        }
        internal static CompFlickable flickableComp(this CompGlower obj)
        {
            return ThingCompUtility.TryGetComp<CompFlickable>(obj.parent);
        }

        internal static CompRefuelable refuelableComp(this CompGlower obj)
        {
            return ThingCompUtility.TryGetComp<CompRefuelable>(obj.parent);
        }

        internal static bool _ShouldBeLitNow(this CompGlower obj)
        {
            if (!obj.parent.Spawned)
            {
                return false;
            }
            CompGlowerToggleable compGlowerToggleable = obj as CompGlowerToggleable;
            if (compGlowerToggleable != null && !compGlowerToggleable.Lit)
            {
                return false;
            }
            CompPowerTrader compPowerTrader = obj.powerComp();
            if (compPowerTrader != null && !compPowerTrader.PowerOn)
            {
                return false;
            }
            CompRefuelable compRefuelable = obj.refuelableComp();
            if (compRefuelable != null && !compRefuelable.HasFuel)
            {
                return false;
            }
           
            CompFlickable compFlickable = obj.flickableComp();
            return compFlickable == null || compFlickable.SwitchIsOn;
        }
    }
}
