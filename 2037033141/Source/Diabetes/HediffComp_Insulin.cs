using System;
using UnityEngine;
using Verse;

namespace Diabetes
{
	public class HediffComp_Insulin : HediffComp_SeverityPerDay
	{
		public HediffCompProperties_Insulin Props
		{
			get
			{
				return this.props as HediffCompProperties_Insulin;
			}
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			Hediff insulinHigh = Pawn.health.hediffSet.GetFirstHediffOfDef(TypeGetter.HediffDef(EHediffDef.InsulinHigh));
			base.CompPostTick(ref severityAdjustment);
			severityAdjustment = Math.Max(severityAdjustment, -insulinHigh.Severity);
			if (severityAdjustment == 0f)
			{
				return;
			}
			Hediff hediff = this.Pawn.health.hediffSet.GetFirstHediffOfDef(TypeGetter.HediffDef(EHediffDef.Hyperglycaemia));
			if (hediff == null)
			{
				hediff = this.Pawn.health.hediffSet.GetFirstHediffOfDef(TypeGetter.HediffDef(EHediffDef.Hypoglycaemia));
				if (hediff == null)
				{
					hediff = HediffMaker.MakeHediff(TypeGetter.HediffDef(EHediffDef.Hypoglycaemia), this.Pawn);
					this.Pawn.health.AddHediff(hediff);
				}
				hediff.Severity += Mathf.Abs(severityAdjustment);
			}
			else
			{
				if (!Pawn.health.hediffSet.HasHediff(TypeGetter.HediffDef(EHediffDef.Diabetes)))
				{
					return;
				}
				if (hediff.Severity >= Mathf.Abs(severityAdjustment))
				{
					hediff.Severity += severityAdjustment;
				}
				else
				{
					Hediff newHediff = HediffMaker.MakeHediff(TypeGetter.HediffDef(EHediffDef.Hypoglycaemia), this.Pawn);
					newHediff.Severity = Mathf.Abs(severityAdjustment) - hediff.Severity;
					hediff.Severity = 0f;
					this.Pawn.health.AddHediff(newHediff);
				}
			}
		}
	}
}
