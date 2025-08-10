using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MultiFloorStorage
{
    public class Building_LinkedStorage : Building_Storage
    {
        public Building_LinkedStorage LinkedMultiMapOutputShelf;
        public Map LinkedMap; // Reference to the linked shelf's map
        public bool AutoTransferEnabled = true;
        private const int DefaultTransferInterval = 60; // 1 second
        public int ScheduledTransferInterval = DefaultTransferInterval;
        public bool IsLinked => LinkedMultiMapOutputShelf != null && (LinkedMap == this.Map || LinkedMultiMapOutputShelf.Spawned);
        private CompPowerTrader powerComp;
        private bool previouslyFailedToTransfer = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref LinkedMultiMapOutputShelf, "LinkedMultiMapOutputShelf");
            Scribe_References.Look(ref LinkedMap, "LinkedMap");
            Scribe_Values.Look(ref AutoTransferEnabled, "AutoTransferEnabled", true);
            Scribe_Values.Look(ref ScheduledTransferInterval, "ScheduledTransferInterval", DefaultTransferInterval);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            if (IsLinked && LinkedMultiMapOutputShelf != null && LinkedMap != null)
            {
                if (LinkedMap == this.Map)
                {
                    // Draw a line between shelves on the same map
                    GenDraw.DrawLineBetween(this.DrawPos, LinkedMultiMapOutputShelf.DrawPos, SimpleColor.Green);
                }
                else
                {
                    // Get the world positions of the map tiles
                    Vector3 inputTilePos = Find.WorldGrid.GetTileCenter(this.Map.Tile);
                    Vector3 outputTilePos = Find.WorldGrid.GetTileCenter(LinkedMap.Tile);

                    // Draw a world line between the map tiles
                    GenDraw.DrawWorldLineBetween(inputTilePos, outputTilePos, 0.1f);
                }
            }
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            if (this.def.defName == "MultiMapInputShelf")
            {


                // Command to view the linked storage shelf
                yield return new Command_Action
                {
                    defaultLabel = "View linked imports shelf",
                    defaultDesc = "Focus the camera on the linked imports shelf.",
                    icon = TexCommandViewLinkedStorage.ViewLinkedStorage, // Use an appropriate icon
                    action = delegate
                    {
                        if (LinkedMultiMapOutputShelf != null)
                        {
                            CameraJumper.TryJump(LinkedMultiMapOutputShelf);
                        }
                        else
                        {
                            Messages.Message("No linked storage shelf to view.", MessageTypeDefOf.RejectInput);
                        }
                    }
                };
            }

            if (this.def.defName == "MultiMapInputShelf")
            {
                yield return new Command_Action
                {
                    defaultLabel = "Link imports shelf",
                    defaultDesc = "Assign an imports shelf on a different map tile to automatically transfer items from this exports shelf. Click this button to select the map and the imports shelf to link.",
                    icon = TexCommand.Attack,
                    action = delegate
                    {
                        var allMaps = Find.Maps.Where(m => m != this.Map).ToList(); // Exclude current map
                        if (allMaps.Count == 0)
                        {
                            Messages.Message("No other map to switch to.", MessageTypeDefOf.RejectInput);
                            return;
                        }

                        // Open a dialog to choose a map
                        Find.WindowStack.Add(new Dialog_MapSelector(allMaps, selectedMap =>
                        {
                            if (selectedMap != null)
                            {
                                Current.Game.CurrentMap = selectedMap;

                                // Start targeting for linking the receiver shelf
                                Find.Targeter.BeginTargeting(new TargetingParameters
                                {
                                    canTargetBuildings = true,
                                    canTargetSelf = false,
                                    validator = target =>
                                        target.Thing is Building_LinkedStorage shelf &&
                                        shelf.def.defName == "MultiMapOutputShelf"
                                }, target =>
                                {
                                    var multiMapOutputShelf = target.Thing as Building_LinkedStorage;
                                    if (multiMapOutputShelf != null)
                                    {
                                        LinkedMultiMapOutputShelf = multiMapOutputShelf;
                                        LinkedMap = multiMapOutputShelf.Map;
                                        Messages.Message($"Linked to {multiMapOutputShelf.LabelCap} on map {multiMapOutputShelf.Map?.Tile ?? -1}.", MessageTypeDefOf.PositiveEvent);

                                        // Optionally switch back to the original map
                                        Current.Game.CurrentMap = this.Map;
                                    }
                                    else
                                    {
                                        Messages.Message("Failed to link to shelf.", MessageTypeDefOf.RejectInput);
                                    }
                                });
                            }
                        }));
                    }
                };



                yield return new Command_Toggle
                {
                    defaultLabel = "Toggle auto-transfer",
                    defaultDesc = "Toggle automatic item transfer from the exports shelf to the imports shelf.",
                    icon = TexCommand.ForbidOff,
                    isActive = () => AutoTransferEnabled,
                    toggleAction = () =>
                    {
                        AutoTransferEnabled = !AutoTransferEnabled;
                        Messages.Message(
                            AutoTransferEnabled
                                ? "Automatic item transfer enabled."
                                : "Automatic item transfer disabled.",
                            MessageTypeDefOf.PositiveEvent
                        );
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "Unlink imports shelf",
                    defaultDesc = "Unlink the currently assigned imports shelf.",
                    icon = TexCommand.ClearPrioritizedWork,
                    action = delegate
                    {
                        if (LinkedMultiMapOutputShelf != null)
                        {
                            Messages.Message(
                                $"Unlinked from {LinkedMultiMapOutputShelf.LabelCap}.",
                                MessageTypeDefOf.PositiveEvent
                            );
                            LinkedMultiMapOutputShelf = null;
                            LinkedMap = null;
                        }
                        else
                        {
                            Messages.Message(
                                "No linked imports shelf to unlink.",
                                MessageTypeDefOf.RejectInput
                            );
                        }
                    }
                };
            }
        }

        public void TransferItems()
        {
            if (!IsLinked || LinkedMultiMapOutputShelf == null || LinkedMap == null)
                return;

            // Ensure both shelves are powered on
            if ((powerComp != null && !powerComp.PowerOn) ||
                (LinkedMultiMapOutputShelf.powerComp != null && !LinkedMultiMapOutputShelf.powerComp.PowerOn))
            {
                return;
            }

            if (LinkedMap != this.Map)
            {
                // Handle cross-map transfer
                var inputSlotGroup = this.GetSlotGroup();
                if (inputSlotGroup == null || !inputSlotGroup.HeldThings.Any())
                    return;

                foreach (var item in inputSlotGroup.HeldThings.ToList())
                {
                    if (!LinkedMultiMapOutputShelf.GetStoreSettings().AllowedToAccept(item))
                        continue;

                    if (LinkedMultiMapOutputShelf.GetSlotGroup().HeldThings.Count() >= 3) // Adjust stack limit
                    {
                        if (!previouslyFailedToTransfer)
                        {
                            Messages.Message($"", MessageTypeDefOf.RejectInput);
                            previouslyFailedToTransfer = true;
                        }
                        continue;
                    }

                    previouslyFailedToTransfer = false; // Reset the flag when a successful transfer occurs

                    var newItem = item.SplitOff(item.stackCount);
                    IntVec3 dropCell = LinkedMultiMapOutputShelf.Position;

                    if (!GenPlace.TryPlaceThing(newItem, dropCell, LinkedMap, ThingPlaceMode.Near))
                    {
                        // If placement fails, revert the split
                        item.stackCount += newItem.stackCount;
                        Messages.Message($"Transfer failed: Could not place {newItem.LabelCap} on map {LinkedMap.Tile}.", MessageTypeDefOf.RejectInput);
                    }
                }
            }
            else
            {
                // Same-map transfer
                var inputSlotGroup = this.GetSlotGroup();
                var outputSlotGroup = LinkedMultiMapOutputShelf.GetSlotGroup();
                if (inputSlotGroup == null || !inputSlotGroup.HeldThings.Any() || outputSlotGroup == null)
                    return;

                foreach (var item in inputSlotGroup.HeldThings.ToList())
                {
                    if (LinkedMultiMapOutputShelf.GetStoreSettings().AllowedToAccept(item))
                    {
                        var movedItem = item.SplitOff(Mathf.Min(item.stackCount, item.def.stackLimit));
                        if (!GenPlace.TryPlaceThing(movedItem, LinkedMultiMapOutputShelf.Position, Map, ThingPlaceMode.Direct))
                        {
                            item.stackCount += movedItem.stackCount; // Revert if placement failed
                        }
                    }
                }
            }
        }


        public override void Tick()
        {
            base.Tick();

            // Ensure this shelf is powered on
            if (powerComp != null && !powerComp.PowerOn)
                return;

            if (AutoTransferEnabled && IsLinked && Find.TickManager.TicksGame % ScheduledTransferInterval == 0)
            {
                TransferItems();
            }
        }

    }

    public class Dialog_MapSelector : Window
    {
        private List<Map> maps;
        private Action<Map> onMapSelected;

        public Dialog_MapSelector(List<Map> maps, Action<Map> onMapSelected)
        {
            this.maps = maps;
            this.onMapSelected = onMapSelected;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(400f, 300f);

        public override void DoWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            foreach (var map in maps)
            {
                if (listing.ButtonText($"Map: {map.Tile} ({map.info.parent.LabelCap})"))
                {
                    onMapSelected?.Invoke(map);
                    Close();
                    return;
                }
            }
            if (listing.ButtonText("Cancel"))
            {
                onMapSelected?.Invoke(null);
                Close();
            }
            listing.End();
        }
    }

}


