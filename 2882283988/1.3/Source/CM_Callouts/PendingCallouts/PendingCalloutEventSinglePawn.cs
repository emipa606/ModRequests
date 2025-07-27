using System;
using Verse;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts
{
	// Token: 0x0200001C RID: 28
	public class PendingCalloutEventSinglePawn : PendingCalloutEvent
	{
		// Token: 0x0600003F RID: 63 RVA: 0x00003A92 File Offset: 0x00001C92
		public PendingCalloutEventSinglePawn(CalloutCategory _category, Pawn _initiator, RulePackDef _initiatorRulePack) : base(_category)
		{
			this.initiator = _initiator;
			this.initiatorRulePack = _initiatorRulePack;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00003ABC File Offset: 0x00001CBC
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
						CalloutTracker component = Current.Game.World.GetComponent<CalloutTracker>();
						bool flag4 = component != null;
						if (flag4)
						{
							bool flag5 = (Prefs.DevMode && CalloutMod.settings.forceInitiatorCallouts) || (component.CheckCalloutChance(this.category, this.initiatorRulePack, "rule") && CalloutUtility.CanCalloutNow(this.initiator));
							bool flag6 = flag5;
							if (flag6)
							{
								this.DoInitiatorCallout(component);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00003B8C File Offset: 0x00001D8C
		private void DoInitiatorCallout(CalloutTracker calloutTracker)
		{
			GrammarRequest grammarRequest = this.PrepareGrammarRequest(this.initiatorRulePack);
			calloutTracker.RequestCallout(this.initiator, this.initiatorRulePack, grammarRequest);
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00003BBC File Offset: 0x00001DBC
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
			return result;
		}

		// Token: 0x04000053 RID: 83
		public Pawn initiator = null;

		// Token: 0x04000054 RID: 84
		public RulePackDef initiatorRulePack = null;
	}
}
