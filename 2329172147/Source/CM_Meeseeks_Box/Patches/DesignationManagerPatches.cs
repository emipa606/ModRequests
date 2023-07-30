using System;

using HarmonyLib;
using Verse;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class DesignationManagerPatches
    {
        [HarmonyPatch(typeof(DesignationManager))]
        [HarmonyPatch("DesignationOn", new Type[] { typeof(Thing), typeof(DesignationDef) })]
        public class DesignationManager_DesignationOn
        {
            [HarmonyPostfix]
            public static void Postfix(Thing t, DesignationDef def, ref Designation __result)
            {
                if (__result == null && DesignatorUtility.getFudgedForToilCheck)
                {
                    __result = new Designation(t, def);
                }
            }
        }

        [HarmonyPatch(typeof(DesignationManager))]
        [HarmonyPatch("DesignationAt", new Type[] { typeof(IntVec3), typeof(DesignationDef) })]
        public class DesignationManager_DesignationAt
        {
            [HarmonyPostfix]
            public static void Postfix(IntVec3 c, DesignationDef def, ref Designation __result)
            {
                if (__result == null && DesignatorUtility.getFudgedForToilCheck)
                {
                    __result = new Designation(c, def);
                }
            }
        }
    }
}
