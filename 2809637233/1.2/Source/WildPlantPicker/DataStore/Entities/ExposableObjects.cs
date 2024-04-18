using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using WildPlantPicker.StaticResources;

namespace WildPlantPicker.DataStore.Entities
{
    public enum AutoHarvestType : byte
    {
        DayCycle = 0,
        Always,
    }
    public enum AlwaysFrequency : byte
    {
        Low = 0,
        Middle,
        High,
    }
    public enum HarvestThreshold : byte
    {
        None = 0,
        Harvestable,
        MaxGrowth,
    }
    public class TargetPlantCondition : IExposable, IEquatable<TargetPlantCondition>, IEqualityComparer<TargetPlantCondition>
    {
        internal ThingDef m_PlantDef;
        internal HarvestThreshold m_HarvestThreshold;
        internal bool m_Immutable;

        [ObsoleteAttribute("This constructor is Reflection only used!!", true)] //expose only
        public TargetPlantCondition()
        {
        }
        public TargetPlantCondition(ThingDef plantDef, HarvestThreshold harvestThreshold = HarvestThreshold.Harvestable, bool isDefault = false)
        {
            this.m_PlantDef = plantDef;
            this.m_HarvestThreshold = harvestThreshold;
            this.m_Immutable = isDefault;
        }

        public bool Valid => this.m_PlantDef?.plant?.harvestedThingDef != null;
        public bool IsEnabled(DataContext dc) => Valid && (!dc.m_OnlyStrictlyWildPlants || Defined.PURE_WILD_PLANTS.Contains(this.m_PlantDef));

        public void ExposeData()
        {
            Scribe_Defs.Look<ThingDef>(ref this.m_PlantDef, "TargetPlantCondition_PlantDef");
            Scribe_Values.Look<HarvestThreshold>(ref this.m_HarvestThreshold, "TargetPlantCondition_HarvestThreshold", HarvestThreshold.Harvestable, false);
            Scribe_Values.Look<bool>(ref this.m_Immutable, "TargetPlantCondition_Immutable", false, false);
        }
        public override bool Equals(object obj)
        {
            TargetPlantCondition targetPlantDef = obj as TargetPlantCondition;
            return targetPlantDef != null && this.GetHashCode() == targetPlantDef.GetHashCode();
        }
        public override int GetHashCode()
        {
            return m_PlantDef?.GetHashCode() ?? -1;
        }
        public override string ToString()
        {
            return $"[plant={m_PlantDef?.LabelCap}, Harvest={m_HarvestThreshold}, Immutable={m_Immutable}]";
        }
        public bool Equals(TargetPlantCondition other)
        {
            return other != null && this.GetHashCode() == other.GetHashCode();
        }
        public bool Equals(TargetPlantCondition x, TargetPlantCondition y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.Equals(y);
        }
        public int GetHashCode(TargetPlantCondition obj)
        {
            return obj?.GetHashCode() ?? -1;
        }
    }
    public class MapConditionUnit : IExposable, IEquatable<MapConditionUnit>, IEqualityComparer<MapConditionUnit>
    {
        internal const int WISH_COUNT_DEFAULT = 75;
        internal bool m_Harvestable; //default: true.
        internal ThingDef m_HarvestSourceDef;
        internal ThingDef m_HarvestedThingDef;
        internal int m_AreaId = -1; //default: Home area.
        internal Area m_Area; //not exposable
        internal bool m_UseWishResouceCount; //default: false.
        internal int m_WishResouceCount; //default: 75.
        internal bool m_AllowGrowingZone; //default: false.


        [ObsoleteAttribute("This constructor is Reflection only used!!", true)] //expose only
        public MapConditionUnit()
        {
        }
        public MapConditionUnit(Map map, ThingDef harvestSourceDef, ThingDef harvestedThingDef)
        {
            this.m_HarvestSourceDef = harvestSourceDef;
            this.m_HarvestedThingDef = harvestedThingDef;
            this.m_AreaId = map.areaManager.Home?.ID ?? -1;
            this.m_Area = map.areaManager.AllAreas.FirstOrDefault(x => x.ID == this.m_AreaId);
            this.m_UseWishResouceCount = false;
            this.m_WishResouceCount = WISH_COUNT_DEFAULT;
            this.m_Harvestable = true;
        }

        public void ClearData()
        {
            this.m_Area = null;
            this.m_AreaId = -1;
        }

        public void SetArea(Area area)
        {
            this.m_Area = area;
            this.m_AreaId = this.m_Area?.ID ?? -1;
        }

        public void ResetArea(Area area, Area defaultArea)
        {
            if (this.m_AreaId < 0)
            {
                return;
            }
            if (this.m_Area == area)
            {
                this.m_Area = defaultArea;
                this.m_AreaId = this.m_Area?.ID ?? -1;
            }
        }

        public bool HasArea(int areaId) => this.m_AreaId > -1 && this.m_AreaId == areaId;
        public bool TryGetArea(out Area area)
        {
            area = m_Area;
            if (area == null)
            {
                return false; 
            }
            return true;
        }

        public bool Valid => this.m_HarvestSourceDef != null && this.m_HarvestedThingDef != null;
        public bool IsEnabled() => this.Valid && this.m_Harvestable;

