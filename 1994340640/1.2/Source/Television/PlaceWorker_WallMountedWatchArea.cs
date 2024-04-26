using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace WallStuff
{
    public class PlaceWorker_WallMountedWatchArea : PlaceWorker_WatchArea
    {
        public override void DrawGhost( ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null )
        {
            Map currentMap = Find.CurrentMap;
            var vecNorth = center + IntVec3.North.RotatedBy(rot);

            if (!vecNorth.InBounds(currentMap))
            {
                return;
            }

            GenDraw.DrawFieldEdges(WatchBuildingUtility.CalculateWatchCells(def, vecNorth, rot, currentMap).ToList<IntVec3>());
        }
    }
}