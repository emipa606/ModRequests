using System;
using System.Collections.Generic;
using System.Linq;
using CM_Callouts.PendingCallouts;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace CM_Callouts
{
	// Token: 0x02000007 RID: 7
	public class CalloutTracker : WorldComponent
	{
		// Token: 0x0600000C RID: 12 RVA: 0x000026A4 File Offset: 0x000008A4
		public CalloutTracker(World world) : base(world)
		{
			this.hashCache = this.GetHashCode();
			CalloutTracker.textMoteQueuesTickBased = new Dictionary<int, TextMoteQueueTickBased>();
			CalloutTracker.textMoteQueuesRealTime = new Dictionary<int, TextMoteQueueRealTime>();
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002700 File Offset: 0x00000900
		public override void WorldComponentTick()
		{
			base.WorldComponentTick();
			int currentTickPlusHash = Find.TickManager.TicksGame + this.hashCache;
			bool flag = currentTickPlusHash % this.checkTicks == 0;
			if (flag)
			{
				this.pawnCalloutExpireTick = (from kvp in this.pawnCalloutExpireTick
				where kvp.Value > currentTickPlusHash
				select kvp).ToDictionary((KeyValuePair<Pawn, int> kvp) => kvp.Key, (KeyValuePair<Pawn, int> kvp) => kvp.Value);
			}
			CalloutTracker.UpdateTextMoteQueuesTickBased();
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000027AC File Offset: 0x000009AC
		private static void UpdateTextMoteQueuesTickBased()
		{
			bool flag = false;
			foreach (TextMoteQueue textMoteQueue in CalloutTracker.textMoteQueuesTickBased.Values)
			{
				flag = (flag || !textMoteQueue.Update());
			}
			bool flag2 = flag;
			if (flag2)
			{
				CalloutTracker.textMoteQueuesTickBased = (from kvp in CalloutTracker.textMoteQueuesTickBased
				where kvp.Value.Valid()
				select kvp).ToDictionary((KeyValuePair<int, TextMoteQueueTickBased> kvp) => kvp.Key, (KeyValuePair<int, TextMoteQueueTickBased> kvp) => kvp.Value);
			}
		}

		// Token: 0x0600000F RID: 15 RVA: 0x0000288C File Offset: 0x00000A8C
		public static void UpdateTextMoteQueuesRealTime()
		{
			bool flag = false;
			foreach (TextMoteQueue textMoteQueue in CalloutTracker.textMoteQueuesRealTime.Values)
			{
				flag = (flag || !textMoteQueue.Update());
			}
			bool flag2 = flag;
			if (flag2)
			{
				CalloutTracker.textMoteQueuesRealTime = (from kvp in CalloutTracker.textMoteQueuesRealTime
				where kvp.Value.Valid()
				select kvp).ToDictionary((KeyValuePair<int, TextMoteQueueRealTime> kvp) => kvp.Key, (KeyValuePair<int, TextMoteQueueRealTime> kvp) => kvp.Value);
			}
		}

		// Token: 0x06000010 RID: 16 RVA: 0x0000296C File Offset: 0x00000B6C
		public static Thing ThrowText(Thing thing, IntVec3 location, Map map, WipeMode wipeMode = WipeMode.Vanish)
		{
			MoteText moteText = thing as MoteText;
			bool flag = moteText != null;
			if (flag)
			{
				int key = map.Index * 1000000 + location.x * 1000 + location.z;
				bool paused = Find.TickManager.Paused;
				if (paused)
				{
					GenSpawn.Spawn(thing, location, map, WipeMode.Vanish);
				}
				else
				{
					bool flag2 = !CalloutMod.settings.queueTextMotes;
					if (flag2)
					{
						thing.def = CalloutDefOf.CM_Callouts_Thing_Mote_Text_Ticked;
						GenSpawn.Spawn(thing, location, map, WipeMode.Vanish);
					}
					else
					{
						bool flag3 = !CalloutTracker.textMoteQueuesTickBased.ContainsKey(key);
						if (flag3)
						{
							CalloutTracker.textMoteQueuesTickBased[key] = new TextMoteQueueTickBased(location, map);
						}
						CalloutTracker.textMoteQueuesTickBased[key].AddMote(moteText);
					}
				}
			}
			return thing;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002A3C File Offset: 0x00000C3C
		public bool CanCalloutNow(Pawn pawn)
		{
			return pawn != null && !pawn.Dead && pawn.Spawned && (CalloutMod.settings.allowCalloutsForAnimals || pawn.def.race.Humanlike) && !this.pawnCalloutExpireTick.ContainsKey(pawn) && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking);
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public bool CheckCalloutChance(CalloutCategory category, RulePackDef rulePackDef, string keyword = "rule")
		{
			float num = CalloutMod.settings.baseCalloutChance * this.ScaledCalloutFrequency(category, rulePackDef, "rule");
			float value = Rand.Value;
			Logger.MessageFormat(this, "calloutChance of {0} = {1}/{2}", new object[]
			{
				rulePackDef,
				value,
				num
			});
			return value <= num;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002B08 File Offset: 0x00000D08
		private float ScaledCalloutFrequency(CalloutCategory category, RulePackDef rulePackDef, string keyword = "rule")
		{
			bool flag = category != CalloutCategory.Combat;
			float result;
			if (flag)
			{
				result = 1f;
			}
			else
			{
				int num = (from rule in rulePackDef.RulesPlusIncludes
				where rule.keyword == keyword
				select rule).Count<Rule>();
				bool flag2 = num > CalloutTracker.maxRulesSeen;
				if (flag2)
				{
					CalloutTracker.maxRulesSeen = num;
				}
				float num2 = (float)num / (float)CalloutTracker.maxRulesSeen;
				result = num2;
			}
			return result;
		}
        public static bool HasHediffOfStage(Pawn pawn, HediffAndStage hdas)
        {
            HediffSet hs = pawn.health.hediffSet;
            for (int i = 0; i < hs.hediffs.Count; i++)
            {
                if (hs.hediffs[i].def == hdas.hediffDef && hs.hediffs[i].CurStageIndex == hdas.stage)
                {
                    return true;
                }
            }
            return false;
        }
        // Token: 0x06000014 RID: 20 RVA: 0x00002B7C File Offset: 0x00000D7C
        public void RequestCallout(Pawn pawn, RulePackDef rulePack, GrammarRequest grammarRequest)
		{
			bool flag = !this.CanCalloutNow(pawn);
			if (!flag)
			{
				bool inMentalState = pawn.InMentalState;
				if (inMentalState)
				{
					grammarRequest.Constants["SPICY"] = "true";
				}
				bool flag2 = pawn.story != null && pawn.story.traits != null;
				if (flag2)
				{
					foreach (CalloutConstantByTraitDef calloutConstantByTraitDef in DefDatabase<CalloutConstantByTraitDef>.AllDefs)
					{
						using (List<TraitDef>.Enumerator enumerator2 = calloutConstantByTraitDef.traits.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								TraitDef traitDef = enumerator2.Current;
								bool flag3 = pawn.story.traits.allTraits.Any((Trait trait) => trait.def == traitDef);
								if (flag3)
								{
									grammarRequest.Constants[calloutConstantByTraitDef.name] = calloutConstantByTraitDef.value;
									break;
								}
							}
						}
					}
				}
                foreach (CalloutConstantByPawnkindDef calloutConstantByPawnkindDef in DefDatabase<CalloutConstantByPawnkindDef>.AllDefs)
                {
                    if (calloutConstantByPawnkindDef.pawnKindDefs.Any((PawnKindDef pkd) => pkd.defName == pawn.kindDef.defName))
                    {
                        grammarRequest.Constants[calloutConstantByPawnkindDef.name] = calloutConstantByPawnkindDef.value;
                    }
                }
                foreach (CalloutConstantByThingDef calloutConstantByThingDef in DefDatabase<CalloutConstantByThingDef>.AllDefs)
                {
                    if (calloutConstantByThingDef.thingDefs.Any((ThingDef td) => td.defName == pawn.def.defName))
                    {
                        grammarRequest.Constants[calloutConstantByThingDef.name] = calloutConstantByThingDef.value;
                    }
                }
                if (pawn.health != null && pawn.health.hediffSet != null)
                {
                    foreach (CalloutConstantByHediffDef calloutConstantByHediffDef in DefDatabase<CalloutConstantByHediffDef>.AllDefs)
                    {
                        if (calloutConstantByHediffDef.hediffDefs.Any((HediffDef hd) => pawn.health.hediffSet.HasHediff(hd)))
                        {
                            grammarRequest.Constants[calloutConstantByHediffDef.name] = calloutConstantByHediffDef.value;
                        }
                    }
                    foreach (CalloutConstantByHediffStage calloutConstantByHediffStage in DefDatabase<CalloutConstantByHediffStage>.AllDefs)
                    {
                        if (calloutConstantByHediffStage.hediffsAndStages.Any((HediffAndStage hdas) => HasHediffOfStage(pawn, hdas)))
                        {
                            grammarRequest.Constants[calloutConstantByHediffStage.name] = calloutConstantByHediffStage.value;
                        }
                    }
                }
                if (pawn.needs != null)
                {
                    foreach (CalloutConstantByNeedDef calloutConstantByNeedDef in DefDatabase<CalloutConstantByNeedDef>.AllDefs)
                    {
                        Need need = pawn.needs.TryGetNeed(calloutConstantByNeedDef.needDef);
                        if (need != null)
                        {
                            bool above = need.CurLevel > calloutConstantByNeedDef.needLevel;
                            if (calloutConstantByNeedDef.aboveLevel ? above : !above)
                            {
                                grammarRequest.Constants[calloutConstantByNeedDef.name] = calloutConstantByNeedDef.value;
                            }
                        }
                    }
                }
                string text = GrammarResolver.Resolve("rule", grammarRequest, null, false, null, null, null, true);
				bool flag4 = !text.NullOrEmpty();
				if (flag4)
				{
					Logger.MessageFormat(this, "Callout resolved.", Array.Empty<object>());
					bool attachCalloutText = CalloutMod.settings.attachCalloutText;
					if (attachCalloutText)
					{
						this.CreateAttachedCalloutText(pawn, text, Color.white, -1f);
					}
					else
					{
						MoteMaker.ThrowText(pawn.DrawPos, pawn.MapHeld, text, -1f);
					}
				}
				else
				{
					Logger.WarningFormat(this, " Could not find text for requested {1} by {0}", new object[]
					{
						pawn,
						rulePack
					});
				}
				int value = Find.TickManager.TicksGame + this.ticksBetweenCallouts + this.hashCache;
				this.pawnCalloutExpireTick.Add(pawn, value);
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002D7C File Offset: 0x00000F7C
		private void CreateAttachedCalloutText(Thing caller, string text, Color color, float timeBeforeStartFadeout = -1f)
		{
			IntVec3 positionHeld = caller.PositionHeld;
			Map mapHeld = caller.MapHeld;
			bool flag = positionHeld.InBounds(mapHeld);
			if (flag)
			{
				MoteAttachedText moteAttachedText = (MoteAttachedText)ThingMaker.MakeThing(CalloutDefOf.CM_Callouts_Thing_Mote_Attached_Text, null);
				moteAttachedText.text = text;
				moteAttachedText.textColor = color;
				bool flag2 = timeBeforeStartFadeout >= 0f;
				if (flag2)
				{
					moteAttachedText.overrideTimeBeforeStartFadeout = timeBeforeStartFadeout;
				}
				GenSpawn.Spawn(moteAttachedText, positionHeld, mapHeld, WipeMode.Vanish);
				moteAttachedText.Attach(caller);
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002DF8 File Offset: 0x00000FF8
		public static void CreateWoundTextMote(Vector3 loc, Map map, string text, Color color, float timeBeforeStartFadeout = -1f)
		{
			IntVec3 intVec = loc.ToIntVec3();
			bool flag = intVec.InBounds(map);
			if (flag)
			{
				MoteText moteText = (MoteText)ThingMaker.MakeThing(CalloutDefOf.CM_Callouts_Thing_Mote_Text_Wound, null);
				moteText.exactPosition = loc;
				moteText.SetVelocity((float)Rand.Range(5, 35), Rand.Range(0.42f, 0.45f));
				moteText.text = text;
				moteText.textColor = color;
				bool flag2 = timeBeforeStartFadeout >= 0f;
				if (flag2)
				{
					moteText.overrideTimeBeforeStartFadeout = timeBeforeStartFadeout;
				}
				CalloutTracker.ThrowText(moteText, intVec, map, WipeMode.Vanish);
			}
		}

		// Token: 0x04000031 RID: 49
		private int ticksBetweenCallouts = 240;

		// Token: 0x04000032 RID: 50
		private int checkTicks = 60;

		// Token: 0x04000033 RID: 51
		private int hashCache = 0;

		// Token: 0x04000034 RID: 52
		private Dictionary<Pawn, int> pawnCalloutExpireTick = new Dictionary<Pawn, int>();

		// Token: 0x04000035 RID: 53
		private static Dictionary<int, TextMoteQueueTickBased> textMoteQueuesTickBased = new Dictionary<int, TextMoteQueueTickBased>();

		// Token: 0x04000036 RID: 54
		private static Dictionary<int, TextMoteQueueRealTime> textMoteQueuesRealTime = new Dictionary<int, TextMoteQueueRealTime>();

		// Token: 0x04000037 RID: 55
		private static int maxRulesSeen = 1;
	}
}
