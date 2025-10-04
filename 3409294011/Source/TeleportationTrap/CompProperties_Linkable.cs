using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using System.Linq;

namespace TeleportationTrap
{
    public class Comp_Linkable : ThingComp
    {
        private List<Thing> linkedThings = new List<Thing>();
        public IReadOnlyList<Thing> LinkedThings => linkedThings.AsReadOnly();

        public CompProperties_Linkable Props => (CompProperties_Linkable)props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref linkedThings, "linkedThings", LookMode.Reference);
        }

        public void ValidateLinkedObjects()
        {
            for (int i = linkedThings.Count - 1; i >= 0; i--)
            {
                var linkedThing = linkedThings[i];
                if (linkedThing == null || linkedThing.Destroyed || linkedThing.Map != parent.Map)
                {
                    // Unlink if the linked object is invalid
                    UnlinkFrom(linkedThing);
                }
            }
        }


        public void DrawLinks()
        {
            foreach (var linkedThing in linkedThings)
            {
                if (linkedThing.Position.IsValid && parent.Map == linkedThing.Map)
                {
                    // Draw a green line between linked objects
                    GenDraw.DrawLineBetween(parent.DrawPos, linkedThing.DrawPos, SimpleColor.Green);
                }
            }
        }







        public void LinkTo(Thing target)
        {
            if (target == null || linkedThings.Contains(target) || !IsLinkValid(target)) return;

            // Unlink from all currently linked objects
            foreach (var linkedThing in linkedThings.ToList())
            {
                UnlinkFrom(linkedThing);
            }

            // Establish the new link
            linkedThings.Add(target);
            if (Props.biDirectional)
            {
                var targetComp = target.TryGetComp<Comp_Linkable>();
                targetComp?.LinkTo(parent);
            }
        }


        public void UnlinkFrom(Thing target)
        {
            if (!linkedThings.Contains(target)) return;

            linkedThings.Remove(target);
            if (Props.biDirectional)
            {
                var targetComp = target.TryGetComp<Comp_Linkable>();
                targetComp?.UnlinkFrom(parent);
            }
        }

        public bool IsLinkValid(Thing target)
        {
            // Check distance
            if (parent.Position.DistanceTo(target.Position) > Props.maxLinkDistance)
            {
                return false;
            }

            // Check if the target's ThingDef is allowed
            if (Props.allowedLinkableDefs != null && Props.allowedLinkableDefs.Count > 0)
            {
                if (!Props.allowedLinkableDefs.Contains(target.def.defName))
                {
                    return false;
                }
            }

            return true;
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            if (parent is MinifiedThing)
            {
                // Clear links as the object is now a minified state
                linkedThings.Clear();
            }
        }

        public void DrawUnlinkedOverlays()
        {
            // Ensure the parent map is valid
            if (parent?.Map == null) return;

            // Check if the selected object is a TeleporterTrapSpike
            if (Find.Selector.SelectedObjects.OfType<Building>().Any(selected =>
                    selected.def.defName == "TeleporterTrapSpike"))
            {
                // Get all TeleporterTrapSpike buildings on the map
                var allLinkableThings = parent.Map.listerThings.AllThings
                    .Where(t => t.def.defName == "TeleporterTrapSpike" && t.TryGetComp<Comp_Linkable>() != null);

                // Iterate through all linkable things and draw overlay for unlinked ones
                foreach (var thing in allLinkableThings)
                {
                    var comp = thing.TryGetComp<Comp_Linkable>();
                    if (comp != null && !comp.LinkedThings.Any()) // Only unlinked buildings
                    {
                        // Draw the static question mark overlay
                        parent.Map.overlayDrawer.DrawOverlay(thing, OverlayTypes.QuestionMark);
                    }
                }
            }
        }



        public override void PostDraw()
        {
            base.PostDraw();
            DrawUnlinkedOverlays();
        }




        public override string CompInspectStringExtra()
        {
            if (!Props.showLinkedObjects) return null;

            // Prevent inspection of a MinifiedThing
            else if (parent is MinifiedThing)
            {
                return null;
            }

            // Basic linked object info
            string linkedInfo = linkedThings.Count > 0
                ? "Linked to: " + string.Join(", ", linkedThings.Select(t => t.LabelShortCap))
                : "Linked to: <color=red>None.</color> Without a linked teleportation trap, this will function as a single-use trap and lose its teleportation ability. To preserve its functionality, either link it to another trap or uninstall it.";

            if (linkedThings.Count == 0)
            {

                // Show overlay and warning
                if (Props.drawOutOfFuelOverlay)
                {
                    parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.QuestionMark);


                }

            }

            // Distance info for the first linked object
            string distanceInfo = "";
            if (Props.showLinkDistance && linkedThings.Count > 0)
            {
                var firstLinkedThing = linkedThings[0];
                if (firstLinkedThing.Position.IsValid)
                {
                    distanceInfo = $" | Distance: {firstLinkedThing.Position.DistanceTo(parent.Position):F1}";
                }
            }

            // Room items info for the first linked object
            string roomInfo = "";
            if (Props.showRoomItemsInfo && linkedThings.Count > 0)
            {
                var firstLinkedThing = linkedThings[0];
                var room = firstLinkedThing.Position.GetRoom(parent.Map);
                if (room != null)
                {
                    var roomThings = room.ContainedAndAdjacentThings;
                    if (roomThings != null && roomThings.Count > 0)
                    {
                        roomInfo = "\nItems in linked room: " +
                                   string.Join(", ", roomThings.Take(5).Select(t => t.LabelShortCap)) +
                                   (roomThings.Count > 5 ? ", ..." : ""); // Limit to 5 items for readability
                    }
                }
            }


            return linkedInfo + distanceInfo + roomInfo;
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (parent.Faction == Faction.OfPlayer) // Ensure only player-owned objects show the Gizmo
            {
                // Link Gizmo
                yield return new Command_Target
                {
                    defaultLabel = "Link trap",
                    defaultDesc = "Link this object to another compatible teleportation trap.",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/LinkLinkables", true),
                    action = target =>
                    {
                        if (target.Thing != null && target.Thing.TryGetComp<Comp_Linkable>() != null)
                        {
                            var comp = target.Thing.TryGetComp<Comp_Linkable>();
                            if (comp != null && comp.IsLinkValid(parent))
                            {
                                LinkTo(target.Thing);
                                Messages.Message("Objects linked successfully.", MessageTypeDefOf.PositiveEvent, false);
                            }
                            else
                            {
                                Messages.Message("Cannot link to this object.", MessageTypeDefOf.RejectInput, false);
                            }
                        }
                    },
                    targetingParams = new TargetingParameters
                    {
                        canTargetBuildings = true,
                        canTargetItems = false,
                        canTargetPawns = false,
                        validator = target =>
                        {
                            if (target.Thing == null) return false;

                            var comp = target.Thing.TryGetComp<Comp_Linkable>();
                            return comp != null && comp.IsLinkValid(parent);
                        }
                    }

                };

                // Unlink Gizmo
                yield return new Command_Action
                {
                    defaultLabel = "Unlink trap",
                    defaultDesc = "Unlink this teleportation trap connection.",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire", true),
                    action = () =>
                    {
                        if (linkedThings != null && linkedThings.Count > 0)
                        {
                            foreach (var linkedThing in linkedThings.ToList())
                            {
                                UnlinkFrom(linkedThing);
                            }
                            Messages.Message("All links removed successfully.", MessageTypeDefOf.PositiveEvent, false);
                        }
                        else
                        {
                            Messages.Message("No links to remove.", MessageTypeDefOf.RejectInput, false);
                        }
                    }
                };


                // Teleport Gizmo
                if (Props.enableTeleport)
                {
                    yield return new Command_Target
                    {
                        defaultLabel = "Teleport",
                        defaultDesc = "Teleport a pawn to the other linked object.",
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/MergeCaravans", true),
                        action = target =>
                        {
                            if (target.Thing is Pawn targetPawn)
                            {
                                // Ensure the target is a player-owned colonist or animal
                                if (Props.onlyColonists &&
                                    (targetPawn.Faction != Faction.OfPlayer ||
                                     (!targetPawn.RaceProps.Humanlike && !targetPawn.RaceProps.Animal)))
                                {
                                    Messages.Message("Only player-owned colonists and pets can be teleported.", MessageTypeDefOf.RejectInput, false);
                                    return;
                                }

                                if (linkedThings.Count > 0)
                                {
                                    // Get the current linked object
                                    var currentLinked = linkedThings.FirstOrDefault();
                                    if (currentLinked != null)
                                    {
                                        Job job = JobMaker.MakeJob(JobDefOf.TeleportPawn, currentLinked);
                                        targetPawn.jobs.TryTakeOrderedJob(job);
                                        Messages.Message($"{targetPawn.LabelShort} is teleporting to {currentLinked.LabelShort}.", MessageTypeDefOf.PositiveEvent, false);
                                    }
                                    else
                                    {
                                        Messages.Message("No valid linked object found.", MessageTypeDefOf.RejectInput, false);
                                    }
                                }
                                else
                                {
                                    Messages.Message("This object is not linked to any other object.", MessageTypeDefOf.RejectInput, false);
                                }
                            }
                            else
                            {
                                Messages.Message("Please select a valid pawn.", MessageTypeDefOf.RejectInput, false);
                            }
                        },
                        targetingParams = new TargetingParameters
                        {
                            canTargetPawns = true,
                            validator = target =>
                            {
                                if (target.Thing is Pawn targetPawn)
                                {
                                    return Props.onlyColonists &&
                                           (targetPawn.Faction == Faction.OfPlayer &&
                                            (targetPawn.RaceProps.Humanlike || targetPawn.RaceProps.Animal));
                                }
                                return false;
                            }
                        }
                    };

                }





            }
        }


    }



    public class CompProperties_Linkable : CompProperties
    {
        public float maxLinkDistance = 25.0f;
        public bool biDirectional = true;
        public bool allowMultipleLinks = true;
        public bool showRoomItemsInfo = false;
        public bool onlyColonists = false;
        public List<string> allowedLinkableDefs;

        // Visibility toggles
        public bool showLinkDistance = true;
        public bool showLinkedObjects = true;

        // Enable teleport toggle
        public bool enableTeleport = false;
        public bool drawOutOfFuelOverlay = true;

        public CompProperties_Linkable()
        {
            this.compClass = typeof(Comp_Linkable);
        }
    }
}