using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WallStuff
{
    class ThoughtWorker_Hifi : ThoughtWorker
    {
        private readonly float listenDistance = 9f;

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
            {
                return false;
            }

            List<Thing> list = p.Map.listerThings.ThingsOfDef(WallStuff.ThingDefOf.Wall_Hifi);

            foreach (Thing hifi in list)
            {
                if ((hifi.TryGetComp<CompPowerTrader>()?.PowerOn ?? true) && p.Position.InHorDistOf(hifi.Position, listenDistance))
                {
                    return ThoughtState.ActiveAtStage(0);
                }
            }
            return false;
        }
    }
}
