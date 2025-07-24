using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace liberator_stuff
{
	[StaticConstructorOnStartup]
	public static class MechUtils
	{
		public static void CreateOrAddToAssaultLord(Pawn pawn, Lord lord = null, bool canKidnap = false, bool canTimeoutOrFlee = false, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = false)
		{
			bool flag = lord == null && pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction).Any((Pawn p) => p != pawn);
			if (flag)
			{
				lord = ((Pawn)GenClosest.ClosestThing_Global(pawn.Position, pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction), 99999f, (Thing p) => p != pawn && ((Pawn)p).GetLord() != null, null)).GetLord();
			}
			bool flag2 = lord == null;
			if (flag2)
			{
				LordJob_AssaultColony lordJob = new LordJob_AssaultColony(pawn.Faction, canKidnap, canTimeoutOrFlee, sappers, useAvoidGridSmart, canSteal, false, false);
				lord = LordMaker.MakeNewLord(pawn.Faction, lordJob, pawn.Map, null);
			}
			lord.AddPawn(pawn);
		}
	}
}
