using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    public class WorkGiver_Kill : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.OnCell;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (!this.ShouldSkip(pawn, false))
            {
                foreach (Pawn target in pawn.Map.mapPawns.AllPawns)
                {
                    yield return target;
                }
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return (pawn.kindDef != MeeseeksDefOf.MeeseeksKind);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn targetPawn = t as Pawn;
            return (targetPawn != null && !targetPawn.Destroyed && !targetPawn.Dead);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!HasJobOnThing(pawn, t, forced))
                return null;

            return JobMaker.MakeJob(MeeseeksDefOf.CM_Meeseeks_Box_Job_Kill, t);
        }
    }
}
