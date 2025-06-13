using System.Linq;
using Hospitality.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hospitality
{
    public class JobGiver_GotoGuestArea : ThinkNode
    {
        public override float GetPriority(Pawn pawn)
        {
            var area = pawn.GetGuestArea();
            if (area == null) return 0;
            if (area.TrueCount == 0) return 0;

            var result = area[pawn.PositionHeld] ? 0 : 10;
            return result;
        }

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            var area = pawn.GetGuestArea();
            if (area == null) return ThinkResult.NoJob;
            if (area.TrueCount == 0) return ThinkResult.NoJob;

            // Find nearby
            bool found = CellFinder.TryFindRandomReachableNearbyCell(pawn.Position, pawn.MapHeld, 20, 
                TraverseParms.For(pawn, Danger.Some, TraverseMode.PassDoors), c => area[c], null, out var closeSpot);

            if (!found)
            {
                // Find any
                closeSpot = area.ActiveCells.InRandomOrder().Where(cell => cell.Walkable(area.Map)).Take(20)
                    .FirstOrDefault(cell => pawn.CanReach(cell, PathEndMode.OnCell, Danger.Some, false, false, TraverseMode.PassDoors));
                found = closeSpot.IsValid;
            }

            var job = found ? new ThinkResult(new Job(JobDefOf.Goto, closeSpot) {locomotionUrgency = LocomotionUrgency.Jog}, this) : ThinkResult.NoJob;

            return job;
        }
    }
}
