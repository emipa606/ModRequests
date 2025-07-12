using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Dormitories
{
    public class RoomRoleWorker_Dormitory : RoomRoleWorker
    {
        private static DormitoriesSettings settings = LoadedModManager.GetMod<DormitoriesMod>().GetSettings<DormitoriesSettings>();

        public override float GetScore(Room room)
        {
            tmpBeds.Clear();
            int num = 0;
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            for (int i = 0; i < containedAndAdjacentThings.Count; i++)
            {
                Building_Bed building_Bed;
                if ((building_Bed = (containedAndAdjacentThings[i] as Building_Bed)) != null && building_Bed.def.building.bed_humanlike && building_Bed.def.building.bed_countsForBedroomOrBarracks)
                {
                    if (building_Bed.ForPrisoners || building_Bed.Medical)
                    {
                        tmpBeds.Clear();
                        return 0f;
                    }
                    tmpBeds.Add(building_Bed);
                    num++;
                }
            }
            bool flag = RoomRoleWorker_Bedroom.IsBedroom(tmpBeds);
            if (flag || tmpBeds.Count > settings.maximumBeds)
            {
                return 0f;
            }
            tmpBeds.Clear();
            return (float)num * 100190f;
        }

        private static List<Building_Bed> tmpBeds = new List<Building_Bed>();
    }
}