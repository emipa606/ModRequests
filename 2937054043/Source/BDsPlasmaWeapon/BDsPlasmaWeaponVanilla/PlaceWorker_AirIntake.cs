using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace BDsPlasmaWeaponVanilla
{
    public class PlaceWorker_AirIntake : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            int radius = 2;
            foreach (CompProperties comp in def.comps)
            {
                if (comp.compClass == typeof(CompAirIntake))
                {
                    CompProperties_AirIntake compAirIntake = (CompProperties_AirIntake)comp;
                    radius = compAirIntake.radius;
                }
            }
            if (def.rotatable)
            {
                GenDraw.DrawFieldEdges(AirIntakeUtility.CalculateAvaliableCells(center, rot, radius, def.size).ToList());
            }
            else
            {
                GenDraw.DrawFieldEdges(AirIntakeUtility.CalculateAvaliableCells(center, radius, def.size).ToList());
            }
        }
    }
}
