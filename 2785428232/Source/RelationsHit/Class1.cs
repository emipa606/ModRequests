using Verse;
using RimWorld;
using HarmonyLib;

namespace RelationsHit
{
    public class NoRelationsHit
    {
        [StaticConstructorOnStartup]
        internal static class HarmonyInit
        {
            static HarmonyInit()
            {
                Harmony harmony = new Harmony("whatamidoing.relationsHit");

                harmony.PatchAll();
            }
        }

        [HarmonyPatch(typeof(Faction), "Notify_MemberDied")]
        class RelationPatch
        {
            static void Prefix(ref bool wasWorldPawn)
            {
                wasWorldPawn = true;
            }
        }
    }
}
