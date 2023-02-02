using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace IntentionalProselytism {
	[StaticConstructorOnStartup]
	internal static class HarmonyBase {

		internal static Harmony instance;

		static HarmonyBase() {
			instance = new Harmony("ordpus.IntentionalProselytism");
			instance.PatchAll();
		}
	}

	[HarmonyPatch(typeof(IdeoUIUtility), nameof(IdeoUIUtility.DoIdeoList))]
	[StaticConstructorOnStartup]
	internal static class IdeoUIUtility__DoIdeoList {

		static IdeoUIUtility__DoIdeoList() {
			HarmonyBase.instance.Patch(
				AccessTools.Method(AccessTools.TypeByName("RimWorld.IdeoUIUtility+<>c__DisplayClass59_0"), "<DoIdeoList>b__1"),
				transpiler: new HarmonyMethod(typeof(IdeoUIUtility__DoIdeoList), nameof(LoopTranspiler))
			);
		}

		internal static IEnumerable<CodeInstruction> LoopTranspiler(IEnumerable<CodeInstruction> instructions) {
			var info1 = AccessTools.DeclaredMethod(typeof(Page_ConfigureIdeo), nameof(Page_ConfigureIdeo.SelectOrMakeNewIdeo));
			var info2 = AccessTools.DeclaredMethod(typeof(IdeoUIUtility__DoIdeoList), nameof(SelectOrMakeNewIdeo));
			var list = instructions.ToList();
			var label = default(Label);
			for(int i = 0; i < list.Count; i++) {
				yield return list[i];
				if(list[i].opcode == OpCodes.Brfalse_S) label = (Label)list[i].operand;
				if(list[i].Calls(info1)) {
					yield return new CodeInstruction(OpCodes.Ldarg_1).WithLabels(label);
					yield return new CodeInstruction(OpCodes.Call, info2);
					yield return new CodeInstruction(OpCodes.Pop);
					list[i + 1].labels.Remove(label);
				}
			}
		}

		internal static Ideo SelectOrMakeNewIdeo(Ideo newIdeo) {
			var ideo = newIdeo ?? IdeoUtility.MakeEmptyIdeo();
			if(!Find.IdeoManager.IdeosListForReading.Contains(ideo)) {
				Find.IdeoManager.Add(ideo);
			}
			if(!ideo.memes.Any()) Find.WindowStack.Add(new Dialog_ChooseMemes(ideo, MemeCategory.Structure, true));
			IdeoUIUtility.SetSelected(ideo);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.EditingMemes, OpportunityType.Critical);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.EditingPrecepts, OpportunityType.Critical);
			return ideo;
		}

		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var info1 = AccessTools.Field(AccessTools.TypeByName("RimWorld.IdeoUIUtility+<>c__DisplayClass59_0"), "gameStartConfig");
			var info2 = AccessTools.DeclaredField(typeof(Page_ConfigureIdeo), nameof(Page_ConfigureIdeo.ideo));
			var info3 = AccessTools.DeclaredField(typeof(IdeoUIUtility), nameof(IdeoUIUtility.selected));
			var info4 = AccessTools.DeclaredMethod(typeof(IdeoUIUtility__DoIdeoList), nameof(SelectOrMakeNewIdeo));
			var info5 = AccessTools.DeclaredMethod(typeof(Page_ConfigureIdeo), nameof(Page_ConfigureIdeo.SelectOrMakeNewIdeo));
			var list = instructions.ToList();
			var ind = -1;
			Label label = default(Label);
			for(int i = 0; i < list.Count; i++) {
				yield return list[i];
				if(i + 8 < list.Count &&
				   list[i].opcode == OpCodes.Brfalse_S &&
				   list[i + 1].opcode == OpCodes.Ldloc_0 &&
				   list[i + 2].LoadsField(info1) &&
				   list[i + 3].opcode == OpCodes.Ldnull &&
				   list[i + 4].Calls(info5) &&
				   list[i + 5].opcode == OpCodes.Ldloc_0 &&
				   list[i + 6].LoadsField(info1) &&
				   list[i + 7].LoadsField(info2) &&
				   list[i + 8].opcode == OpCodes.Stsfld && list[i + 8].OperandIs(info3)
				) {
					ind = i + 8;
					label = (Label)list[i].operand;
				} else if(i == ind) {
					yield return new CodeInstruction(OpCodes.Ldnull).WithLabels(label);
					yield return new CodeInstruction(OpCodes.Call, info4);
					yield return new CodeInstruction(OpCodes.Stsfld, info3);
					list[i + 1].labels.Remove(label);
				}
			}
		}

	}



	[HarmonyPatch(typeof(IdeoUIUtility), nameof(IdeoUIUtility.DoIdeoListAndDetails))]
	internal static class IdeoUIUtility__DoIdeoListAndDetails {

		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			foreach(var code in instructions) {
				if(code.IsLdarg(6) || code.IsLdarg(13)) yield return new CodeInstruction(OpCodes.Ldc_I4_1).WithLabels(code.labels);
				else yield return code;
			}
		}
	}

	[HarmonyPatch(typeof(IdeoUIUtility), "DrawIdeoRow")]
	internal static class IdeoUIUtility__DrawIdeoRow {
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
			var info1 = AccessTools.DeclaredMethod(typeof(IdeoUIUtility__DrawIdeoRow), nameof(DoDeleteIcon));
			var info2 = AccessTools.DeclaredMethod(typeof(IdeoUIUtility__DrawIdeoRow), nameof(FilterPawns));
			var info3 = AccessTools.DeclaredMethod(typeof(IdeoUIUtility__DrawIdeoRow), nameof(FilterFactions));
			CodeInstruction Call(MethodInfo info) => new CodeInstruction(OpCodes.Call, info);

			var list = instructions.ToList();
			var label = il.DefineLabel();
			var retLabel = list.Last().labels[0];
			list.Last().labels.Remove(retLabel);
			for(int i = 0; i < list.Count - 1; i++) {
				yield return list[i];
			}
			yield return new CodeInstruction(OpCodes.Ldarg_0).WithLabels(retLabel);
			yield return Call(info2);
			yield return new CodeInstruction(OpCodes.Ldc_I4_0);
			yield return new CodeInstruction(OpCodes.Bgt, label);
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return Call(info3);
			yield return new CodeInstruction(OpCodes.Ldc_I4_0);
			yield return new CodeInstruction(OpCodes.Bgt, label);
			yield return new CodeInstruction(OpCodes.Ldloc_0);
			yield return new CodeInstruction(OpCodes.Ldloc_S, 8);
			yield return new CodeInstruction(OpCodes.Ldc_R4, 22f);
			yield return new CodeInstruction(OpCodes.Sub);
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return Call(info1);
			yield return list.Last().WithLabels(label);
		}

		internal static int FilterFactions(Ideo ideo) {
			var res = 0;
			foreach(var item in Find.FactionManager.AllFactionsInViewOrder) {
				if(item.ideos != null && (item.ideos.IsPrimary(ideo) || item.ideos.IsMinor(ideo))) res++;
			}
			return res;
		}

		internal static int FilterPawns(Ideo ideo) {
			var res = 0;
			foreach(var item in PawnsFinder.AllMapsAndWorld_Alive) {
				if(item.Ideo == ideo) res++;
			}
			return res;
		}

		internal static void DoDeleteIcon(Rect viewRect, float curY, Ideo ideo) {
			var rect = new Rect(viewRect.width - 12, curY + 12, 24, 24);
			if(Widgets.ButtonImage(rect, TexButton.DeleteX, Color.white, GenUI.SubtleMouseoverColor)) {
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(ideo.name), () => {
					Find.IdeoManager.Remove(ideo);
				}, destructive: true));
			}
		}

	}

	[HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawSocialCard))]
	internal static class SocialCardUtility__DrawSocialCard {

		internal static FieldInfo SocialCardUtility__RoleChangeButtonSize = AccessTools.DeclaredField(typeof(SocialCardUtility), "RoleChangeButtonSize");

		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var info1 = AccessTools.DeclaredMethod(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawPawnRoleSelection));
			var info2 = AccessTools.DeclaredMethod(typeof(SocialCardUtility__DrawSocialCard), nameof(DrawPawnIdeoSelection));
			var info3 = AccessTools.DeclaredMethod(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawPawnRole));
			var info4 = AccessTools.DeclaredMethod(typeof(SocialCardUtility__DrawSocialCard), nameof(DrawPawnIdeo));

			CodeInstruction Call(MethodInfo info) => new CodeInstruction(OpCodes.Call, info);

			foreach(var code in instructions) {
				yield return code;
				if(code.Calls(info1)) {
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Ldloc_3);
					yield return Call(info2);
				} else if(code.Calls(info3)) {
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Ldloc_3);
					yield return Call(info4);
				}
			}
		}

		internal static void DrawPawnIdeoSelection(Pawn pawn, Rect rect) {
			if(pawn.IsFreeNonSlaveColonist) return;
			var RoleChangeButtonSize = (Vector2)SocialCardUtility__RoleChangeButtonSize.GetValue(null);
			float y = rect.y + rect.height / 2f - 14f;
			Rect rect2 = new Rect(rect.width - 150f, y, RoleChangeButtonSize.x, RoleChangeButtonSize.y);
			rect2.xMax = rect.width - 26f - 4f;
			var flag = true;
			var rituals = Find.IdeoManager.GetActiveRituals(pawn.Map);
			foreach(var r in rituals)
				if(r.PawnsToCountTowardsPresence.Contains(pawn)) {
					flag = false;
					break;
				}
			if(Widgets.ButtonText(rect2, TranslationKeys.ChooseIdeo.Translate() + "...") && flag) {
				var options = new List<FloatMenuOption>();
				Ideo ideo = IntentionalProselytismMod._datastorage.GetIdeo(pawn);
				if(ideo != null)
					options.Add(new FloatMenuOption(TranslationKeys.RemoveCurrentIdeo.Translate(), () => IntentionalProselytismMod._datastorage.RemoveIdeo(pawn), Widgets.PlaceholderIconTex, Color.white));
				foreach(var i in Find.IdeoManager.IdeosListForReading)
					if(i != pawn.Ideo)
						options.Add(new FloatMenuOption(i.name, () => IntentionalProselytismMod._datastorage.SetIdeo(pawn, i), i.Icon, i.Color, MenuOptionPriority.Default));
				Find.WindowStack.Add(new FloatMenu(options));
			}
		}

		internal static void DrawPawnIdeo(Pawn pawn, Rect rect) {
			if(pawn.IsFreeNonSlaveColonist) return;
			var ideo = IntentionalProselytismMod._datastorage.GetIdeo(pawn);
			float num = rect.x + 17f;
			if(ideo != null) {
				float y = rect.y + rect.height / 2f - 16f;
				Rect outerRect = rect;
				outerRect.x = num;
				outerRect.y = y;
				outerRect.width = 32f;
				outerRect.height = 32f;
				GUI.color = ideo.Color;
				Widgets.DrawTextureFitted(outerRect, ideo.Icon, 1f);
				GUI.color = Color.white;
				num += 42f;
			} else GUI.color = Color.grey;
			var RoleChangeButtonSize = (Vector2)SocialCardUtility__RoleChangeButtonSize.GetValue(null);
			Rect rect2 = new Rect(rect.x + 17f, rect.y + rect.height / 2f - 16f, rect.width - num - RoleChangeButtonSize.x, 32f);
			Rect rect3 = rect;
			rect3.xMin = num;
			Text.Anchor = TextAnchor.MiddleLeft;
			var label = ideo == null ? TranslationKeys.NoIdeoAssigned.Translate().ToString() : ideo.name;
			Widgets.Label(rect3, label);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			if(Mouse.IsOver(rect2)) {
				string roleDesc = TranslationKeys.IdeoDesc.Translate().Resolve();
				if(ideo != null) {
					roleDesc += "\n\n" + ideo.name + "\n\n" + ideo.description + "\n\n" + "Memes".Translate() + "\n" + ideo.memes.Join(x => x.LabelCap, "\n");
				}
				Widgets.DrawHighlight(rect2);
				TipSignal tip = new TipSignal(() => roleDesc, pawn.thingIDNumber * 40);
				TooltipHandler.TipRegion(rect2, tip);
			}
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Widgets.DrawLineHorizontal(0f, rect.yMax, rect.width);
			GUI.color = Color.white;
		}
	}

	[HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawPawnRole))]
	internal static class SocialCardUtility__DrawPawnRole {

		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
			var info1 = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.IsFreeNonSlaveColonist));

			var label = il.DefineLabel();

			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Call, info1);
			yield return new CodeInstruction(OpCodes.Ldc_I4_1);
			yield return new CodeInstruction(OpCodes.Beq, label);
			yield return new CodeInstruction(OpCodes.Ret);
			var list = instructions.ToList();
			yield return list[0].WithLabels(label);
			for(int i = 1; i < list.Count; i++) yield return list[i];
		}
	}

	[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoConversionAttempt))]
	internal static class Pawn_IdeoTracker__IdeoConversionAttempt {
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var info1 = AccessTools.PropertyGetter(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.PreviousIdeos));
			var info2 = AccessTools.DeclaredField(typeof(Pawn_IdeoTracker), "pawn");
			var info3 = AccessTools.DeclaredMethod(typeof(Pawn_IdeoTracker__IdeoConversionAttempt), nameof(GetIdeo));
			foreach(var code in instructions) {
				yield return code;
				if(code.Calls(info1)) {
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, info2);
					yield return new CodeInstruction(OpCodes.Ldarg_2);
					yield return new CodeInstruction(OpCodes.Call, info3);
					yield return new CodeInstruction(OpCodes.Starg, 2);
				}
			}
		}

		internal static Ideo GetIdeo(Pawn pawn, Ideo ideo) {
			if(pawn.IsFreeNonSlaveColonist) return ideo;
			var res = IntentionalProselytismMod._datastorage.GetIdeo(pawn);
			return res == null ? ideo : res;
		}
	}

	[HarmonyPatch(typeof(CompAbilityEffect_Convert), nameof(CompAbilityEffect_Convert.Apply))]
	internal static class CompAbilityEffect_Convert__Apply {

		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var info1 = AccessTools.DeclaredField(typeof(Pawn), nameof(Pawn.ideo));
			var info2 = AccessTools.DeclaredField(typeof(AbilityComp), nameof(AbilityComp.parent));
			var info3 = AccessTools.DeclaredField(typeof(Ability), nameof(Ability.pawn));
			var info4 = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Ideo));
			var info5 = AccessTools.DeclaredMethod(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.SetIdeo));
			var info6 = AccessTools.DeclaredField(typeof(Ideo), nameof(Ideo.name));

			var list = instructions.ToList();
			for(int i = 0; i < list.Count; i++) {
				if(i + 6 < list.Count &&
				   list[i].opcode == OpCodes.Ldloc_1 &&
				   list[i + 1].LoadsField(info1) &&
				   list[i + 2].opcode == OpCodes.Ldarg_0 &&
				   list[i + 3].LoadsField(info2) &&
				   list[i + 4].LoadsField(info3) &&
				   list[i + 5].Calls(info4) &&
				   list[i + 6].Calls(info5)
				) i += 6;
				else if(
					i + 3 < list.Count &&
					list[i].opcode == OpCodes.Ldloc_0 &&
					list[i + 1].Calls(info4) &&
					list[i + 2].LoadsField(info6) &&
					list[i + 3].opcode == OpCodes.Ldstr && list[i + 3].OperandIs("IDEO")
				) yield return new CodeInstruction(OpCodes.Ldloc_1);
				else yield return list[i];
			}
		}
	}

	[HarmonyPatch(typeof(InteractionWorker_ConvertIdeoAttempt), nameof(InteractionWorker_ConvertIdeoAttempt.CertaintyReduction))]
	internal static class InteractionWorker_ConvertIdeoAttempt__CertaintyReduction {

		internal static void Postfix(ref float __result, Pawn initiator, Pawn recipient) {
			var ideo = IntentionalProselytismMod._datastorage.GetIdeo(recipient);
			if(ideo != null && ideo != initiator.Ideo) __result *= IntentionalProselytismSettings.certaintyReduceFactor;
		}

	}

	[HarmonyPatch(typeof(AbilityUtility), nameof(AbilityUtility.ValidateNotSameIdeo))]
	internal static class AbilityUtility__ValidateNotSameIdeo {

		internal static void Postfix(ref bool __result, Pawn casterPawn, Pawn targetPawn) {
			if(!__result && casterPawn.Ideo != IntentionalProselytismMod._datastorage.GetIdeo(targetPawn)) __result = true;
		}

	}

	[HarmonyPatch(typeof(RitualRoleConvertee), nameof(RitualRoleConvertee.AppliesToPawn))]
	internal static class RitualRoleConvertee__AppliesToPawn {

		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var info1 = AccessTools.PropertyGetter(typeof(RitualRoleAssignments), nameof(RitualRoleAssignments.Ritual));
			var info2 = AccessTools.DeclaredField(typeof(Precept), nameof(Precept.ideo));
			var info3 = AccessTools.PropertyGetter(typeof(Pawn), nameof(Ideo));
			var info4 = AccessTools.DeclaredMethod(typeof(RitualRoleConvertee__AppliesToPawn), nameof(SameIdeo));

			var list = instructions.ToList();
			for(int i = 0; i < list.Count; i++) {
				yield return list[i];
				if(i + 9 < list.Count &&
				   list[i].opcode == OpCodes.Ldarg_1 &&
				   list[i + 1].Calls(info3) &&
				   list[i + 2].IsLdarg(4) &&
				   list[i + 3].opcode == OpCodes.Brtrue_S &&
				   list[i + 4].opcode == OpCodes.Ldnull &&
				   list[i + 5].opcode == OpCodes.Br_S &&
				   list[i + 6].IsLdarg(4) &&
				   list[i + 7].Calls(info1) &&
				   list[i + 8].LoadsField(info2) &&
				   list[i + 9].opcode == OpCodes.Bne_Un_S
				) {
					list[i + 9].opcode = OpCodes.Brfalse_S;
					yield return new CodeInstruction(OpCodes.Ldarg, 4);
					yield return new CodeInstruction(OpCodes.Call, info4);
					yield return list[i + 9];
					i += 9;
				}
			}
		}

		internal static bool SameIdeo(Pawn p, RitualRoleAssignments assignment) {
			var ideo = IntentionalProselytismMod._datastorage.GetIdeo(p) ?? p.Ideo;
			return ideo == assignment?.Ritual.ideo;
		}

	}

	[HarmonyPatch(typeof(RitualOutcomeEffectWorker_Conversion), nameof(RitualOutcomeEffectWorker_Conversion.Apply))]
	internal static class RitualOutcomeEffectWorker_Conversion__Apply {

		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var info1 = AccessTools.DeclaredMethod(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.SetIdeo));
			foreach(var code in instructions) {
				if(code.Calls(info1)) {
					yield return new CodeInstruction(OpCodes.Pop);
					yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
					yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(DataStorage), nameof(DataStorage.GetIdeoStatic)));
				}
				yield return code;
			}
		}

	}

}

