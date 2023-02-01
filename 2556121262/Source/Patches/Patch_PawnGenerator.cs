using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;

namespace HalfDragons.Patch2
{
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new[] { typeof(PawnGenerationRequest) })]
    static class RV2_Patch_GeneratePawn
    {
        [HarmonyPostfix]
        private static void HalfDragons_ChangeHairColor(ref Pawn __result)
        {
            try
            {
                IEnumerable<TraitColorChangesDef> colorChangeDefs = DefDatabase<TraitColorChangesDef>.AllDefsListForReading;
                if (colorChangeDefs.EnumerableNullOrEmpty())
                {
                    return;
                }
                foreach(TraitColorChangesDef colorChangeDef in colorChangeDefs)
                {
                    //Log.Message("Applying colorChangeDef " + colorChangeDef.defName + " previous hair color : " + __result.story?.hairColor);
                    colorChangeDef.SetHairColor(__result);
                    //Log.Message("new hair color : " + __result.story?.hairColor);
                }
            }
            catch (Exception e)
            {
                Log.Warning("Something went wrong during GeneratePawn patch:\n" + e);
            }
        }
    }
}