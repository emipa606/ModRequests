using RimWorld;
using Verse;
namespace VFECore.Abilities
{
    public class Verb_VoltaicMaybeCombo : Verb_MeleeAttackDamage
    {
		protected override bool TryCastShot()
		{
			Hediff comboHD = CasterPawn.health.hediffSet.GetFirstHediffOfDef(WP_DefOf.WP_VoltaicComboHD);
			if (comboHD!= null)
			{
				if (comboHD.Severity > 1)
				{
					HealthUtility.AdjustSeverity(this.CasterPawn, WP_DefOf.WP_VoltaicAttackSpeedHD, 0.5f);
				}
				else
				{
					HealthUtility.AdjustSeverity(this.CasterPawn, WP_DefOf.WP_VoltaicAttackSpeedHD, 2f);
				}
				//the hediff either doubles or halves melee cooldown and vanishes after one tick
				//therefore, it's only taken into account for the attack we do with base.TryCastShot
			}
			base.TryCastShot();
			//possible it will halve cooldown every time it tries to cast shot; try right clicking a target multiple times and see what happens
			//try PostCastShot instead
			//Stance_Cooldown stance_Cooldown = CasterPawn.stances.curStance as Stance_Cooldown;
			if (comboHD != null)
			{
				if ((this.currentTarget.Thing is Pawn comboTarget && !comboTarget.Dead) || !(this.currentTarget.Thing is Pawn))
				{
					//Log.Message("stance is cooldown for " + stance_Cooldown.ticksLeft);
					if (comboHD.Severity > 1)	//hediff has severity 5, so this is one of the first four hits; we halve melee cooldown
					{
						//this.CasterPawn.stances.SetStance(new Stance_Cooldown(stance_Cooldown.ticksLeft/2, this.currentTarget, this));
						//the above does not work because we are not in a cooldown stance.
						//Log.Message("tried halving CD, now stance is cooldown for " + stance_Cooldown.ticksLeft);
						FleckMaker.Static(this.currentTarget.Thing.DrawPos, this.currentTarget.Thing.Map, WP_DefOf.MoteComboSlash, 1f);
					}
					else						//last hit, we throw a different mote, double CD, and deal damage to things around target as well
					{
						foreach (var thing in GenRadial.RadialDistinctThingsAround(CasterPawn.Position, CasterPawn.Map, 1.9f, false))
						{
							if (thing is Pawn hitPawn && hitPawn != CasterPawn)
							{
								//Log.Message("bombed something: " + hitPawn.GetUniqueLoadID());
								hitPawn.TakeDamage(new DamageInfo(WP_DefOf.WP_WhiteHotBurn, 30, 2f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
							}
						}
						//this.CasterPawn.stances.SetStance(new Stance_Cooldown(stance_Cooldown.ticksLeft*2, this.currentTarget, this));
						//Log.Message("tried doubling CD, now stance is cooldown for " + stance_Cooldown.ticksLeft);
						FleckMaker.Static(this.currentTarget.Thing.DrawPos, this.currentTarget.Thing.Map, WP_DefOf.MoteComboBlast, 3f);
					}
				}
				HealthUtility.AdjustSeverity(this.CasterPawn, WP_DefOf.WP_VoltaicComboHD, -1f);
			}
			return true;
		}
    }
}