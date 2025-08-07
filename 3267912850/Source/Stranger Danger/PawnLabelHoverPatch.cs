using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Stranger_Danger
{
    //Pawn label mouseover text
    [HarmonyPatch]
    static class PawnLabelHoverPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn), "LabelNoCount", MethodType.Getter)]
        static void LabelNoCountPatch(Pawn __instance, ref string __result)
        {
            if (__instance.Name == null)
                return;

            if (__instance.story == null)
                return;

            if (CustomMethods.ShouldHide(__instance, CustomMethods.HideType.Story))
                __result = __instance.Name.ToStringShort;
        }
    }
}
