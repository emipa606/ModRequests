using HarmonyLib;
using RimWorld;
using Verse;

namespace DrugScheduleWhileWorking.Patches;

//This should officially be a transpiler, but given that I've seen multiple mods mess with drug schedule related functions, I opted to make this a postfix instead.
//Repeat calculation of ShouldTryToTakeScheduledNow was no concern, neglagable impact on performance.
[HarmonyPatch(typeof(JobGiver_TakeDrugsForDrugPolicy), "GetPriority")]
public static class JobGiver_TakeDrugsForDrugPolicy_GetPriority_Patch
{
    static void Postfix(ref float __result, Pawn pawn)
    {
        DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
        for (int i = 0; i < currentPolicy.Count; i++)
        {
            if (pawn.drugs.ShouldTryToTakeScheduledNow(currentPolicy[i].drug) && DrugScheduleWhileWorkingSettings.drugsEnabledDuringWorkSettingsDictionary[currentPolicy[i].drug.defName].drugEnabledBool)
            {
                //9.1f chosen as work and meditation are both at prio 9.
                __result = 9.1f;
            }
        }
    }
}