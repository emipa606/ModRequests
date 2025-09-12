using System;
using RimWorld;
using Verse;

namespace Necro
{
    
    public class SpawnParasites : ThingComp
    {
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.SpawnPawn();
            this.parent.Destroy(DestroyMode.Vanish);
        }

        
        public void SpawnPawn()
        {
            GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(NecroDefOf.Necronoid_FetidParasite, Find.FactionManager.FirstFactionOfDef(FactionDefOf.Necronoid), PawnGenerationContext.NonPlayer, -1, false, false, false, true, false, 1f, false, true, false, true, true, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, false, false, false, false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, false, false, false, -1, 0, false)), this.parent.Position, this.parent.Map, WipeMode.Vanish);
        }

        
        public SpawnParasites()
        {
        }
    }
}
