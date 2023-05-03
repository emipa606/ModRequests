using System;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace Mashed_BlackPlague
{
    class DeathActionWorker_Tuurngait : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse)
        {
            if (corpse != null && corpse.InnerPawn != null && corpse.Map != null)
            {
                List<Pawn> pawns = Utility.TuurngaitInFactionList(corpse.InnerPawn.Faction);
                foreach (Pawn p in pawns)
                {
                    p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BlackPlague_TuurngaitConnectionLost, corpse.InnerPawn, null);
                }
            }

        }
    }
}
