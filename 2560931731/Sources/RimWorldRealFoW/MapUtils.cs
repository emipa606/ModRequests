using System;
using Verse;
using UnityEngine;
using RimWorld;
using System.Collections.Generic;

namespace RimWorldRealFoW
{
    // Token: 0x02000008 RID: 8
    public static class MapUtils
    {
        // Token: 0x0600002D RID: 45 RVA: 0x000051F0 File Offset: 0x000033F0
        public static MapComponentSeenFog getMapComponentSeenFog(this Map map)
        {
            MapComponentSeenFog mapComponentSeenFog = map.GetComponent<MapComponentSeenFog>();
            if (mapComponentSeenFog == null)
            {
                mapComponentSeenFog = new MapComponentSeenFog(map);
                map.components.Add(mapComponentSeenFog);
            }
            return mapComponentSeenFog;
        }

        public static void RevealMap(Map map)
        {
            MapComponentSeenFog mapComponentSeenFog = map.GetComponent<MapComponentSeenFog>();
            if (mapComponentSeenFog != null)
            {

            }
        }
        public static void MakeSoundWave(Vector3 loc, Map map, float size, float velocity)
        {
            //if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
            //{
            //	return;
            //}
            MoteSoundWave moteSoundWave = (MoteSoundWave)ThingMaker.MakeThing(FoWDef.Mote_SoundWave, null);
            moteSoundWave.Initialize(loc, size, velocity);
            GenSpawn.Spawn(moteSoundWave, loc.ToIntVec3(), map, WipeMode.Vanish);
        }
        public static IEnumerable<Pawn> GetPawnsAround(IntVec3 center, int radius, Map map)
        {
            int numCells = GenRadial.NumCellsInRadius(radius);
			List<Pawn> pawnList = new List<Pawn>();
            var thingGrid = map.thingGrid;
            for (int i = 1; i < numCells; i++)
            {
                var c = center + GenRadial.RadialPattern[i];
                if (c.InBounds(map) == false) continue;
                var things = thingGrid.ThingsListAtFast(c);
                for (int j = 0; j < things.Count; j++)
                    if (things[j].def.category == ThingCategory.Pawn)
						pawnList.Add(things[j] as Pawn);
                        //yield return things[j] as Pawn;
            }
			return pawnList;
		
        }
    }
}
