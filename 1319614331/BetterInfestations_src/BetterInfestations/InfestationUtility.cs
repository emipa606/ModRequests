using RimWorld;
using Verse;

namespace BetterInfestations
{
    public static class InfestationUtility
    {
        public static Thing SpawnTunnels(int hiveCount, Map map, bool spawnAnywhereIfNoGoodCell = false, string questTag = null)
        {
            IntVec3 loc;
            if (!Patches.Patch_InfestationCellFinder_TryFindCell.TryFindCell(out loc, map))
            {
                if (!spawnAnywhereIfNoGoodCell)
                {
                    return null;
                }
                if (!RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(delegate (IntVec3 x)
                {
                    if (!x.Standable(map))
                    {
                        return false;
                    }
                    return true;
                }, map, out loc))
                {

                    return null;
                }
            }
            Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.BI_TunnelHiveSpawner, null), loc, map, WipeMode.FullRefund);
            QuestUtility.AddQuestTag(thing, questTag);
            for (int i = 0; i < hiveCount - 1; i++)
            {
                loc = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, ThingDefOf.BI_Hive, ThingDefOf.BI_Hive.GetCompProperties<CompProperties_SpawnerHives>(), true);
                if (loc.IsValid)
                {
                    thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.BI_TunnelHiveSpawner, null), loc, map, WipeMode.FullRefund);
                    QuestUtility.AddQuestTag(thing, questTag);
                }
            }
            return thing;
        }
    }
}