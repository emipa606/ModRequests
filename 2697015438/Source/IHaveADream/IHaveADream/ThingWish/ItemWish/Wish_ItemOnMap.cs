using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace HDream
{
    public class Wish_ItemOnMap : Wish_Item
    {

        protected override int CountMatch()
        {
            int count = 0;
            if (Def.roomRole != null)
            {
                List<Room> rooms = pawn.Map.regionGrid.allRooms.FindAll(room => room.Role == Def.roomRole && (!Def.shoulBeRoomOwner || room.Owners.Contains(pawn)));
                for (int i = 0; i < rooms.Count; i++)
                {
                    for (int j = 0; j < ThingsWanted.Count; j++)
                    {
                        count += AdjustForSpecifiedCount(ThingMatching(rooms[i].ContainedThings(ThingsWanted[j].def).ToList(), ThingsWanted[j]), ThingsWanted[j].needAmount);
                    }
                }
                return count;
            }
            else
            {
                ListerThings lister = pawn.Map.listerThings;
                for (int i = 0; i < ThingsWanted.Count; i++)
                {
                    count += AdjustForSpecifiedCount(ThingMatching(lister.ThingsOfDef(GetThingDef(ThingsWanted[i])), ThingsWanted[i]), ThingsWanted[i].needAmount);
                }
            }
            return count;
        }
    }
}
