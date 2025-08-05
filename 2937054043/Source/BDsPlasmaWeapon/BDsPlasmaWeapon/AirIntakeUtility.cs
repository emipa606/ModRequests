using System.Collections.Generic;
using Verse;

namespace BDsPlasmaWeapon
{
    public static class AirIntakeUtility
    {
        public static IEnumerable<IntVec3> CalculateAvaliableCells(IntVec3 center, Rot4 rot, int radius, IntVec2 size)
        {
            CellRect rectA = default(CellRect);
            if (rot.IsHorizontal)
            {
                rectA.Width = size.x + (2 * radius);
                rectA.Height = size.z + (2 * radius);
                rectA.minZ = center.x - radius - (size.x / 2);
                rectA.minX = center.z - radius - (size.z / 2);
            }
            else
            {
                rectA.Width = size.x + (2 * radius);
                rectA.Height = size.z + (2 * radius);
                rectA.minX = center.x - radius - (size.x / 2);
                rectA.minZ = center.z - radius - (size.z / 2);
            }
            for (int z = rectA.minZ; z <= rectA.maxZ; z++)
            {
                for (int x = rectA.minX; x <= rectA.maxX; x++)
                {
                    yield return new IntVec3(x, 0, z);
                }
            }
        }

        public static IEnumerable<IntVec3> CalculateAvaliableCells(IntVec3 center, int radius, IntVec2 size)
        {
            int Width = size.x + (2 * radius) - 1;
            int Height = size.z + (2 * radius) - 1;
            int minX = center.x - radius - (size.x / 2) + 1;
            int minZ = center.z - radius - (size.z / 2) + 1;
            int selfMinX = center.x - (size.x / 2) + 1;
            int selfMinZ = center.z - (size.z / 2) + 1;
            for (int z = minZ; z <= (minZ + Height); z++)
            {
                for (int x = minX; x <= (minX + Width); x++)
                {
                    if ((z < selfMinZ || z >= selfMinZ + size.z) || (x < selfMinX || x >= selfMinX + size.x))
                    {
                        yield return new IntVec3(x, 0, z);
                    }
                }
            }
        }
    }
}
