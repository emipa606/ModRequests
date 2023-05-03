using System;
using Verse;
using RimWorld;

namespace Mashed_BlackPlague
{
    class ThoughtWorker_Situational_TuurngaitInColony : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return p.Faction != null && Utility.PawnIsTuurngait(p);
        }
    }
}
