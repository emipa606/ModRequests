using System.Collections.Generic;
using Verse;

namespace KriilMod_CD
{
	public class RoomRoleWorker_CombatTrainingRoom : RoomRoleWorker
	{
		public override float GetScore(Room room)
		{
			int num = 0;
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				Thing thing = containedAndAdjacentThings[i];
				Building_CombatDummy building_CombatDummy = thing as Building_CombatDummy;
				if (building_CombatDummy != null)
				{
					num++;
				}
			}
			return (float)num * 5f;
		}
	}
}
