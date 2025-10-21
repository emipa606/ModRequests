using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace zed_0xff.CPS;

[HarmonyPatch(typeof(Toils_LayDown), "ApplyBedThoughts")]
static class Patch_ApplyBedThoughts
{
    // remove 'slept outside' debuff
    // not necessary if Cabin is a room for itself
    static void Postfix(Pawn actor, Building_Bed bed){
        if (actor.needs.mood == null) return;

        if (bed is Building_Cabin && actor.GetRoom().PsychologicallyOutdoors){
            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
        }
    }
}
