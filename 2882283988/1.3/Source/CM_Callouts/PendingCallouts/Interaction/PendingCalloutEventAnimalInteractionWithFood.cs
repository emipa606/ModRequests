using System;
using Verse;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts.Interaction
{
	// Token: 0x02000023 RID: 35
	public class PendingCalloutEventAnimalInteractionWithFood : PendingCalloutEventAnimalInteraction
	{
		// Token: 0x0600004F RID: 79 RVA: 0x00003F21 File Offset: 0x00002121
		public PendingCalloutEventAnimalInteractionWithFood(Pawn _initiator, Pawn _recipient, ThingDef _food, RulePackDef _initiatorRulePack, RulePackDef _recipientRulePack) : base(_initiator, _recipient, _initiatorRulePack, _recipientRulePack)
		{
			this.food = _food;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00003F40 File Offset: 0x00002140
		protected override GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
		{
			GrammarRequest result = base.PrepareGrammarRequest(rulePack);
			result.Rules.AddRange(GrammarUtility.RulesForDef("FOOD", this.food));
			return result;
		}

		// Token: 0x0400005B RID: 91
		public ThingDef food = null;
	}
}
