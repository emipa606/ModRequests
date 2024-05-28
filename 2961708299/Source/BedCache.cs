using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using RimWorld.Planet;

namespace zed_0xff.LoftBed
{
    class BedCache : WorldComponent
    {
        private static HashSet<ThingWithComps> loftBeds = new HashSet<ThingWithComps>();
        private static Dictionary<int, Dictionary<IntVec3, ThingWithComps>> mapPosLoftBeds = new Dictionary<int, Dictionary<IntVec3, ThingWithComps>>();

        public BedCache(World w) : base(w)
        {
            // clear caches whenever a game is created or loaded.
            loftBeds.Clear();
            mapPosLoftBeds.Clear();
        }

        // null-safe
        public static bool isLoftBed(Thing t){
            return t is ThingWithComps twc && loftBeds.Contains(twc);
        }

        // null-safe
        public static ThingWithComps currentBedFor(Pawn p){
            if( p == null )
                return null;

            if( !mapPosLoftBeds.ContainsKey(p.Map.uniqueID) )
                return null;

            ThingWithComps t = null;
            if (mapPosLoftBeds[p.Map.uniqueID].TryGetValue(p.Position, out t)){
                if( t is Building_Bed bed && bed.OwnersForReading.Contains(p) ){
                    return bed;
                }
            }

            return null;
        }

        public static void Add(ThingWithComps t){
            loftBeds.Add(t);
            if( !mapPosLoftBeds.ContainsKey(t.Map.uniqueID) ){
                mapPosLoftBeds.Add(t.Map.uniqueID, new Dictionary<IntVec3, ThingWithComps>());
            }
            if( t is Building_Bed bed ){
                for( int i=0; i<bed.SleepingSlotsCount; i++ ){
                    mapPosLoftBeds[t.Map.uniqueID][bed.GetSleepingSlotPos(i)] = t;
                }
            } else {
                Log.ErrorOnce("[?] LeftBed: should not be here", 1431242932);
            }
        }

        public static void Remove(ThingWithComps t, Map map){
            loftBeds.Remove(t);
            if( mapPosLoftBeds.ContainsKey(map.uniqueID) ){
                if( t is Building_Bed bed ){
                    ThingWithComps t1 = null;
                    for( int i=0; i<bed.SleepingSlotsCount; i++ ){
                        var pos = bed.GetSleepingSlotPos(i);
                        if (mapPosLoftBeds[map.uniqueID].TryGetValue(pos, out t1) && t1 == t){
                            mapPosLoftBeds[map.uniqueID].Remove(pos);
                        } else {
                            // bed is already replaced by hospitality
                        }
                    }
                } else {
                    Log.ErrorOnce("[?] LeftBed: should not be here 2", 1431242933);
                }
            }
        }
    }
}
