using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace BaseRobot
{
    // Token: 0x02000017 RID: 23
    public class Need_Battery : Need
	{
		// Token: 0x0600006A RID: 106 RVA: 0x000056E0 File Offset: 0x000038E0
		public Need_Battery(Pawn pawn) : base(pawn)
		{
            threshPercents = new List<float>
            {
                0.28f,
                0.14f
            };
        }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600006B RID: 107 RVA: 0x00005738 File Offset: 0x00003938
		public RestCategory CurCategory
		{
			get
			{
				if (CurLevel < 0.01)
				{
					return RestCategory.Exhausted;
				}
				if (CurLevel < 0.14)
				{
					return RestCategory.VeryTired;
				}
				if (CurLevel < 0.28)
				{
					return RestCategory.Tired;
				}
				return RestCategory.Rested;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600006C RID: 108 RVA: 0x0000578B File Offset: 0x0000398B
		public override int GUIChangeArrow
		{
			get
			{
				if (Resting)
				{
					return 1;
				}
				return -1;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600006D RID: 109 RVA: 0x0000579C File Offset: 0x0000399C
		private float RestFallFactor
		{
			get
			{
				var num = 1f;
				List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
				for (var i = 0; i < hediffs.Count; i++)
				{
					HediffStage curStage = hediffs[i].CurStage;
					if (curStage != null)
					{
						num *= curStage.restFallFactor;
					}
				}
				return num;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600006E RID: 110 RVA: 0x000057FC File Offset: 0x000039FC
		public float RestFallPerTick
		{
			get
			{
				switch (CurCategory)
				{
				case RestCategory.Rested:
					return BaseRestFallPerTick * RestFallFactor;
				case RestCategory.Tired:
					return BaseRestFallPerTick * RestFallFactor * 0.7f;
				case RestCategory.VeryTired:
					return BaseRestFallPerTick * RestFallFactor * 0.3f;
				case RestCategory.Exhausted:
					return BaseRestFallPerTick * RestFallFactor * 0.6f;
				default:
					return 999f;
				}
			}
		}

        // Token: 0x1700000C RID: 12
        // (get) Token: 0x0600006F RID: 111 RVA: 0x00005876 File Offset: 0x00003A76
        private bool Resting => Find.TickManager.TicksGame < lastRestTick + 2;

        // Token: 0x1700000D RID: 13
        // (get) Token: 0x06000070 RID: 112 RVA: 0x0000588C File Offset: 0x00003A8C
        public int TicksAtZero => ticksAtZero;

        // Token: 0x06000071 RID: 113 RVA: 0x00005894 File Offset: 0x00003A94
        public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksAtZero, "ticksAtZero", 0, false);
		}

		// Token: 0x06000072 RID: 114 RVA: 0x000058B0 File Offset: 0x00003AB0
		public override void NeedInterval()
		{
			if (!base.IsFrozen)
			{
				if (Resting)
				{
					CurLevel += 0.005714286f * lastRestEffectiveness;
				}
				else
				{
					CurLevel -= RestFallPerTick * 150f;
				}
			}
			if (CurLevel < 0.0001)
			{
				ticksAtZero += 150;
			}
			else
			{
				ticksAtZero = 0;
			}
			if (ticksAtZero > 1000 && pawn.Spawned)
			{
				float mtb;
				if (ticksAtZero < 15000)
				{
					mtb = BaseInvoluntarySleepMTBDays;
				}
				else if (ticksAtZero < 30000)
				{
					mtb = 0.125f;
				}
				else if (ticksAtZero < 45000)
				{
					mtb = 0.08333334f;
				}
				else
				{
					mtb = 0.0625f;
				}
				if (Rand.MTBEventOccurs(mtb, 60000f, 150f))
				{
					pawn.jobs.StartJob(new Job(JobDefOf.LayDown, pawn.Position), JobCondition.InterruptForced, null, false, true, null, null, false);
					if (PawnUtility.ShouldSendNotificationAbout(pawn))
					{
						Messages.Message("MessageInvoluntarySleep".Translate(new object[]
						{
							pawn.LabelShort
						}), pawn, MessageTypeDefOf.NegativeHealthEvent, true);
					}
				}
			}
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00005A46 File Offset: 0x00003C46
		public override void SetInitialLevel()
		{
			CurLevel = Rand.Range(0.9f, 1f);
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00005A5D File Offset: 0x00003C5D
		public void TickResting(float restEffectiveness)
		{
			lastRestTick = Find.TickManager.TicksGame;
			lastRestEffectiveness = restEffectiveness;
		}

		// Token: 0x0400003F RID: 63
		private const float FullSleepHours = 10.5f;

		// Token: 0x04000040 RID: 64
		private const float BaseInvoluntarySleepMTBDays = 0.25f;

		// Token: 0x04000041 RID: 65
		public const float CanWakeThreshold = 0.2f;

		// Token: 0x04000042 RID: 66
		public const float DefaultNaturalWakeThreshold = 1f;

		// Token: 0x04000043 RID: 67
		public const float DefaultFallAsleepMaxLevel = 0.75f;

		// Token: 0x04000044 RID: 68
		public const float ThreshVeryTired = 0.14f;

		// Token: 0x04000045 RID: 69
		public const float ThreshTired = 0.28f;

		// Token: 0x04000046 RID: 70
		private const float BaseRestFallPerTick = 1.583333E-05f;

		// Token: 0x04000047 RID: 71
		public const float BaseRestGainPerTick = 3.809524E-05f;

		// Token: 0x04000048 RID: 72
		private int ticksAtZero;

		// Token: 0x04000049 RID: 73
		private float lastRestEffectiveness = 1f;

		// Token: 0x0400004A RID: 74
		private int lastRestTick = -999;
	}
}
