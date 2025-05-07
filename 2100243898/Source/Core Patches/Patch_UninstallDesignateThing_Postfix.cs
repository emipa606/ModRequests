using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using HarmonyLib;
using Verse;

namespace DTechprinting.Core_Patches
{
    [HarmonyPatch(typeof(Designator_Uninstall))]
    [HarmonyPatch("DesignateThing")]
    class Patch_UninstallDesignateThing_Postfix
    {
        private static void Postfix(ref Designator_Uninstall __instance, Thing t)
        {
            __instance.Map.designationManager.TryRemoveDesignationOn(t, Base.DefOf.ShardBuilding);
        }
    }
}
