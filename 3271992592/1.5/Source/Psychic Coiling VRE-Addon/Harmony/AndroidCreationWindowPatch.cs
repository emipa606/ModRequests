using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Unity;
using HarmonyLib;
using VREAndroids;

namespace Psychic_Coiling_VRE_Addon
{
    [HarmonyPatch(typeof(Window_CreateAndroidBase), "CanAccept")]
    public static class AndroidCreationWindowCanAcceptPatch
    {
        public static void Postfix(Window_CreateAndroidBase __instance, ref bool __result, ref List<GeneLeftChosenGroup> ___leftChosenGroups)
        {
            if (___leftChosenGroups.Count == 1)
            {
                GeneLeftChosenGroup geneLeftChosenGroup = ___leftChosenGroups[0];

                if (geneLeftChosenGroup.leftChosen == VREAPC_InternalDefs.VREA_Addon_PsychicCoils)
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Window_CreateAndroidBase), "DoBottomButtons")]
    public static class AndroidCreationWindowBottomButtonsPatch
    {
        public static bool Prefix(Window_CreateAndroidBase __instance, ref List<GeneLeftChosenGroup> ___leftChosenGroups)
        {
            if (___leftChosenGroups.Count == 1)
            {
                GeneLeftChosenGroup geneLeftChosenGroup = ___leftChosenGroups[0];

                if (geneLeftChosenGroup.leftChosen == VREAPC_InternalDefs.VREA_Addon_PsychicCoils)
                {
                    ___leftChosenGroups.Pop();
                }
            }
            return true;
        }
    }
}