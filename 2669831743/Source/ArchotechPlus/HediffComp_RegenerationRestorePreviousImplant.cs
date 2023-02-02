using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace ArchotechPlus
{
	public class HediffCompProperties_RegenerationRestorePreviousImplant : HediffCompProperties
    {
        public IntRange disappearsAfterTicks;
        public HediffCompProperties_RegenerationRestorePreviousImplant()
        {
            compClass = typeof (HediffComp_RegenerationRestorePreviousImplant);
        }
    }

	public class HediffComp_RegenerationRestorePreviousImplant : HediffComp
	{
		public int ticksToDisappear;

		public HediffCompProperties_RegenerationRestorePreviousImplant Props => (HediffCompProperties_RegenerationRestorePreviousImplant)props;
		public override bool CompShouldRemove
		{
			get
			{
				if (!base.CompShouldRemove)
				{
					return ticksToDisappear <= 0;
				}
				return true;
			}
		}

        public override void CompPostPostRemoved()
        {
			var hediff = Pawn.health.hediffSet.hediffs.FirstOrDefault(x => x is Hediff_Regeneration);
			var hediffComp = hediff as Hediff_Regeneration;
			if (hediffComp.previousImplants != null && hediffComp.previousImplants.TryGetValue(this.parent.Part, out var implant))
            {
				Pawn.health.AddHediff(implant, this.parent.Part);
            }
			base.CompPostPostRemoved();
        }
		public override void CompPostMake()
		{
			base.CompPostMake();
			ticksToDisappear = Props.disappearsAfterTicks.RandomInRange;
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			ticksToDisappear--;
		}

		public override void CompPostMerged(Hediff other)
		{
			base.CompPostMerged(other);
			HediffComp_Disappears hediffComp_Disappears = other.TryGetComp<HediffComp_Disappears>();
			if (hediffComp_Disappears != null && hediffComp_Disappears.ticksToDisappear > ticksToDisappear)
			{
				ticksToDisappear = hediffComp_Disappears.ticksToDisappear;
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref ticksToDisappear, "ticksToDisappear", 0);
		}

		public override string CompDebugString()
		{
			return "ticksToDisappear: " + ticksToDisappear;
		}
	}

}