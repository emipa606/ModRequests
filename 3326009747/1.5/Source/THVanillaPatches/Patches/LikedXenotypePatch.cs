using System;
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
    public class LikedXenotypePatch(THPatchDef def) : ToggleablePatchParent(def)
    {
        protected override List<PatchInfo> Patches => new List<PatchInfo>()
        {
            new PatchInfo(AccessTools.Method(typeof(GeneUtility), nameof(GeneUtility.PawnIsCustomXenotype)), Prefix: new HarmonyMethod(GetType(), nameof(NewPawnIsCustomXenotype)))
        };
        
        private static bool NewPawnIsCustomXenotype(Pawn pawn, CustomXenotype custom, ref bool __result)
        {
            if (!ModsConfig.BiotechActive || pawn.genes == null)
            {
                __result = false;
                return false;
            }

            if (custom.name != pawn.genes.xenotypeName)
            {
                __result = false;
                return false;
            }
            
            List<Gene> pawnGenes = pawn.genes.GenesListForReading;

            int numberNeeded = (int)(custom.genes.Count * 0.9f);
            int numberShared = Enumerable.Count(custom.genes, geneDef => pawnGenes.Any(x => x.def == geneDef));

            __result = numberShared > numberNeeded;
            return false;
        }
    }
}