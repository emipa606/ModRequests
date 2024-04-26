using RimWorld;
using Verse;


namespace WallStuff
{
    class Blackboard_Patches
    {

        public static int ConnectedBlackboardsHarmonyPatch(int values, Thing desk)
		{
            int num = 0;
			CompAffectedByFacilities compAffectedByFacilities = desk.TryGetComp<CompAffectedByFacilities>();
			if (compAffectedByFacilities != null)
			{
				foreach (Thing item in compAffectedByFacilities.LinkedFacilitiesListForReading)
					{
						if (item.def == RimWorld.ThingDefOf.Blackboard || item.def.defName == "WallMountedBlackboard")
						{
							num++;
							if (num >= 3)
							{
								return num;
							}
					}
				}
				return num;
			}
			return num;
		}
    }
}
