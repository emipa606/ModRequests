using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Wheres_Daddy_Going
{
    public class MentalStateWorker_OutToGetSomeMilk : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            Logger.MessageFormat(this, "Checking for {0}", pawn);
            if (pawn == null || pawn.Dead || !pawn.Spawned || pawn.relations == null || pawn.relations.RelatedPawns == null)
                return false;

            List<Pawn> children = pawn.relations.Children.ToList();

            if (children == null || children.Count == 0)
                return false;

            Logger.MessageFormat(this, "{0} has {1} children", pawn, children.Count);

            int childrenOnMap = children.Where(child => child.Map == pawn.Map).Count();

            Logger.MessageFormat(this, "{0} has {1} children on the map", pawn, childrenOnMap);
            return childrenOnMap > 0;
        }
    }
}