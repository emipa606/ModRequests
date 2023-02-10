using SearchAndDestroy.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace SearchAndDestroy
{
    class ThinkNode_ConditionalSearchAndDestroy : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            return pawn.Drafted && pawnData.SD_enabled;
        }
    }
}
