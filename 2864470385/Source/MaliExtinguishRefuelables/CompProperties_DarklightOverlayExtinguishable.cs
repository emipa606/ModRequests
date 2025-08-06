using RimWorld;
using UnityEngine;
using Verse;

namespace MaliExtinguishRefuelables
{
    public class CompProperties_DarklightOverlayExtinguishable : CompProperties_FireOverlayExtinguishable
    {
        public CompProperties_DarklightOverlayExtinguishable()
        {
            compClass = typeof(CompDarklightOverlayExtinguishable);
        }

        public override void DrawGhost(IntVec3 center, Rot4 rot, ThingDef thingDef, Color ghostCol, AltitudeLayer drawAltitude, Thing thing = null)
        {
            GhostUtility.GhostGraphicFor(CompDarklightOverlayExtinguishable.DarklightGraphic, thingDef, ghostCol).DrawFromDef(center.ToVector3ShiftedWithAltitude(drawAltitude), rot, thingDef);
        }
    }
}