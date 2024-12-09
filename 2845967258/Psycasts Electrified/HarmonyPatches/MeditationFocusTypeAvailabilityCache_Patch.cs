using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Psycasts_Electrified
{
    [HarmonyPatch(typeof(MeditationFocusTypeAvailabilityCache))]
    public static class MeditationFocusTypeAvailabilityCache_Patch
    {
        [HarmonyPatch("PawnCanUseInt")]
        static bool Postfix(bool __result, Pawn p, MeditationFocusDef type)
        {
            if (type.HasModExtension<MeditationFocusDefExtension>())
            {
                if (Helper.HasChargingEar(p))
                {
                    return true;
                }

                return type.GetModExtension<MeditationFocusDefExtension>().giveAllPawns;
            }
            return __result;
        }
    }
}
