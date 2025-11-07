using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BillDoorsLootsAndShelves
{
    internal class JoyGiver_ViewDisplay : JoyGiver
    {
        private static List<Thing> candidates = new List<Thing>();

        public override Job TryGiveJob(Pawn pawn)
        {
            bool allowedOutside = JoyUtility.EnjoyableOutsideNow(pawn);
            try
            {
                candidates.AddRange(pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Where(delegate (Thing thing)
                {
                    if (thing.Faction != Faction.OfPlayer || thing.IsForbidden(pawn) || (!allowedOutside && !thing.Position.Roofed(thing.Map)) || !pawn.CanReserveAndReach(thing, PathEndMode.Touch, Danger.None) || !thing.IsPoliticallyProper(pawn))
                    {
                        return false;
                    }
                    Building_Locker display = thing as Building_Locker;
                    if (display == null)
                    {
                        return false;
                    }
                    if (!display.isDisplay || !display.tempStorage.Any)
                    {
                        return false;
                    }
                    Room room = thing.GetRoom();
                    if (room == null)
                    {
                        return true;
                    }

                    return ((room.Role != RoomRoleDefOf.Bedroom && room.Role != RoomRoleDefOf.Barracks && room.Role != RoomRoleDefOf.PrisonCell && room.Role != RoomRoleDefOf.PrisonBarracks && room.Role != RoomRoleDefOf.Hospital) || (pawn.ownership != null && pawn.ownership.OwnedRoom != null && pawn.ownership.OwnedRoom == room)) ? true : false;
                }));

                if (!candidates.TryRandomElementByWeight((Thing target) => Mathf.Max(target.GetStatValue(RimWorld.StatDefOf.Beauty), 0.5f), out var result))
                {
                    return null;
                }

                Thing potentialTargetA = null;

                Thing potentialTargetB = null;

                if (result.GetRoom() != null)
                {
                    List<Thing> things = result.GetRoom()?.ContainedAndAdjacentThings.Intersect(candidates).ToList();
                    things.Remove(result);
                    if (things.Any())
                    {
                        potentialTargetA = things.RandomElementByWeight((Thing target) => Mathf.Max(target.GetStatValue(RimWorld.StatDefOf.Beauty), 0.5f));
                        things.Remove(potentialTargetA);
                    }
                    if (things.Any())
                    {
                        potentialTargetB = things.RandomElementByWeight((Thing target) => Mathf.Max(target.GetStatValue(RimWorld.StatDefOf.Beauty), 0.5f));
                    }
                }

                return JobMaker.MakeJob(def.jobDef, result, potentialTargetA, potentialTargetB);
            }
            finally
            {
                candidates.Clear();
            }
        }
    }
}
