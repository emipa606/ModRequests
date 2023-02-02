using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Rarr
{
    public class Building_TrapRat : Building_Trap
    {
        private static readonly PawnKindDef rat = PawnKindDef.Named("Rat");
        protected override void SpringSub(Pawn p)
        {
            Faction faction = FactionUtility.DefaultFactionFrom(rat.defaultFactionType);
            Pawn pawn = GenSpawn.Spawn(PawnGenerator.GeneratePawn(rat, faction), base.Position, base.Map) as Pawn;
            pawn.health.AddHediff(HediffDefOf.Scaria);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
        }
    }
}
