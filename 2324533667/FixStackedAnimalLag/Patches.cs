using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace FixStackedAnimalLag
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.github.harmony.rimworld.maarx.fixstackedanimallag");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    /*
    [HarmonyPatch(typeof(WanderUtility))]
    [HarmonyPatch("GetColonyWanderRoot")]
    class WanderUtility_GetColonyWanderRoot
    {
        private static Dictionary<Area, IntVec3> cachedRoots = new Dictionary<Area, IntVec3>();
        private static Dictionary<Area, int> cachedTimes = new Dictionary<Area, int>();
        private static int cacheTTL = 1000;
        static void Postfix(ref Pawn pawn, ref IntVec3 __result, ref bool __state)
        {
            if (!__state)
            {
                //Log.Message("Hello from GetColonyWanderRoot Postfix");
                Area pawnArea = pawn.playerSettings.AreaRestriction;
                cachedRoots.SetOrAdd(pawnArea, __result);
                cachedTimes.SetOrAdd(pawnArea, Find.TickManager.TicksGame);
            }
        }

        static bool Prefix(ref Pawn pawn, ref IntVec3 __result, ref bool __state)
        {
            //Log.Message("Hello from GetColonyWanderRoot Prefix");
            Area pawnArea = pawn.playerSettings.AreaRestriction;
            __state = false;
            if (cachedTimes.TryGetValue(pawnArea, int.MaxValue) < Find.TickManager.TicksGame + cacheTTL)
            {
                IntVec3 cachedValue = cachedRoots.TryGetValue(pawnArea, IntVec3.Invalid);
                if (cachedValue != IntVec3.Invalid)
                {
                    __result = cachedValue;
                    __state = true;
                    return false;
                }
            }
            Log.Message("Hello from GetColonyWanderRoot Original");
            return true;
        }
    }

    [HarmonyPatch(typeof(RCellFinder))]
    [HarmonyPatch("RandomWanderDestFor")]
    class RCellFinder_RandomWanderDestFor
    {
        private static Dictionary<IntVec3, IntVec3> cachedDests = new Dictionary<IntVec3, IntVec3>();
        private static Dictionary<IntVec3, int> cachedTimes = new Dictionary<IntVec3, int>();
        private static int cacheTTL = 1000;

        static void Postfix(ref Pawn pawn, ref IntVec3 __result, ref bool __state)
        {
            if (!__state)
            {
                //Log.Message("Hello from RandomWanderDestFor Postfix");
                IntVec3 startPos = pawn.Position;
                cachedDests.SetOrAdd(startPos, __result);
                cachedTimes.SetOrAdd(startPos, Find.TickManager.TicksGame);
            }
        }

        static bool Prefix(ref Pawn pawn, ref IntVec3 root, ref float radius, ref Func<Pawn, IntVec3, IntVec3, bool> validator, ref Danger maxDanger, ref IntVec3 __result, ref bool __state)
        {
            //Log.Message("Hello from RandomWanderDestFor Prefix");
            if (pawn.training == null)
            {
                //Log.Message("Hello from RandomWanderDestFor Original - no Training");
                return true;
            }
            IntVec3 startPos = pawn.Position;
            __state = false;
            if (cachedTimes.TryGetValue(startPos, int.MaxValue) < Find.TickManager.TicksGame + cacheTTL)
            {
                IntVec3 cachedValue = cachedDests.TryGetValue(startPos, IntVec3.Invalid);
                if (cachedValue != IntVec3.Invalid)
                {
                    __result = cachedValue;
                    __state = true;
                    return false;
                }
            }
            Log.Message("Hello from RandomWanderDestFor Original");
            return true;
        }
    }
    */


    [HarmonyPatch(typeof(PawnCollisionTweenerUtility))]
    [HarmonyPatch("PawnCollisionPosOffsetFor")]
    class PawnCollisionTweenerUtility_PawnCollisionPosOffsetFor
    {
        static bool Prefix(ref Pawn pawn, ref Vector3 __result)
        {
            if (pawn != null && pawn.AnimalOrWildMan())
            {
                __result = Vector3.zero;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnUtility))]
    [HarmonyPatch("PawnBlockingPathAt")]
    class PawnUtility_PawnBlockingPathAt
    {
        static bool Prefix(ref IntVec3 c, ref Pawn forPawn, ref bool actAsIfHadCollideWithPawnsJob, ref bool collideOnlyWithStandingPawns, ref bool forPathFinder, ref Pawn __result)
        {
            if (FixStackedAnimalLag_GlobalRuntimeSettings.shouldCollideEnemies && forPawn.HostileTo(Faction.OfPlayer))
            {
                List<Thing> thingList = c.GetThingList(forPawn.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Pawn pawn = thingList[i] as Pawn;
                    if (pawn != null && pawn != forPawn && !pawn.Downed)
                    {
                        __result = pawn;
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnUtility))]
    [HarmonyPatch("ShouldCollideWithPawns")]
    class PawnUtility_ShouldCollideWithPawns
    {
        static bool Prefix(ref Pawn p, ref bool __result)
        {
            if (p.RaceProps.Animal && p.Faction == Faction.OfPlayer)
            {
                __result = false;
                return false;
            }
            if (FixStackedAnimalLag_GlobalRuntimeSettings.shouldCollideEnemies && p.HostileTo(Faction.OfPlayer) && !p.Downed)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
