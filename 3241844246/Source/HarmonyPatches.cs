using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Reflection;
using Verse;

namespace RestockNotification {
    [StaticConstructorOnStartup]
    public class HarmonyPatches {
        static HarmonyPatches() {
            Harmony harmony = new Harmony("bluebird.tammybee.restocknotification");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Settlement_TraderTracker))]
    [HarmonyPatch("TryDestroyStock")]
    class Settlement_TraderTracker_TryDestroyStock_Patch {
        const int TickDay = 60000;

        [HarmonyPrefix]
        static void AlertRestock(Settlement_TraderTracker __instance) {
            Traverse trv = Traverse.Create(__instance);
            int lastStockGenerationTicks = trv.Field("lastStockGenerationTicks").GetValue<int>();
            int regenerateStockEveryDays = trv.Property("RegenerateStockEveryDays").GetValue<int>();

            bool isAfterRestockDays = Find.TickManager.TicksGame - lastStockGenerationTicks > regenerateStockEveryDays * TickDay;
            if (!isAfterRestockDays) return;

            ThingOwner<Thing> stock = trv.Field("stock").GetValue<ThingOwner<Thing>>();
            if (stock == null) return;

            string label = "RestockNotification.RestockLetterLabel".Translate();
            string text = "RestockNotification.RestockLetterDesc".Translate(__instance.settlement.Named("SETTLEMENT"));
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, __instance.settlement);
        }
    }
}
