using System;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace Necro
{
	
	public class DamageWorker_AddInjuryNecronoid : DamageWorker
	{
		
		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing thing)
		{
			Pawn pawn = thing as Pawn;
			DamageWorker.DamageResult result;
			if (pawn == null)
			{
				dinfo.SetAmount(0f);
				result = base.Apply(dinfo, thing);
			}
			else
			{
				if (thing.def.race.IsMechanoid)
				{
					dinfo.SetAmount(0f);
				}
				result = this.ApplyToPawn(dinfo, pawn);
			}
			return result;
		}

		
		private DamageWorker.DamageResult ApplyToPawn(DamageInfo dinfo, Pawn pawn)
		{
			DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();
			DamageWorker.DamageResult result;
			if (dinfo.Amount <= 0f)
			{
				result = damageResult;
			}
			else if (!DebugSettings.enablePlayerDamage && pawn.Faction == Faction.OfPlayer)
			{
				result = damageResult;
			}
			else
			{
				Map mapHeld = pawn.MapHeld;
				bool spawnedOrAnyParentSpawned = pawn.SpawnedOrAnyParentSpawned;
				if (dinfo.AllowDamagePropagation && dinfo.Amount >= (float)dinfo.Def.minDamageToFragment)
				{
					int num = Rand.RangeInclusive(2, 4);
					for (int i = 0; i < num; i++)
					{
						DamageInfo dinfo2 = dinfo;
						dinfo2.SetAmount(dinfo.Amount / (float)num);
						this.ApplyDamageToPart(dinfo2, pawn, damageResult);
					}
				}
				else
				{
					this.ApplyDamageToPart(dinfo, pawn, damageResult);
					this.ApplySmallPawnDamagePropagation(dinfo, pawn, damageResult);
				}
				if (damageResult.wounded)
				{
					DamageWorker_AddInjuryNecronoid.PlayWoundedVoiceSound(dinfo, pawn);
					pawn.Drawer.Notify_DamageApplied(dinfo);
					EffecterDef damageEffecter = pawn.RaceProps.FleshType.damageEffecter;
					if (damageEffecter != null)
					{
						if (pawn.health.woundedEffecter != null && pawn.health.woundedEffecter.def != damageEffecter)
						{
							pawn.health.woundedEffecter.Cleanup();
						}
						pawn.health.woundedEffecter = damageEffecter.Spawn();
						pawn.health.woundedEffecter.Trigger(pawn, dinfo.Instigator ?? pawn, -1);
					}
					if (dinfo.Def.damageEffecter != null)
					{
						Effecter effecter = dinfo.Def.damageEffecter.Spawn();
						effecter.Trigger(pawn, pawn, -1);
						effecter.Cleanup();
					}
				}
				if (damageResult.headshot && pawn.Spawned)
				{
					MoteMaker.ThrowText(new Vector3((float)pawn.Position.x + 1f, (float)pawn.Position.y, (float)pawn.Position.z + 1f), pawn.Map, "Headshot".Translate(), Color.white, -1f);
					if (dinfo.Instigator != null)
					{
						Pawn pawn2 = dinfo.Instigator as Pawn;
						if (pawn2 != null)
						{
							pawn2.records.Increment(RecordDefOf.Headshots);
						}
					}
				}
				if ((damageResult.deflected || damageResult.diminished) && spawnedOrAnyParentSpawned)
				{
					EffecterDef effecterDef = damageResult.deflected ? ((damageResult.deflectedByMetalArmor && dinfo.Def.canUseDeflectMetalEffect) ? ((dinfo.Def != DamageDefOf.Bullet) ? EffecterDefOf.Deflect_Metal : EffecterDefOf.Deflect_Metal_Bullet) : ((dinfo.Def != DamageDefOf.Bullet) ? EffecterDefOf.Deflect_General : EffecterDefOf.Deflect_General_Bullet)) : ((!damageResult.diminishedByMetalArmor) ? EffecterDefOf.DamageDiminished_General : EffecterDefOf.DamageDiminished_Metal);
					if (pawn.health.deflectionEffecter == null || pawn.health.deflectionEffecter.def != effecterDef)
					{
						if (pawn.health.deflectionEffecter != null)
						{
							pawn.health.deflectionEffecter.Cleanup();
							pawn.health.deflectionEffecter = null;
						}
						pawn.health.deflectionEffecter = effecterDef.Spawn();
					}
					pawn.health.deflectionEffecter.Trigger(pawn, dinfo.Instigator ?? pawn, -1);
					if (damageResult.deflected)
					{
						pawn.Drawer.Notify_DamageDeflected(dinfo);
					}
				}
				if (!damageResult.deflected && spawnedOrAnyParentSpawned)
				{
					ImpactSoundUtility.PlayImpactSound(pawn, dinfo.Def.impactSoundType, mapHeld);
				}
				result = damageResult;
			}
			return result;
		}

		
		[Obsolete]
		private void CheckApplySpreadDamage(DamageInfo dinfo, Thing t)
		{
			if ((dinfo.Def != DamageDefOf.Flame || t.FlammableNow) && Rand.Chance(0.5f))
			{
				dinfo.SetAmount((float)Mathf.CeilToInt(dinfo.Amount * Rand.Range(0.35f, 0.7f)));
				t.TakeDamage(dinfo);
			}
		}

		
		private void ApplySmallPawnDamagePropagation(DamageInfo dinfo, Pawn pawn, DamageWorker.DamageResult result)
		{
			if (dinfo.AllowDamagePropagation && result.LastHitPart != null && dinfo.Def.harmsHealth && result.LastHitPart != pawn.RaceProps.body.corePart && result.LastHitPart.parent != null && pawn.health.hediffSet.GetPartHealth(result.LastHitPart.parent) > 0f && result.LastHitPart.parent.coverageAbs > 0f && dinfo.Amount >= 10f && pawn.HealthScale <= 0.5001f)
			{
				DamageInfo dinfo2 = dinfo;
				dinfo2.SetHitPart(result.LastHitPart.parent);
				this.ApplyDamageToPart(dinfo2, pawn, result);
			}
		}

		
		private void ApplyDamageToPart(DamageInfo dinfo, Pawn pawn, DamageWorker.DamageResult result)
		{
			BodyPartRecord exactPartFromDamageInfo = this.GetExactPartFromDamageInfo(dinfo, pawn);
			if (exactPartFromDamageInfo != null)
			{
				dinfo.SetHitPart(exactPartFromDamageInfo);
				float num = dinfo.Amount;
				bool flag = !dinfo.InstantPermanentInjury && !dinfo.IgnoreArmor;
				bool deflectedByMetalArmor = false;
				if (flag)
				{
					DamageDef def = dinfo.Def;
					bool diminishedByMetalArmor;
					num = ArmorUtility.GetPostArmorDamage(pawn, num, dinfo.ArmorPenetrationInt, dinfo.HitPart, ref def, out deflectedByMetalArmor, out diminishedByMetalArmor);
					dinfo.Def = def;
					if (num < dinfo.Amount)
					{
						result.diminished = true;
						result.diminishedByMetalArmor = diminishedByMetalArmor;
					}
				}
				if (dinfo.Def.ExternalViolenceFor(pawn))
				{
					num *= pawn.GetStatValue(StatDefOf.IncomingDamageFactor, true, -1);
				}
				if (num <= 0f)
				{
					result.AddPart(pawn, dinfo.HitPart);
					result.deflected = true;
					result.deflectedByMetalArmor = deflectedByMetalArmor;
					return;
				}
				if (DamageWorker_AddInjuryNecronoid.IsHeadshot(dinfo, pawn))
				{
					result.headshot = true;
				}
				if (!dinfo.InstantPermanentInjury || (HealthUtility.GetHediffDefFromDamage(dinfo.Def, pawn, dinfo.HitPart).CompPropsFor(typeof(HediffComp_GetsPermanent)) != null && dinfo.HitPart.def.permanentInjuryChanceFactor != 0f && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(dinfo.HitPart)))
				{
					if (!dinfo.AllowDamagePropagation)
					{
						this.FinalizeAndAddInjury(pawn, num, dinfo, result);
						return;
					}
					this.ApplySpecialEffectsToPart(pawn, num, dinfo, result);
				}
			}
		}

		
		protected virtual void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
		{
			totalDamage = this.ReduceDamageToPreserveOutsideParts(totalDamage, dinfo, pawn);
			this.FinalizeAndAddInjury(pawn, totalDamage, dinfo, result);
			this.CheckDuplicateDamageToOuterParts(dinfo, pawn, totalDamage, result);
		}

		
		protected float FinalizeAndAddInjury(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
		{
			float result2;
			if (pawn.health.hediffSet.PartIsMissing(dinfo.HitPart))
			{
				result2 = 0f;
			}
			else
			{
				HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(dinfo.Def, pawn, dinfo.HitPart);
				Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDefFromDamage, pawn, null);
				hediff_Injury.Part = dinfo.HitPart;
				hediff_Injury.sourceDef = dinfo.Weapon;
				hediff_Injury.sourceBodyPartGroup = dinfo.WeaponBodyPartGroup;
				hediff_Injury.sourceHediffDef = dinfo.WeaponLinkedHediff;
				hediff_Injury.Severity = totalDamage;
				if (dinfo.InstantPermanentInjury)
				{
					HediffComp_GetsPermanent hediffComp_GetsPermanent = hediff_Injury.TryGetComp<HediffComp_GetsPermanent>();
					if (hediffComp_GetsPermanent != null)
					{
						hediffComp_GetsPermanent.IsPermanent = true;
					}
					else
					{
						Log.Error(string.Concat(new object[]
						{
							"Tried to create instant permanent injury on Hediff without a GetsPermanent comp: ",
							hediffDefFromDamage,
							" on ",
							pawn
						}));
					}
				}
				result2 = this.FinalizeAndAddInjury(pawn, hediff_Injury, dinfo, result);
			}
			return result2;
		}

		
		protected float FinalizeAndAddInjury(Pawn pawn, Hediff_Injury injury, DamageInfo dinfo, DamageWorker.DamageResult result)
		{
			HediffComp_GetsPermanent hediffComp_GetsPermanent = injury.TryGetComp<HediffComp_GetsPermanent>();
			if (hediffComp_GetsPermanent != null)
			{
				hediffComp_GetsPermanent.PreFinalizeInjury();
			}
			float partHealth = pawn.health.hediffSet.GetPartHealth(injury.Part);
			if (pawn.IsColonist && !dinfo.IgnoreInstantKillProtection && dinfo.Def.ExternalViolenceFor(pawn) && !Rand.Chance(Find.Storyteller.difficulty.allowInstantKillChance))
			{
				float num = (injury.def.lethalSeverity > 0f) ? (injury.def.lethalSeverity * 1.1f) : 1f;
				float min = 1f;
				float max = Math.Min(injury.Severity, partHealth);
				int i = 0;
				while (i < 7 && pawn.health.WouldDieAfterAddingHediff(injury))
				{
					float num2 = Mathf.Clamp(partHealth - num, min, max);
					if (DebugViewSettings.logCauseOfDeath)
					{
						Log.Message(string.Format("CauseOfDeath: attempt to prevent death for {0} on {1} attempt:{2} severity:{3}->{4} part health:{5}", new object[]
						{
							pawn.Name,
							injury.Part.Label,
							i + 1,
							injury.Severity,
							num2,
							partHealth
						}));
					}
					injury.Severity = num2;
					num *= 2f;
					min = 0f;
					i++;
				}
			}
			pawn.health.AddHediff(injury, null, new DamageInfo?(dinfo), result);
			float num3 = Math.Min(injury.Severity, partHealth);
			result.totalDamageDealt += num3;
			result.wounded = true;
			result.AddPart(pawn, injury.Part);
			result.AddHediff(injury);
			return num3;
		}

		
		private void CheckDuplicateDamageToOuterParts(DamageInfo dinfo, Pawn pawn, float totalDamage, DamageWorker.DamageResult result)
		{
			if (dinfo.AllowDamagePropagation && dinfo.Def.harmAllLayersUntilOutside && dinfo.HitPart.depth == BodyPartDepth.Inside)
			{
				BodyPartRecord parent = dinfo.HitPart.parent;
				do
				{
					if (pawn.health.hediffSet.GetPartHealth(parent) != 0f && parent.coverageAbs > 0f)
					{
						Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(HealthUtility.GetHediffDefFromDamage(dinfo.Def, pawn, parent), pawn, null);
						hediff_Injury.Part = parent;
						hediff_Injury.sourceDef = dinfo.Weapon;
						hediff_Injury.sourceBodyPartGroup = dinfo.WeaponBodyPartGroup;
						hediff_Injury.Severity = totalDamage;
						if (hediff_Injury.Severity <= 0f)
						{
							hediff_Injury.Severity = 1f;
						}
						this.FinalizeAndAddInjury(pawn, hediff_Injury, dinfo, result);
					}
					if (parent.depth == BodyPartDepth.Outside)
					{
						break;
					}
					parent = parent.parent;
				}
				while (parent != null);
			}
		}

		
		private static bool IsHeadshot(DamageInfo dinfo, Pawn pawn)
		{
			return !dinfo.InstantPermanentInjury && dinfo.HitPart.groups.Contains(BodyPartGroupDefOf.FullHead) && dinfo.Def == DamageDefOf.Bullet;
		}

		
		private BodyPartRecord GetExactPartFromDamageInfo(DamageInfo dinfo, Pawn pawn)
		{
			BodyPartRecord result;
			if (dinfo.HitPart != null)
			{
				if (!pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).Any((BodyPartRecord x) => x == dinfo.HitPart))
				{
					result = null;
				}
				else
				{
					result = dinfo.HitPart;
				}
			}
			else
			{
				BodyPartRecord bodyPartRecord = this.ChooseHitPart(dinfo, pawn);
				if (bodyPartRecord == null)
				{
					Log.Warning("ChooseHitPart returned null (any part).");
				}
				result = bodyPartRecord;
			}
			return result;
		}

		
		protected virtual BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, dinfo.Depth, null);
		}

		
		private static void PlayWoundedVoiceSound(DamageInfo dinfo, Pawn pawn)
		{
			if (!pawn.Dead && !dinfo.InstantPermanentInjury && pawn.SpawnedOrAnyParentSpawned && dinfo.Def.ExternalViolenceFor(pawn))
			{
				LifeStageUtility.PlayNearestLifestageSound(pawn, (LifeStageAge ls) => ls.soundWounded, (GeneDef g) => g.soundWounded, null, 1f);
			}
		}

		
		protected float ReduceDamageToPreserveOutsideParts(float postArmorDamage, DamageInfo dinfo, Pawn pawn)
		{
			float result;
			if (!DamageWorker_AddInjuryNecronoid.ShouldReduceDamageToPreservePart(dinfo.HitPart))
			{
				result = postArmorDamage;
			}
			else
			{
				float partHealth = pawn.health.hediffSet.GetPartHealth(dinfo.HitPart);
				if (postArmorDamage < partHealth)
				{
					result = postArmorDamage;
				}
				else
				{
					float maxHealth = dinfo.HitPart.def.GetMaxHealth(pawn);
					float f = (postArmorDamage - partHealth) / maxHealth;
					if (Rand.Chance(this.def.overkillPctToDestroyPart.InverseLerpThroughRange(f)))
					{
						result = postArmorDamage;
					}
					else
					{
						postArmorDamage = (result = partHealth - 1f);
					}
				}
			}
			return result;
		}

		
		public static bool ShouldReduceDamageToPreservePart(BodyPartRecord bodyPart)
		{
			return bodyPart.depth == BodyPartDepth.Outside && !bodyPart.IsCorePart;
		}

		
		public DamageWorker_AddInjuryNecronoid()
		{
		}
	}
}
