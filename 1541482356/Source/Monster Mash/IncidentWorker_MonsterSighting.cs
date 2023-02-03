using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MonsterMash
{
    public class IncidentWorker_MonsterSighting : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 intVec;
            return !map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && this.TryFindEntryCell(map, out intVec);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 intVec;
            if (!this.TryFindEntryCell(map, out intVec))
            {
                return false;
            }

            PawnKindDef monster = selectMonster();

            float num = StorytellerUtility.DefaultThreatPointsNow(map);
            int num2 = GenMath.RoundRandom(num / monster.combatPower);
            int max = Rand.RangeInclusive(3, 6);
            num2 = Mathf.Clamp(num2, 1, max);
            int num3 = Rand.RangeInclusive(90000, 150000);
            IntVec3 invalid = IntVec3.Invalid;
            if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(intVec, map, 10f, out invalid))
            {
                invalid = IntVec3.Invalid;
            }
            Pawn pawn = null;
            for (int i = 0; i < num2; i++)
            {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10, null);
                pawn = PawnGenerator.GeneratePawn(monster, null);
                GenSpawn.Spawn(pawn, loc, map, Rot4.Random, WipeMode.Vanish, false);
                pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + num3;
                if (invalid.IsValid)
                {
                    pawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(invalid, map, 10, null);
                }
            }
            Find.LetterStack.ReceiveLetter("LetterLabelMonsterSighting".Translate(monster.label).CapitalizeFirst(), "LetterMonsterSighting".Translate(monster.label), LetterDefOf.NeutralEvent, pawn, null, null);
            return true;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f);
        }

        public static PawnKindDef selectMonster()
        {
            IEnumerable<PawnKindDef> monsters = from defs in DefDatabase<PawnKindDef>.AllDefs
                                             where defs.defName.Equals("CarrionCrawler") |  defs.defName.Equals("InfernoBeetle") | defs.defName.Equals("LandKraken") | defs.defName.Equals("MM_PolarColossus") | defs.defName.Equals("SanguineDrake")
                                                select defs;
            return monsters.RandomElement();
        }
    }
}
