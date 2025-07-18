using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class ThoughtWorker_TrophySoothe : ThoughtWorker
{
    private const float Radius = 15f;

    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        ThoughtState result;
        if (!p.Spawned)
        {
            result = false;
        }
        else
        {
            var list = p.Map.listerThings.ThingsOfDef(ThingDefOf.SupremeApocritonTrophy);
            for (var i = 0; i < list.Count; i++)
            {
                var compPowerTrader = list[i].TryGetComp<CompPowerTrader>();
                if ((compPowerTrader == null || compPowerTrader.PowerOn) && p.Position.InHorDistOf(list[i].Position, 15f)) return true;
            }

            result = false;
        }

        return result;
    }
}