using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;


namespace LWM.PrayerSpot {
    public class JoyGiver_Spiritual_Simple : JoyGiver_InPrivateRoom {
        // I THINK I will need a separate JoyGiver for the vanilla Pray - it's
        //   possible I won't, but this is safe enought to do.  I hope.
        static JoyGiver_Spiritual_Simple() {
            if (Defs.LWM_PrayerSpot == null) {
                Log.Error("Failed to load PrayerSpot Def");
            }
            vanillaPray = (JoyGiver)Activator.CreateInstance(typeof(JoyGiver_InPrivateRoom));
            vanillaPray.def= DefDatabase<JoyGiverDef>.GetNamed("Pray");
        }

        // for random order:
        static Random rng = new Random();

        public override Job TryGiveJob(Pawn pawn) {
            if (pawn.Map == null) return null;
            List<Building> mapPrayerSpots = pawn.Map.listerBuildings.AllBuildingsColonistOfDef(Defs.LWM_PrayerSpot).ToList();
            mapPrayerSpots.AddRange(pawn.Map.listerBuildings.AllBuildingsColonistOfDef(Defs.LWM_PrayerSpot_Dir));
            Building spot = null;

            // Add a null prayerspot to represent vanilla praying/meditating in room:
            //   (but only if there's not already a prayerspot in the room!)
            Room r1;
            if ((r1 = pawn?.ownership?.OwnedRoom)!=null) {
                bool hasSpotInRoom=false;
                List<Thing> l=r1.ContainedAndAdjacentThings;
                for (int i=0; i<l.Count; i++) {
                    if (l[i].def == Defs.LWM_PrayerSpot ||
                        l[i].def == Defs.LWM_PrayerSpot_Dir) {
                        hasSpotInRoom=true;
                        break;
                    }
                }
                if (!hasSpotInRoom) mapPrayerSpots.Add(null);
            }

            // Randomize mapPrayerSpots:
            //   (I got this off the internet)
            /*
            for (int i=mapPrayerSpots.Count-1; i>0; i--) {
                int j = rng.Next(i + 1);
                spot = mapPrayerSpots[j];
                mapPrayerSpots[j] = mapPrayerSpots[i];
                mapPrayerSpots[i] = spot;
            }*/
            for (int i = mapPrayerSpots.Count - 1; i >= 0; i--) { // >=0 so we try 0th spot
                int j = rng.Next(i + 1);
                spot = mapPrayerSpots[j];
                if (spot==null) { // pawn in own room.
                    Job job=vanillaPray.TryGiveJob(pawn);
                    if (job!=null) {
                        return job;
                    } // maybe the door was locked because insects are rampaging in the room?
                    continue;
                }

                Room room = spot.GetRoom();
                IntVec3 c;
                // Don't pray in other people's rooms, eh?
                if (room!=null) {
//                    Log.Message("  In a room!");
                    if (room.Role == RoomRoleDefOf.PrisonBarracks || room.Role == RoomRoleDefOf.PrisonCell) {
                        // prison room: Should we allow praying in prison rooms?
                        // Seems kind of rude.  TODO: revisit
                        if (pawn.IsPrisoner) {
                            c = spot.Position;
                            if (c.Standable(pawn.Map) && !c.IsForbidden(pawn) &&
                                pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None, 1, -1, null, false)) {
                                return new Job(this.def.jobDef, c);
                            }
                        } // /pawn.IsPrisoner
                    } else {
                        // not a prison room
                        IEnumerable<Pawn> owners = room.Owners;
                        if (owners.Contains(pawn)|| !owners.Any()) {
                            c = spot.Position;
                            if (c.Standable(pawn.Map) && !c.IsForbidden(pawn) &&
                                pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None, 1, -1, null, false)) {
                                return JobMaker.MakeJob(this.def.jobDef, c);
                            }
                        }
                    } // /not prison room
                } else { // not in a room, so anyone can use
//                    Log.Message("  Rando spot outside a room!");
                    c = spot.Position;
                    if (c.Standable(pawn.Map) && !c.IsForbidden(pawn) &&
                        pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None, 1, -1, null, false)) {
                        return JobMaker.MakeJob(this.def.jobDef, c);
                    }
                }
                // carry on to the next random spot:
                if (j<i) mapPrayerSpots[j] = mapPrayerSpots[i];
                //mapPrayerSpots[i] = spot; Meh. Done with it anyway
            }
            // All spots failed or there were no prayer spots.
            // Default to vanilla:
//            Log.Message("Defaulting to Vanilla:"+vanillaPray+" ("+vanillaPray.def+")");
            return vanillaPray.TryGiveJob(pawn);
        }
        static JoyGiver vanillaPray; // vanilla fallback
    }
}
