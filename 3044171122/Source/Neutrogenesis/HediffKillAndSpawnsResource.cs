using System.Collections.Generic;
using RimWorld;
using Verse;

namespace DRNE_NeutroamineExpansion
{
    public class HediffThatKillsAndSpawnsResource : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
            if ((double)Severity >= 1.0 && pawn.Map != null)
            {
                KillAndSpawn();
            }
        }
        private void KillAndSpawn()
        {
            ModExt_Neutrobug modExt = def.GetModExtension<ModExt_Neutrobug>();

            Thing spawned = ThingMaker.MakeThing(modExt.resource);

            spawned.stackCount = modExt.amountOfResource;

            Thing secondSpawned = ThingMaker.MakeThing(modExt.secondResource);

            secondSpawned.stackCount = modExt.amountOfSecondResource;

            if (spawned != null && secondSpawned != null)
            {
                GenPlace.TryPlaceThing(spawned, base.pawn.Position, base.pawn.Map, ThingPlaceMode.Near);

                GenPlace.TryPlaceThing(secondSpawned, base.pawn.Position, base.pawn.Map, ThingPlaceMode.Near);

                base.pawn.Kill(null, this);
            }



            
        }
    }
}
