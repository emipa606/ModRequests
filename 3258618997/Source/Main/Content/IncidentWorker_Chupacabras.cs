using System;
using RimWorld;
using Verse;

namespace Necro
{
    
    public class IncidentWorker_Chupacabras : IncidentWorker
    {
        
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Necronoid);
            bool result;
            if (faction == null || faction.defeated)
            {
                result = false;
            }
            else
            {
                Map map = (Map)parms.target;
                IntVec3 intVec;
                result = RCellFinder.TryFindRandomPawnEntryCell(out intVec, map, CellFinder.EdgeRoadChance_Animal, false, null);
            }
            return result;
        }

        
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 root;
            RCellFinder.TryFindRandomPawnEntryCell(out root, map, CellFinder.EdgeRoadChance_Animal, false, null);
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(root, map, 10, null);
            Pawn pawn = PawnGenerator.GeneratePawn(NecroDefOf.Necronoid_Chupacabra, null);
            GenSpawn.Spawn(pawn, loc, map, Rot4.Random, WipeMode.Vanish, false, false);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, true, false, false, null, false, false, false);
            return true;
        }

        
        public IncidentWorker_Chupacabras()
        {
        }
    }
}
