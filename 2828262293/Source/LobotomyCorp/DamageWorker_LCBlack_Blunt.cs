using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse
{
	public class DamageWorker_LCBlack_Blunt : DamageWorker_LCBlack
	{
		protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside, null);
		}

		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
		{
			bool flag = Rand.Chance(this.def.bluntInnerHitChance);
			float num = flag ? this.def.bluntInnerHitDamageFractionToConvert.RandomInRange : 0f;
			float num2 = totalDamage * (1f - num);
			DamageInfo lastInfo = dinfo;
			for (; ; )
			{
				num2 -= base.FinalizeAndAddInjury(pawn, num2, lastInfo, result);
				if (!pawn.health.hediffSet.PartIsMissing(lastInfo.HitPart) || num2 <= 1f)
				{
					break;
				}
				BodyPartRecord parent = lastInfo.HitPart.parent;
				if (parent == null)
				{
					break;
				}
				lastInfo.SetHitPart(parent);
			}
			if (flag && !lastInfo.HitPart.def.IsSolid(lastInfo.HitPart, pawn.health.hediffSet.hediffs) && lastInfo.HitPart.depth == BodyPartDepth.Outside)
			{
				BodyPartRecord hitPart;
				if ((from x in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)
					 where x.parent == lastInfo.HitPart && x.def.IsSolid(x, pawn.health.hediffSet.hediffs) && x.depth == BodyPartDepth.Inside
					 select x).TryRandomElementByWeight((BodyPartRecord x) => x.coverageAbs, out hitPart))
				{
					DamageInfo lastInfo2 = lastInfo;
					lastInfo2.SetHitPart(hitPart);
					float totalDamage2 = totalDamage * num + totalDamage * this.def.bluntInnerHitDamageFractionToAdd.RandomInRange;
					base.FinalizeAndAddInjury(pawn, totalDamage2, lastInfo2, result);
				}
			}
			if (!pawn.Dead)
			{
				SimpleCurve simpleCurve = null;
				if (lastInfo.HitPart.parent == null)
				{
					simpleCurve = this.def.bluntStunChancePerDamagePctOfCorePartToBodyCurve;
				}
				else
				{
					foreach (BodyPartRecord lhs in pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource))
					{
						if (this.InSameBranch(lhs, lastInfo.HitPart))
						{
							simpleCurve = this.def.bluntStunChancePerDamagePctOfCorePartToHeadCurve;
							break;
						}
					}
				}
				if (simpleCurve != null)
				{
					float x2 = totalDamage / pawn.def.race.body.corePart.def.GetMaxHealth(pawn);
					if (Rand.Chance(simpleCurve.Evaluate(x2)))
					{
						DamageInfo dinfo2 = dinfo;
						dinfo2.Def = DamageDefOf.Stun;
						dinfo2.SetAmount((float)this.def.bluntStunDuration.SecondsToTicks() / 30f);
						pawn.TakeDamage(dinfo2);
					}
				}
			}
		}

		private bool InSameBranch(BodyPartRecord lhs, BodyPartRecord rhs)
		{
			while (lhs.parent != null)
			{
				if (lhs.parent.parent == null)
				{
					break;
				}
				lhs = lhs.parent;
			}
			while (rhs.parent != null && rhs.parent.parent != null)
			{
				rhs = rhs.parent;
			}
			return lhs == rhs;
		}
	}
}
