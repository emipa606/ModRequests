using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    public class MentalStateWorker_MeeseeksKillCreator : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            if (!base.StateCanOccur(pawn))
                return false;

            if (!pawn.SpawnedOrAnyParentSpawned)
                return false;

            CompMeeseeksMemory memory = pawn.GetComp<CompMeeseeksMemory>();

            if (memory == null)
                return false;

            // If our creator is a Meeseeks, try his creator and so on until we get a target
            Pawn creator = memory.Creator;

            // If we have no creator, um, just go ahead and we'll figure this out later
            if (!TargetIsValid(pawn, creator))
                return true;

            CompMeeseeksMemory creatorMemory = creator.GetComp<CompMeeseeksMemory>();
            while (creator != null && creatorMemory != null)
            {
                creator = creatorMemory.Creator;
                if (creator == null)
                    creatorMemory = null;
                else
                    creatorMemory = creator.GetComp<CompMeeseeksMemory>();
            }

            return this.TargetIsValid(pawn, creator);
        }

        private bool TargetIsValid(Pawn pawn, Pawn target)
        {
            if (target == null || target.Dead || !target.Spawned || target.Map != pawn.MapHeld)
                return false;

            return true;
        }
    }
}