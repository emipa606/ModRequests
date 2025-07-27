using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Callouts
{
    [StaticConstructorOnStartup]
    public static class MoteText_Patches
    {
        [HarmonyPatch(typeof(MoteText))]
        [HarmonyPatch("DrawGUIOverlay", MethodType.Normal)]
        public static class MoteText_DrawGUIOverlay
        {
            [HarmonyPrefix]
            public static bool Prefix(MoteText __instance)
            {
                if (CalloutMod.settings.drawLabelBackgroundForTextMotes)
                {
                    float timeBeforeStartFadeout = !(__instance.overrideTimeBeforeStartFadeout >= 0f) ? __instance.def.mote.solidTime : __instance.overrideTimeBeforeStartFadeout;

                    float a = 1f - (__instance.AgeSecs - timeBeforeStartFadeout) / __instance.def.mote.fadeOutTime;
                    CalloutUtility.DrawText(textColor: new Color(__instance.textColor.r, __instance.textColor.g, __instance.textColor.b, a), worldPos: new Vector2(__instance.exactPosition.x, __instance.exactPosition.z), text: __instance.text);
                    //GenMapUI.DrawText(textColor: new Color(__instance.textColor.r, __instance.textColor.g, __instance.textColor.b, a), worldPos: new Vector2(__instance.exactPosition.x, __instance.exactPosition.z), text: __instance.text);
                    return false;
                }

                return true;
            }

            
        }
    }
}
