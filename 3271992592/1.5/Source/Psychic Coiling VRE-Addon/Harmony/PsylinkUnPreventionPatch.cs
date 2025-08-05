using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using VREAndroids;
using RimWorld;
using Verse;

namespace Psychic_Coiling_VRE_Addon
{
    [HarmonyPatch(typeof(Hediff_Psylink_ChangeLevel_Patch), "Prefix")]
    public static class Psylink_Unprevention_patch
    {
        public static void Postfix(ref bool __result, object[] __args)
        {
            Hediff_Psylink instance = (Hediff_Psylink)__args[0];
            if (instance.pawn.IsAndroid() && instance.pawn.HasActiveGene(VREAPC_InternalDefs.VREA_Addon_PsychicCoils))
            {
                __result = true;
            }
        }
    }
}
