using Hospitality.Utilities;
using Verse;
using Verse.AI;

namespace Hospitality
{
    public class ThinkNode_InGuestArea : ThinkNode_Conditional
    {
        public override bool Satisfied(Pawn pawn)
        {
            var area = pawn.GetGuestArea();
            if (area == null) return true;

            return area[pawn.PositionHeld];
        }
    }
}