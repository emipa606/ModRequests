using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using VREAndroids;
using HarmonyLib;
using UnityEngine;

namespace Psychic_Coiling_VRE_Addon
{
    // The below code is courtesy of @Vera from the Dubs Mods discord server
    // Huge thanks to them and @TruGerman for all the help!

    [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), "PsychicEntropyTrackerTick")]
    public class AndroidEntropyHandler
    {
        [HarmonyPostfix]
        public static void EnsureEntropyHediff(float ___currentEntropy, Pawn ___pawn)
        {
            if (___currentEntropy > float.Epsilon && ___pawn.IsAndroid())
            {
                if (!___pawn.health.hediffSet.HasHediff(VREAPC_InternalDefs.VREAPC_PsychicCoilStress))
                {
                    ___pawn.health.AddHediff(VREAPC_InternalDefs.VREAPC_PsychicCoilStress);
                }
            }
        }
    }
}