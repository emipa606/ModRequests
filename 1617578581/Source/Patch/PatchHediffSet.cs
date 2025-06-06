﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Radiology.Patch
{
    /// <summary>
    /// Call PostLoad for mutations that need it.
    /// </summary>
    [HarmonyPatch(typeof(HediffSet), "ExposeData", new Type[] { })]
    public static class PatchHediffSet
    {
        static void Postfix(HediffSet __instance)
        {
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                foreach (Mutation m in __instance.hediffs.OfType<Mutation>())
                {
                    m.PostLoad();
                }
            }
        }

    }

    
}
