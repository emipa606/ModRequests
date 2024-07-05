using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorldRealFoW
{
	public class Verb_Look : Verb
	{

		protected override bool TryCastShot()
		{
			bool result;
			if ((this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map) || !(this.currentTarget.Thing is Pawn))
			{
				result = false;
			}
			else
			{
				ShootLine shootLine;
				bool flag2 = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine);
				if (this.verbProps.stopBurstWithoutLos && !flag2)
				{
					result = false;
				}
				else
				{
					result = true;
				}
			}
			return result;
		}
	}
}
