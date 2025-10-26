using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Clutter_Structure
{
    public class PlaceWorker_NoAdjacentDoor : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            GenDraw.DrawFieldEdges(GenAdj.CellsAdjacentCardinal(center, rot, def.Size).ToList<IntVec3>(), Color.white);
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(loc, rot, checkingDef.Size))
            {
                foreach (Thing thing2 in current.GetThingList(map))
                {
                    if (thing2.def.defName == "UtilityWindows")
                    {
                        return "Cant be placed next to window";
                    }
                    else if (thing2.def.IsDoor)
                    {
                        return "Cant be placed next to doors";
                    }

                }
            }
            return true;
        }
    }
}
