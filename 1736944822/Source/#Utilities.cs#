using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LWM.PrayerSpot {
    public static class Utilities {
        public static void GiveRelaxAloneThought(Verse.AI.JobDriver driver) {
            Pawn pawn=driver.pawn;
/*        }
        public static void GivePrayerT(Verse.AI.JobDriver driver) {
            GivePrayerThoughts(driver.pawn);
            Log.Message("Second try");
        }
        public static void GivePrayerThoughts(Pawn pawn) {
        */
            if (pawn==null) return;
            if (!pawn.Spawned) return;
            if (pawn.Dead) return; // trying to cover the bases
            /* The way "prayer" works:
             * the pawn walks to where they're going to "relax alone"
             * the pawn relaxes/prays/meditates
             *   The tick counter is set to some number - 3000 in vanilla?
             *   The ticks count down (pawn.jobs.curDriver.ticksLeftThisToil)
             *   The count of ticks for the relaxing counts up
             *       (pawn.jobs.curDriver.debugTicksSpentThisToil)
             *   Maybe something ends the job early (full? attacked? break?)
             *   Or the ticks reach 0 and the job ends.
             * this gets called to give the pawn "-2 we need a nice place to pray/meditate/relax"
             *
             * Ideally, I would like to scale the -2,-1,3,5,etc to the time the pawn
             * spent praying/meditating/etc, but let's start small - similar to the
             * way eating/playing billairds/drinking beer/etc works in vanilla:
             * ANY amount of time spent doing the recreation grants the joy.
             *
             * ALSO NOTE: this only works for pawns in chapel.  We'll expand that too.
             */
            //int ticksSpent=pawn.jobs.curDriver.debugTicksSpentThisToil;
            //int totalTicksForFull=Math.Min(pawn.jobs.curDriver.ticksLeftThisToil + ticksSpent, 3000);

            Room room = pawn.GetRoom(RegionType.Set_Passable);
			if (room != null && room.Role == Defs.LWM_Chapel)
			{
				int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
				if (pawn.needs.mood != null && Defs.LWM_PrayedInChapel.stages[scoreStageIndex] != null)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.
                             MakeThought(Defs.LWM_PrayedInChapel, scoreStageIndex), null);
				}
			}

            // var thought=ThoughtMaker.MakeThought(ThoughtDef def, int forcedStage)
            // var thought=ThoughtMaker.MakeThought(DefDatabase<ThoughtDef>.GetNamed(""));
            // pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef def)
            // thought = (Thought_Memory) ThoughtMaker.MakeThought(thoughtDef);
            // targetPawn.needs.mood.thoughts.memories.TryGainMemory(thought);

            //Log.Warning("Testing: Pawn "+pawn+": On tick: "+pawn.jobs.curDriver.ticksLeftThisToil);
            //Log.Message("   ..."+pawn.jobs.curDriver.debugTicksSpentThisToil);
        }


    }


}
