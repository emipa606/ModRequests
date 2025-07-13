using RimWorld;
using Verse;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireplace
{
    public class CompProperties_DarklightOverlay : CompProperties_FireOverlay
    {
        public CompProperties_DarklightOverlay()
        {
            compClass = typeof(Fireplace.CompDarklightOverlay);
        }

        public override void DrawGhost(IntVec3 center, Rot4 rot, ThingDef thingDef, Color ghostCol, AltitudeLayer drawAltitude, Thing thing = null)
        {
            Graphic graphic = GhostUtility.GhostGraphicFor(Fireplace.CompDarklightOverlay.DarklightGraphic, thingDef, ghostCol, null);
            Vector3 loc = center.ToVector3ShiftedWithAltitude(drawAltitude) + thingDef.graphicData.DrawOffsetForRot(rot) + DrawOffsetForRot(rot);
            graphic.DrawFromDef(loc, rot, thingDef, 0f);
        }
    }
}
