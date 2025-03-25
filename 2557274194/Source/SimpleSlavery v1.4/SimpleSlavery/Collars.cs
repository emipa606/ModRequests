using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SimpleSlaveryCollars
{
	public abstract class SlaveApparel : Apparel
	{
		public abstract IEnumerable<Gizmo> SlaveGizmos();
	}

	[StaticConstructorOnStartup]
	public class SlaveCollar_Explosive : SlaveApparel
	{
		public bool armed = false;
		public int arm_cooldown = 0;

		public override IEnumerable<Gizmo> SlaveGizmos()
		{
			// 1. Arm the collar
			var armCollar = new Command_Toggle();
			Func<bool> isArmed = () => armed;
			armCollar.isActive = isArmed;
			armCollar.defaultLabel = "Label_CollarExplosive_Arm".Translate();
			armCollar.defaultDesc = "Desc_CollarExplosive_Arm".Translate();
			armCollar.toggleAction = delegate
			{
				armed = !armed;
				if (armed)
				{
					if (arm_cooldown == 0)
					{
						// Doesn't matter if pawn is slave or not when armed
						SlaveUtility.TryInstantBreak(Wearer, Rand.Range(0.25f, 0.33f));
						arm_cooldown = 2500;
					}
				}
			};
			armCollar.activateSound = SoundDefOf.Click;
			armCollar.icon = ContentFinder<Texture2D>.Get("UI/Commands/ArmCollar", true);
			yield return armCollar;

			// 2. Detonate the collar
			if (armed)
			{
				var detonate = new Command_Action();
				detonate.defaultLabel = "Label_CollarExplosive_Detonate".Translate();
				detonate.defaultDesc = "Desc_CollarExplosive_Detonate".Translate();
				detonate.action = delegate
				{
					GoBoom();
				};
				detonate.activateSound = SoundDefOf.Click;
				detonate.icon = ContentFinder<Texture2D>.Get("UI/Commands/DetonateCollar", true);
				yield return detonate;
			}
		}

		public void GoBoom()
		{
			GenExplosion.DoExplosion(Wearer.Position, Wearer.Map, 1.5f, DamageDefOf.Bomb, this, 50);
			var destroyNeck = new DamageInfo(DamageDefOf.Bomb, 100f, 100f, -1f, this, Wearer.RaceProps.body.AllParts.Find(part => part.def == BodyPartDefOf.Neck));
			Wearer.TakeDamage(destroyNeck);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref armed, "armed", false);
			Scribe_Values.Look<int>(ref arm_cooldown, "arm_cooldown", 0);
		}

		public override void Tick()
		{
			base.Tick();
			if (arm_cooldown > 0)
				arm_cooldown -= 1;
		}
	}

	public class SlaveCollar_Electric : SlaveApparel
	{
		public bool armed = false;
		public int zap_cooldown = 0;
		public const int zap_period = 50;

		public override IEnumerable<Gizmo> SlaveGizmos()
		{
			// 1. Arm the collar
			var armCollar = new Command_Toggle();
			Func<bool> isArmed = () => armed;
			armCollar.isActive = isArmed;
			armCollar.defaultLabel = "Label_CollarElectric_Arm".Translate();
			armCollar.defaultDesc = "Desc_CollarElectric_Arm".Translate();
			armCollar.toggleAction = delegate
			{
				armed = !armed;
			};
			armCollar.activateSound = SoundDefOf.Click;
			armCollar.icon = ContentFinder<Texture2D>.Get("UI/Commands/DetonateCollar", true);
			yield return armCollar;
		}
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref armed, "armed", false);
			Scribe_Values.Look<int>(ref zap_cooldown, "zap_cooldown", 0);
		}

		public override void Tick()
		{
			base.Tick();
			if (armed)
			{
				if (zap_cooldown == 0) zap_cooldown = zap_period;
				if (zap_cooldown == zap_period) Zap();
			}
			if (zap_cooldown > 0) zap_cooldown -= 1;
		}

		public void Zap()
		{
			var zap = new DamageInfo(DamageDefOf.Burn, 1f, 100f, -1f, this, Wearer.RaceProps.body.AllParts.Find(part => part.def == BodyPartDefOf.Neck));
			var zap2 = new DamageInfo(DamageDefOf.Stun, 1f, 100f, -1f, this, Wearer.RaceProps.body.AllParts.Find(part => part.def == BodyPartDefOf.Neck));
			if (Wearer.Downed || !Wearer.Spawned)
			{
				armed = false;
				return;
			}
			SoundInfo info = SoundInfo.InMap(new TargetInfo(Wearer.PositionHeld, Wearer.MapHeld));
			SoundDefOf.Power_OffSmall.PlayOneShot(info);
			Wearer.TakeDamage(zap);
			Wearer.TakeDamage(zap2);
			Wearer.health.AddHediff(SS_HediffDefOf.Electrocuted);
			SlaveUtility.TryHeartAttack(Wearer);
		}
	}


	public class SlaveCollar_Crypto : SlaveApparel
	{
		public bool armed = false;

		public override IEnumerable<Gizmo> SlaveGizmos()
		{
			// 1. Arm the collar
			var armCollar = new Command_Toggle();
			Func<bool> isArmed = () => armed;
			armCollar.isActive = isArmed;
			armCollar.defaultLabel = "Label_CollarCrypto_Arm".Translate();
			armCollar.defaultDesc = "Desc_CollarCrypto_Arm".Translate();
			armCollar.toggleAction = delegate
			{
				armed = !armed;
				if (!armed)
					RevertMentalState();
			};
			armCollar.activateSound = SoundDefOf.Click;
			armCollar.icon = ContentFinder<Texture2D>.Get("UI/Commands/DetonateCollar", true);
			yield return armCollar;

		}

		public void RevertMentalState()
		{
			Hediff cryptoStasis = Wearer.health.hediffSet.GetFirstHediffOfDef(SS_HediffDefOf.Crypto_Stasis);
			var memory = ((Hediff_CryptoStasis)cryptoStasis).revertMentalStateDef;
			if (memory != null)
			{
				Wearer.mindState.mentalStateHandler.TryStartMentalState(memory, null, true, false, null, true);
				Wearer.health.RemoveHediff(cryptoStasis);
				if (Rand.Value > 0.66f)
				{
					Wearer.health.AddHediff(HediffDefOf.CryptosleepSickness);
				}
			}
			else
			{
				Wearer.health.RemoveHediff(cryptoStasis);
				Wearer.mindState.mentalStateHandler.Reset();
				if (Rand.Value > 0.66f)
				{
					Wearer.health.AddHediff(HediffDefOf.CryptosleepSickness);
				}
			}
		}

		public void CryptoStasis()
		{
			Hediff_CryptoStasis revertMentalState = null;
			if (!Wearer.health.hediffSet.HasHediff(SS_HediffDefOf.Crypto_Stasis))
			{
				Wearer.health.AddHediff(SS_HediffDefOf.Crypto_Stasis);
				revertMentalState = Wearer.health.hediffSet.GetFirstHediffOfDef(SS_HediffDefOf.Crypto_Stasis) as Hediff_CryptoStasis;
				revertMentalState.SaveMemory();
			}
			if (Wearer.InBed())
			{
				armed = false;
				RevertMentalState();
				return;
			}
			if (Wearer.mindState.mentalStateHandler.CurStateDef != SS_MentalStateDefOf.CryptoStasis)
				Wearer.mindState.mentalStateHandler.TryStartMentalState(SS_MentalStateDefOf.CryptoStasis, null, true, false, null, true);
			if (Rand.Value < 0.33f)
			{
				FleckMaker.ThrowTornadoDustPuff(Wearer.TrueCenter() + Vector3Utility.RandomHorizontalOffset(0.5f), Wearer.Map, Rand.Range(0.25f, 1f), new Color(0.65f, 0.9f, 0.93f));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref armed, "armed", false);
		}

		public override void Tick()
		{
			base.Tick();
			if (armed)
			{
				CryptoStasis();
			}
		}
	}
}
