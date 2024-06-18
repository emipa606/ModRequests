using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Reunion
{
    [DefOf]

    public static class RE_DefOf
    {
        public static ThingDef ReunionHeavyArmor;
        public static ThingDef ReunionHeavyHelmet;
    }

    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("Reunion.Mod").PatchAll();
        }
    }

    //[HarmonyPatch(typeof(PawnRenderer), "DrawHeadHair")]
    //public static class DrawHeadHair_Patch
    //{
    //    public static bool Prefix(Pawn ___pawn, Vector3 rootLoc, Vector3 headOffset, float angle, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
    //    {
    //        Pawn pawn = ___pawn;
    //        if (pawn.apparel.AnyApparel)
    //        {
    //            if (pawn.apparel.WornApparel.Any(x => x.def == RE_DefOf.ReunionHeavyHelmet))
    //            {
    //                return false;
    //            }
    //        }
    //        return true;
    //    }
    //}

    //[HarmonyPatch(typeof(PawnGraphicSet), "HairMatAt")]
    //public static class PawnGraphicSet_HairMatAt_Patch
    //{
    //    public static void Postfix(PawnGraphicSet __instance, ref Material __result, Rot4 facing, bool portrait = false, bool cached = false)
    //    {
    //        Pawn pawn = __instance.pawn;
    //        if (pawn.apparel.AnyApparel && !portrait)
    //        {
    //            if (pawn.apparel.WornApparel.Any(x => x.def == RE_DefOf.ReunionHeavyHelmet))
    //            {
    //                __result = BaseContent.ClearMat;
    //            }
    //        }
    //    }
    //}

    //[HarmonyPatch(typeof(PawnGraphicSet), "HeadMatAt")]
    //public static class PawnGraphicSet_HeadMatAt_Patch
    //{
    //    public static void Postfix(PawnGraphicSet __instance, ref Material __result, Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false, bool portrait = false, bool allowOverride = true)
    //    {
    //        Pawn pawn = __instance.pawn;
    //        if (pawn.apparel.AnyApparel && !portrait)
    //        {
    //            if (pawn.apparel.WornApparel.Any(x => x.def == RE_DefOf.ReunionHeavyHelmet))
    //            {
    //                __result = BaseContent.ClearMat;
    //            }
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(PawnGraphicSet), "MatsBodyBaseAt")]
    public static class PawnGraphicSet_MatsBodyBaseAt_Test_Patch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix(PawnGraphicSet __instance, ref List<Material> __result)
        {
            Pawn pawn = __instance.pawn;
            if (!pawn.RaceProps.Humanlike)
            {
                return;
            }
            if (pawn.apparel.AnyApparel)
            {
                if (pawn.apparel.WornApparel.Any(x => x.def == RE_DefOf.ReunionHeavyArmor))
                {
                    for (int i = 0; i < __result.Count; i++)
                    {
                        __result[i] = BaseContent.ClearMat;
                    }
                }
            }
        }
    }
}
