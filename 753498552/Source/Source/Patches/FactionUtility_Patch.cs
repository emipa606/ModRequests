using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality.Patches;

/// <summary>
///     Prevent guests from using joy items that don't belong to them
/// </summary>
public class FactionUtility_Patch
{
    [HarmonyPatch(typeof(FactionUtility), nameof(FactionUtility.IsPoliticallyProper))]
    public class IsPoliticallyProper
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, Thing thing, Pawn pawn)
        {
            if (!__result) return;

            // Is guest
            if (!pawn.IsGuest()) return;

            // It's an item
            if (thing.def.category == ThingCategory.Item)
            {
                // Only things we already have
                if (thing.ParentHolder == pawn.inventory) return;

                // Except for books
                if (thing is Book) return;

                // Items of other pawns, in shelves or on the ground are off limits
                __result = false;
            }
        }
    }
}