using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RadWorld
{
	public class IngestionOutcomeDoer_AdjustHediff : IngestionOutcomeDoer
	{
		public HediffDef hediffDef;

		public float severity = -1f;

		public ChemicalDef toleranceChemical;

		private bool divideByBodySize;
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			if (pawn.def.GetStatValueAbstract(RW_DefOf.RW_RadiationResistance) <= 0 && (!pawn.story?.traits?.HasTrait(RW_DefOf.RW_MutantBlood) ?? false))
            {
				float effect = severity;
				if (divideByBodySize)
				{
					effect /= pawn.BodySize;
				}
				AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref effect);
				for (var i = 0; i < ingested.stackCount; i++)
				{
					HealthUtility.AdjustSeverity(pawn, hediffDef, effect);
				}
			}

		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			if (parentDef.IsDrug && chance >= 1f)
			{
				foreach (StatDrawEntry item in hediffDef.SpecialDisplayStats(StatRequest.ForEmpty()))
				{
					yield return item;
				}
			}
		}
	}
}