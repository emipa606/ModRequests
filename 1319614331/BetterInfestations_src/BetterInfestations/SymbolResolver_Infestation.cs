using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace BetterInfestations
{
    public class SymbolResolver_Infestation : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            if (BetterInfestationsMod.settings == null) return;

            if (!Patches.Patch_InfestationCellFinder_TryFindCell.TryFindCell(out IntVec3 pos, BaseGen.globalSettings.map))
            {
                return;
            }
            int num = BetterInfestationsMod.settings.maxHivesPerMap;
            Hive hive = (Hive)ThingMaker.MakeThing(ThingDefOf.BI_Hive);
            hive.SetFaction(Faction.OfInsects);
            hive = (Hive)GenSpawn.Spawn(hive, pos, BaseGen.globalSettings.map);
            if (hive != null)
            {
                CompSpawner compSpawner = hive.GetComp<CompSpawner>();
                if (compSpawner.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.GlowPod)
                {
                    compSpawner.TryDoSpawn();
                }
                CompSpawnerJelly compSpawnerJelly = hive.GetComp<CompSpawnerJelly>();
                if (compSpawnerJelly.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.InsectJelly)
                {
                    for (int i = 0; i < Rand.Range(6, 20); i++)
                    {
                        compSpawnerJelly.TryDoSpawn();
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < Rand.Range(2, 8); j++)
                    {
                        hive.CompSpawnerPawns.TrySpawnPawn(i, out Pawn _, hive.CompSpawnerPawns.RandomPawnKindDef(), false);
                    }
                }
                hive.CompSpawnerPawns.TrySpawnPawn(0, out Pawn _, PawnKindDefOf.BI_Queen, false);
                for (int i = 0; i < num - 1; i++)
                {
                    if (hive.GetComp<CompSpawnerHives>().TrySpawnChildHive(out Hive newHive))
                    {
                        newHive.SetFaction(hive.Faction);
                        compSpawner = newHive.GetComp<CompSpawner>();
                        if (compSpawner.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.GlowPod)
                        {
                            compSpawner.TryDoSpawn();
                        }
                        compSpawnerJelly = newHive.GetComp<CompSpawnerJelly>();
                        if (compSpawnerJelly.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.InsectJelly)
                        {
                            for (int j = 0; j < Rand.Range(6, 20); j++)
                            {
                                compSpawnerJelly.TryDoSpawn();
                            }
                        }
                        for (int j = 0; j < 2; j++)
                        {
                            for (int k = 0; k < Rand.Range(3, 8); k++)
                            {
                                newHive.CompSpawnerPawns.TrySpawnPawn(j, out Pawn _, newHive.CompSpawnerPawns.RandomPawnKindDef(), false);
                            }
                        }
                        if (Rand.Range(1, 100) <= 50)
                        {
                            newHive.CompSpawnerPawns.TrySpawnPawn(0, out Pawn _, PawnKindDefOf.BI_Queen, false);
                        }
                        HiveUtility.SpawnRandomItems(newHive, DefDatabase<ThingDef>.AllDefsListForReading);
                    }
                }
                HiveUtility.SpawnRandomCorpses(hive);
            }
        }
    }
}