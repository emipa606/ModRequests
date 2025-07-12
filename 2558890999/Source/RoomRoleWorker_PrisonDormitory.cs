using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Dormitories
{
    public class RoomRoleWorker_PrisonDormitory : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            int num = 0; // number of medical beds
            int num2 = 0; // number of prison beds
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				Building_Bed building_Bed = containedAndAdjacentThings[i] as Building_Bed;
				if (building_Bed != null && building_Bed.def.building.bed_humanlike)
				{
					if (!building_Bed.ForPrisoners)
					{
						return 0f;
					}
					if (building_Bed.Medical)
					{
						num++;
					}
					else if (building_Bed.TotalSleepingSlots > 1)
                    {
						return 0f;
                    }
					else
                    {
						num2++;
                    }
				}
			}
			if (num2 + num <= 1 || num2 + num > 3)
			{
				return 0f;
			}
			return (float)num2 * 100200f + (float)num * 50001f;
		}
    }
}