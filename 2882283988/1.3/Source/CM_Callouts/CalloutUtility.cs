using System;
using System.Collections.Generic;
using CM_Callouts.PendingCallouts;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace CM_Callouts
{
	// Token: 0x02000008 RID: 8
	public static class CalloutUtility
	{
		// Token: 0x06000018 RID: 24 RVA: 0x00002EA0 File Offset: 0x000010A0
		public static bool CanCalloutNow(Thing thing)
		{
			Pawn pawn = thing as Pawn;
			CalloutTracker component = Current.Game.World.GetComponent<CalloutTracker>();
			bool flag = pawn != null && component != null;
			return flag && component.CanCalloutNow(pawn);
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002EE4 File Offset: 0x000010E4
		public static bool CanCalloutAtTarget(Thing target)
		{
			Pawn pawn = target as Pawn;
			return pawn != null && (CalloutMod.settings.allowCalloutsWhenTargetingAnimals || pawn.RaceProps.Humanlike);
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002F20 File Offset: 0x00001120
		public static void CollectPawnRules(Pawn pawn, string symbol, ref GrammarRequest grammarRequest)
		{
			grammarRequest.Rules.AddRange(GrammarUtility.RulesForPawn(symbol, pawn, null, true, true));
			grammarRequest.Constants.Add(symbol + "_gender", pawn.gender.GetLabel(false));
			CalloutUtility.CollectWeaponRules(pawn, symbol + "_WEAPON", ref grammarRequest);
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002F7C File Offset: 0x0000117C
		private static void CollectWeaponRules(Pawn pawn, string symbol, ref GrammarRequest grammarRequest)
		{
			bool flag = pawn.equipment != null && pawn.equipment.Primary != null;
			if (flag)
			{
				grammarRequest.Rules.AddRange(CalloutUtility.GetRulesForWeapon(symbol, pawn.equipment.Primary));
			}
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002FC4 File Offset: 0x000011C4
		public static void CollectCoverRules(Pawn pawn, Pawn target, string symbol, Verb_LaunchProjectile verb, ref GrammarRequest grammarRequest)
		{
			Thing randomCoverToMissInto = ShotReport.HitReportFor(pawn, verb, target).GetRandomCoverToMissInto();
			ThingDef thingDef = (randomCoverToMissInto != null) ? randomCoverToMissInto.def : null;
			bool flag = thingDef != null;
			if (flag)
			{
				grammarRequest.Rules.AddRange(GrammarUtility.RulesForDef(symbol, thingDef));
			}
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00003014 File Offset: 0x00001214
		public static void CollectHediffRules(Hediff hediff, string symbol, ref GrammarRequest grammarRequest)
		{
			grammarRequest.Rules.AddRange(GrammarUtility.RulesForHediffDef(symbol, hediff.def, hediff.Part));
			bool flag = hediff.Part != null;
			if (flag)
			{
				grammarRequest.Rules.AddRange(GrammarUtility.RulesForBodyPartRecord(symbol + "_target", hediff.Part));
			}
			bool flag2 = hediff.CurStage != null;
			if (flag2)
			{
				grammarRequest.Constants[symbol + "_stage"] = hediff.CurStage.label;
			}
		}

		// Token: 0x0600001E RID: 30 RVA: 0x0000309E File Offset: 0x0000129E
		private static IEnumerable<Rule> GetRulesForWeapon(string prefix, Thing thing)
		{
			ThingDef projectileDef = null;
			bool flag = thing.def.Verbs != null && thing.def.Verbs.Count > 0;
			if (flag)
			{
				projectileDef = thing.def.Verbs[0].defaultProjectile;
			}
			foreach (Rule rule in PlayLogEntryUtility.RulesForOptionalWeapon(prefix, thing.def, projectileDef))
			{
				Rule_String defRule = (Rule_String)rule;
				yield return defRule;
				defRule = null;
			}
			bool flag2 = thing.Stuff != null;
			if (flag2)
			{
				yield return new Rule_String(prefix + "_stuffLabel", thing.Stuff.label);
			}
			CompArt compArt = thing.TryGetComp<CompArt>();
			bool flag3 = compArt != null && compArt.Active;
			if (flag3)
			{
				yield return new Rule_String(prefix + "_title", compArt.Title.ApplyTag(TagType.Name, null).Resolve());
			}
			CompQuality compQuality = thing.TryGetComp<CompQuality>();
			bool flag4 = compQuality != null;
			if (flag4)
			{
				yield return new Rule_String(prefix + "_quality", compQuality.Quality.GetLabel());
			}
			yield break;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000030B8 File Offset: 0x000012B8
		public static void DrawText(Vector2 worldPos, string text, Color textColor)
		{
			Vector3 position = new Vector3(worldPos.x, 0f, worldPos.y);
			Vector2 vector = Find.Camera.WorldToScreenPoint(position) / Prefs.UIScale;
			vector.y = (float)UI.screenHeight - vector.y;
			Text.Font = GameFont.Tiny;
			float x = Text.CalcSize(text).x;
			GUI.DrawTexture(new Rect(vector.x - x / 2f - 4f, vector.y, x + 8f, 12f), TexUI.GrayTextBG);
			GUI.color = textColor;
			Text.Anchor = TextAnchor.UpperCenter;
			Widgets.Label(new Rect(vector.x - x / 2f, vector.y - 2f, x, 999f), text);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		// Token: 0x04000038 RID: 56
		public static PendingCalloutEvent pendingCallout = null;
	}
}
