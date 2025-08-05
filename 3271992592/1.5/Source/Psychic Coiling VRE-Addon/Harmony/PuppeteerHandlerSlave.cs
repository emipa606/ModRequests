using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Psychic_Coiling_VRE_Addon
{
    public static class PuppeteerHandlerSlave
    {
        public static void CheckForAndHandlePuppeteerMod()
        {
            var harmony = new HarmonyLib.Harmony("com.Psychic_Coiling_VRE_Addon");

            var myPrefixInfo = SymbolExtensions.GetMethodInfo(() => HandlePuppeteerCompatibilityPatch.MyPrefix(null, false, null));
            var originalMethod = AccessTools.Method(typeof(VPEPuppeteer.MentalStateHandler_TryStartMentalState_Patch), "Prefix");

            harmony.Patch(originalMethod, prefix: myPrefixInfo);
        }
    }

    public static class HandlePuppeteerCompatibilityPatch
    {
        public static bool MyPrefix(MentalStateDef stateDef, bool __result, Pawn __1)
        {
            Log.Message("Test 2");
            if ((stateDef.defName == "VREA_Reformatting" || stateDef.defName == "VREA_SolarFlared") && VPEPuppeteer.VPEPUtils.IsPuppet(__1))
            {
                Log.Message("Test 3");
                __result = true;
                return false;
            }
            return true;
        }
    }
}
