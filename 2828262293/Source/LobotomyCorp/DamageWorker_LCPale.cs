using System;
using RimWorld;
using LobotomyCorp;

namespace Verse
{
	public class DamageWorker_LCPale : DamageWorker
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
				dinfo.SetAmount(dinfo.Amount * 1.0f); // Mechanoid Pale multiplier
			}
			else if (pawn.RaceProps.Insect)
			{
				dinfo.SetAmount(dinfo.Amount * 2.0f); // Insect Pale multiplier
			}
			else if (pawn.RaceProps.Animal)
			{
				dinfo.SetAmount(dinfo.Amount * 2.0f); // Animal Pale multiplier
			}
			else if (pawn.RaceProps.Dryad)
			{
				dinfo.SetAmount(dinfo.Amount * 1.8f); // Dryad Pale multiplier
			}
			else if (pawn.RaceProps.Humanlike)
			{
				dinfo.SetAmount(dinfo.Amount * 2.0f); // Humanlike Pale multiplier
			}
			Log.Message("Damage final: " + dinfo.Amount.ToString());
			if (((dinfo.Amount * (0.9f + (suitGrade / 10.0f))) / initialDamage) >= 1.19f) // If pawn is at least 1.2x weak to Pale damage
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

			float paleMult = 0.01f; // Arbitrary multiplier for all Pale damage to be tweaked for balance reasons
			dinfo.SetAmount(dinfo.Amount * paleMult);
			Hediff hediff = HediffMaker.MakeHediff(dinfo.Def.hediff, pawn, null);
			hediff.Severity = (dinfo.Amount);
			pawn.health.AddHediff(hediff); // Inflict Pale damage hediff

			if (dinfo.Def.damageEffecter != null)
			{
				Effecter effecter = dinfo.Def.damageEffecter.Spawn(); // Particle effect
				effecter.Trigger(pawn, pawn);
				effecter.Cleanup();
			}

			return new DamageWorker.DamageResult();
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