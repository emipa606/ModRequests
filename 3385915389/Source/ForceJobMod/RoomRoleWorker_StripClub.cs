using System.Collections.Generic;
using Verse;

namespace ForceJobMod
{
    public class RoomRoleWorker_StripClub : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            // Initialize a counter for StrippingTableTable objects
            int numStrippingTables = 0;

            // Get all things in and around the room
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;

            // Iterate through all items in the room
            for (int i = 0; i < containedAndAdjacentThings.Count; i++)
            {
                Thing thing = containedAndAdjacentThings[i];
                // Check if the thing matches the required item definition
                if (thing.def.defName == "StrippingTableTable")
                {
                    numStrippingTables++;
                }
            }

            // Return a score based on the presence of StrippingTableTable
            return numStrippingTables > 0 ? numStrippingTables * 10f : 0f;
        }
    }
}
