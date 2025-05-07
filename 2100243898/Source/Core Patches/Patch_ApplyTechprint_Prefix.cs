using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using HarmonyLib;
using Verse;

namespace DTechprinting
{
    [HarmonyPatch(typeof(ResearchManager))]
    [HarmonyPatch("ApplyTechprint")]
    class Patch_ApplyTechprint_Prefix
    {
        private static bool Prefix(ResearchProjectDef proj, Pawn applyingPawn)
        {
            ShardApplier.RefundUnlock(proj, applyingPawn, 100);
            return true;
        }

    }
}
