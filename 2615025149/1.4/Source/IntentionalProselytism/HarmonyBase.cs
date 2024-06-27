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
using Verse.Sound;

namespace IntentionalProselytism {
	[StaticConstructorOnStartup]
	internal static class HarmonyBase {

		internal static Harmony instance;

		static HarmonyBase() {
			instance = new Harmony("ordpus.IntentionalProselytism");
			instance.PatchAll();
		}
	}

	[HarmonyPatch(typeof(IdeoUIUtility), nameof(IdeoUIUtility.DoIdeoDetails))]
	internal static class RimWorld__IdeoUIUtility__DoIdeoDetails {
		internal static string text;
		internal static float textWidth;

		static RimWorld__IdeoUIUtility__DoIdeoDetails() {
			LongEventHandler.ExecuteWhenFinished(() => {
				text = "IntentionalProselytism.MakeFluid".Translate();
				textWidth = Text.CalcSize(text).x + 15f;
			});
		}

		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var hasFound = false;
			var foundLabel = false;
			var label = default(Label);
			var list = instructions.ToList();
			for(int i = 0; i < list.Count; ++i) {
				var code = list[i];
				if(!foundLabel && code.opcode == OpCodes.Brfalse_S && list[i - 1].Calls(AccessTools.PropertyGetter(typeof(Ideo), nameof(Ideo.Fluid)))) {
					label = (Label)code.operand;
					foundLabel = true;
				} else if(!hasFound && foundLabel && code.labels.Contains(label)) {
					code.labels.Remove(label);
					yield return new CodeInstruction(OpCodes.Ldloc_3).WithLabels(label);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RimWorld__IdeoUIUtility__DoIdeoDetails), nameof(MakeFluid)));
					hasFound = true;
				}
				yield return code;
			}
		}

		internal static void MakeFluid(float curY, Rect inRect, Ideo ideo) {
			if(ideo.Fluid || !CanFluid(ideo)) return;
			var width = inRect.width - 16f;
			var x2 = (width - IdeoUIUtility.PreceptBoxSize.x * 3f - 16f) / 2f;
			if(Widgets.ButtonText(new Rect(x2, curY, textWidth, Text.LineHeight + 5f), text)) {
				ideo.Fluid = true;
				ideo.development.reformCount = ideo.memes.Count - 2;
			}
		}

		internal static bool CanFluid(Ideo ideo) => !Find.FactionManager.AllFactionsInViewOrder.Any(x => x != Faction.OfPlayer && (x.ideos?.Has(ideo) ?? false)) && !PawnsFinder.AllMapsAndWorld_Alive.Any(x => x.Faction != Faction.OfPlayer && x.Ideo == ideo);
	}


	[HarmonyPatch(typeof(IdeoUIUtility), nameof(IdeoUIUtility.DoIdeoListAndDetails))]
	internal static class IdeoUIUtility__DoIdeoListAndDetails {
		private static void CreateNewIdeo() {
			var ideo = IdeoUtility.MakeEmptyIdeo();
			Find.WindowStack.Add(new Dialog_ChooseMemes(ideo, MemeCategory.Structure, initialSelection: false, () => {
				SoundDefOf.Click.PlayOneShotOnCamera();
				IdeoUIUtility.SetSelected(ideo);
				if(!Find.IdeoManager.IdeosListForReading.Contains(ideo)) Find.IdeoManager.Add(ideo);
			}));
		}

		internal static void Prefix(ref bool showCreateIdeoButton, ref bool showLoadExistingIdeoBtn, ref Action createCustomBtnActOverride) {
			showCreateIdeoButton = true;
			showLoadExistingIdeoBtn = true;
			if(Find.WindowStack.WindowOfType<Dialog_ConfigureIdeo>() == null) createCustomBtnActOverride = CreateNewIdeo;
		}
	}


	// Allow Midgame Edit
	[HarmonyPatch(typeof(IdeoUIUtility), nameof(IdeoUIUtility.DoIdeoDetails))]
	internal static class IdeoUIUtility__DoIdeoDetails {
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			bool firstLoc = true;
			foreach(var code in instructions) {
				yield return code;
				if(firstLoc && code.opcode == OpCodes.Ldloc_1) {
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IdeoUIUtility__DoIdeoDetails), nameof(ChangeMode)));
					yield return new CodeInstruction(OpCodes.Stloc_1);
					yield return new CodeInstruction(OpCodes.Ldloc_1);
				}
			}
		}

		internal static IdeoEditMode ChangeMode(IdeoEditMode mode) {
			return mode == IdeoEditMode.Dev || mode == IdeoEditMode.Reform ? mode : (IdeoUIUtility__DrawIdeoRow.canDelete ? IdeoEditMode.GameStart : mode);
		}
	}

	// Disable relic selection
	[HarmonyPatch(typeof(IdeoUIUtility), nameof(IdeoUIUtility.DoPrecepts))]
	internal static class IdeoUIUtility__DoPrecepts {
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			string ldstr = null;
			bool first = true;
			foreach(var code in instructions) {
				yield return code;
				if(code.opcode == OpCodes.Ldstr)
					ldstr = (string)code.operand;
				else if(first && code.opcode == OpCodes.Ldarg_3 && ldstr == "IdeoRelic") {
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(IdeoUIUtility__DoPrecepts), nameof(ChangeMode)));
					first = false;
				}
			}
		}

		internal static IdeoEditMode ChangeMode(IdeoEditMode mode) {
			return mode == IdeoEditMode.Dev || mode == IdeoEditMode.Reform || Current.ProgramState != ProgramState.Playing ? mode : IdeoEditMode.None;
		}
	}

	// Disable Relic Generation
	[HarmonyPatch(typeof(IdeoFoundation), nameof(IdeoFoundation.CanAddForFaction))]
	internal static class IdeoFoundation__CanAddForFaction {

		internal static bool Prefix(PreceptDef precept) {
			return precept != PreceptDefOf.IdeoRelic || Current.ProgramState != ProgramState.Playing;
		}

	}

	[HarmonyPatch(typeof(IdeoUIUtility), "DrawIdeoRow")]
	internal static class IdeoUIUtility__DrawIdeoRow {

		internal static bool canDelete = false;

		internal static void Postfix(Ideo ideo, float curY, Rect fillRect, List<Pawn> pawns) {
			if(ideo != IdeoUIUtility.selected || pawns?.Count > 0 || !ideo.CanDelete()) return;
			DoDeleteIcon(new Rect(0, curY - 46, fillRect.width, 46f), ideo);
		}

		internal static bool CanDelete(this Ideo ideo) {
			canDelete = Find.FactionManager.AllFactionsInViewOrder.All(x => !(x.ideos?.Has(ideo) ?? false)) && PawnsFinder.AllMapsAndWorld_Alive.All(x => x.Ideo != ideo);
			return canDelete;
		}

		internal static void DoDeleteIcon(Rect viewRect, Ideo ideo) {
			var tex = TexButton.DeleteX;
			var size = 24f;
			var rect = new Rect(viewRect.x + viewRect.width - size, viewRect.y + (viewRect.height - size) / 2, size, size);
			if(Widgets.ButtonImage(rect, TexButton.DeleteX, Color.white, GenUI.SubtleMouseoverColor)) {
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(ideo.name), () => {
					Find.IdeoManager.Remove(ideo);
				}, destructive: true));
			}
		}

	}

	[HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawSocialCard))]
	internal static class SocialCardUtility__DrawSocialCard {
		internal static Texture2D Icon_DisabledProselyting = ContentFinder<Texture2D>.Get("UI/DisabledProselyting");

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
				options.Add(new FloatMenuOption(TranslationKeys.DisableProselyting.Translate(), () => IntentionalProselytismMod._datastorage.SetDiabled(pawn), Icon_DisabledProselyting, Color.white));
				Find.WindowStack.Add(new FloatMenu(options));
			}
		}

		internal static void DrawPawnIdeo(Pawn pawn, Rect rect) {
			if(pawn.IsFreeNonSlaveColonist) return;
			var ideo = IntentionalProselytismMod._datastorage.GetIdeo(pawn);
			var disabled = IntentionalProselytismMod._datastorage.GetDisabled(pawn);
			float num = rect.x + 17f;
			if(disabled || ideo != null) {
				float y = rect.y + rect.height / 2f - 16f;
				Rect outerRect = rect;
				outerRect.x = num;
				outerRect.y = y;
				outerRect.width = 32f;
				outerRect.height = 32f;
				GUI.color = ideo?.Color ?? Color.white;
				Widgets.DrawTextureFitted(outerRect, disabled ? Icon_DisabledProselyting : ideo.Icon, 1f);
				GUI.color = Color.white;
				num += 42f;
			} else GUI.color = Color.grey;
			var RoleChangeButtonSize = (Vector2)SocialCardUtility__RoleChangeButtonSize.GetValue(null);
			Rect rect2 = new Rect(rect.x + 17f, rect.y + rect.height / 2f - 16f, rect.width - num - RoleChangeButtonSize.x, 32f);
			Rect rect3 = rect;
			rect3.xMin = num;
			Text.Anchor = TextAnchor.MiddleLeft;
			var label = disabled ? TranslationKeys.DisableProselyting.Translate().ToString() : (ideo == null ? TranslationKeys.NoIdeoAssigned.Translate().ToString() : ideo.name);
			Widgets.Label(rect3, label);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			if(Mouse.IsOver(rect2)) {
				string roleDesc = null;
				if(disabled) {
					roleDesc = TranslationKeys.IdeoDesc.Translate() + "\n\n" + TranslationKeys.DisableProselyting__Desc.Translate();
				} else if(ideo != null) {
					roleDesc = TranslationKeys.IdeoDesc.Translate() + "\n\n" + ideo.name + "\n\n" + ideo.description + "\n\n" + "Memes".Translate() + "\n" + ideo.memes.Join(x => x.LabelCap, "\n");
				}
				Widgets.DrawHighlight(rect2);
				if(!roleDesc.NullOrEmpty())
					TooltipHandler.TipRegion(rect2, roleDesc);
			}
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Widgets.DrawLineHorizontal(0f, rect.yMax, rect.width);
			GUI.color = Color.white;
		}
	}

	[HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawPawnRole))]
	internal static class SocialCardUtility__DrawPawnRole {

		internal static bool Prefix(Pawn pawn) => pawn.IsFreeNonSlaveColonist;
	}

	[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoConversionAttempt))]
	internal static class Pawn_IdeoTracker__IdeoConversionAttempt {

		internal static void Prefix(Pawn ___pawn, ref Ideo initiatorIdeo) {
			if(___pawn.IsFreeNonSlaveColonist) return;
			var res = IntentionalProselytismMod._datastorage.GetIdeo(___pawn);
			if(res == null) return;
			initiatorIdeo = res;
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

	[HarmonyPatch(typeof(InteractionWorker_ConvertIdeoAttempt), nameof(InteractionWorker_ConvertIdeoAttempt.RandomSelectionWeight))]
	internal static class InteractionWorker_ConvertIdeoAttempt__RandomSelectionWeight {

		internal static void Postfix(ref float __result, Pawn initiator, Pawn recipient) {
			if(IntentionalProselytismMod._datastorage.GetDisabled(recipient)) __result = 0;
			else if(IntentionalProselytismSettings.disableInterColonistProselytizing && initiator.IsFreeNonSlaveColonist && recipient.IsFreeNonSlaveColonist) __result = 0;
		}

	}

	[HarmonyPatch(typeof(AbilityUtility), nameof(AbilityUtility.ValidateNotSameIdeo))]
	internal static class AbilityUtility__ValidateNotSameIdeo {

		internal static void Postfix(ref bool __result, Pawn casterPawn, Pawn targetPawn) {
			if(!__result && !targetPawn.IsFreeNonSlaveColonist && casterPawn.Ideo != IntentionalProselytismMod._datastorage.GetIdeo(targetPawn)) __result = true;
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


	[HarmonyPatch]
	internal static class VFEMech__Building_IndoctrinationPod__GetGizmos__d__13__MoveNext {
		internal static IEnumerable<MethodBase> TargetMethods() => new List<MethodBase> {
			AccessTools.EnumeratorMoveNext(AccessTools.Method("VFEMech.Building_IndoctrinationPod:GetGizmos"))
		}.Union(AccessTools.TypeByName("VFEMech.Building_IndoctrinationPod").GetMethods(AccessTools.all).Where(x => x.Name.Contains("GetGizmos") || x.Name.Contains("Tick")));
		internal static bool Prepare() => ModLister.HasActiveModWithName(IntentionalProselytismMod.Modname_VFEM);

		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			foreach(var code in instructions) {
				yield return code;
				if(code.Calls(AccessTools.PropertyGetter(typeof(FactionIdeosTracker), nameof(FactionIdeosTracker.AllIdeos))))
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VFEMech__Building_IndoctrinationPod__GetGizmos__d__13__MoveNext), nameof(GetIdeos)));
			}
		}

		internal static IEnumerable<Ideo> GetIdeos(IEnumerable<Ideo> original) {
			if(IntentionalProselytismSettings.unlockVFEMIndoctrinationPod) return Find.IdeoManager.IdeosListForReading;
			else return original;
		}
	}

}

