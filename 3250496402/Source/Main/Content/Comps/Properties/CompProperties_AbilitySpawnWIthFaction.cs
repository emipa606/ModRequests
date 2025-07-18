using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class CompProperties_AbilitySpawnWithFaction : CompProperties_AbilityEffect
{
    public bool allowOnBuildings = true;

    public bool sendSkipSignal = true;

    public ThingDef thingDef;

    public CompProperties_AbilitySpawnWithFaction()
    {
        compClass = typeof(CompAbilityEffect_SpawnWithFaction);
    }
}