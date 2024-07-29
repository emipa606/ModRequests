using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;

namespace AssignedWorkbenches
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            Log.Message("Hello from Harmony in scope: com.github.harmony.rimworld.maarx.assignedworkbenches");
            var harmony = new Harmony("com.github.harmony.rimworld.maarx.assignedworkbenches");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    //RimWorld.WorkGiver_DoBill
    //public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
    [HarmonyPatch(typeof(WorkGiver_DoBill))]
    [HarmonyPatch("JobOnThing")]
    class Patch_WorkGiver_DoBill_JobOnThing
    {
        static bool Prefix(Pawn pawn, Thing thing, bool forced)
        {
            return AssignedWorkbenchesComp.AllowedToWorkBench(pawn, thing);
        }
    }

    //RimWorld.WorkGiver_CreateXenogerm
    //public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    [HarmonyPatch(typeof(WorkGiver_CreateXenogerm))]
    [HarmonyPatch("JobOnThing")]
    class Patch_WorkGiver_CreateXenogerm_JobOnThing
    {
        static bool Prefix(Pawn pawn, Thing t, bool forced)
        {
            return AssignedWorkbenchesComp.AllowedToWorkBench(pawn, t);
        }
    }

    //RimWorld.WorkGiver_CreateXenogerm
    //public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    [HarmonyPatch(typeof(WorkGiver_CreateXenogerm))]
    [HarmonyPatch("HasJobOnThing")]
    class Patch_WorkGiver_CreateXenogerm_HasJobOnThing
    {
        static bool Prefix(Pawn pawn, Thing t, bool forced)
        {
            return AssignedWorkbenchesComp.AllowedToWorkBench(pawn, t);
        }
    }

    //RimWorld.WorkGiver_OperateScanner
    //public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    [HarmonyPatch(typeof(WorkGiver_OperateScanner))]
    [HarmonyPatch("JobOnThing")]
    class Patch_WorkGiver_OperateScanner_JobOnThing
    {
        static bool Prefix(Pawn pawn, Thing t, bool forced)
        {
            return AssignedWorkbenchesComp.AllowedToWorkBench(pawn, t);
        }
    }

    //RimWorld.WorkGiver_OperateScanner
    //public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    [HarmonyPatch(typeof(WorkGiver_OperateScanner))]
    [HarmonyPatch("HasJobOnThing")]
    class Patch_WorkGiver_OperateScanner_HasJobOnThing
    {
        static bool Prefix(Pawn pawn, Thing t, bool forced)
        {
            return AssignedWorkbenchesComp.AllowedToWorkBench(pawn, t);
        }
    }

    //RimWorld.WorkGiver_Researcher
    //public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    [HarmonyPatch(typeof(WorkGiver_Researcher))]
    [HarmonyPatch("JobOnThing")]
    class Patch_WorkGiver_Researcher_JobOnThing
    {
        static bool Prefix(Pawn pawn, Thing t, bool forced)
        {
            return AssignedWorkbenchesComp.AllowedToWorkBench(pawn, t);
        }
    }

    //RimWorld.WorkGiver_Researcher
    //public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    [HarmonyPatch(typeof(WorkGiver_Researcher))]
    [HarmonyPatch("HasJobOnThing")]
    class Patch_WorkGiver_Researcher_HasJobOnThing
    {
        static bool Prefix(Pawn pawn, Thing t, bool forced)
        {
            return AssignedWorkbenchesComp.AllowedToWorkBench(pawn, t);
        }
    }
}
