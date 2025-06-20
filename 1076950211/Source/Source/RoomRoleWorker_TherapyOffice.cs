using System.Linq;
using Verse;

namespace Therapy
{
    public class RoomRoleWorker_TherapyOffice : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            var containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            int countCouch = containedAndAdjacentThings.OfType<Building_Couch>().Count();
            int countChair = containedAndAdjacentThings.OfType<Building>().Count(b=>b.def.building.isSittable);
            float couchScore = countCouch >= 1 ? 1 : 0;
            float chairScore = countChair >= 1 ? 1 : 0;
            return couchScore * chairScore * 30;
        }
    }
}