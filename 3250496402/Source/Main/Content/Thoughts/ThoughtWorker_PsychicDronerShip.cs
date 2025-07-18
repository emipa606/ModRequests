using System;
using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class ThoughtWorker_PsychicDronerShip : ThoughtWorker
{
    private const float IntenseDistance = 6.9f;

    private const float StrongDistance = 15.9f;

    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        var dronerzLevel = DronerzLevel.None;
        if (p.Map != null)
        {
            var list = p.Map.listerThings.ThingsOfDef(ThingDefOf.PsychicDronerShip);
            list.SortBy(m => m.Position.DistanceToSquared(m.Position));
            if (list.Count > 0)
            {
                var num = list[0].Position.DistanceTo(p.Position);
                if (num <= 6.9f)
                    dronerzLevel = DronerzLevel.Intense;
                else if (num <= 15.9f)
                    dronerzLevel = DronerzLevel.Strong;
                else
                    dronerzLevel = DronerzLevel.Distant;
            }
        }

        ThoughtState result;
        switch (dronerzLevel)
        {
            case DronerzLevel.None:
                result = false;
                break;
            case DronerzLevel.Intense:
                result = ThoughtState.ActiveAtStage(0);
                break;
            case DronerzLevel.Strong:
                result = ThoughtState.ActiveAtStage(1);
                break;
            case DronerzLevel.Distant:
                result = ThoughtState.ActiveAtStage(2);
                break;
            default:
                throw new NotImplementedException();
        }

        return result;
    }

    private enum DronerzLevel
    {
        None,
        Intense,
        Strong,
        Distant
    }
}