using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ExecutionModByYuno
{
    public static class ExecutionThoughtGiverUtility
    {

		public static void GiveThoughtsForPawnExecuted(Pawn victim, PawnExecutionKind kind)
		{
			if (!victim.RaceProps.Humanlike)
			{
				return;
			}
			int forcedStage = 1;
			if (victim.guilt.IsGuilty)
			{
				forcedStage = 0;
			}
			
			ThoughtDef def;

			if (victim.IsColonist)
			{
				def = YunoThoughtDefOf.YunoMod_KnowColonistExecuted;
			}
			else
			{
				def = YunoThoughtDefOf.YunoMod_KnowGuestExecuted;
			}
			foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
			{
				if (pawn.IsColonist && pawn.needs.mood != null)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(def, forcedStage), null);
				}
			}
		}

	}
}
