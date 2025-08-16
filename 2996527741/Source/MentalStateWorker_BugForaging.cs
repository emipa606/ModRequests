using RimWorld;
using Verse;
using Verse.AI;

namespace Entomophagy
{
    class MentalStateWorker_BugForaging : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            MemoryThoughtHandler curMemories = pawn.needs.mood.thoughts.memories;
            if (pawn.Drafted  ||
                HealthAIUtility.ShouldBeTendedNowByPlayer(pawn) ||
                pawn.needs.food == null ||
                curMemories.GetFirstMemoryOfDef(Entomophagy_DefOf.ForagedForBugs) != null ||
                curMemories.GetFirstMemoryOfDef(Entomophagy_DefOf.AteInsectMeatAsEntomophagous) != null ||
                !JoyUtility.EnjoyableOutsideNow(pawn.Map))
            {
                return false;
            }
            return base.StateCanOccur(pawn);
        }
    }
}
