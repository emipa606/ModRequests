using System;
using Verse;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts.Combat
{
	// Token: 0x0200001E RID: 30
	public class PendingCalloutEventMeleeAttempt : PendingCalloutEventDoublePawn
	{
		// Token: 0x06000045 RID: 69 RVA: 0x00003C57 File Offset: 0x00001E57
		public PendingCalloutEventMeleeAttempt(Pawn _initiator, Pawn _recipient, Verb _verb) : base(CalloutCategory.Combat, _initiator, _recipient, CalloutDefOf.CM_Callouts_RulePack_Melee_Attack, null)
		{
			this.verb = _verb;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00003C78 File Offset: 0x00001E78
		protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
		{
			GrammarRequest result = base.PrepareGrammarRequest(rulePack);
			bool flag = this.verb != null && this.verb.maneuver != null && this.verb.maneuver.combatLogRulesHit != null;
			if (flag)
			{
				result.Includes.Add(this.verb.maneuver.combatLogRulesHit);
			}
			return result;
		}

		// Token: 0x04000055 RID: 85
		public Verb verb = null;
	}
}
