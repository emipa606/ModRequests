using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace WallStuff
{
    class PlaceWorker_ShowWallMountedTradeBeaconRadius : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            Map currentMap = Find.CurrentMap;
            IntVec3 rotatedPosition = center + IntVec3.North.RotatedBy(rot);
            GenDraw.DrawFieldEdges(Building_OrbitalTradeBeacon.TradeableCellsAround(rotatedPosition, currentMap));
        }
    }
}
