using System;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts.Interaction
{
	// Token: 0x02000022 RID: 34
	public class PendingCalloutEventAnimalInteraction : PendingCalloutEventDoublePawn
	{
		// Token: 0x0600004D RID: 77 RVA: 0x00003EB0 File Offset: 0x000020B0
		public PendingCalloutEventAnimalInteraction(Pawn _initiator, Pawn _recipient, RulePackDef _initiatorRulePack, RulePackDef _recipientRulePack) : base(CalloutCategory.Animal, _initiator, _recipient, _initiatorRulePack, _recipientRulePack)
		{
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00003EC0 File Offset: 0x000020C0
		protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
		{
			GrammarRequest result = base.PrepareGrammarRequest(rulePack);
			bool flag = this.recipient.relations != null && this.recipient.relations.DirectRelationExists(PawnRelationDefOf.Bond, this.initiator);
			if (flag)
			{
				result.Constants.Add("BONDED", "true");
			}
			return result;
		}
	}
}
