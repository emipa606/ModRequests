using RimWorld;
using Verse;

namespace BetterParenting
{
    public class ThoughtWorker_Child : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            if (!p.RaceProps.Humanlike)
            {
                return false;
            }
            if (!p.story.traits.HasTrait(TraitDefOf.BetterParentingDislikesChildren))
            {
                return false;
            }
            if (!RelationsUtility.PawnsKnowEachOther(p, other))
            {
                return false;
            }
            if (other.def != p.def)
            {
                return false;
            }
            if (other.ageTracker.Adult)
            {
                return false;
            }
            if (other.relations.DirectRelationExists(PawnRelationDefOf.Parent, p))
            {
                return false;
            }
            return true;
        }
    }
}
