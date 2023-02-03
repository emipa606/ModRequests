using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace LWM.PrayerSpot
{
    public class RoomRoleWorker_Chapel : RoomRoleWorker {
        public override float GetScore(Room room) {
			int num = 0;
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++) {
				Thing thing = containedAndAdjacentThings[i];
                if (thing is Building_Bed) return 0f; // prayer spots in rooms are private *bedroom* spots
                if (thing.def.defName=="LWM_PrayerSpot" || thing.def.defName=="LWM_PrayerSpot_Dir") {
                    num++;
                } else if (thing.def.defName=="PrayerFocus") {
                    num+=6;
                }
            }
            return (float)num * 4f; // So you can have small prayer spots in rooms?
        }
    }
}
