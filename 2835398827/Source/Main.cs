using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using Verse.Noise;
using Verse.Grammar;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;

// using System.Reflection;
// using HarmonyLib;

namespace ShardMods
{

    [StaticConstructorOnStartup]
    public static class Start
    {
        static Start()
        {
            var har = new Harmony("Shard.PsycastRange");
            Verse.Log.Message("Psycast Sensitivity-based Ranges loading");
            har.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(CompAbilityEffect), "Apply")]
    class ReversePatch_CompAbilityEffect
    {
        [HarmonyReversePatch]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [HarmonyPatch(new Type[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo) })]
        public static void Apply(CompAbilityEffect instance, LocalTargetInfo target, LocalTargetInfo dest)
        {
        }
    }

}
