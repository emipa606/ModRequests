using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Diabetes
{
	public static class DiabetesUtility
	{
		public const float InsulinValue = 0.1f;

		public static int NumToIngest(Thing thing, Hediff hediff, float value)
		{
			return Mathf.Min(new int[]
				{
					thing.stackCount,
					thing.def.ingestible.maxNumToIngestAtOnce,
					Mathf.CeilToInt(hediff.Severity / value)
				});
		}

		public static Thing FindNextInsulin(Pawn pawn)
		{
			Thing insulin = FindInInventory(pawn.inventory.innerContainer);

			if (insulin != null)
			{
				return insulin;
			}

			List<Thing> candidates = new List<Thing>();

			candidates.AddRange(pawn.Map.listerThings.ThingsOfDef(TypeGetter.ThingDef(EThingDef.Insulin)));
			for (int i = 0; i < candidates.Count; i++)
			{
				Thing thing = candidates[i];
				if (thing.IsForbidden(pawn))
				{
					candidates.Remove(thing);
					i--;
				}
			}
			Log.Message("Count: " + candidates.Count);
			if (candidates.Count == 0)
			{
				return null;
			}

			insulin = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, candidates,
				PathEndMode.OnCell, TraverseParms.For(pawn));

			return insulin;
		}

		private static Thing FindInInventory(ThingOwner<Thing> container)
		{
			for (int i = 0; i < container.Count; i++)
			{
				Thing thing = container[i];
				if (thing.def.defName.Equals(EThingDef.Insulin.ToString()))
				{
					return thing;
				}
			}

			return null;
		}

		public static bool HasIrregularBloodSugar(Pawn pawn)
		{
			HediffSet set = pawn.health.hediffSet;
			if (set.HasHediff(TypeGetter.HediffDef(EHediffDef.Hypoglycaemia)))
			{
				return true;
			}
			if (set.HasHediff(TypeGetter.HediffDef(EHediffDef.Hyperglycaemia)))
			{
				if (set.HasHediff(TypeGetter.HediffDef(EHediffDef.InsulinHigh)))
				{
					return false;
				}
				if(set.HasHediff(TypeGetter.HediffDef(EHediffDef.AdvancedInsulinPump)))
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}
}
