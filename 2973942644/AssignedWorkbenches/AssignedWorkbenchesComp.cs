using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AssignedWorkbenches
{
    public class AssignedWorkbenchesComp : CompAssignableToPawn
    {
        public static bool AllowedToWorkBench(Pawn pawn, Thing thing)
        {
            CompAssignableToPawn awbc = thing.TryGetComp<AssignedWorkbenchesComp>();
            if (awbc != null)
            {
                List<Pawn> assignedPawns = awbc.AssignedPawnsForReading;
                if (assignedPawns.Count == 0)
                {
                    return true;
                }
                else if (assignedPawns.Contains(pawn))
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}
