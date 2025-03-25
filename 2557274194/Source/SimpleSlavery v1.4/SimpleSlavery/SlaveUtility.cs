using RimWorld;
using System;
using Verse;

namespace SimpleSlaveryCollars
{
	public static class SlaveUtility
	{
		public static bool IsColonyMember(Pawn pawn)
		{
			if (pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony)
				return true;
			return false;
		}
		public static bool IsSlaveCollar(Apparel apparel)
		{
			if (apparel == null) { return false; }
			return apparel.def.defName.Contains("SlaveCollar");
		}

		public static bool HasSlaveCollar(Pawn pawn)
		{
			if (pawn.apparel == null)
				return false;
			foreach (Apparel item in pawn.apparel.WornApparel)
			{
				if (IsSlaveCollar(item))
					return true;
			}
			return false;
		}

		public static Apparel GetSlaveCollar(Pawn pawn)
		{
			return HasSlaveCollar(pawn) ? pawn.apparel.WornApparel.Find(IsSlaveCollar) : null;
		}
		public static void GiveSlaveCollar(Pawn pawn, Apparel collar = null)
		{
			if (pawn == null)
			{
				Log.Error("Tried to give a collar to a null pawn.");
				return;
			}
			Apparel newCollar = collar;
			pawn.apparel.Wear(newCollar, true);
			if (pawn.outfits == null)
				pawn.outfits = new Pawn_OutfitTracker();
			pawn.outfits.forcedHandler.SetForced(newCollar, true);
		}

		public static Hediff_Enslaved GetEnslavedHediff(Pawn pawn)
		{
			return pawn.health.hediffSet.GetFirstHediffOfDef(SS_HediffDefOf.Enslaved) as Hediff_Enslaved;
		}

		public static void TryInstantBreak(Pawn pawn, float chance, MentalStateDef breakDef)
		{
			if (pawn.Downed || pawn.jobs.curDriver.asleep || pawn.InMentalState) return;
			if (Rand.Chance(chance))
				pawn.mindState.mentalStateHandler.TryStartMentalState(breakDef, "ReasonArmedExplosiveCollar".Translate(pawn.Name.ToStringShort));
		}
		public static void TryInstantBreak(Pawn pawn, float chance)
		{
			if (pawn.InMentalState) return;
			TryInstantBreak(pawn, chance, MentalStateDefOf.Berserk);
		}

		public static void TryHeartAttack(Pawn pawn)
		{
			int age = pawn.ageTracker.AgeBiologicalYears;

			const float youngAge = 30f;

			float oldAge = pawn.RaceProps.lifeExpectancy;

			const float minChance = 0.0001f;

			const float maxChance = 0.01f;

			float chance = Math.Max(((Math.Min(Math.Max(age, youngAge), oldAge) - youngAge) / (oldAge - youngAge)) * maxChance, minChance);

			//Log.Message("Chance was : " + chance.ToStringSafe());

			BodyPartRecord heart = pawn.RaceProps.body.AllParts.Find(part => part.def == BodyPartDefOf.Heart);

			if (heart != null && Rand.Chance(chance))
			{
				pawn.health.AddHediff(HediffDef.Named("HeartAttack"), heart);
				string text = "LetterIncidentECHeartAttack".Translate(pawn.Name.ToString());
				Find.LetterStack.ReceiveLetter("LetterLabelECHeartAttack".Translate(), text, LetterDefOf.NegativeEvent, null);
			}
		}
		public static bool IsSteadfast(Pawn pawn)
		{
			if (pawn.story.traits.HasTrait(TraitDef.Named("Wimp")))
				return false;
			if (pawn.story.traits.HasTrait(TraitDefOf.Nerves))
			{
				if (pawn.story.traits.GetTrait(TraitDefOf.Nerves).Degree > 0)
					return true;
				else
					return false;
			}
			return false;
		}

		public static bool EverBeenSlave(Pawn pawn) => pawn.records.GetAsInt(SS_RecordDefOf.TimeAsSlave) > 0;
		public static float TimeAsSlave(Pawn pawn) => pawn.records.GetValue(SS_RecordDefOf.TimeAsSlave);
		public static float SlaveStage1 => 60000f * SimpleSlaveryCollarsSetting.Slavestage1Period;
		public static float SlaveStage2 => SlaveStage1 + (60000f * SimpleSlaveryCollarsSetting.Slavestage2Period);
		public static float SlaveStage3 => SlaveStage2 + (60000f * SimpleSlaveryCollarsSetting.Slavestage3Period);
		public static float SlaveStage4 => SlaveStage3 + (60000f * SimpleSlaveryCollarsSetting.Slavestage4Period);
	}
}

