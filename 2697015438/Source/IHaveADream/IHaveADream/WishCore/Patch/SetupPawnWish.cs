using RimWorld;
using HarmonyLib;
using Verse;
using System;

namespace HDream
{

    [HarmonyPatch(typeof(Pawn), "Tick")]
    public class AddWishTick
    {
        public static void Postfix(ref Pawn __instance)
        {
            if (__instance.wishes() == null) return;
            if (!__instance.Suspended)
            {
                if (!__instance.Dead)
                {
                    __instance.wishes().WishesTick();
                }
            }
        }
    }

    /*
    [HarmonyPatch(typeof(Pawn), "ClearMind")]
    public class ClearWish
    {
        static public void Prefix(ref Pawn __instance)
        {
            if(__instance.wishes() != null)
            {
                __instance.wishes().Clear();
            }
        }
    }*/


    [HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
    public class AddWishesComponents
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.needs?.mood != null)
            {
                if (pawn.wishes() == null)
                {
                    pawn.CreatePawn_WishTracker();
                }
                
            }
        }
    }
    [HarmonyPatch(typeof(PawnComponentsUtility), "RemoveComponentsOnDespawned")]
    public class RemoveWishesOnDespawned
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.wishes() == null)
            {
                pawn.RemovePawn_WishTracker();
            }
        }
    }
    [HarmonyPatch(typeof(PawnComponentsUtility), "RemoveComponentsOnKilled")]
    public class RemoveWishesOnKilled
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.wishes() == null)
            {
                pawn.RemovePawn_WishTracker();
            }
        }
    }
}
