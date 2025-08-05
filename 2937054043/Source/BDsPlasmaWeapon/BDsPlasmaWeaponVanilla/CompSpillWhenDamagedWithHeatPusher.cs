using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using PipeSystem;
using UnityEngine;
using Verse.Noise;

namespace BDsPlasmaWeaponVanilla
{
    public class CompSpillWhenDamagedWithHeatPusher : CompSpillWhenDamaged
    {
        public CompProperties_SpillWhenDamagedWithHeatPusher Props
        {
            get
            {
                return (CompProperties_SpillWhenDamagedWithHeatPusher)props;
            }
        }

        private float snowRadius;

        private ModuleBase snowNoise;

        private bool createEffecter;

        private static HashSet<IntVec3> reachableCells = new HashSet<IntVec3>();

        protected virtual bool ShouldPushHeatNow
        {
            get
            {
                if (!parent.SpawnedOrAnyParentSpawned)
                {
                    return false;
                }
                float ambientTemperature = parent.AmbientTemperature;
                if (ambientTemperature < Props.heatPushMaxTemperature)
                {
                    return ambientTemperature > Props.heatPushMinTemperature;
                }
                return false;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            createEffecter = Props.chooseEffecterFrom.Count > 0;
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref snowRadius, "snowRadius", 0f);
        }

        private void TryExpandSnow()
        {
            if (snowNoise == null)
            {
                snowNoise = new Perlin(0.054999999701976776, 2.0, 0.5, 5, Rand.Range(0, 651431), QualityMode.Medium);
            }
            if (snowRadius < 8f)
            {
                snowRadius += 1.3f;
            }
            else if (snowRadius < 17f)
            {
                snowRadius += 0.7f;
            }
            else if (snowRadius < 30f)
            {
                snowRadius += 0.4f;
            }
            else
            {
                snowRadius += 0.1f;
            }
            snowRadius = Mathf.Min(snowRadius, Props.snowMaxRadius);
            CellRect occupiedRect = parent.OccupiedRect();
            reachableCells.Clear();
            parent.Map.floodFiller.FloodFill(parent.Position, delegate (IntVec3 x)
            {
                if ((float)x.DistanceToSquared(parent.Position) > snowRadius * snowRadius)
                {
                    return false;
                }
                return occupiedRect.Contains(x) || !x.Filled(parent.Map);
            }, delegate (IntVec3 x)
            {
                reachableCells.Add(x);
            });
            int num = GenRadial.NumCellsInRadius(snowRadius);
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = parent.Position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(parent.Map) && reachableCells.Contains(intVec))
                {
                    float value = snowNoise.GetValue(intVec);
                    value += 1f;
                    value *= 0.5f;
                    if (value < 0.1f)
                    {
                        value = 0.1f;
                    }
                    if (!(parent.Map.snowGrid.GetDepth(intVec) > value))
                    {
                        float lengthHorizontal = (intVec - parent.Position).LengthHorizontal;
                        float num2 = 1f - lengthHorizontal / snowRadius;
                        parent.Map.snowGrid.AddDepth(intVec, num2 * Props.snowAddAmount * value);
                    }
                }
            }
        }
        /*
        public override void MoreSpillEffect(Map map, IntVec3 position)
        {
            if (Props.amountToDraw > 0 && parent.GetComp<CompResource>().PipeNet is PipeNet p && p.Stored > Props.amountToDraw)
            {
                if (Props.pushHeat)
                {
                    GenTemperature.PushHeat(position, map, Props.heatPerSecond);
                }
                if (Props.generateSnow)
                {
                    TryExpandSnow();
                }
                if (createEffecter)
                {
                    TargetInfo a = parent;
                    Effecter effecter = Props.chooseEffecterFrom.RandomElement().Spawn(position, map);
                    effecter.Trigger(a, TargetInfo.Invalid);
                }
            }
        }
        */
    }

    public class CompProperties_SpillWhenDamagedWithHeatPusher : CompProperties_SpillWhenDamaged
    {
        public bool pushHeat = false;

        public float heatPerSecond;

        public float heatPushMaxTemperature = 99999f;

        public float heatPushMinTemperature = -99999f;

        public bool generateSnow = false;

        public float snowAddAmount = 0.12f;

        public float snowMaxRadius = 5f;

        public List<EffecterDef> chooseEffecterFrom = new List<EffecterDef>();

        public CompProperties_SpillWhenDamagedWithHeatPusher()
        {
            compClass = typeof(CompSpillWhenDamagedWithHeatPusher);
        }
    }

    public class CompShootFromPipeNet : CompResource
    {
        public CompProperties_ShootFromPipeNet Props
        {
            get
            {
                return (CompProperties_ShootFromPipeNet)props;
            }
        }
    }

    public class CompProperties_ShootFromPipeNet : CompProperties_Resource
    {
        public float resourceConsumption = 1f;

        public bool useAmmo = false;
        public CompProperties_ShootFromPipeNet()
        {
            compClass = typeof(CompShootFromPipeNet);
        }
    }
}
