using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using WildPlantPicker.Collection;
using WildPlantPicker.DataStore.Entities;
using WildPlantPicker.Dialog;
using WildPlantPicker.Helper;
using WildPlantPicker.StaticResources;

namespace WildPlantPicker.DataStore
{
    [StaticConstructorOnStartup]
    public class DataContext : WorldComponent
    {
        #region persistent
        public bool m_AutoHarvestEnabled = false;
        public AutoHarvestType m_AutoHarvestType = AutoHarvestType.DayCycle;
        public bool m_DayCycleNoMessage = false;
        public AlwaysFrequency m_AlwaysFrequency = AlwaysFrequency.Low;
        public bool m_OnlyPlayerColony = true;
        public bool m_ForceDesignation = false;
        public bool m_OnlyStrictlyWildPlants = true;
        HashSet<TargetPlantCondition> m_TargetPlantConditions;
        Dictionary<int, MapCondition> m_MapConditions;
        public Dictionary<int, int> m_SpawnedPlantsIndexes;
        #endregion

        #region not persistent
        public SettingDialog m_SettingDialog = new SettingDialog();
        public Dictionary<int, LinearBufferHashSet<Plant>> m_SpawnedPlants = new Dictionary<int, LinearBufferHashSet<Plant>>();

        public readonly IDictionary<string, string> m_TranslateCache = new SortedDictionary<string, string>() {
            { "WPP_AutoHarvestType_DayCycle", "WPP_AutoHarvestType_DayCycle".Translate() },
            { "WPP_AutoHarvestType_Always", "WPP_AutoHarvestType_Always".Translate() },
            { "WPP_AlwaysFrequency_Low", "WPP_AlwaysFrequency_Low".Translate() },
            { "WPP_AlwaysFrequency_Middle", "WPP_AlwaysFrequency_Middle".Translate() },
            { "WPP_AlwaysFrequency_High", "WPP_AlwaysFrequency_High".Translate() },
            { "WPP_HarvestThreshold_None", "WPP_HarvestThreshold_None".Translate() },
            { "WPP_HarvestThreshold_Harvestable", "WPP_HarvestThreshold_Harvestable".Translate() },
            { "WPP_HarvestThreshold_MaxGrowth", "WPP_HarvestThreshold_MaxGrowth".Translate() },
        };
        #endregion

        public static DataContext Current() => Find.World.GetComponent<DataContext>(); //in game only!

        public void Notify_MapFinalizeInit(Map map)
        {
            if (m_MapConditions == null)
            {
                m_MapConditions = new Dictionary<int, MapCondition>();
            }
            if (!m_MapConditions.ContainsKey(map.uniqueID))
            {
                m_MapConditions[map.uniqueID] = new MapCondition(map, this);
            }
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            this.InitializeSettingData();
        }

        private void InitializeSettingData()
        {
            if (m_SpawnedPlantsIndexes == null)
            {
                m_SpawnedPlantsIndexes = new Dictionary<int, int>();
            }
            if (m_TargetPlantConditions == null)
            {
                m_TargetPlantConditions = new HashSet<TargetPlantCondition>();
                foreach (ThingDef defaultPlant in new ThingDef[] { ThingDefOf.Plant_Ambrosia, Defs.ThingDefOf.Plant_Berry, Defs.ThingDefOf.Plant_HealrootWild })
                {
                    if (defaultPlant == ThingDefOf.Plant_Ambrosia)
                    {
                        m_TargetPlantConditions.Add(new TargetPlantCondition(defaultPlant, HarvestThreshold.MaxGrowth, true));
                    }
                    else
                    {
                        m_TargetPlantConditions.Add(new TargetPlantCondition(defaultPlant, HarvestThreshold.Harvestable, false));
                    }
                }
            }
        }

        public HashSet<TargetPlantCondition> GetTargetPlantConditions() => m_TargetPlantConditions;
        public Dictionary<int, MapCondition> GetMapConditions() => m_MapConditions;

