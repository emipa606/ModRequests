using HarmonyLib;
using RimWorld;
using SearchAndDestroy.Storage;
using System.Linq;
using Verse;
using Verse.AI;
using System;
using System.Collections.Generic;

namespace SearchAndDestroy.Harmony
{
    [HarmonyPatch(typeof(AttackTargetsCache), "GetPotentialTargetsFor")]
    static class AttackTargetsCache_GetPotentialTargetsFor
    {
        static void Postfix(IAttackTargetSearcher th, ref List<IAttackTarget> __result)
        {
            List<IAttackTarget> shouldRemove = new List<IAttackTarget>();
            var searchPawn = th as Pawn;
            if (searchPawn == null)
            {
                return;
            }
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store != null)
            {
                ExtendedPawnData pawnData = store.GetExtendedDataFor(searchPawn);
                if (!pawnData.SD_enabled) //only apply patch for SD pawns
                {
                    return;
                }
            }
            foreach (var target in __result)
            {
                var targetPawn = target as Pawn;
                if (targetPawn != null && targetPawn.NonHumanlikeOrWildMan() && !targetPawn.IsAttacking())
                {
                    shouldRemove.Add(target);
                }
                if(targetPawn == null)//thing is building
                {
                    shouldRemove.Add(target);
                }
            }
            __result = __result.Except(shouldRemove).ToList();
        }
    }
}
