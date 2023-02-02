using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Verse;
using Verse.Sound;

namespace MoreIdeoColors
{
    [HarmonyPatch(typeof(Ideo))]
    [HarmonyPatch("ApparelColor", MethodType.Getter)]
    class ApparelColorPatch
    {
        static bool Prefix(Ideo __instance, ref Color __result)
        {
            __result = __instance.Color;
            return false;
        }
	}
}
