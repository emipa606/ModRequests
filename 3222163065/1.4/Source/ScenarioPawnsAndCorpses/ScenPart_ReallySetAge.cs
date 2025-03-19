using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
namespace ScenarioPawnsAndCorpses
{
	public class ScenPart_ReallySetAge : ScenPart_PawnModifier
	{
		// Extreme minimum age for a pawn (including outside of generation)
		private const int ExtremeRangeMin = 0;

		// Extreme maximum age for a pawn (including outside of generation)
		private const int ExtremeRangeMax = 999999;

		// Minimum age for a pawn
		private const int RangeMin = 0;

		// Vanilla minimum age for a pawn
		private static readonly int VanillaRangeMin;

		// Maximum age for a pawn
		private static readonly int RangeMax;

		// The range of ages can not have a maximum below this value
		private const int RangeMinMax = 0;

		// The min and max ages must have at least this must difference
		private const int RangeMinWidth = 0;

		// The allowed ages as per the scenario
		public IntRange AllowedAgeRange = new IntRange(ScenPart_ReallySetAge.ExtremeRangeMin, ScenPart_ReallySetAge.ExtremeRangeMax);

		/// <summary>
		/// Initialize the constructor; get the age max from ScenPart_PawnFilter_Age.RangeMax
		/// </summary>
		static ScenPart_ReallySetAge()
		{
			FieldInfo rangeMaxField = typeof(ScenPart_PawnFilter_Age).GetField("RangeMax", BindingFlags.NonPublic | BindingFlags.Static);
			ScenPart_ReallySetAge.RangeMax = (int)rangeMaxField.GetValue(null);

			FieldInfo rangeMinField = typeof(ScenPart_PawnFilter_Age).GetField("RangeMin", BindingFlags.NonPublic | BindingFlags.Static);
			ScenPart_ReallySetAge.VanillaRangeMin = (int)rangeMinField.GetValue(null);
		}

		/// <summary>
		/// Setup the UI for the part in the scenario edit interface
		/// </summary>
		/// <param name="listing">The list host for the scen part</param>
		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f + 31f);

			Widgets.IntRange(scenPartRect.TopPart(0.333f), listing.CurHeight.GetHashCode(), ref this.AllowedAgeRange, ScenPart_ReallySetAge.RangeMin, ScenPart_ReallySetAge.RangeMax, null, ScenPart_ReallySetAge.RangeMinWidth);

			this.DoPawnModifierEditInterface(scenPartRect.BottomPart(0.666f));
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref AllowedAgeRange, "AllowedAgeRange");
		}

		public override string Summary(Scenario scen)
		{
			return "ScenarioPawnsAndCorpses.ScenPart_ReallySetAge".Translate(
				this.context.ToStringHuman(),
				this.chance.ToStringPercent(),
				this.AllowedAgeRange.min,
				this.AllowedAgeRange.max);
		}

		public override void Randomize()
		{
			base.Randomize();

			this.chance = 1f;

			this.AllowedAgeRange = new IntRange(ScenPart_ReallySetAge.RangeMin, ScenPart_ReallySetAge.RangeMax);
			
			int first = Rand.Range(20, 60);
			int second = Rand.Range(20, 60);

			if (first < second)
            {
				this.AllowedAgeRange.min = first;
				this.AllowedAgeRange.max = second;
			}
			else
            {
				this.AllowedAgeRange.min = second;
				this.AllowedAgeRange.max = first;
			}

			
					
			MakeAllowedAgeRangeValid();
		}

		private void MakeAllowedAgeRangeValid()
		{
			if (this.AllowedAgeRange.max < ScenPart_ReallySetAge.RangeMinMax)
			{
				this.AllowedAgeRange.max = ScenPart_ReallySetAge.RangeMinMax;
			}
			if (this.AllowedAgeRange.max - this.AllowedAgeRange.min < ScenPart_ReallySetAge.RangeMinWidth)
			{
				this.AllowedAgeRange.min = this.AllowedAgeRange.max - ScenPart_ReallySetAge.RangeMinWidth;
			}
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ this.AllowedAgeRange.GetHashCode();
		}

		/// <summary>
		/// Whether the age range this scenpart is adding can coexist with other age ranges from other scenparts
		/// </summary>
		/// <param name="other">A scenpart to review</param>
		/// <returns>Whether this scenpart can coexist with the other</returns>
		public override bool CanCoexistWith(ScenPart other)
		{
			ScenPart_ReallySetAge scenPart_ReallySetAge = other as ScenPart_ReallySetAge;
			if (scenPart_ReallySetAge != null && context.OverlapsWith(scenPart_ReallySetAge.context))
			{
				return false;
			}

			ScenPart_PawnFilter_Age scenPart_PawnFilter_Age = other as ScenPart_PawnFilter_Age;
			if (scenPart_PawnFilter_Age != null && context.OverlapsWith(PawnGenerationContext.PlayerStarter))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Get the pawn context for the pawns this scenpart modifies
		/// </summary>
		/// <returns>the pawn context of this scenpart</returns>
		public PawnGenerationContext GetContext()
		{
			return this.context;
		}

		/// <summary>
		/// Handle the Notify_NewPawnAge notification if applicable
		/// </summary>
		/// <param name="pawn">The pawn being generated</param>
		/// <param name="request">The pawn generation request</param>
		public void Notify_NewPawnAge(Pawn pawn, ref PawnGenerationRequest request)
		{
			if (request.AllowedDevelopmentalStages.Newborn())
			{
				return;
			}

			if (!pawn.RaceProps.Humanlike)
            {
				return;
			}

			if (this.context.Includes(request.Context) && Rand.Chance(chance))
			{
				float age;

				if (request.FixedBiologicalAge.HasValue)
				{
					age = request.FixedBiologicalAge.Value;

					if (age < this.AllowedAgeRange.min)
					{
						age = this.AllowedAgeRange.min;
					}
					else if (age > this.AllowedAgeRange.max)
					{
						age = this.AllowedAgeRange.max;
					}
				}
				else
				{
					age = Rand.Range(this.AllowedAgeRange.min, this.AllowedAgeRange.max);
				}

				request.FixedBiologicalAge = age;

				if (request.FixedChronologicalAge.HasValue && age > request.FixedChronologicalAge)
                {
					request.FixedChronologicalAge = age;
				}

				request.AllowedDevelopmentalStages = LifeStageUtility.CalculateDevelopmentalStage(pawn, age);
				MethodInfo alwaysDownedLifeStagesMethod = typeof(PawnGenerationRequest).GetMethod("AlwaysDownedLifeStages", BindingFlags.NonPublic | BindingFlags.Instance);
				object result = alwaysDownedLifeStagesMethod.Invoke(request, new object[] { null });

				if ((bool)result)
				{
					request.AllowDowned = true;
					request.ValidatorPreGear = null;
					request.ValidatorPostGear = null;
					request.MustBeCapableOfViolence = false;
				}
			}
		}
	}
}