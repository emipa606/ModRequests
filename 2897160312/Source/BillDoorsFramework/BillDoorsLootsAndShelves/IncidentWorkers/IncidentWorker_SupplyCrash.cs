using BillDoorsFramework;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BillDoorsLootsAndShelves
{
    public class IncidentWorker_SupplyCrash : IncidentWorker_DropThings
    {
        SupplyCrashIncidentExtension LootExtension => def.GetModExtension<SupplyCrashIncidentExtension>();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            if (!def.HasModExtension<SupplyCrashIncidentExtension>())
            {
                Log.Error(def.defName + " does not have SupplyCrashIncidentExtension");
                return false;
            }
            Map map = (Map)parms.target;
            IntVec3 pos;
            return CellFinderLoose.TryFindSkyfallerCell(extension.skyfallerDef, map, out pos, 10, map.Center, 9999);
        }


        protected override void SpawnSkyfaller(IncidentParms parms, IntVec3 pos)
        {
            Thing thing = ThingMaker.MakeThing(extension.possibleThings.RandomElement());
            if (thing is LootContainer container)
            {
                container.clearAndGenerateLootFromOption(LootExtension.Options);
                if (container.def.rotatable)
                {
                    container.Rotation = Rot4.Random;
                }

                ThingDef temp = extension.skyfallerDef;

                temp.size = container.def.size;

                SkyfallerMaker.SpawnSkyfaller(temp, container, pos, (Map)parms.target);
            }
            else
            {
                Log.Warning("Non loot container " + thing.def.defName + " found in " + def.defName);
            }
        }
    }

    public class SupplyCrashIncidentExtension : DefModExtension
    {

        public List<LootContentDefWeight> Options;
    }
}