        public void AddCondition(TargetPlantCondition targetPlantCondition)
        {
            this.m_TargetPlantConditions.Add(targetPlantCondition);
            foreach (MapCondition mc in this.m_MapConditions.Values)
            {
                mc.m_Units.Add(new MapConditionUnit(mc.m_Map, targetPlantCondition.m_PlantDef, targetPlantCondition.m_PlantDef.plant.harvestedThingDef));
            }
        }

        public void RemoveCondition(TargetPlantCondition targetPlantCondition)
        {
            this.m_TargetPlantConditions.Remove(targetPlantCondition);
            foreach (MapCondition mc in this.m_MapConditions.Values)
            {
                Queue<MapConditionUnit> removableUnits = new Queue<MapConditionUnit>();
                foreach (MapConditionUnit mcu in mc.m_Units.Where(mcu => mcu.m_HarvestSourceDef == targetPlantCondition.m_PlantDef))
                {
                    removableUnits.Enqueue(mcu);
                }
                while (removableUnits.Any())
                {
                    mc.m_Units.Remove(removableUnits.Dequeue());
                }
            }

        }

        public void SettingDataReset()
        {
            this.m_AutoHarvestEnabled = false;
            this.m_AutoHarvestType = AutoHarvestType.DayCycle;
            this.m_DayCycleNoMessage = false;
            this.m_AlwaysFrequency = AlwaysFrequency.Low;
            this.m_OnlyPlayerColony = true;
            this.m_ForceDesignation = false;
            this.m_OnlyStrictlyWildPlants = true;
            foreach (Map map in Find.Maps)
            {
                this.RemoveMap(map);
            }
            this.m_MapConditions = null;
            this.m_TargetPlantConditions = null;
            this.m_SpawnedPlantsIndexes = null;
            this.InitializeSettingData();
            foreach (Map map in Find.Maps)
            {
                this.Notify_MapFinalizeInit(map);
            }
        }

        public void Restore()
        {
            int plantCount = 0;
            IEnumerable<TargetPlantCondition> conditions = this.GetTargetPlantConditions();
            m_SpawnedPlants.Clear();
            foreach (Plant plant in Find.Maps.Where(x => CommonHelper.IsHarvestableMap(x, this.m_OnlyPlayerColony, true)).SelectMany(x => x.listerThings.AllThings.Where(y => y.Map != null && y.Spawned && y.def.category == ThingCategory.Plant && conditions.Any(p => p.m_PlantDef == y.def))))
            {
                Map map = plant.Map;
                this.Add(map, plant);
                plantCount++;
            }
            Messages.Message("WPP_ReCacheMessage_Temp".Translate(plantCount), MessageTypeDefOf.NeutralEvent, false);
        }

        public void Add(Map map, Plant plant)
        {
            if (!m_SpawnedPlants.ContainsKey(map.uniqueID))
            {
                m_SpawnedPlants[map.uniqueID] = new LinearBufferHashSet<Plant>();
            }
            m_SpawnedPlants[map.uniqueID].Add(plant);
        }

        public void Remove(Map map, Plant plant)
        {
            if (m_SpawnedPlants.ContainsKey(map.uniqueID))
            {
                m_SpawnedPlants[map.uniqueID].Remove(plant);
            }
        }

        public void RemoveMap(Map map)
        {
            if (map == null)
            {
                return;
            }
            Queue<int> removableMapConditionMapIds = new Queue<int>();
            foreach (KeyValuePair<int, MapCondition> mapConditionPair in this.m_MapConditions.Where(pair => pair.Key == map.uniqueID))
            {
                removableMapConditionMapIds.Enqueue(mapConditionPair.Key);
                mapConditionPair.Value.Notify_RemoveMap(map);
            }
            while (removableMapConditionMapIds.Any())
            {
                this.m_MapConditions.Remove(removableMapConditionMapIds.Dequeue());
            }
            if (m_SpawnedPlants.ContainsKey(map.uniqueID))
            {
                m_SpawnedPlants.Remove(map.uniqueID);
            }
        }

