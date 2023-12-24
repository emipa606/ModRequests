using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace NoMoreRiceWorld
{

    [HarmonyPatch(typeof(Toils_Ingest))]
    [HarmonyPatch(nameof(Toils_Ingest.FinalizeIngest))]
    static class ToilsIngestFinalizeIngestPatch
    {
        static void FinishAction()
        {
            Log.Message("Eating completed");
        }

        static void Postfix(ref Toil result)
        {
            result.AddFinishAction(FinishAction);
        }
    }

    [StaticConstructorOnStartup]
    public static class NoMoreRiceWorld
    {
        static NoMoreRiceWorld()
        {
            var harmony = new Harmony("com.voidfirefly.nomorericeworld");
            harmony.PatchAll();
            Log.Message("Patching Eating");
        }
    }
}