using RimWorld;
using UnityEngine;
using Verse;

namespace MaliExtinguishRefuelables
{
	public class CompProperties_FireOverlayExtinguishable : CompProperties
    {
        public float fireSize = 1f;

        public float finalFireSize = 1f;

        public float fireGrowthDurationTicks = -1f;

        public Vector3 offset;

        public Vector3? offsetNorth;

        public Vector3? offsetSouth;

        public Vector3? offsetWest;

        public Vector3? offsetEast;

        public CompProperties_FireOverlayExtinguishable()
		{
			compClass = typeof(CompFireOverlayExtinguishable);
        }

        public Vector3 DrawOffsetForRot(Rot4 rot)
        {
            switch (rot.AsInt)
            {
                case 0:
                    return offsetNorth ?? offset;
                case 1:
                    return offsetEast ?? offset;
                case 2:
                    return offsetSouth ?? offset;
                case 3:
                    return offsetWest ?? offset;
                default:
                    return offset;
            }
        }
    }
}
