using RimWorld;
using Verse;

namespace Kraltech_Industries;

public class SpawnSomething : ThingComp
{
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        SpawnPawn();
        parent.Destroy();
    }

    public void SpawnPawn()
    {
        var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Mech_Valkyrian, Faction.OfMechanoids));
        GenSpawn.Spawn(pawn, parent.Position, parent.Map);
        pawn.health.AddHediff(HediffDefOf.EnhancedCombatModule);
    }
}