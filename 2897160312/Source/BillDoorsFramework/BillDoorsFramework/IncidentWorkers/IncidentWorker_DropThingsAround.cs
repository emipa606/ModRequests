using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BillDoorsFramework
{
    public class IncidentWorker_DropThings : IncidentWorker
    {
        public DropThingsExtension extension => def.GetModExtension<DropThingsExtension>();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            if (!def.HasModExtension<DropThingsExtension>())
            {
                Log.Error(def.defName + " does not have DropThingsExtension");
                return false;
            }
            Map map = (Map)parms.target;
            IntVec3 pos;
            return CellFinderLoose.TryFindSkyfallerCell(extension.skyfallerDef, map, out pos, 10, map.Center, 9999);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            SpawnSkyFallers(parms, extension.countRange.RandomInRange);
            Messages.Message(extension.messageKey.Translate(extension.skyfallerDef.label), new TargetInfo(parms.spawnCenter, map), MessageTypeDefOf.NeutralEvent);
            return true;
        }

        protected void SpawnSkyFallers(IncidentParms parms, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (CellFinderLoose.TryFindSkyfallerCell(extension.skyfallerDef, (Map)parms.target, out var pos, 10, parms.spawnCenter, extension.radius))
                {
                    SpawnSkyfaller(parms, pos);
                }
            }
        }

        protected void SpawnSkyfaller(IncidentParms parms)
        {
            SpawnSkyfaller(parms, parms.spawnCenter);
        }

        protected virtual void SpawnSkyfaller(IncidentParms parms, IntVec3 pos)
        {
            Thing thing = ThingMaker.MakeThing(extension.possibleThings.RandomElement());
            SkyfallerMaker.SpawnSkyfaller(extension.skyfallerDef, thing, pos, (Map)parms.target);
        }
    }

    public class DropThingsExtension : DefModExtension
    {
        public int radius = 9999;

        public ThingDef skyfallerDef = RimWorld.ThingDefOf.ShipChunkIncoming;

        public List<ThingDef> possibleThings = new List<ThingDef>();

        public IntRange countRange = new IntRange(3, 5);

        public string messageKey = "MessageShipChunkDrop";
    }
}
