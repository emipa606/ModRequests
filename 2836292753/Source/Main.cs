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

// using System.Reflection;
// using HarmonyLib;

namespace ShardMods
{

    [StaticConstructorOnStartup]
    public static class Start
    {
        static Start()
        {
            var har = new Harmony("Shard.TeleportVisionPatch");
            Verse.Log.Message("Shard.TeleportVisionPatch loading.");
            har.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
