using System;
using Verse;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts
{
	// Token: 0x0200001A RID: 26
	public class PendingCalloutEventDoublePawn : PendingCalloutEvent
	{
		// Token: 0x06000038 RID: 56 RVA: 0x00003790 File Offset: 0x00001990
		public PendingCalloutEventDoublePawn(CalloutCategory _category, Pawn _initiator, Pawn _recipient, RulePackDef _initiatorRulePack, RulePackDef _recipientRulePack) : base(_category)
		{
			this.initiator = _initiator;
			this.recipient = _recipient;
			this.initiatorRulePack = _initiatorRulePack;
			this.recipientRulePack = _recipientRulePack;
		}

		// Token: 0x06000039 RID: 57 RVA: 0x000037E0 File Offset: 0x000019E0
		public override void AttemptCallout()
		{
			bool flag = !CalloutMod.settings.CalloutCategoryEnabled(this.category);
			if (!flag)
			{
				base.AttemptCallout();
				bool flag2 = Scribe.mode > LoadSaveMode.Inactive;
				if (!flag2)
				{
					bool flag3 = this.initiator == null;
					if (flag3)
					{
						Logger.WarningFormat(this, "Initiator null", Array.Empty<object>());
					}
					else
					{
						bool flag4 = this.recipient == null;
						if (flag4)
						{
							Logger.WarningFormat(this, "Recipient null", Array.Empty<object>());
						}
						else
						{
							CalloutTracker component = Current.Game.World.GetComponent<CalloutTracker>();
							bool flag5 = component != null;
							if (flag5)
							{
								bool flag6 = this.initiatorRulePack != null;
								bool flag7 = this.recipientRulePack != null;
								bool flag8 = flag6 && Prefs.DevMode && CalloutMod.settings.forceInitiatorCallouts;
								bool flag9 = flag7 && Prefs.DevMode && CalloutMod.settings.forceRecipientCallouts;
								bool flag10 = flag6 && (flag8 || ((!flag7 || Rand.Bool) && component.CheckCalloutChance(this.category, this.initiatorRulePack, "rule") && CalloutUtility.CanCalloutNow(this.initiator) && !flag9));
								bool flag11 = flag7 && (flag9 || ((!flag6 || Rand.Bool) && component.CheckCalloutChance(this.category, this.recipientRulePack, "rule") && CalloutUtility.CanCalloutNow(this.initiator)));
								bool flag12 = flag10;
								if (flag12)
								{
									this.DoInitiatorCallout(component);
								}
								else
								{
									bool flag13 = flag11;
									if (flag13)
									{
										this.DoRecipientCallout(component);
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00003984 File Offset: 0x00001B84
		private void DoInitiatorCallout(CalloutTracker calloutTracker)
		{
			GrammarRequest grammarRequest = this.PrepareGrammarRequest(this.initiatorRulePack);
			calloutTracker.RequestCallout(this.initiator, this.initiatorRulePack, grammarRequest);
		}

		// Token: 0x0600003B RID: 59 RVA: 0x000039B4 File Offset: 0x00001BB4
		private void DoRecipientCallout(CalloutTracker calloutTracker)
		{
			GrammarRequest grammarRequest = this.PrepareGrammarRequest(this.recipientRulePack);
			calloutTracker.RequestCallout(this.recipient, this.recipientRulePack, grammarRequest);
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000039E4 File Offset: 0x00001BE4
		protected virtual GrammarRequest PrepareGrammarRequest(RulePackDef rulePack)
		{
			GrammarRequest result = new GrammarRequest
			{
				Includes = 
				{
					rulePack
				}
			};
			CalloutUtility.CollectPawnRules(this.initiator, "INITIATOR", ref result);
			bool flag = this.recipient != null;
			if (flag)
			{
				CalloutUtility.CollectPawnRules(this.recipient, "RECIPIENT", ref result);
			}
			return result;
		}

		// Token: 0x0400004E RID: 78
		public Pawn initiator = null;

		// Token: 0x0400004F RID: 79
		public Pawn recipient = null;

		// Token: 0x04000050 RID: 80
		public RulePackDef initiatorRulePack = null;

		// Token: 0x04000051 RID: 81
		public RulePackDef recipientRulePack = null;
	}
}
