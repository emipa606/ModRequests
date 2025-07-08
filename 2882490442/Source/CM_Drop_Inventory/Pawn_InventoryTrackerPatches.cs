using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace KB_Drop_Inventory
{
    [StaticConstructorOnStartup]
    public static class Pawn_InventoryTrackerPatches
    {
        [HarmonyPatch(typeof(Pawn_InventoryTracker))]
        [HarmonyPatch("GetGizmos", MethodType.Normal)]
        public static class Pawn_InventoryTracker_GetGizmos
        {
            [HarmonyPostfix]
            public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, Pawn_InventoryTracker __instance)
            {
                foreach (Gizmo gizmo in gizmos)
                    yield return gizmo;

                Pawn pawn = __instance.pawn;

                bool playerControlsInventory = (pawn.Faction != null && (pawn.IsColonistPlayerControlled || pawn.IsPrisonerOfColony || (pawn.AnimalOrWildMan() && pawn.Faction.IsPlayer)));
                
                if (playerControlsInventory)
                {
                    bool canDropStuff = (__instance.FirstUnloadableThing != default(ThingCount) && pawn.SpawnedOrAnyParentSpawned);

                    if (canDropStuff)
                    {
                        Command_Action unloadInventoryAction = new Command_Action
                        {
                            action = delegate
                            {
                                __instance.DropAllNearPawn(__instance.pawn.PositionHeld, false);
                            },
                            defaultLabel = "KB_Drop_Inventory_DropLabel".Translate(),
                            defaultDesc = "KB_Drop_Inventory_DropDescription".Translate(),
                            icon = ContentFinder<Texture2D>.Get("UI/Down_Arrow"),
                        };

                        yield return unloadInventoryAction;
                    }
                }
            }
        }
    }
}
