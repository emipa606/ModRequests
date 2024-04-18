using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WildPlantPicker.DataStore;
using WildPlantPicker.DataStore.Entities;
using WildPlantPicker.StaticResources;

namespace WildPlantPicker.Helper
{
    internal class CommonHelper
    {
        private static string TranslateGeneral(string token, DataContext dc) => dc.m_TranslateCache[$"{Defined.MOD_ABBREVIATED_NAME}_{token}"];
        internal static string TranslateEnum<T>(T enumValue, DataContext dc) where T : Enum => TranslateGeneral($"{typeof(T).Name}_{enumValue}", dc);

        internal static bool IsHarvestableMap(Map map, bool onlyPlayerColony, bool cache = false)
        {
            if (cache)
            {
                return true;
            }
            else
            {
                if (onlyPlayerColony)
                {
                    return map.IsPlayerHome;
                }
                else
                {
                    return map.mapPawns?.AnyFreeColonistSpawned ?? false;
                }
            }
        }
        internal static bool InArea(IntVec3 pos, Map map, Area area)
        {
            if (area == null || map?.cellIndices == null)
            {
                return false;
            }
            return area.GetCellBool(map.cellIndices.CellToIndex(pos.x, pos.z));
        }
        internal static bool IsFoggCovered(Thing thing, Map map) => map.fogGrid.IsFogged(thing.Position);
        internal static bool IsHumanManagedPlant(Plant plant, Map map) => OnPlantingZone(plant, map) || OnPlantingBuilding(plant, map);
        static bool OnPlantingZone(Thing thing, Map map) => map.zoneManager.AllZones.Any(x => x is Zone_Growing && x.ContainsCell(thing.Position));
        static bool OnPlantingBuilding(Thing thing, Map map) => map.thingGrid.ThingsListAtFast(thing.Position).Any(x => x.def.building?.sowTag != null);
    }
}
