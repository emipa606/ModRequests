using HarmonyLib;
using Hospitality.Utilities;
using Verse;
using Verse.AI;

namespace Hospitality.Patches 
{
    /// <summary>
    /// So guests don't break while arriving or leaving
    /// </summary>
    public class MentalStateHandler_Patch
    {
        [HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]

        public class TryStartMentalState
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn ___pawn)
            {
                if (___pawn != null && ___pawn.IsGuest() && !___pawn.IsArrived())
                {
                    Log.Message($"{___pawn.LabelShort} was about to have a mental break. Cancelled, because guest is traveling.");
                    return false;
                }

                return true;
            }
        }
    }
}