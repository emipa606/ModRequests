using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace MechPsyControlRange_Hydroxyapatite
{
    [HarmonyPatch(
            typeof(Pawn_MechanitorTracker),
            nameof(Pawn_MechanitorTracker.DrawCommandRadius))]
    internal class PatchMechanitorTracker_DrawCommandRadius
    {
        static Pawn_MechanitorTracker mechanitor;
        delegate bool commandDel(LocalTargetInfo info);
        static commandDel commandToDel;
        static int? lastCacheTick = null;
        static float range = 0;
        static bool Prefix(
            Pawn_MechanitorTracker __instance
            )
        {
            if (__instance.Pawn.Spawned && __instance.AnySelectedDraftedMechs)
            {
                int currentTick = Find.TickManager.TicksGame;
                mechanitor = __instance;
                commandToDel = __instance.CanCommandTo;
                if (!lastCacheTick.HasValue || currentTick - lastCacheTick > 60)
                {
                    lastCacheTick = currentTick;
                    range = mechanitor.Pawn.GetStatValue(MechanitorDefOf.Hydroxyapatite_MechCommandDistance, applyPostProcess: true, 60);
                }
                range = Math.Min(range, (float)Math.Floor(GenRadial.MaxRadialPatternRadius));
                GenDraw.DrawRadiusRing(mechanitor.Pawn.Position, range, Color.white, (IntVec3 c) => commandToDel(c));
            }
            return false;
        }
    }
}
