using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using System.Reflection.Emit;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Verse.AI;

namespace EverythingIsATrait
{
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GenerateGenes")]
    public static class EverythingIsATrait_PawnGenerator_GenerateGenes_Patch
    {
        [HarmonyPrefix]
        public static bool GiveGeneForTrait(Pawn pawn)
        {
            if (pawn != null){
                for (int i = 0; i < pawn.story.traits.allTraits.Count; i++) // Cycle through every trait the pawn has check for the gene equivalent
			    {
                    Trait trait = pawn.story.traits.allTraits[i];
                    string traitStr = "Genetic_" + trait.Label.ToLower(); // Make the trait.Label lowercase, since some mods appear to mess with this
                    traitStr = traitStr.Replace(" ", "");
                    if (DefDatabase<GeneDef>.GetNamed(traitStr) != null){
                            GeneDef geneDef1 = DefDatabase<GeneDef>.GetNamed(traitStr);
                            pawn.genes.AddGene(geneDef1, false);
                    }
			    }
                return true;
            }
            return false;
        }
    }
}