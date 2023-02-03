using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

// ReSharper disable UnusedMember.Global

namespace OneBigFamily
{
    internal static class PawnGenerator_Patch
    {
        // To use our override chance in all cases
        [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawnRelations))]
        public class GeneratePawnRelations
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn, ref PawnGenerationRequest request)
            {
                if (!pawn.RaceProps.Humanlike)
                {
                    return false;
                }
                List<KeyValuePair<Pawn, PawnRelationDef>> list = new List<KeyValuePair<Pawn, PawnRelationDef>>();
                List<PawnRelationDef> allDefsListForReading = DefDatabase<PawnRelationDef>.AllDefsListForReading;
                IEnumerable<Pawn> enumerable = from x in PawnsFinder.AllMapsWorldAndTemporary_Alive // AllMapsAndWorldAliveOrDead
                    where x.def == pawn.def && !x.Dead // Apparently it's still possible...
                    select x;
                
                var currents = enumerable.ToArray();  // Added for performance
                
                foreach (Pawn current in currents)
                {
                    if (current.Discarded)
                    {
                        Log.Warning($"Warning during generating pawn relations for {pawn}: Pawn {current} is discarded, yet he was yielded by PawnUtility. Discarding a pawn means that he is no longer managed by anything.");
                        return false;
                    }
                    else
                    {
                        list.AddRange(
                            allDefsListForReading.Where(t => t.generationChanceFactor > 0f)
                                .Select(t => new KeyValuePair<Pawn, PawnRelationDef>(current, t)));
                    }
                }

                // CHANGED
                var localReq = request;
                int min = ModBaseOneBigFamily.minRelationships;
                int max = ModBaseOneBigFamily.maxRelationships;
                int avg = ModBaseOneBigFamily.avgRelationships;
                int spread = Mathf.Max(Mathf.Abs(max - avg), Mathf.Abs(min - avg));

                var pawns = currents.Length;
                
                int amount = Mathf.Min(max, Mathf.Max(min, Mathf.Min(Mathf.Abs(Mathf.RoundToInt(Rand.Gaussian(avg, spread))), pawns)));
                for (int i = 0; i < amount; i++)
                {
                    var keyValuePair = list.RandomElementByWeightWithDefault(x => x.Value.generationChanceFactor*x.Value.Worker.GenerationChance(pawn, x.Key, localReq), 1f);
                    if (keyValuePair.Key == null) return false;
                    // Final check if it's actually an allowed relationship
                    if(keyValuePair.Value.Worker.GenerationChance(pawn, keyValuePair.Key, localReq) <= 0) continue;
                    
                    keyValuePair.Value.Worker.CreateRelation(pawn, keyValuePair.Key, ref request);
                    list.RemoveAll(pair => pair.Key == keyValuePair.Key);
                }

                return false;
            }
        }
    }
}