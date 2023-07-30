using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class GenMapUIPatches
    {
        [HarmonyPatch(typeof(GenMapUI))]
        [HarmonyPatch("DrawPawnLabel", new Type[] { typeof(Pawn), typeof(Rect), typeof(float), typeof(float), typeof(Dictionary<string, string>), typeof(GameFont), typeof(bool), typeof(bool) })]
        public class GenMapUI_DrawPawnLabel
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                return (!Prefs.DevMode || !MeeseeksMod.settings.screenShotDebug);
            }
        }

        [HarmonyPatch(typeof(GenMapUI))]
        [HarmonyPatch("DrawThingLabel", new Type[] { typeof(Vector2), typeof(string), typeof(Color) })]
        public class GenMapUI_DrawThingLabel
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                return (!Prefs.DevMode || !MeeseeksMod.settings.screenShotDebug);
            }
        }
    }
}
