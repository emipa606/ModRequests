using Verse;
using Verse.AI;

namespace Entomophagy
{
    class JobGiver_WanderForaging : JobGiver_Wander
    {
        public JobGiver_WanderForaging()
        {
            wanderRadius = 7f;
            locomotionUrgency = LocomotionUrgency.Amble;
            ticksBetweenWandersRange = new IntRange(200, 500);
            wanderDestValidator = ((Pawn pawn, IntVec3 loc, IntVec3 root) => loc.GetRoom(pawn.Map).PsychologicallyOutdoors && !loc.GetTerrain(pawn.Map).HasTag("Floor"));
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            return pawn.Position;
        }
    }
}
