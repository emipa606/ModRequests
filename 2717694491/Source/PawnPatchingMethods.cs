using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace CustomSchedules
{
    [HarmonyPatch(typeof(Pawn), "TickRare")]
    public class Pawn_TickRare_Patch
    {
        private static CustomSchedulesSaveData saveData;

        public static void Postfix(Pawn __instance)
        {
            //Log.Message(__instance != null ? __instance.Name + " Is Mech Player Controlled?:" + __instance.IsColonyMechPlayerControlled + " || Is Colonist Controlled?:" + __instance.IsColonistPlayerControlled + " || PlayerSettings?" + (__instance.playerSettings != null) + " || Timetable?:" + (__instance.timetable != null) : "");
            //Make sure its a valid pawn
            if (__instance == null || !__instance.IsColonistPlayerControlled || __instance.Drafted ||
                __instance.timetable == null || __instance.playerSettings == null)
                return;

            //Get saved data.
            if (saveData == null)
                saveData = __instance.Map.GetComponent<CustomSchedulesSaveData>();

            //Is auto assignment enabled?
            if (!saveData.PawnScheduleEnabled(__instance))
                return;

            string curAssignment = __instance.timetable.CurrentAssignment.label;

            //Find an Area for a specific Schedule and Pawn name
            Area curArea = TryGetAutoAreaMultipleModifiers(__instance, curAssignment, __instance.Name.ToStringShort);

            //Find an Area for a specific Schedule and "job title"
            if ((curArea == null || curArea.TrueCount == 0) && __instance.story.title != null)
                curArea = TryGetAutoAreaMultipleModifiers(__instance, curAssignment, __instance.story.title);

            //Find an Area for a specific Schedule
            if (curArea == null || curArea.TrueCount == 0)
                curArea = __instance.Map.areaManager.AllAreas.FirstOrDefault(area => area.Label == curAssignment);

            //If we couldn't find an area, set the last manual area as the current assigned area, or assign the new area.
            if (curArea == null || curArea.TrueCount == 0)
                saveData.AssignLastManualAreaForPawn(__instance);
            else if (__instance.playerSettings.AreaRestriction != curArea)
                saveData.SetAssignment(__instance, curArea);
        }

        private static Area TryGetAutoAreaMultipleModifiers(Pawn pawn, string assignment, string modifier = "")
        {
            List<Area> areaList = pawn.Map.areaManager.AllAreas;
            List<string> areaNameWords = new List<string>();

            for (int i = 0; i < areaList.Count; i++)
            {
                areaNameWords.Clear();
                areaNameWords.AddRange(areaList[i].Label.Split(';'));

                if (areaNameWords[0] == assignment && areaNameWords.Count > 1)
                {
                    string modifierWord = areaNameWords.FirstOrDefault(word => word == modifier);

                    if (modifierWord != null) 
                        return areaList[i];
                }
            }

            return null;
        }
    }

    [HarmonyPatch(typeof(ThinkNode_Priority_GetJoy), "GetPriority")]
    public static class ThinkNode_Priority_GetJoy_Patch
    {
        public static bool Prefix(ref float __result, Pawn pawn)
        {
            float result = -1;

            if (pawn.needs.joy == null || Find.TickManager.TicksGame < 5000 || JoyUtility.LordPreventsGettingJoy(pawn))
                result = 0.0f;

            if (result == -1)
            {
                TimeAssignmentDef timeAssignmentDef = ((pawn.timetable == null) ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment);
                float curLevel = pawn.needs.joy.CurLevel;

                if (timeAssignmentDef.allowJoy)
                {
                    if (timeAssignmentDef == CSDefOf.CS_SchedA)
                        result = curLevel < 0.349999994039536 ? 6f : 0.0f;
                    else if (timeAssignmentDef == CSDefOf.CS_SchedB)
                        result = curLevel < 0.349999994039536 ? 6f : 0.0f;
                    else if (timeAssignmentDef == CSDefOf.CS_SchedC)
                        result = curLevel < 0.349999994039536 ? 6f : 0.0f;
                    else if (timeAssignmentDef == CSDefOf.CS_SchedD)
                        result = curLevel < 0.349999994039536 ? 6f : 0.0f;
                    else if (timeAssignmentDef == CSDefOf.CS_SchedE)
                        result = curLevel < 0.349999994039536 ? 6f : 0.0f;
                    else if (timeAssignmentDef == CSDefOf.CS_SchedF)
                        result = curLevel < 0.349999994039536 ? 6f : 0.0f;
                }
                else
                    result = 0;
            }

            if (result == -1)
                return true;

            __result = result;
            return false;
        }
    }

    [HarmonyPatch(typeof(JobGiver_GetRest), "GetPriority")]
    public static class JobGiver_GetRest_Patch
    {
        public static bool Prefix(ref float __result, Pawn pawn)
        {
            float result = -1;

            Need_Rest rest = pawn.needs.rest;
            if (rest == null || rest.CurCategory < RestCategory.Rested || rest.CurLevel > 1 || Find.TickManager.TicksGame < pawn.mindState.canSleepTick)
                result = 0.0f;

            //Log.Message("CurCategory: " + rest.CurCategory + " || CurLevel: " + rest.CurLevel);

            Lord lord = pawn.GetLord();

            if (lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds || !RestUtility.CanFallAsleep(pawn))
                result = 0.0f;

            if (result == -1)
            {
                TimeAssignmentDef timeAssignmentDef;
                if (pawn.RaceProps.Humanlike)
                {
                    timeAssignmentDef = pawn.timetable == null ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment;
                }
                else
                {
                    int num = GenLocalDate.HourOfDay((Thing)pawn);
                    timeAssignmentDef = num < 7 || num > 21 ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Anything;
                }

                float restCurLevel = rest.CurLevel;

                if (timeAssignmentDef == CSDefOf.CS_SchedA)
                    result = restCurLevel < 0.300000011920929 ? 8f : 0.0f;
                else if (timeAssignmentDef == CSDefOf.CS_SchedB)
                    result = restCurLevel < 0.300000011920929 ? 8f : 0.0f;
                else if (timeAssignmentDef == CSDefOf.CS_SchedC)
                    result = restCurLevel < 0.300000011920929 ? 8f : 0.0f;
                else if (timeAssignmentDef == CSDefOf.CS_SchedD)
                    result = restCurLevel < 0.300000011920929 ? 8f : 0.0f;
                else if (timeAssignmentDef == CSDefOf.CS_SchedE)
                    result = restCurLevel < 0.300000011920929 ? 8f : 0.0f;
                else if (timeAssignmentDef == CSDefOf.CS_SchedF)
                    result = restCurLevel < 0.300000011920929 ? 8f : 0.0f;
            }

            if (result == -1)
                return true;

            __result = result;
            return false;
        }
    }

    [HarmonyPatch(typeof(JobGiver_Work), "GetPriority")]
    public static class JobGiver_Work_Patch
    {
        public static bool Prefix(ref float __result, Pawn pawn)
        {
            float result = -1;

            if (pawn.workSettings == null || !pawn.workSettings.EverWork)
                result = 0.0f;

            if (result == -1)
            {
                TimeAssignmentDef timeAssignmentDef = ((pawn.timetable == null) ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment);

                if (timeAssignmentDef == CSDefOf.CS_SchedA)
                    result = 5.5f;
                else if (timeAssignmentDef == CSDefOf.CS_SchedB)
                    result = 5.5f;
                else if (timeAssignmentDef == CSDefOf.CS_SchedC)
                    result = 5.5f;
                else if (timeAssignmentDef == CSDefOf.CS_SchedD)
                    result = 5.5f;
                else if (timeAssignmentDef == CSDefOf.CS_SchedE)
                    result = 5.5f;
                else if (timeAssignmentDef == CSDefOf.CS_SchedF)
                    result = 5.5f;
            }

            if (result == -1)
                return true;

            __result = result;
            return false;
        }
    }
}