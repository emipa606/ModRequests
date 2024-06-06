using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FF_Drugs
{
	public class HediffCompProperties_SelectiveRegeneration : HediffCompProperties
	{
		public List<BodyPartDef> exclusionList = new List<BodyPartDef>();
		public int minTimeToHeal;
		public int maxTimeToHeal;
		public bool excludeChronic = false;
		public bool excludeNonChronic = false;
		public bool excludePermanent = false;
		public bool excludeNonPermanent = false;
		public HediffCompProperties_SelectiveRegeneration()
		{
			compClass = typeof(HediffComp_SelectiveRegeneration);
		}
	}

	public class HediffComp_SelectiveRegeneration : HediffComp
	{
		private int ticksToHeal;
		public HediffCompProperties_SelectiveRegeneration Props => (HediffCompProperties_SelectiveRegeneration)props;
		public override void CompPostMake()
		{
			base.CompPostMake();
			ResetTicksToHeal();
		}
		private void ResetTicksToHeal()
		{
			ticksToHeal = Rand.Range(Props.minTimeToHeal, Props.maxTimeToHeal);
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			ticksToHeal--;
			if (ticksToHeal <= 0)
			{
				TryHealRandomDermalWound();
				ResetTicksToHeal();
			}
		}
		public void TryHealRandomDermalWound()
		{
			if (base.Pawn.health.hediffSet.hediffs.TryRandomElement(out Hediff result))
			{
				if (Props.excludeChronic && result.def.chronic)
					return;
				if (Props.excludeNonChronic && !result.def.chronic)
					return;
				if (Props.excludePermanent && result.IsPermanent())
					return;
				if (Props.excludeNonPermanent && !result.IsPermanent())
					return;
				foreach (BodyPartDef bodyPart in Props.exclusionList)
					if (result.Part.def == bodyPart)
						return;
				HealthUtility.Cure(result);
				if (PawnUtility.ShouldSendNotificationAbout(base.Pawn))
				{
					Messages.Message("MessagePermanentWoundHealed".Translate(parent.LabelCap, base.Pawn.LabelShort, result.Label, base.Pawn.Named("PAWN")), base.Pawn, MessageTypeDefOf.PositiveEvent);
				}
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0);
		}
	}
}
