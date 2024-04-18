using RimWorld;
using System.Linq;
using Verse;
using WildPlantPicker.DataStore;
using WildPlantPicker.DataStore.Entities;

namespace WildPlantPicker.Helper
{
    internal class DesignationHelper
    {
        internal static bool TryDesignationAll(DataContext dc, Map map, bool ignoreArea, bool ignoreResourceCount, bool silent = false)
        {
            int designatedCount = 0;
            foreach (Plant plant in dc.m_SpawnedPlants.Where(x => map == null || x.Key == map.uniqueID).Select(y => y.Value).SelectMany(plantPerMap => plantPerMap.GetCollection()))
            {
                if (TryDesignation(plant, dc, ignoreArea, ignoreResourceCount))
                {
                    designatedCount++;
                }
            }
            if (!silent)
            {
                if (designatedCount > 0)
                {
                    Messages.Message("WPP_HarvestDesignationMessage_Temp".Translate(designatedCount), MessageTypeDefOf.PositiveEvent);
                    return true;
                }
                else
                {
                    Messages.Message("WPP_HarvestablePlantNotFound".Translate(), MessageTypeDefOf.NeutralEvent);
                    return false;
                }
            }
            return designatedCount > 0;
        }
        internal static bool TryDesignation(Plant plant, DataContext dc, bool ignoreArea = false, bool ignoreResourceCount = false)
        {
            if (!plant.HarvestableNow)
            {
                return false;
            }
            Map map = plant.Map;
            MapCondition mapCondition = dc.GetMapConditions().FirstOrDefault(mc => mc.Key == map.uniqueID).Value;
            if (map == null || !CommonHelper.IsHarvestableMap(map, dc.m_OnlyPlayerColony) || mapCondition == null || !mapCondition.IsEnabled())
            {
                return false;
            }
            TargetPlantCondition targetPlant = dc.GetTargetPlantConditions().FirstOrDefault(tp => tp.IsEnabled(dc) && tp.m_PlantDef == plant.def);
            if (targetPlant == null || targetPlant.m_HarvestThreshold == HarvestThreshold.None)
            {
                return false;
            }
            else if (targetPlant.m_HarvestThreshold == HarvestThreshold.Harvestable ||
            (targetPlant.m_HarvestThreshold == HarvestThreshold.MaxGrowth && plant.Growth >= 0.99f))
            {
                Designation plantDisgnation = map.designationManager.DesignationOn(plant);
                MapConditionUnit unit = mapCondition.m_Units.FirstOrDefault(mcu => mcu.m_HarvestSourceDef == plant.def && mcu.IsEnabled());
                if (unit == null)
                {
                    return false;
                }
                if (plantDisgnation == null)
                {
                    if (CommonHelper.IsFoggCovered(plant, map) || (!unit.m_AllowGrowingZone && CommonHelper.IsHumanManagedPlant(plant, map)) || (!ignoreArea && unit.TryGetArea(out Area area) && !CommonHelper.InArea(plant.Position, map, area)) || (!ignoreResourceCount && unit.m_UseWishResouceCount && map.resourceCounter.GetCount(unit.m_HarvestSourceDef.plant.harvestedThingDef) >= unit.m_WishResouceCount))
                    {
                        return false;
                    }
                }
                else
                {
                    if (plantDisgnation.def == DesignationDefOf.HarvestPlant)
                    {
                        return false;
                    }
                    else if (!dc.m_ForceDesignation || CommonHelper.IsFoggCovered(plant, map) || (!unit.m_AllowGrowingZone && CommonHelper.IsHumanManagedPlant(plant, map)) || (!ignoreArea && unit.TryGetArea(out Area area) && !CommonHelper.InArea(plant.Position, map, area)) || (!ignoreResourceCount && unit.m_UseWishResouceCount && map.resourceCounter.GetCount(unit.m_HarvestSourceDef.plant.harvestedThingDef) >= unit.m_WishResouceCount))
                    {
                        return false;
                    }
                }
                map.designationManager.AddDesignation(new Designation(plant, DesignationDefOf.HarvestPlant));
                return true;
            }
            return false;
        }

        internal static bool TryCancelHarvestDesignationAll(Map map)
        {
            int removeDesignatedCount = 0;
            foreach (Map targetMap in Find.Maps.Where(x => map == null || x.uniqueID == map.uniqueID))
            {
                foreach (Plant plant in targetMap.listerThings.AllThings.Where(p => p is Plant && p.def.category == ThingCategory.Plant && p.def.plant?.harvestedThingDef != null && !CommonHelper.IsHumanManagedPlant((Plant)p, targetMap)))
                {
                    Designation harvestDestination = targetMap.designationManager.DesignationOn(plant);
                    if (harvestDestination?.def != DesignationDefOf.HarvestPlant)
                    {
                        continue;
                    }
                    targetMap.designationManager.RemoveDesignation(harvestDestination);
                    removeDesignatedCount++;
                }
            }
            if (removeDesignatedCount > 0)
            {
                Messages.Message("WPP_HarvestDesignationCancelMessage_Temp".Translate(removeDesignatedCount), MessageTypeDefOf.PositiveEvent);
                return true;
            }
            else
            {
                Messages.Message("WPP_HarvestDesignationNotFound".Translate(), MessageTypeDefOf.NeutralEvent);
                return false;
            }
        }

    }

}
