using System;
using Verse;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts.Combat
{
	// Token: 0x02000021 RID: 33
	public class PendingCalloutEventRangedImpact : PendingCalloutEventDoublePawn
	{
		// Token: 0x0600004B RID: 75 RVA: 0x00003DCC File Offset: 0x00001FCC
		public PendingCalloutEventRangedImpact(Pawn _initiator, Pawn _recipient, Pawn _originalTarget, ThingDef _weaponDef, ThingDef _projectileDef, ThingDef _coverDef) : base(CalloutCategory.Combat, _initiator, _recipient, CalloutDefOf.CM_Callouts_RulePack_Ranged_Attack_Landed_OriginalTarget, CalloutDefOf.CM_Callouts_RulePack_Ranged_Attack_Received_OriginalTarget)
		{
			this.originalTarget = _originalTarget;
			this.weaponDef = _weaponDef;
			this.projectileDef = _projectileDef;
			this.coverDef = _coverDef;
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00003E2C File Offset: 0x0000202C
		protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
		{
			GrammarRequest result = base.PrepareGrammarRequest(rulePack);
			bool flag = this.recipient != null;
			if (flag)
			{
				result.Rules.AddRange(PlayLogEntryUtility.RulesForDamagedParts("PART", this.body, this.bodyPartsDamaged, this.bodyPartsDestroyed, result.Constants));
				bool flag2 = this.coverDef != null;
				if (flag2)
				{
					result.Rules.AddRange(GrammarUtility.RulesForDef("RECIPIENT_COVER", this.coverDef));
				}
			}
			return result;
		}

		// Token: 0x04000057 RID: 87
		public Pawn originalTarget = null;

		// Token: 0x04000058 RID: 88
		public ThingDef weaponDef = null;

		// Token: 0x04000059 RID: 89
		public ThingDef projectileDef = null;

		// Token: 0x0400005A RID: 90
		public ThingDef coverDef = null;
	}
}
