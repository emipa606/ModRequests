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
	public class ThoughtWorker_MutantBlood : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
		{
			if (!other.RaceProps.Humanlike || other.Dead)
			{
				return false;
			}

			if ((!pawn.story?.traits?.HasTrait(RW_DefOf.RW_MutantBlood) ?? false) || (!other.story?.traits?.HasTrait(RW_DefOf.RW_MutantBlood) ?? false))
            {
				return false;
            }
			return true;
		}
	}
}
