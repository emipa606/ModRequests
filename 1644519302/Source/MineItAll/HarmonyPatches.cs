using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace MineItAll
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("eagle0600.mineItAll");
            harmony.Patch(AccessTools.Method(typeof(ReverseDesignatorDatabase), "InitDesignators"), prefix: null,
                postfix: new HarmonyMethod(patchType, nameof(ReverseDesignatorDatabase_InitDesignatorsPostfix)));

            harmony.Patch(AccessTools.Method(typeof(Designator_Cancel), nameof(Designator_Cancel.CanDesignateThing)), prefix: null,
                postfix: new HarmonyMethod(patchType, nameof(Designator_Cancel_CanDesignateThingPostfix)));

            harmony.Patch(AccessTools.Method(typeof(Designator_Cancel), nameof(Designator_Cancel.DesignateThing)), prefix: null,
                postfix: new HarmonyMethod(patchType, nameof(Designator_Cancel_DesignateThingPostfix)));
        }

        private static void ReverseDesignatorDatabase_InitDesignatorsPostfix(ref ReverseDesignatorDatabase __instance)
        {
            Designator_MineAll designator_MineAll = new Designator_MineAll();
            if (Current.Game.Rules.DesignatorAllowed(designator_MineAll))
                __instance.AllDesignators.Add(designator_MineAll);
        }

        private static void Designator_Cancel_CanDesignateThingPostfix(Thing t, ref AcceptanceReport __result, ref Designator_Cancel __instance)
        {
            if (__result.Accepted) return;
            if (t.def.mineable && __instance.Map.designationManager.DesignationAt(t.Position, DefDatabase<DesignationDef>.GetNamed("MineAll")) != null)
                __result = true;
        }

        private static void Designator_Cancel_DesignateThingPostfix(Thing t, ref Designator_Cancel __instance)
        {
            if (t.def.mineable)
            {
                DesignationManager designationManager = __instance.Map.designationManager;
                Designation designation = designationManager.DesignationAt(t.Position, DefDatabase<DesignationDef>.GetNamed("MineAll"));
                if (designation != null)
                    designationManager.RemoveDesignation(designation);
            }
        }
    }
}