        public void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.m_Harvestable, "MapConditionUnit_Harvestable", false, false);
            Scribe_Defs.Look<ThingDef>(ref this.m_HarvestSourceDef, "MapConditionUnit_HarvestSourceDef");
            Scribe_Defs.Look<ThingDef>(ref this.m_HarvestedThingDef, "MapConditionUnit_HarvestedThingDef");
            Scribe_Values.Look<int>(ref this.m_AreaId, "MapConditionUnit_AreaId", -1, false);
            Scribe_Values.Look<bool>(ref this.m_UseWishResouceCount, "MapConditionUnit_UseWishResouceCount", false, false);
            Scribe_Values.Look<int>(ref this.m_WishResouceCount, "MapConditionUnit_WishResouceCount", WISH_COUNT_DEFAULT, false);
            Scribe_Values.Look<bool>(ref this.m_AllowGrowingZone, "MapConditionUnit_AllowGrowingZone", false, false);
        }
        public override bool Equals(object obj)
        {
            MapConditionUnit mapConditionUnit = obj as MapConditionUnit;
            return mapConditionUnit != null && this.GetHashCode() == mapConditionUnit.GetHashCode();
        }
        public override int GetHashCode()
        {
            return m_HarvestSourceDef?.GetHashCode() ?? -1;
        }
        public override string ToString()
        {
            return $"[resouce={this.m_HarvestedThingDef?.LabelCap}, wishCount={this.m_WishResouceCount}, harvestable={this.m_Harvestable}]";
        }
        public bool Equals(MapConditionUnit other)
        {
            return other != null && this.GetHashCode() == other.GetHashCode();
        }
        public bool Equals(MapConditionUnit x, MapConditionUnit y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.Equals(y);
        }
        public int GetHashCode(MapConditionUnit obj)
        {
            return obj?.GetHashCode() ?? -1;
        }

    }
    public class MapCondition : IExposable, IEquatable<MapCondition>, IEqualityComparer<MapCondition>
    {
        internal bool m_IsMapAllowed; //default: first colony only true.
        int m_MapId = -1;
        internal Map m_Map; //not exposable

        internal HashSet<MapConditionUnit> m_Units;

        [ObsoleteAttribute("This constructor is Reflection only used!!", true)] //expose only
        public MapCondition()
        {
        }
        public MapCondition(Map map, DataContext dc)
        {
            this.m_IsMapAllowed = map.IsPlayerHome;
            this.m_MapId = map.uniqueID;
            this.m_Map = map;
            
            this.m_Units = new HashSet<MapConditionUnit>();
            foreach (TargetPlantCondition tp in dc.GetTargetPlantConditions().Where(x => x.m_PlantDef?.plant?.harvestedThingDef != null))
            {
                m_Units.Add(new MapConditionUnit(this.m_Map, tp.m_PlantDef, tp.m_PlantDef.plant.harvestedThingDef));
            }
        }

        public void Notify_RemoveMap(Map map)
        {
            if (map != null && map == m_Map)
            {
                this.m_Map = null;
                this.m_MapId = -1;
                foreach (MapConditionUnit unit in m_Units)
                {
                    unit.ClearData();
                }
                m_Units.Clear();
            }
        }
        public void Notify_RemoveArea(Area area)
        {
            if (area == null)
            {
                return;
            }
            foreach (MapConditionUnit unit in m_Units)
            {
                unit.ResetArea(area, m_Map?.areaManager?.Home);
            }
        }

        public bool Valid => this.m_Map != null;
        public bool IsEnabled() => this.Valid && this.m_IsMapAllowed;

        public void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.m_IsMapAllowed, "MapCondition_IsMapAllowed", false, false);
            Scribe_Values.Look<int>(ref this.m_MapId, "MapCondition_MapId", -1, false);
            Scribe_Collections.Look<MapConditionUnit>(ref this.m_Units, "MapCondition_Units", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                m_Map = Find.Maps.FirstOrDefault(x => x.uniqueID == this.m_MapId);
                if (m_Map != null && m_Units != null)
                {
                    Queue<MapConditionUnit> removableUnit = new Queue<MapConditionUnit>();
                    foreach (MapConditionUnit unit in m_Units)
                    {
                        if (!unit.Valid)
                        {
                            unit.ClearData();
                            removableUnit.Enqueue(unit);
                            continue;
                        }
                        unit.m_Area = m_Map.areaManager.AllAreas.FirstOrDefault(x => unit.HasArea(x.ID));
                        if (unit.m_Area == null && unit.m_AreaId > -1)
                        {
                            unit.m_Area = m_Map.areaManager.Home;
                            unit.m_AreaId = unit.m_Area?.ID ?? -1;
                        }
                    }
                    while(removableUnit.Any())
                    {
                        m_Units.Remove(removableUnit.Dequeue());
                    }
                }
            }
        }
        public override bool Equals(object obj)
        {
            MapCondition mapCondition = obj as MapCondition;
            return mapCondition != null && this.GetHashCode() == mapCondition.GetHashCode();
        }
        public override int GetHashCode()
        {
            return m_MapId;
        }
        public bool Equals(MapCondition other)
        {
            return other != null && this.GetHashCode() == other.GetHashCode();
        }
        public bool Equals(MapCondition x, MapCondition y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.Equals(y);
        }
        public int GetHashCode(MapCondition obj)
        {
            return obj?.GetHashCode() ?? -1;
        }
    }
}
