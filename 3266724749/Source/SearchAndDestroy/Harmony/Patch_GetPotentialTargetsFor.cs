using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using Settings = Tacticowl.ModSettings_Tacticowl;

namespace Tacticowl
{
    [HarmonyPatch(typeof(AttackTargetsCache), nameof(AttackTargetsCache.GetPotentialTargetsFor))]
    class Patch_GetPotentialTargetsFor
    {
        static bool Prepare()
		{
			return Settings.searchAndDestroyEnabled;
		}
        static void Postfix(IAttackTargetSearcher th, List<IAttackTarget> __result)
        {    
            if (th is not Pawn searchPawn || !searchPawn.SearchesAndDestroys() || __result.NullOrEmpty()) return;

            for (int i = __result.Count; i-- > 0;)
            {
                var target = __result[i];
                if (target is Pawn targetPawn && (!targetPawn.NonHumanlikeOrWildMan() || targetPawn.IsAttacking() || targetPawn.IsMutant)) continue;
                // TODO check turret in here, needs to maybe have settings
                // Also add check for hives, and also have a gizmo to toggle hitting hives
                // if(target is Building_Turret) continue
                __result.Remove(target);
            }
            
            
            
        }
    }
}
