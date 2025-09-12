using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace Necro
{
	
	public class ThoughtWorker_HowlingSpider : ThoughtWorker
	{
		
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			ThoughtWorker_HowlingSpider.OminousAuraLevel dreadfulAuraLevel = ThoughtWorker_HowlingSpider.OminousAuraLevel.None;
			if (p.Map != null)
			{
				List<Thing> list = p.Map.listerThings.ThingsOfDef(ThingDefOf.Necronoid_HowlingSpider);
				list.SortBy((Thing m) => m.Position.DistanceToSquared(m.Position));
				if (list.Count > 0)
				{
					float num = list[0].Position.DistanceTo(p.Position);
					if (num <= 6.9f)
					{
						dreadfulAuraLevel = ThoughtWorker_HowlingSpider.OminousAuraLevel.Intense;
					}
					else if (num <= 15.9f)
					{
						dreadfulAuraLevel = ThoughtWorker_HowlingSpider.OminousAuraLevel.Strong;
					}
					else
					{
						dreadfulAuraLevel = ThoughtWorker_HowlingSpider.OminousAuraLevel.Distant;
					}
				}
			}
			ThoughtState result;
			switch (dreadfulAuraLevel)
			{
			case ThoughtWorker_HowlingSpider.OminousAuraLevel.None:
				result = false;
				break;
			case ThoughtWorker_HowlingSpider.OminousAuraLevel.Intense:
				result = ThoughtState.ActiveAtStage(0);
				break;
			case ThoughtWorker_HowlingSpider.OminousAuraLevel.Strong:
				result = ThoughtState.ActiveAtStage(1);
				break;
			case ThoughtWorker_HowlingSpider.OminousAuraLevel.Distant:
				result = ThoughtState.ActiveAtStage(2);
				break;
			default:
				throw new NotImplementedException();
			}
			return result;
		}

		
		public ThoughtWorker_HowlingSpider()
		{
		}

		
		private const float IntenseDistance = 6.9f;

		
		private const float StrongDistance = 12.9f;

		
		private enum OminousAuraLevel
		{
			
			None,
			
			Intense,
			
			Strong,
			
			Distant
		}
        
	}
}
