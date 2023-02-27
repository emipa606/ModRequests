using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RadWorld
{
	public class ThoughtWorker_ExtraArm : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
		{
			if (!other.RaceProps.Humanlike || other.Dead)
			{
				return false;
			}

			if (other.health?.hediffSet?.GetFirstHediffOfDef(RW_DefOf.RW_ExtraArm) is null)
            {
				return false;
            }
			return true;
		}
	}
}
