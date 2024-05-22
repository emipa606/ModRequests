using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
//using AppearanceClothes;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace TakeOffIndoor
{
    public static class Harmony_WornApparel
    {
        public static void GetWornApparelPost(ref List<Apparel> __result, ref Pawn_ApparelTracker apparelTracker)
        {
            _WornApparel(ref __result, ref apparelTracker);
        }
        //public static void WornApparelPost(ref List<Apparel> __result, ref Pawn_ApparelTracker __instance)
        //{
        //    _WornApparel(ref __result, ref __instance);
        //}


        public static List<Apparel> GetWornApparel(Pawn_ApparelTracker apparelTracker)
        {
            List<Apparel> ret = apparelTracker.WornApparel;
            _WornApparel(ref ret, ref apparelTracker);
            return ret;
        }


        //頻繁に呼び出される場所はこちらを使用
        public static List<Apparel> GetWornApparelCached(Pawn_ApparelTracker apparelTracker)
        {
            if (apparelTracker == null)
            {
                return null;
            }
            TakeOffComp comp = apparelTracker.pawn.GetComp<TakeOffComp>();

            if (comp != null)
            {
                List<Apparel> ret = new List<Apparel>();
                if (comp.portrait)
                {
                    if (comp.wornApparelPortraitCash == null)
                    {
                        comp.wornApparelPortraitCash = GetWornApparel(apparelTracker);
                    }
                    return comp.wornApparelPortraitCash;
                }
                else
                {
                    if (comp.wornApparelCash == null)
                    {

                        comp.wornApparelCash = GetWornApparel(apparelTracker);
                    }
                    return comp.wornApparelCash;
                }
            }
            else
            {
                return apparelTracker.WornApparel;
            }
        }

        public static void _WornApparel(ref List<Apparel> __result, ref Pawn_ApparelTracker apparelTracker)
        {
            if (apparelTracker == null || __result == null)
            {
                return;
            }
            TakeOffComp comp = apparelTracker.pawn.GetComp<TakeOffComp>();

            if (comp != null)
            {
                comp.WornApparel(ref __result);
                if (comp.portrait)
                {
                    comp.wornApparelPortraitCash = __result;
                }
                else
                {
                    comp.wornApparelCash = __result;
                }
            }
            return;
        }


    }


}
