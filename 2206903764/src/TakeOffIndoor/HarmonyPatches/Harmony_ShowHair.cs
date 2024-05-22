using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using System.Reflection;
using HarmonyLib;
//using ShowHair;

namespace TakeOffIndoor
{
    public static class Harmony_ShowHair
    {

        //GaramRaceに対してShowHairを処理しない(Postfixに対してPrefix)
        public static bool RenderPawnInternal(Pawn ___pawn)
        {
            Pawn pawn = ___pawn;
            if (pawn == null)
            {
                return true;
            }

            if (ModUtil.PawnIsGaramRace(pawn))
            {
                return false;
            }

            return true;
        }

        public static void PatchShowHair(Harmony harmony)
        {
            MethodInfo original = AccessTools.Method(ModUtil.ShowHair_RPI, "Postfix");
            HarmonyMethod prefix = Util.HM(typeof(Harmony_ShowHair), "RenderPawnInternal");
            harmony.Patch(original, prefix, null);
            Log.Message("TakeOffCoat:ShowHair Patched.");
            debug.WriteLine("ShowHair Patched.");
        }
    }
}
