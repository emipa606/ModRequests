using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using THVanillaPatches.HelperThings;
using UnityEngine;
using Verse;

namespace THVanillaPatches.Patches
{
    public class CoveredBodyPatch(THPatchDef def) : ToggleablePatchParent(def)
    {
        protected override List<PatchInfo> Patches => new List<PatchInfo>
        {
            new PatchInfo(AccessTools.Method(typeof(ThoughtWorker_InSunlight), "CurrentStateInternal"), Postfix: new HarmonyMethod(GetType(), nameof(PostGetInternalState)))
        };

        private static void PostGetInternalState(ref ThoughtState __result, Pawn p)
        {
            if (!__result.Active) return;
            if (p.IsCoveredByClothing())
            {
                __result = false;
            }
        }
    }
}