using Verse;
using RimWorld;

namespace UnderWhere
{
    public class ThoughtWorker_NoUnderwear : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return p.apparel.PsychologicallyNude;
        }
    }
}
