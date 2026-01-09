using RimWorld;
using System.Collections.Generic;
using Verse;

namespace KriilMod_CD
{
	public static class CombatTrainingTracker
	{
		public static Dictionary<string, SkillXpValues> PawnMeleeSkillValues = new Dictionary<string, SkillXpValues>();

		public static Dictionary<string, SkillXpValues> PawnShootingSkillValues = new Dictionary<string, SkillXpValues>();

		public static void TrackPawnMeleeSkill(Pawn pawn, SkillRecord skill)
		{
			int dayOfYear = GenLocalDate.DayOfYear(pawn);
			PawnMeleeSkillValues[pawn.ThingID] = new SkillXpValues(dayOfYear, skill.xpSinceMidnight, skill.xpSinceLastLevel);
		}

		public static void TrackPawnShootingSkill(Pawn pawn, SkillRecord skill)
		{
			int dayOfYear = GenLocalDate.DayOfYear(pawn);
			PawnShootingSkillValues[pawn.ThingID] = new SkillXpValues(dayOfYear, skill.xpSinceMidnight, skill.xpSinceLastLevel);
		}

		public static bool ShouldSkipCombatTraining(Pawn pawn)
		{
			ClearYesterdaysSkillValues(pawn);
			SkillRecord currentSkill = GetCurrentSkill(pawn);
			if (currentSkill.xpSinceMidnight > 4000f)
			{
				return true;
			}
			if (currentSkill.Level >= 20)
			{
				return true;
			}
			SkillXpValues lastSkillXpValues = GetLastSkillXpValues(pawn);
			if (lastSkillXpValues == null)
			{
				return false;
			}
			if (lastSkillXpValues.XpSinceMidnight >= 4000f)
			{
				return (double)currentSkill.xpSinceMidnight > 3000.0;
			}
			return false;
		}

		public static void ClearYesterdaysSkillValues(Pawn pawn)
		{
			int num = GenLocalDate.DayOfYear(pawn);
			if (PawnMeleeSkillValues.ContainsKey(pawn.ThingID) && PawnMeleeSkillValues[pawn.ThingID].DayOfYear != num)
			{
				PawnMeleeSkillValues.Remove(pawn.ThingID);
			}
			if (PawnShootingSkillValues.ContainsKey(pawn.ThingID) && PawnShootingSkillValues[pawn.ThingID].DayOfYear != num)
			{
				PawnShootingSkillValues.Remove(pawn.ThingID);
			}
		}

		public static SkillRecord GetCurrentSkill(Pawn pawn)
		{
			SkillRecord skill = pawn.skills.GetSkill(SkillDefOf.Shooting);
			SkillRecord skill2 = pawn.skills.GetSkill(SkillDefOf.Melee);
			ThingWithComps primary = pawn.equipment.Primary;
			if (primary == null)
			{
				return skill2;
			}
			if (primary.def.IsRangedWeapon)
			{
				return skill;
			}
			return skill2;
		}

		public static SkillXpValues GetLastSkillXpValues(Pawn pawn)
		{
			ThingWithComps primary = pawn.equipment.Primary;
			if (primary != null && primary.def.IsRangedWeapon && PawnShootingSkillValues.ContainsKey(pawn.ThingID))
			{
				return PawnShootingSkillValues[pawn.ThingID];
			}
			if (PawnMeleeSkillValues.ContainsKey(pawn.ThingID))
			{
				return PawnMeleeSkillValues[pawn.ThingID];
			}
			return null;
		}
	}
}
