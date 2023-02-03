using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace StunWeapons
{
	public class Projectile_StunBulletPowerful : Bullet
	{
		#region Properties
		public ThingDef_StunBullet Def
		{
			get
			{
				//Case sensitive! If you use Def it will return Def, which is this getter. This will cause a never ending cycle and a stack overflow.
				return this.def as ThingDef_StunBullet;
			}
		}
		#endregion

		#region Overrides
		protected override void Impact(Thing hitThing)

		{
			base.Impact(hitThing);
			if (Def != null && hitThing != null && hitThing is Pawn hitPawn)
			{
				float rand = Rand.Value;
				if (rand <= Def.addHediffChance)
				{
					MoteMaker.ThrowText(hitThing.PositionHeld.ToVector3(), hitThing.MapHeld, "StunBullet_SuccessMote".Translate(Def.addHediffChance.ToString()), 6f);
					Hediff stunOnPawn = hitPawn.health?.hediffSet?.GetFirstHediffOfDef(Def.hediffToAdd);
					var randomSeverity = Rand.Range(0.15f, 0.4f);
					var SeveritytoAdd = randomSeverity;
					if (hitPawn.BodySize<=0.51f)
					{
					SeveritytoAdd = SeveritytoAdd*2;
					}
					else if (hitPawn.BodySize >= 3.0f)
					{
						SeveritytoAdd = SeveritytoAdd/3;
					}
					else
                    {
					SeveritytoAdd = SeveritytoAdd / hitPawn.BodySize;
					}
					if (stunOnPawn != null)
					{
						stunOnPawn.Severity += SeveritytoAdd;
					}
					else
					{
						Hediff hediff = HediffMaker.MakeHediff(Def.hediffToAdd, hitPawn);
						hediff.Severity = SeveritytoAdd;
						hitPawn.health.AddHediff(hediff);
					}
				}
				else
				{
					MoteMaker.ThrowText(hitThing.PositionHeld.ToVector3(), hitThing.MapHeld, "StunBullet_FailureMote".Translate((1 - Def.addHediffChance).ToString()), 6f);
				}
			}
		}
		#endregion Overrides
	}
}
