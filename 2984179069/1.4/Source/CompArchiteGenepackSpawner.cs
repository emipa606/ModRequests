using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MoreArchotechGarbage
{
    public class CompArchiteGenepackSpawner : CompSpawner
    {
        [HarmonyPatch(typeof(CompSpawner), "TryDoSpawn")]
        public static class CompSpawner_TryDoSpawn_Patch
        {
            public static bool Prefix(CompSpawner __instance)
            {
                if (__instance is CompArchiteGenepackSpawner architeGeneAssembler)
                {
                    architeGeneAssembler.TryDoSpawnOverride();
                    return false;
                }
                return true;
            }
        }

        public bool TryDoSpawnOverride()
        {
            if (!parent.Spawned)
            {
                return false;
            }

            if (TryFindSpawnCell(parent, PropsSpawner.thingToSpawn, PropsSpawner.spawnCount, out var result))
            {
                var thing = ThingMaker.MakeThing(PropsSpawner.thingToSpawn) as Genepack;
                thing.stackCount = PropsSpawner.spawnCount;
                if (thing == null)
                {
                    Log.Error("Could not spawn anything for " + parent);
                }
                if (PropsSpawner.inheritFaction && thing.Faction != parent.Faction)
                {
                    thing.SetFaction(parent.Faction);
                }
                GenPlace.TryPlaceThing(thing, result, parent.Map, ThingPlaceMode.Direct, out var lastResultingThing);
                if (PropsSpawner.spawnForbidden)
                {
                    lastResultingThing.SetForbidden(value: true);
                }
                if (PropsSpawner.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
                {
                    Messages.Message("MessageCompSpawnerSpawnedItem".Translate(PropsSpawner.thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent);
                }
                List<GeneDef> genesToAdd = DefDatabase<GeneDef>.AllDefs.Where(x => x.biostatArc > 0).InRandomOrder()
                    .Take(MoreArchotechGarbageSettings.architeGenepackAssemblerRandomAmountOfGenesRange.RandomInRange).ToList();
                thing.Initialize(genesToAdd);
                return true;
            }
            return false;
        }
    }
}