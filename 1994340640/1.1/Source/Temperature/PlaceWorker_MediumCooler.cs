﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace WallStuff
{
    public class PlaceWorker_MediumCooler : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {

            var vecNorth = center + IntVec3.North.RotatedBy(rot);
            var map = Find.CurrentMap;
            if (!vecNorth.InBounds(map))
            {
                return;
            }

            GenDraw.DrawFieldEdges(new List<IntVec3>() { vecNorth }, GenTemperature.ColorRoomCold);
            var room = vecNorth.GetRoom(map);
            if (room == null || room.UsesOutdoorTemperature)
            {
                return;
            }
            GenDraw.DrawFieldEdges(room.Cells.ToList(), GenTemperature.ColorRoomCold);
        }
    }
}