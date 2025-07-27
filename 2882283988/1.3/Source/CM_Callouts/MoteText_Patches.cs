using System;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace CM_Callouts
{
	// Token: 0x0200000E RID: 14
	[StaticConstructorOnStartup]
	public static class MoteText_Patches
	{
		// Token: 0x02000034 RID: 52
		[HarmonyPatch(typeof(MoteText))]
		[HarmonyPatch("DrawGUIOverlay", MethodType.Normal)]
		public static class MoteText_DrawGUIOverlay
		{
			// Token: 0x0600006E RID: 110 RVA: 0x00004684 File Offset: 0x00002884
			[HarmonyPrefix]
			public static bool Prefix(MoteText __instance)
			{
				bool drawLabelBackgroundForTextMotes = CalloutMod.settings.drawLabelBackgroundForTextMotes;
				bool result;
				if (drawLabelBackgroundForTextMotes)
				{
					float num = (__instance.overrideTimeBeforeStartFadeout < 0f) ? __instance.def.mote.solidTime : __instance.overrideTimeBeforeStartFadeout;
					float a = 1f - (__instance.AgeSecs - num) / __instance.def.mote.fadeOutTime;
					Color textColor = new Color(__instance.textColor.r, __instance.textColor.g, __instance.textColor.b, a);
					CalloutUtility.DrawText(new Vector2(__instance.exactPosition.x, __instance.exactPosition.z), __instance.text, textColor);
					result = false;
				}
				else
				{
					result = true;
				}
				return result;
			}
		}
	}
}
