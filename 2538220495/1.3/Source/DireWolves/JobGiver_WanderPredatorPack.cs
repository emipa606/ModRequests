using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DireWolves
{
	public class JobGiver_WanderPredatorPack : JobGiver_Wander
	{
		public JobGiver_WanderPredatorPack()
		{
			wanderRadius = 5f;
			ticksBetweenWandersRange = new IntRange(125, 200);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			Predicate<Thing> validator = delegate (Thing t)
			{
				if (((Pawn)t).RaceProps != pawn.RaceProps || t == pawn)
				{
					return false;
				}
				if (t.Faction != pawn.Faction)
				{
					return false;
				}
				if (t.Position.IsForbidden(pawn))
				{
					return false;
				}
				if (!pawn.CanReach(t, PathEndMode.OnCell, Danger.Deadly))
				{
					return false;
				}
				if (Rand.Value < 0.5f)
				{
					return false;
				}
				return true;
			};
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn), 35f, validator, null, 13)?.Position ?? pawn.Position;
		}
	}
}

