using System;
using HarmonyLib;
using Verse;

namespace ReservedFor
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.GetInspectStringLowPriority), new Type[0])]
    static class Thing_GetInspectStringLowPriority_Patch
    {
        static void Postfix(Thing __instance, ref string __result)
        {
            foreach (var reservation in __instance.Map.reservationManager.ReservationsReadOnly)
            {
                if (reservation.Target.Thing != __instance)
                {
                    continue;
                }

                if (!ReservedFor.Instance.ShowOtherFactions && !reservation.Faction.IsPlayer)
                {
                    continue;
                }

                var pawn = reservation.Claimant;
                var job = reservation.Job.GetReport(pawn);

                if (!string.IsNullOrEmpty(__result))
                {
                    __result += "\n";
                }

                if (reservation.StackCount == 0)
                {
                    __result += "ReservedFor_notReserved".Translate(pawn.Named("PAWN"), job.Named("JOB"));
                }
                else if (reservation.StackCount != -1 && reservation.Target.HasThing && reservation.Target.Thing.stackCount != reservation.StackCount)
                {
                    __result += "ReservedFor_reservedCount".Translate(reservation.StackCount.Named("COUNT"), pawn.Named("PAWN"), job.Named("JOB"));
                }
                else
                {
                    __result += "ReservedFor_reserved".Translate(pawn.Named("PAWN"), job.Named("JOB"));
                }
            }
        }
    }
}
