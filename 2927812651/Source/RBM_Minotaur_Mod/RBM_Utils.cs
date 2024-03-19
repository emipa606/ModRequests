using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RBM_Minotaur
{
    internal class RBM_Utils
    {


        // Generate a tile to flee to from two points and a distance.
        public static IntVec3 GenFleeTile(IntVec3 startPosition, IntVec3 fleeFrom, float fleeDistance, Pawn pawn)
        {
            return GenFleeTile(startPosition.ToVector3(), fleeFrom.ToVector3(), fleeDistance, pawn);
        }
        public static IntVec3 GenFleeTile(Vector3 startPosition, Vector3 fleeFrom, float fleeDistance, Pawn pawn)
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: public static IntVec3 genFleeTile(Vector3 startPosition, Vector3 fleeFrom, float fleeDistance, Pawn pawn)"); }

            //Create a direction vector (heading) and normalise
            Vector3 relativePos = (startPosition - fleeFrom);
            Vector3 NormalizedDirection = relativePos.normalized;

            //Create a flee position from the vecctor and distance
            Vector3 fleeToVector = startPosition + (NormalizedDirection * fleeDistance);
            IntVec3 fleeToIntVec = fleeToVector.ToIntVec3();

            //If no path is found, try a shorter distance
            while (!pawn.CanReach(fleeToIntVec, PathEndMode.OnCell, Danger.Deadly) && fleeDistance > 0)
            {
                fleeDistance--;
                fleeToVector = startPosition + (NormalizedDirection * fleeDistance);
                fleeToIntVec = fleeToVector.ToIntVec3();
            }

            //If still no path is found, flee to current location
            if (fleeDistance <= 0)
            {
                fleeToIntVec = startPosition.ToIntVec3();
                Log.Warning("RBM_Minotaur: Reset flee path for " + pawn.Name + ". No path was found.");
            }

            return fleeToIntVec;
        }

        // Get Heading from two coordinates
        public static Vector3 GetDirection(Vector3 from, Vector3 to)
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: public static Vector3 getDirection(Vector3 from, Vector3 to)"); }
            Vector3 relativePos = (to - from);
            float distance = relativePos.magnitude;
            Vector3 direction = relativePos / distance;
            return direction;
        }
        public static Vector3 GetDirection(IntVec3 from, IntVec3 to)
        {
            return GetDirection(from.ToVector3(), to.ToVector3());
        }

        // Apply terrify effect in an area
        public static bool TerrifyInArea(IntVec3 position, Map map, float radius = 5, Pawn originPawn = null)
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: public static bool terrifyInArea(IntVec3 position, Map map, float radius = 5, Pawn originPawn = null)"); }
            if (map == null) { return false; }
            List<Pawn> mapPawns = map.mapPawns.AllPawnsSpawned;

            for (int i = 0; i < mapPawns.Count; i++)
            {
                bool isHumanlike = mapPawns[i].RaceProps.Humanlike;
                bool isInRange = position.InHorDistOf(mapPawns[i].Position, radius);
                bool isDowned = mapPawns[i].Downed;
                bool isInMentalState = mapPawns[i].InMentalState;
                bool isOriginPawn = false;
                if (originPawn != null && originPawn == mapPawns[i])
                {
                    isOriginPawn = true;
                }

                if (isHumanlike && isInRange && !isDowned && !isInMentalState && !isOriginPawn)
                {
                    LocalTargetInfo t = new LocalTargetInfo(RBM_Utils.GenFleeTile(mapPawns[i].Position, position, MinotaurSettings.SeeRedFleeRadius, mapPawns[i]));
                    Job job = new Job(JobDefOf.FleeAndCower, t);
                    MentalStateDef mentalStateFlee = RBM_DefOf.RBM_TerrifiedFlee;
                    mentalStateFlee.minTicksBeforeRecovery = MinotaurSettings.SeeRedFearDuration;
                    mentalStateFlee.maxTicksBeforeRecovery = MinotaurSettings.SeeRedFearDuration + 1;

                    mapPawns[i].mindState.mentalStateHandler.TryStartMentalState(mentalStateFlee, "scared by something nearby", true, false, null, true);
                    mapPawns[i].jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
            }
            return true;
        }

        //Tries to locate an available milking machine.
        public static bool TryFindMilkingSpot(Pawn pawn, out IntVec3 cell)
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: public static bool TryFindMilkingSpot(Pawn pawn, out IntVec3 cell)"); }
            cell = IntVec3.Zero;
            List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < allBuildingsColonist.Count; i++)
            {
                Building building = allBuildingsColonist[i];
                if (building.def.defName == "RBM_MilkingMachine")
                {
                    //LocalTargetInfo target_location = new LocalTargetInfo(building.InteractionCell);
                    if (pawn.CanReserveAndReach(building.InteractionCell, PathEndMode.OnCell, Danger.Deadly))
                    {
                        cell = building.InteractionCell;
                        return true;
                    }
                }
            }
            return false;
        }




    }
}
