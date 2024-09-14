// Patch2.cs
using HarmonyLib;
using RimWorld;
using Verse;

[HarmonyPatch(typeof(WorkGiver_OperateScanner), "ShouldSkip")]
internal class Patch2
{
    private static void Postfix(ref bool __result, Pawn pawn, bool forced)
    {
        if (pawn.IsColonyMech)
        {
            __result = true;
        }
    }
}
