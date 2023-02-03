using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Harmony;

namespace MonsterMash
{
    [StaticConstructorOnStartup]
    public static class MM_HarmonyPatches
    {
        private static readonly Type patchType = typeof(MM_HarmonyPatches);

        static MM_HarmonyPatches()
        {
            // Harmony Setup and Debug
            Log.Message("MM :: Performing Hamrony Patches");
            HarmonyInstance.DEBUG = false;
            HarmonyInstance harmony = HarmonyInstance.Create(id: "com.mash.monster");

            // Harmony Patches required for Core operation.
            harmony.Patch(original: AccessTools.Method(type: typeof(JobGiver_Mate), name: "TryGiveJob"), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(JobGiver_Mate_TryGiveJobPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnUtility), name: nameof(PawnUtility.FertileMateTarget)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(PawnUtility_FertileMateTargetPostfix)));
        }

        private static void JobGiver_Mate_TryGiveJobPostfix(Pawn pawn, ref Job __result)
        {
            if (pawn.gender == Gender.None && pawn.TryGetComp<CompEggLayer>() is CompEggLayer comp && !comp.FullyFertilized)
            {
                Predicate<Thing> validator = delegate (Thing t)
                {
                    Pawn pawn3 = t as Pawn;
                    return pawn3 != pawn && !pawn3.Downed && pawn3.CanCasuallyInteractNow(false) && !pawn3.IsForbidden(pawn) && pawn3.Faction == pawn.Faction && PawnUtility.FertileMateTarget(pawn, pawn3);
                };
                Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(pawn.def), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 30f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
                if (pawn2 == null)
                {
                    __result = null;
                    return;
                }
                __result = new Job(JobDefOf.Mate, pawn2);
                return;
            }
        }

        private static void PawnUtility_FertileMateTargetPostfix(Pawn female, ref bool __result)
        {
            if (female.gender == Gender.None && female.TryGetComp<CompEggLayer>() is CompEggLayer comp && female.ageTracker.CurLifeStage.reproductive)
            {
                __result = !comp.FullyFertilized;
                return;
            }
            return;
        }
    }
}
