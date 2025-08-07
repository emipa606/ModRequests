using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace MechPsyControlRange_Hydroxyapatite
{
    [HarmonyPatch(
            typeof(Pawn_MechanitorTracker),
            nameof(Pawn_MechanitorTracker.CanCommandTo),
            new Type[] { typeof(LocalTargetInfo) })]
    internal class PatchMechanitorTracker_CanCommandTo
    {
        static Pawn_MechanitorTracker mechanitor;
        static int? lastCacheTick = null;
        static float range = 0;
        static void Postfix(
            Pawn_MechanitorTracker __instance,
            LocalTargetInfo target,
            ref bool __result
            )
        {
            mechanitor = __instance;
            if (!target.Cell.InBounds(mechanitor.Pawn.MapHeld))
            {
                __result = false;
            }
            int currentTick = Find.TickManager.TicksGame;
            if (!lastCacheTick.HasValue || currentTick - lastCacheTick > 60)
            {
                lastCacheTick = currentTick;
                range = (float)Math.Pow(mechanitor.Pawn.GetStatValue(MechanitorDefOf.Hydroxyapatite_MechCommandDistance, applyPostProcess: true, 60), 2);
            }
            float maxRange = (float)Math.Pow(Math.Floor(GenRadial.MaxRadialPatternRadius), 2);
            __result = mechanitor.Pawn.Position.DistanceToSquared(target.Cell) < Math.Min(range, maxRange);
        }
    }
}