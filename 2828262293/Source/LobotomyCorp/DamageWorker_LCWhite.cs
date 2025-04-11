using System;
using RimWorld;
using LobotomyCorp;

namespace Verse
{
	public class DamageWorker_LCWhite : DamageWorker
	{

		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			
			Pawn pawn = victim as Pawn;
			if (pawn == null || pawn.Dead)
			{
				//Log.Message("Pawn Null or Dead");
				return new DamageWorker.DamageResult();
			}

			dinfo.SetIgnoreArmor(true);

			float initialDamage = dinfo.Amount;
			//Log.Message("Initial damage amount: " + dinfo.Amount.ToString());

			bool hasEGOSuit = false;
			int suitGrade = 1;

			if (pawn.apparel != null)
            {
				foreach (ThingWithComps thingWComps in pawn.apparel.WornApparel)
				{
					Comp_EGOSuitBase EGOSuitComp = thingWComps.TryGetComp<Comp_EGOSuitBase>();
					if (EGOSuitComp != null)
					{
						hasEGOSuit = true;
						EGOSuitComp.ApplyEGOResistance(ref dinfo);
						suitGrade = EGOSuitComp.grade;
						break;
					}
				}
			}

			if (hasEGOSuit)
            {
				//Log.Message("Has EGO suit");
			}
			else if (pawn.RaceProps.IsMechanoid)
            {
				dinfo.SetAmount(dinfo.Amount * 0.5f); // Mechanoid White multiplier
			}
			else if (pawn.RaceProps.Insect)
            {
				dinfo.SetAmount(dinfo.Amount * 0.8f); // Insect White multiplier
			}
			else if (pawn.RaceProps.Animal)
			{
				dinfo.SetAmount(dinfo.Amount * 1.5f); // Animal White multiplier
			}
			else if (pawn.RaceProps.Dryad)
			{
				dinfo.SetAmount(dinfo.Amount * 0.6f); // Dryad White multiplier
			}
			//Log.Message("Damage final: " + dinfo.Amount.ToString());

			if (((dinfo.Amount * (0.9f + (suitGrade / 10.0f))) / initialDamage) >= 1.19f) // If pawn is at least 1.2x weak to White damage
			{
				ImpactSoundUtility.PlayImpactSound(pawn, dinfo.Def.impactSoundType, pawn.MapHeld); // Play crit sound

				/*DamageInfo dinfo2 = new DamageInfo(dinfo);
				dinfo2.Def = DamageDefOf.Stun;
				dinfo2.SetAmount(1f);
				pawn.TakeDamage(dinfo2); // Stun pawn*/

				if (pawn != null && pawn.stances != null)
				{
					pawn.stances.stagger.StaggerFor(95);
				}
			}

			PlayWoundedVoiceSound(dinfo, pawn);
			pawn.Drawer.Notify_DamageApplied(dinfo);
			EffecterDef damageEffecter = pawn.RaceProps.FleshType.damageEffecter;
			if (damageEffecter != null)
			{
				if (pawn.health.woundedEffecter != null && pawn.health.woundedEffecter.def != damageEffecter)
				{
					pawn.health.woundedEffecter.Cleanup();
				}
				pawn.health.woundedEffecter = damageEffecter.Spawn();
				pawn.health.woundedEffecter.Trigger(pawn, dinfo.Instigator ?? pawn);
			}

			if (pawn.RaceProps.Humanlike)
            {
				float multFromBreakThreshold = (pawn.mindState.mentalBreaker.BreakThresholdMinor - 0.35f + 1); // Pawns with higher or lower than default mental break threshold will take higher or lower % white damage respectively
				dinfo.SetAmount(dinfo.Amount * multFromBreakThreshold);
			}

			if (dinfo.Def.damageEffecter != null)
			{
				Effecter effecter = dinfo.Def.damageEffecter.Spawn(); // Particle effect
				effecter.Trigger(pawn, pawn);
				effecter.Cleanup();
			}

			if (pawn.MentalState != null && pawn.MentalState.def == MentalStateDefOf.Berserk)
			{
				if (Rand.Chance(dinfo.Amount * 0.0075f))
                {
					Hediff hediffShock = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn, null);
					hediffShock.Severity = 0.1f;
					BodyPartRecord part = null;
					pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource).TryRandomElement(out part);
					pawn.health.AddHediff(hediffShock, part, null, null);
				}
			}
			applyWhitePostResist(dinfo, pawn);

			

			return new DamageWorker.DamageResult();
		}

		public static void applyWhitePostResist(DamageInfo dinfo, Thing victim)
		{
			Pawn pawn = pawn = victim as Pawn;

			if (pawn == null || pawn.Dead)
			{
				//Log.Message("Pawn Null or Dead");
				return;
			}

			dinfo.Def = DefDatabase<DamageDef>.AllDefsListForReading.Find(d => d.defName == "WhiteDmg");

			

			float whiteMult = 0.0075f; // Arbitrary multiplier for all White damage to be tweaked for balance reasons
			dinfo.SetAmount(dinfo.Amount * whiteMult);
			Hediff hediff = HediffMaker.MakeHediff(dinfo.Def.hediff, pawn, null);
			hediff.Severity = (dinfo.Amount);
			pawn.health.AddHediff(hediff); // Inflict White damage hediff

			if (pawn.needs != null && pawn.needs.mood != null)
			{
				pawn.needs.mood.CurLevel -= (dinfo.Amount); // Instant mood reduction
			}

			if (pawn.RaceProps.Humanlike && pawn.mindState.mentalBreaker.BreakExtremeIsImminent)
			{
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk); // If mood is under extreme break threshold, make berserk
			}

			if (!pawn.RaceProps.Humanlike && (pawn.health.hediffSet.GetFirstHediffOfDef(dinfo.Def.hediff).Severity >= 1.0f))
			{
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk); // If non-humanlike pawn has White Damage severity of 1.0, try to make berserk
			}
		}

		private void PlayWoundedVoiceSound(DamageInfo dinfo, Pawn pawn)
		{
			if (pawn.Dead)
			{
				return;
			}
			if (dinfo.InstantPermanentInjury)
			{
				return;
			}
			if (!pawn.SpawnedOrAnyParentSpawned)
			{
				return;
			}
			if (dinfo.Def.ExternalViolenceFor(pawn))
			{
				LifeStageUtility.PlayNearestLifestageSound(pawn, (LifeStageAge ls) => ls.soundWounded, (GeneDef g) => g.soundWounded, 1f);
			}
		}

	}
}