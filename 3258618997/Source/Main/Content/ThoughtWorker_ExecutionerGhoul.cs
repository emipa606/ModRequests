using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace Necro
{
	
	public class ThoughtWorker_ExecutionerGhoul : ThoughtWorker
	{
		
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			ThoughtWorker_ExecutionerGhoul.BloodChillingAuraLevel dreadfulAuraLevel = ThoughtWorker_ExecutionerGhoul.BloodChillingAuraLevel.None;
			if (p.Map != null)
			{
				List<Thing> list = p.Map.listerThings.ThingsOfDef(ThingDefOf.Necronoid_ExecutionerGhoul);
				list.SortBy((Thing m) => m.Position.DistanceToSquared(m.Position));
				if (list.Count > 0)
				{
					float num = list[0].Position.DistanceTo(p.Position);
					if (num <= 6.9f)
					{
						dreadfulAuraLevel = ThoughtWorker_ExecutionerGhoul.BloodChillingAuraLevel.Intense;
					}
					else if (num <= 15.9f)
					{
						dreadfulAuraLevel = ThoughtWorker_ExecutionerGhoul.BloodChillingAuraLevel.Strong;
					}
					else
					{
						dreadfulAuraLevel = ThoughtWorker_ExecutionerGhoul.BloodChillingAuraLevel.Distant;
					}
				}
			}
			ThoughtState result;
			switch (dreadfulAuraLevel)
			{
			case ThoughtWorker_ExecutionerGhoul.BloodChillingAuraLevel.None:
				result = false;
				break;
			case ThoughtWorker_ExecutionerGhoul.BloodChillingAuraLevel.Intense:
				result = ThoughtState.ActiveAtStage(0);
				break;
			case ThoughtWorker_ExecutionerGhoul.BloodChillingAuraLevel.Strong:
				result = ThoughtState.ActiveAtStage(1);
				break;
			case ThoughtWorker_ExecutionerGhoul.BloodChillingAuraLevel.Distant:
				result = ThoughtState.ActiveAtStage(2);
				break;
			default:
				throw new NotImplementedException();
			}
			return result;
		}

		
		public ThoughtWorker_ExecutionerGhoul()
		{
		}

		
		private const float IntenseDistance = 6.9f;

		
		private const float StrongDistance = 12.9f;

		
		private enum BloodChillingAuraLevel
		{
			
			None,
			
			Intense,
			
			Strong,
			
			Distant
		}
        
	}
}
