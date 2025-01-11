﻿using System.Collections.Generic;
using Verse;

namespace HumanResources
{
    public class RoomRoleWorker_Library : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            int num = 0;
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            for (int i = 0; i < containedAndAdjacentThings.Count; i++)
            {
                if (containedAndAdjacentThings[i] is Building_BookStore)
                {
                    num++;
                }
            }
            return 13.5f * (float)num;
        }
    }
}
