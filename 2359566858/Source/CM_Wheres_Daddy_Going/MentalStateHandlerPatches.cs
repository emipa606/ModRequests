using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Wheres_Daddy_Going
{
    [StaticConstructorOnStartup]
    public static class MentalStateHandlerPatches
    {
        //TaggedString label, TaggedString text, LetterDef textLetterDef, LookTargets lookTargets, Faction relatedFaction = null, Quest quest = null, List<ThingDef> hyperlinkThingDefs = null, string debugInfo = null
        [HarmonyPatch(typeof(MentalStateHandler))]
        [HarmonyPatch("ClearMentalStateDirect", MethodType.Normal)]
        public static class MentalStateHandler_ClearMentalStateDirect
        {
            [HarmonyPrefix]
            public static void Prefix(MentalStateHandler __instance)
            {
                Logger.MessageFormat(__instance, "Calling ClearMentalStateDirect Prefix");
                if (__instance.CurStateDef != null && __instance.CurStateDef == WheresDaddyDefOf.CM_Wheres_Daddy_Going_MentalState_OutToGetSomeMilk)
                    (__instance.CurState as MentalState_OutToGetSomeMilk).MindCleared();
            }
        }
    }
}
