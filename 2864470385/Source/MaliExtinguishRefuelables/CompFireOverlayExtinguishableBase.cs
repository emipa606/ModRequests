using RimWorld;
using UnityEngine;
using Verse;

namespace MaliExtinguishRefuelables
{
	public abstract class CompFireOverlayExtinguishableBase : ThingComp
	{
		protected int startedGrowingAtTick = -1;

		public CompProperties_FireOverlayExtinguishable Props => (CompProperties_FireOverlayExtinguishable)props;

		public float FireSize
		{
			get
			{
				if (startedGrowingAtTick < 0)
				{
					return Props.fireSize;
				}
				return Mathf.Lerp(Props.fireSize, Props.finalFireSize, (float)(GenTicks.TicksAbs - startedGrowingAtTick) / Props.fireGrowthDurationTicks);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref startedGrowingAtTick, "startedGrowingAtTick", -1);
		}
	}
}
