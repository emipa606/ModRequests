using System.Text;

using UnityEngine;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace SurvivalistsAdditions
{

    public class HardyPlant : Plant
    {


        private string hardyCachedLabelMouseover;
        private HardyPlantStats ext;

        private HardyPlantStats Ext
        {
            get
            {
                if (ext == null)
                {
                    if (!def.HasModExtension<HardyPlantStats>())
                    {
                        Log.Error($"Survivalists Additions:: No mod extension found for hardy plant - {def.LabelCap}. Assigning default values.");
                        def.modExtensions.Add(new HardyPlantStats());
                    }
                    ext = def.GetModExtension<HardyPlantStats>();
                }
                return ext;
            }
        }

        public override float GrowthRate
        {
            get
            {
                return GrowthRateFactor_Fertility * HardyGrowthRateFactor_Temperature * GrowthRateFactor_Light;
            }
        }

        public float HardyGrowthRateFactor_Temperature
        {
            get
            {
                if (!GenTemperature.TryGetTemperatureForCell(Position, Map, out float num))
                {
                    return 1f;
                }
                if (num < Ext.minOptimalGrowthTemperature)
                {
                    return Mathf.InverseLerp(Ext.minGrowthTemperature, Ext.minOptimalGrowthTemperature, num);
                }
                if (num > Ext.maxOptimalGrowthTemperature)
                {
                    return Mathf.InverseLerp(Ext.maxGrowthTemperature, Ext.maxOptimalGrowthTemperature, num);
                }
                return 1f;
            }
        }

        protected override float LeaflessTemperatureThresh
        {
            get
            {
                float num = 16f;
                return (float)this.HashOffset() * 0.01f % num - num + -2f;
            }
        }

        public override string LabelMouseover
        {
            get
            {
                if (hardyCachedLabelMouseover == null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(def.LabelCap);
                    stringBuilder.Append(" (" + "PercentGrowth".Translate(GrowthPercentString));
                    if (Dying)
                    {
                        stringBuilder.Append(", " + "DyingLower".Translate());
                    }
                    stringBuilder.Append(")");
                    hardyCachedLabelMouseover = stringBuilder.ToString();
                }
                return hardyCachedLabelMouseover;
            }
        }


        // Display additional info on the inspect window that isn't shown normally
        //TODO this has been moved to CompProperties
        //public override IEnumerable<StatDrawEntry> SpecialDisplayStats
        //{
        //    get
        //    {
        //        foreach (StatDrawEntry entry in base.SpecialDisplayStats)
        //        {
        //            yield return entry;
        //        }

        //        yield return new StatDrawEntry(StatCategoryDefOf.PawnMisc, $"{"TabGrowing".Translate()} {Static.TemperatureRangeLower}", $"{Ext.minGrowthTemperature.ToStringTemperature("F0")} ~ {Ext.maxGrowthTemperature.ToStringTemperature("F0")}");
        //        yield return new StatDrawEntry(StatCategoryDefOf.PawnMisc, $"{"Healthy".Translate()} {Static.TemperatureRangeLower}", $"{Ext.minLeaflessTemperature.ToStringTemperature("F0")} ~ {100f.ToStringTemperature("F0")}");
        //    }
        //}


        public bool GrowthSeasonNow(IntVec3 c, Map map)
        {
            Room roomOrAdjacent = c.GetRoomOrAdjacent(map, RegionType.Set_All);
            if (roomOrAdjacent == null)
            {
                return false;
            }
            if (roomOrAdjacent.UsesOutdoorTemperature)
            {
                return map.weatherManager.growthSeasonMemory.GrowthSeasonOutdoorsNow;
            }
            float temperature = c.GetTemperature(map);
            return temperature > Ext.minGrowthTemperature && temperature < Ext.maxGrowthTemperature;
        }


        public override void TickLong()
        {
            CheckTemperatureMakeLeafless();
            if (Destroyed)
            {
                return;
            }
            if (GrowthSeasonNow(Position, Map))
            {
                if (!HasEnoughLightToGrow)
                {
                    unlitTicks += 2000;
                }
                else
                {
                    unlitTicks = 0;
                }
                float num = growthInt;
                bool flag = LifeStage == PlantLifeStage.Mature;
                growthInt += GrowthPerTick * 2000f;
                if (growthInt > 1f)
                {
                    growthInt = 1f;
                }
                if (((!flag && LifeStage == PlantLifeStage.Mature) || (int)(num * 10f) != (int)(growthInt * 10f)) && CurrentlyCultivated())
                {
                    Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);
                }
                if (def.plant.LimitedLifespan)
                {
                    ageInt += 2000;
                    if (Dying)
                    {
                        Map map = Map;
                        bool isCrop = IsCrop;
                        int amount = Mathf.CeilToInt(10f);
                        TakeDamage(new DamageInfo(DamageDefOf.Rotting, amount, -1f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
                        if (Destroyed)
                        {
                            if (isCrop && def.plant.Harvestable && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfRot-" + def.defName, 240f))
                            {
                                Messages.Message("MessagePlantDiedOfRot".Translate(Label).CapitalizeFirst(), new TargetInfo(Position, map, false), MessageTypeDefOf.NegativeEvent);
                            }
                            return;
                        }
                    }
                }
                //Spawn new plants
                //if (def.plant.reproduces && growthInt >= 0.6f && Rand.MTBEventOccurs(def.plant.reproduceMtbDays, 60000f, 2000f))
                //{
                //    if (!PlantUtility.SnowAllowsPlanting(Position, Map))
                //    {
                //        return;
                //    }
                //    GenSpawn.Spawn(plant, Position, Map);
                //}
            }
            hardyCachedLabelMouseover = null;
        }


        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (LifeStage == PlantLifeStage.Growing)
            {
                stringBuilder.AppendLine("PercentGrowth".Translate(GrowthPercentString));
                stringBuilder.AppendLine("GrowthRate".Translate() + ": " + GrowthRate.ToStringPercent());
                if (Resting)
                {
                    stringBuilder.AppendLine("PlantResting".Translate());
                }
                if (!HasEnoughLightToGrow)
                {
                    stringBuilder.AppendLine("PlantNeedsLightLevel".Translate() + ": " + this.def.plant.growMinGlow.ToStringPercent());
                }
                float growthRateFactor_Temperature = HardyGrowthRateFactor_Temperature;
                if (growthRateFactor_Temperature < 0.99f)
                {
                    if (growthRateFactor_Temperature < 0.01f)
                    {
                        stringBuilder.AppendLine("OutOfIdealTemperatureRangeNotGrowing".Translate());
                    }
                    else
                    {
                        stringBuilder.AppendLine("OutOfIdealTemperatureRange".Translate(Mathf.RoundToInt(growthRateFactor_Temperature * 100f).ToString()));
                    }
                }
            }
            else if (LifeStage == PlantLifeStage.Mature)
            {
                if (def.plant.Harvestable)
                {
                    stringBuilder.AppendLine("ReadyToHarvest".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("Mature".Translate());
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}
