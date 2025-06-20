using UnityEngine;
using Verse;

namespace Therapy
{
    public class PlaceWorker_Couch : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            GenDraw.DrawFieldEdges(MainUtility.ChairCellsAround(center, Find.CurrentMap, out var hasChair), hasChair ? Color.green : Color.yellow);
        }
    }
}