        public void RemoveArea(Area area)
        {
            if (area == null)
            {
                return;
            }
            foreach (MapCondition mapCondition in DataContext.Current().m_MapConditions.Values)
            {
                mapCondition.Notify_RemoveArea(area);
            }
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            int tick = Find.TickManager.TicksGame;
            if (tick % Defined.FREQUENCY_VALUES[AlwaysFrequency.Low] == 0)
            {
                if (m_AutoHarvestEnabled && tick > 0)
                {
                    if (m_AutoHarvestType == AutoHarvestType.DayCycle)
                    {
                        if (tick % GenDate.TicksPerDay == 0)
                        {
                            DesignationHelper.TryDesignationAll(this, null, false, false, this.m_DayCycleNoMessage);
                        }
                    }
                    else
                    {
                        bool nextHarvest = m_AlwaysFrequency == AlwaysFrequency.Low;
                        nextHarvest = nextHarvest || (m_AlwaysFrequency == AlwaysFrequency.Middle && tick % Defined.FREQUENCY_VALUES[AlwaysFrequency.Middle] == 0);
                        nextHarvest = nextHarvest || (m_AlwaysFrequency == AlwaysFrequency.High && tick % Defined.FREQUENCY_VALUES[AlwaysFrequency.High] == 0);
                        if (nextHarvest)
                        {
                            foreach (KeyValuePair<int, LinearBufferHashSet<Plant>> collectionPerMap in m_SpawnedPlants)
                            {
                                m_SpawnedPlantsIndexes[collectionPerMap.Key] = collectionPerMap.Value.Next(x =>
                                {
                                    DesignationHelper.TryDesignation(x, this);
                                });
                            }
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.m_AutoHarvestEnabled, "DataContext_AutoHarvestEnabled", false, false);
            Scribe_Values.Look<AutoHarvestType>(ref this.m_AutoHarvestType, "DataContext_AutoHarvestType", AutoHarvestType.DayCycle, false);
            Scribe_Values.Look<bool>(ref this.m_DayCycleNoMessage, "DataContext_DayCycleNoMessage", false, false);
            Scribe_Values.Look<AlwaysFrequency>(ref this.m_AlwaysFrequency, "DataContext_AlwaysFrequency", AlwaysFrequency.Low, false);
            Scribe_Values.Look<bool>(ref this.m_OnlyPlayerColony, "DataContext_OnlyPlayerColony", true, false);
            Scribe_Values.Look<bool>(ref this.m_ForceDesignation, "DataContext_ForceDesignation", false, false);
            Scribe_Values.Look<bool>(ref this.m_OnlyStrictlyWildPlants, "DataContext_OnlyStrictlyWildPlants", false, false);

            Scribe_Collections.Look<TargetPlantCondition>(ref this.m_TargetPlantConditions, "DataContext_TargetPlantConditions", LookMode.Deep);
            Scribe_Collections.Look<int, MapCondition>(ref this.m_MapConditions, "DataContext_MapConditions", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look<int, int>(ref this.m_SpawnedPlantsIndexes, "DataContext_SpawnedPlantsIndexes", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                if (this.m_TargetPlantConditions != null)
                {
                    Queue<TargetPlantCondition> removes = new Queue<TargetPlantCondition>(this.m_TargetPlantConditions.Where(x => !x.Valid));
                    while (removes.Any())
                    {
                        m_TargetPlantConditions.Remove(removes.Dequeue());
                    }
                }
                if (this.m_SpawnedPlantsIndexes != null)
                {
                    foreach (KeyValuePair<int, int> indexPerMap in this.m_SpawnedPlantsIndexes)
                    {
                        m_SpawnedPlants[indexPerMap.Key] = new LinearBufferHashSet<Plant>(indexPerMap.Value);
                    }
                }
            }
        }

        public DataContext(World world) : base(world)
        {
        }
    }
}
