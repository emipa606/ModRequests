using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Threading.Tasks;
using UnityEngine;

namespace AP_PredatorsRework
{
    class IncidentWorker_TitanboaSpawn : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
            {
                return false;
            }
            IntVec3 cell;
            return TryFindEntryCell(map, out cell);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!TryFindEntryCell(map, out var cell))
            {
                return false;
            }
            PawnKindDef boadef = Defs.AP_TitanboaBaseForm;
            Pawn pawn = null;
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(cell, map, 10);
            pawn = PawnGenerator.GeneratePawn(boadef);
            GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
            SendStandardLetter("AP_TitanboaSpawnLetterLabel".Translate(boadef.label).CapitalizeFirst(), "AP_TitanboaSpawnLetterText".Translate(boadef.label), LetterDefOf.NeutralEvent, parms, pawn);
            return true;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f);
        }
    }
}
