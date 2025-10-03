using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace PassThroughWindow
{
    public class Building_PassThroughWindow : Building_Storage
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.Rotation = WindowRotationAt(base.Position, map);
        }

        private static Rot4 WindowRotationAt(IntVec3 loc, Map map)
        {
            int horizontalQuality = AlignQualityAgainst(loc + IntVec3.East, map) + AlignQualityAgainst(loc + IntVec3.West, map);
            int verticalQuality = AlignQualityAgainst(loc + IntVec3.North, map) + AlignQualityAgainst(loc + IntVec3.South, map);

            // Default to North/South if the qualities are equal.
            return horizontalQuality >= verticalQuality ? Rot4.North : Rot4.East;
        }

        private static int AlignQualityAgainst(IntVec3 c, Map map)
        {
            if (!c.InBounds(map))
            {
                return 0;
            }

            List<Thing> thingList = c.GetThingList(map);
            foreach (Thing thing in thingList)
            {
                // Check if the Thing is a building categorized as a wall.
                if (thing.def != null && thing.def.defName == "Wall")
                {
                    return 1; // Strong alignment with walls.
                }
            }

            return 0; // No alignment.
        }
    }
}
