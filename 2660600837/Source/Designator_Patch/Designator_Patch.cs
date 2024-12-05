using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using FilterableDesignator.ExtensionMethod;

namespace FilterableDesignator
{
	public abstract class Designator_Patch
	{
		private protected abstract string CommandLabel(Designator instance);
		private protected abstract void BuildFloatMenuOption(Designator instance, Event ev);
		private protected virtual string MouseAttachmentText(Designator instance)
		{
			return instance.Label;
		}

		public abstract void ClearMode();

		private protected Texture2D variableIcon = null;
		private protected static ThingDef filteringStuff = null;

		private protected static FirstTarget firstTarget = new();

		private protected static Designation FirstDesignation => firstTarget.designation;
		private protected static Thing FirstThing => firstTarget.thing;

		private protected static bool IsReverseDesignator()
		{
			for (int i = 0; i < 3; i++)
			{
				var caller = new System.Diagnostics.StackFrame(i + 2, false);
				if (caller.GetMethod().DeclaringType == typeof(InspectGizmoGrid))
				{
					return true;
				}
			}
			return false;
		}

		[HarmonyPatch(typeof(DesignationDragger), nameof(DesignationDragger.StartDrag))]
		public class StartDrag_Patch
		{
			[HarmonyPostfix]
			static void Postfix()
			{
				firstTarget.Clear();
			}
		}

		[HarmonyPatch(typeof(DesignationDragger), nameof(DesignationDragger.EndDrag))]
		public class EndDrag_Patch
		{
			[HarmonyPrefix]
			static bool Prefix()
			{
				firstTarget.Clear();
				return true;
			}
		}

		[HarmonyPatch(typeof(Command), nameof(Command.Label), MethodType.Getter)]
		public class Label_Patch
		{
			[HarmonyPrefix]
			static bool Prefix(object __instance, ref string __result)
			{
				if (__instance is Designator instance)
				{
					if (instance.PatchInstance() is Designator_Patch patchInstance)
					{
						MethodInfo CommandLabelInfo = AccessTools.Method(patchInstance.GetType(), "CommandLabel");
						__result = (string)CommandLabelInfo?.Invoke(patchInstance, new object[] { __instance });
					}
					else
					{
						__result = instance.defaultLabel;
					}
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Designator), nameof(Designator.LabelCapReverseDesignating))]
		public class LabelCapReverseDesignating_Patch
		{
			[HarmonyPostfix]
			static void Postfix(Designator __instance, ref string __result)
			{
				if (__instance.PatchInstance() is not null)
				{
					__result = "FilterableDesignator.Reverse.Label".Translate(__instance.defaultLabel).CapitalizeFirst();
				}
				else
				{
					__result = __instance.defaultLabel.CapitalizeFirst();
				}
			}
		}

		[HarmonyPatch(typeof(Command), nameof(Command.DrawIcon))]
		public class DrawIcon_Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Command __instance, out Texture __state)
			{
				__state = null;
				if (__instance.PatchInstance()?.variableIcon is Texture2D anotherIcon)
				{
					__state = __instance.icon;
					__instance.icon = anotherIcon;
				}
				return true;
			}

			[HarmonyPostfix]
			static void Postfix(Command __instance, Texture __state)
			{
				if (__state != null)
				{
					__instance.icon = __state;
				}
			}
		}

		[HarmonyPatch(typeof(Designator), nameof(Designator.ProcessInput))]
		public class ProcessInput_Patch
		{
			[HarmonyPostfix]
			static void Postfix(Designator __instance, Event ev)
			{
				if (!Utility.ReversePatches.CheckCanInteract(__instance))
				{
					return;
				}

				var patchInstance = __instance.PatchInstance();
				if (patchInstance != null)
				{
					MethodInfo BuildFloatMenuOptionInfo = AccessTools.Method(patchInstance.GetType(), "BuildFloatMenuOption");
					BuildFloatMenuOptionInfo?.Invoke(patchInstance, new object[] { __instance, ev });
				}
			}
		}

		[HarmonyPatch(typeof(Designator), nameof(Designator.DrawMouseAttachments))]
		public class DrawMouseAttachments_Patch
		{
			[HarmonyPostfix]
			static void Postfix(object __instance)
			{
				if (__instance is Designator instance)
				{
					FieldInfo useMouseIconInfo = AccessTools.Field(typeof(Designator), "useMouseIcon");
					if ((bool)useMouseIconInfo.GetValue(instance))
					{
						FieldInfo iconInfo = AccessTools.Field(typeof(Command), "icon");
						Texture2D icon = (Texture2D)iconInfo.GetValue(instance);
						if (instance.PatchInstance() is Designator_Patch patchInstance)
						{
							if (patchInstance.variableIcon != null)
							{
								icon = patchInstance.variableIcon;
							}
						}
						string text = instance.MouseAttachmentText();
						GenUI.DrawMouseAttachment(icon, text);
					}
				}
			}
		}
	}
}