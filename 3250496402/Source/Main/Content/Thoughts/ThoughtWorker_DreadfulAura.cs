using System;
using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class ThoughtWorker_DreadfulAura : ThoughtWorker
{
    private const float IntenseDistance = 6.9f;

    private const float StrongDistance = 15.9f;

    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        var dreadfulAuraLevel = DreadfulAuraLevel.None;
        if (p.Map != null)
        {
            var list = p.Map.listerThings.ThingsOfDef(ThingDefOf.Mech_SupremeApocriton);
            list.SortBy(m => m.Position.DistanceToSquared(m.Position));
            if (list.Count > 0)
            {
                var num = list[0].Position.DistanceTo(p.Position);
                if (num <= 6.9f)
                    dreadfulAuraLevel = DreadfulAuraLevel.Intense;
                else if (num <= 15.9f)
                    dreadfulAuraLevel = DreadfulAuraLevel.Strong;
                else
                    dreadfulAuraLevel = DreadfulAuraLevel.Distant;
            }
        }

        ThoughtState result;
        switch (dreadfulAuraLevel)
        {
            case DreadfulAuraLevel.None:
                result = false;
                break;
            case DreadfulAuraLevel.Intense:
                result = ThoughtState.ActiveAtStage(0);
                break;
            case DreadfulAuraLevel.Strong:
                result = ThoughtState.ActiveAtStage(1);
                break;
            case DreadfulAuraLevel.Distant:
                result = ThoughtState.ActiveAtStage(2);
                break;
            default:
                throw new NotImplementedException();
        }

        return result;
    }

    private enum DreadfulAuraLevel
    {
        None,
        Intense,
        Strong,
        Distant
    }
}