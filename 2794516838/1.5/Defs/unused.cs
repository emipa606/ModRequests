using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using Verse.AI;

namespace AP_PredatorsRework
{
    public class JobGiver_FightEnemyAlone : JobGiver_AIFightEnemy
    {
        private float targetAcquireRadius = 56f;

        private float targetKeepRadius = 65f;

        private bool needLOSToAcquireNonPawnTargets;

        private bool chaseTarget;
        private bool PrimaryVerbIsIncendiary(Pawn pawn)
        {
            if (pawn.equipment != null && pawn.equipment.Primary != null)
            {
                List<Verb> allVerbs = pawn.equipment.Primary.GetComp<CompEquippable>().AllVerbs;
                for (int i = 0; i < allVerbs.Count; i++)
                {
                    if (allVerbs[i].verbProps.isPrimary)
                    {
                        return allVerbs[i].IsIncendiary();
                    }
                }
            }
            return false;
        }

        protected override Thing FindAttackTarget(Pawn pawn)
        {
            TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            if (needLOSToAcquireNonPawnTargets)
            {
                targetScanFlags |= TargetScanFlags.NeedLOSToNonPawns;
            }
            if (PrimaryVerbIsIncendiary(pawn))
            {
                targetScanFlags |= TargetScanFlags.NeedNonBurning;
            }
            Thing p = null;
            p=(Thing)AttackTargetFinderCustom.BestAttackTarget(pawn, targetScanFlags, (Thing x) => ExtraTargetValidator(pawn, x), 0f, targetAcquireRadius, GetFlagPosition(pawn), GetFlagRadius(pawn));
            if(p is Pawn)
            {
                Log.Message("target id: "+p.ThingID);
            }
            if(p==null)
            {
                Log.Message("error at FindAttackTarget");
            }
            return p;
        }

        protected override bool ExtraTargetValidator(Pawn pawn, Thing target)
        {
            
            if (IsMostAlonePawn(target))
            {
                Log.Message("ExtraTargetValidator true " + target.ThingID);
                return true;
            }
            else
            {
                Log.Message("returned base validator");
                return base.ExtraTargetValidator(pawn, target);
            }
        }
        public static bool IsMostAlonePawn(Thing target)
        {
            if (target.def.race.Humanlike)
            {
                Pawn targ = target as Pawn;
                Log.Message("checking " + targ.Name);
            }
            List<Pawn> pawnlist = Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer).Take(20).ToList();
            Dictionary<Pawn, float> dic = new Dictionary<Pawn, float>();
            foreach (Pawn firstpawn in pawnlist)
            {
                float acumDistance = 0f;
                for (int i = 0; i < pawnlist.Count; i++)
                {
                    if (firstpawn.ThingID == pawnlist[i].ThingID)
                    {
                        continue;
                    }
                    acumDistance += firstpawn.Position.DistanceTo(pawnlist[i].Position);
                }
                dic.Add(firstpawn, acumDistance);
            }
            Pawn pawnAlone = dic.FirstOrDefault(x => x.Value == dic.Values.Max()).Key;
            if (target as Pawn == pawnAlone)
            {
                Log.Message("chosen target is correct " + pawnAlone.Name);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
        {
            Thing enemyTarget = pawn.mindState.enemyTarget;
            bool allowManualCastWeapons = !pawn.IsColonist;
            Verb verb = pawn.TryGetAttackVerb(enemyTarget, allowManualCastWeapons);
            if (verb == null)
            {
                dest = IntVec3.Invalid;
                return false;
            }
            CastPositionRequest newReq = default(CastPositionRequest);
            newReq.caster = pawn;
            newReq.target = enemyTarget;
            newReq.verb = verb;
            newReq.maxRangeFromTarget = verb.verbProps.range;
            newReq.wantCoverFromTarget = verb.verbProps.range > 5f;
            return CastPositionFinder.TryFindCastPosition(newReq, out dest);
        }
    }
}
