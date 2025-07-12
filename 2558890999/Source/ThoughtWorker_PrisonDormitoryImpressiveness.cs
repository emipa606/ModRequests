using RimWorld;
using Verse;

namespace Dormitories
{
    public class ThoughtWorker_PrisonDormitoryImpressiveness : ThoughtWorker_RoomImpressiveness
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.IsPrisoner)
            {
                return ThoughtState.Inactive;
            }
            ThoughtState result = base.CurrentStateInternal(p);
            if (result.Active && p.GetRoom().Role == DormitoriesDefOf.PrisonDormitory)
            {
                return result;
            }
            return ThoughtState.Inactive;
        }
    }
}