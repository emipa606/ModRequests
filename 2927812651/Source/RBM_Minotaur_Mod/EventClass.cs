using Verse;
using RimWorld;
using UnityEngine;
using System;
using System.Collections.Generic;
using Verse.AI.Group;


namespace RimWorld
{
    // Token: 0x02000E25 RID: 3621
    public class IncidentWorker_MinotaurGroup : IncidentWorker_NeutralGroup
    {
        // Token: 0x06005623 RID: 22051 RVA: 0x001CFBD8 File Offset: 0x001CDDD8
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map target = (Map)parms.target;
            if (!base.TryResolveParms(parms))
            {
                return false;
            }
            IntVec3 travelDest;
            if (!RCellFinder.TryFindTravelDestFrom(parms.spawnCenter, target, out travelDest))
            {
                Log.Warning("Failed to do traveler incident from " + parms.spawnCenter + ": Couldn't find anywhere for the traveler to go.");
                return false;
            }
            List<Pawn> pawnList = base.SpawnPawns(parms);
            if (pawnList.Count == 0)
            {
                return false;
            }
            string text;
            if (pawnList.Count == 1)
            {
                text = "SingleTravelerPassing".Translate(pawnList[0].story.Title, parms.faction.Name, pawnList[0].Name.ToStringFull, pawnList[0].Named("PAWN"));
                text = text.AdjustedFor(pawnList[0], "PAWN", true);
            }
            else
            {
                text = "GroupTravelersPassing".Translate(parms.faction.Name);
            }
            Messages.Message(text, pawnList[0], MessageTypeDefOf.NeutralEvent, true);
            LordJob_TravelAndExit jobTravelAndExit = new LordJob_TravelAndExit(travelDest);
            LordMaker.MakeNewLord(parms.faction, jobTravelAndExit, target, pawnList);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(pawnList, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, true, true);
            return true;
        }

        // Token: 0x06005624 RID: 22052 RVA: 0x001CFD3C File Offset: 0x001CDF3C
        protected override void ResolveParmsPoints(IncidentParms parms)
        {
            if (parms.points >= 0f)
            {
                return;
            }
            parms.points = Rand.ByCurve(IncidentWorker_TravelerGroup.PointsCurve);
        }

        // Token: 0x0400354C RID: 13644
        private static readonly SimpleCurve PointsCurve = new SimpleCurve
        {
            {
                new CurvePoint(40f, 0f),
                true
            },
            {
                new CurvePoint(50f, 1f),
                true
            },
            {
                new CurvePoint(100f, 1f),
                true
            },
            {
                new CurvePoint(200f, 0.5f),
                true
            },
            {
                new CurvePoint(300f, 0.1f),
                true
            },
            {
                new CurvePoint(500f, 0f),
                true
            }
        };
    }
}
