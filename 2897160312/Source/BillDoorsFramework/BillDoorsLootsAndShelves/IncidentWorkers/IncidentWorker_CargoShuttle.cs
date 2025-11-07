using BillDoorsFramework;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BillDoorsLootsAndShelves
{
    public class IncidentWorker_CargoShuttle : IncidentWorker_DropThings
    {
        Thing intendedThing;

        CompWorldTileTargetable CompWorldTileTargetable;

        public CargoShuttleIncidentExtension cargoExtension => def.GetModExtension<CargoShuttleIncidentExtension>();
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (cargoExtension == null)
            {
                Log.Error(def.defName + " does not have cargoExtension");
                return false;
            }
            intendedThing = null;
            Map map = (Map)parms.target;
            Thing thing = GenClosest.ClosestThing_Global(parms.spawnCenter, map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.ThingHolder)), cargoExtension.radius, x => x.Spawned && x.Faction == parms.faction && cargoExtension.acceptedThings.Contains(x.def) && (x as IThingHolder).GetDirectlyHeldThings().Any && !x.Position.Roofed(map));
            if (thing != null)
            {
                intendedThing = thing;

                SpawnSkyfaller(parms, thing.Position);
                Messages.Message(extension.messageKey.Translate(extension.skyfallerDef.label), new TargetInfo(parms.spawnCenter, map), MessageTypeDefOf.NeutralEvent);
                return true;
            }
            return false;
        }

        protected override void SpawnSkyfaller(IncidentParms parms, IntVec3 pos)
        {
            Skyfaller skyfaller = SkyfallerMaker.SpawnSkyfaller(extension.skyfallerDef, pos, (Map)parms.target);
            if (skyfaller is Skyfaller_MedEvac evac)
            {
                evac.intendedThing = intendedThing;
            }
        }
    }

    public class CargoShuttleIncidentExtension : DefModExtension
    {
        public int radius = 9999;

        public List<ThingDef> acceptedThings = new List<ThingDef>();
    }
}
