using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace SR
{
    internal static class HarmonyPatchSharedData
    {
        internal static float GravelMultiplier = .33f;

        internal static Dictionary<TerrainDef, float> MultiplierByWaterDef = new Dictionary<TerrainDef, float>
        {
            { TerrainDefs.Marsh, 2.8f },
            { TerrainDefOf.WaterShallow, 4 },
            { TerrainDefOf.WaterMovingShallow, 10 },
            { TerrainDefOf.WaterDeep, 32 },
            { TerrainDefOf.WaterMovingChestDeep, 64 },
            { TerrainDefOf.WaterOceanShallow, 64 },
            { TerrainDefOf.WaterOceanDeep, 128  },
        };

        internal static float DeriveMultiplierForFill(TerrainDef currentTerrain, TerrainDef newTerrain, bool gravelAdjusts = true)
        {
            float multiplier = 1;
            var currentIsWetBridgeable = currentTerrain.IsWetBridgeable();
            var currentIsWater = currentTerrain.IsWater;
            //If it's water now and it will become soil (but not ice).
            if ((currentIsWater || currentIsWetBridgeable) && newTerrain != TerrainDefOf.Ice && newTerrain.HasSoilPlaceWorker())
            {
                if (currentIsWater && MultiplierByWaterDef.ContainsKey(currentTerrain))
                    multiplier = MultiplierByWaterDef[currentTerrain];
                else if (currentIsWetBridgeable)
                    multiplier = 2;
                else //Default catch-all for modded water..
                    multiplier = 4; //Hope it's appropriate, lol.
                if (gravelAdjusts && newTerrain == TerrainDefOf.Gravel)
                    multiplier *= GravelMultiplier;
            }
            return multiplier;
        }

        internal static List<ThingDefCountClass> GetWaterFillAdjustedCostListForFrame(BuildableDef entDef, ThingDef stuff, bool errorOnNullStuff, Frame frame)
        {
            var originalList = entDef.CostListAdjusted(stuff);
            var map = frame.Map;
            if (map == null)
                return originalList;
            var newTerrain = entDef as TerrainDef;
            if (newTerrain == null || newTerrain == TerrainDefOf.Ice) //If it's not a TerrainDef (or it's ice)
                return originalList; //We don't need to touch it.
            var cell = frame.Position;
            var currentTerrain = frame.Position.GetTerrain(map);
            float multiplier = DeriveMultiplierForFill(currentTerrain, newTerrain);
            if (multiplier != 1) //If the multiplier will do anything..
            {
                var newList = new List<ThingDefCountClass>();
                var sb = new StringBuilder();
                for (int i = 0; i < originalList.Count; i++)
                {
                    var oldPair = originalList[i];
                    newList.Add(new ThingDefCountClass(oldPair.thingDef, Mathf.RoundToInt(oldPair.count * multiplier)));
                }
                return newList;
            }
            return originalList;
        }
    }
}