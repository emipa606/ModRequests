using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace MineItAll
{
    class JobDriver_Mine : RimWorld.JobDriver_Mine
    {
        protected int ticksToPickHit = -1000;
        protected Effecter effecter;
        DesignationDef MineAllDef = DefDatabase<DesignationDef>.GetNamed("MineAll");

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            FailOnCellMissingMiningDesignation(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil mine = new Toil();
            mine.tickAction = delegate ()
            {
                Pawn actor = mine.actor;
                Thing mineTarget = TargetThingA;

                if (ticksToPickHit < -100)
                    ResetTicksToPickHit();

                if (actor.skills != null && (mineTarget.Faction != actor.Faction || actor.Faction == null))
                    actor.skills.Learn(SkillDefOf.Mining, 0.07f, false);

                ticksToPickHit--;
                if (ticksToPickHit <= 0)
                {
                    IntVec3 position = mineTarget.Position;
                    if (effecter == null)
                        effecter = EffecterDefOf.Mine.Spawn();

                    effecter.Trigger(actor, mineTarget);
                    int damage = (!mineTarget.def.building.isNaturalRock) ? 40 : 80;
                    Mineable mineable = mineTarget as Mineable;
                    bool mineAll = false;
                    if (mineable == null || mineTarget.HitPoints > damage)
                    {
                        mineTarget.TakeDamage(new DamageInfo(DamageDefOf.Mining, damage, 0f, -1f, mine.actor));
                    }
                    else
                    {
                        mineAll = actor.Map.designationManager.DesignationAt(position, MineAllDef) != null;
                        mineable.Notify_TookMiningDamage(mineTarget.HitPoints, mine.actor);
                        mineable.HitPoints = 0;
                        mineable.DestroyMined(actor);
                    }

                    if (mineTarget.Destroyed)
                    {
                        if (mineAll)
                        {
                            foreach (IntVec3 direction in GenAdj.AdjacentCells)
                            {
                                IntVec3 adjacentCell = position + direction;
                                if (!adjacentCell.InBounds(Map) || adjacentCell.Fogged(Map))
                                    continue;

                                Building edifice = adjacentCell.GetEdifice(Map);
                                if (edifice != null && edifice.def == mineTarget.def)
                                {
                                    DesignationManager designationManager = Map.designationManager;
                                    if (designationManager.DesignationAt(adjacentCell, MineAllDef) == null)
                                    {
                                        designationManager.AddDesignation(new Designation(adjacentCell, MineAllDef));
                                        designationManager.TryRemoveDesignation(adjacentCell, DesignationDefOf.SmoothWall);
                                    }
                                }
                            }
                        }
                        actor.Map.mineStrikeManager.CheckStruckOre(position, mineTarget.def, actor);
                        actor.records.Increment(RecordDefOf.CellsMined);
                        if (pawn.Faction != Faction.OfPlayer)
                        {
                            List<Thing> thingList = position.GetThingList(Map);
                            for (int i = 0; i < thingList.Count; i++)
                            {
                                thingList[i].SetForbidden(true, false);
                            }
                        }
                        else
                        {
                            if (MineStrikeManager.MineableIsVeryValuable(mineTarget.def))
                                TaleRecorder.RecordTale(TaleDefOf.MinedValuable, new object[] { pawn, mineTarget.def.building.mineableThing });

                            if (MineStrikeManager.MineableIsValuable(mineTarget.def) && !pawn.Map.IsPlayerHome)
                                TaleRecorder.RecordTale(TaleDefOf.CaravanRemoteMining, new object[] { pawn, mineTarget.def.building.mineableThing });
                        }

                        ReadyForNextToil();
                        return;
                    }
                    ResetTicksToPickHit();
                }
            };
            mine.defaultCompleteMode = ToilCompleteMode.Never;
            mine.WithProgressBar(TargetIndex.A, () => 1f - (float)TargetThingA.HitPoints / TargetThingA.MaxHitPoints);
            mine.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            mine.activeSkill = (() => SkillDefOf.Mining);
            yield return mine;
            yield break;
        }

        protected virtual void FailOnCellMissingMiningDesignation(TargetIndex index)
        {
            AddEndCondition(delegate
            {
                Pawn actor = GetActor();
                Job curJob = actor.jobs.curJob;
                if (curJob.ignoreDesignations)
                    return JobCondition.Ongoing;

                DesignationManager designationManager = actor.Map.designationManager;
                IntVec3 cell = curJob.GetTarget(index).Cell;
                if (designationManager.DesignationAt(cell, DesignationDefOf.Mine) == null && designationManager.DesignationAt(cell, MineAllDef) == null)
                    return JobCondition.Incompletable;

                return JobCondition.Ongoing;
            });
        }

        protected virtual void ResetTicksToPickHit()
        {
            float speed = pawn.GetStatValue(StatDefOf.MiningSpeed, true);
            if (speed < 0.6f && pawn.Faction != Faction.OfPlayer)
            {
                speed = 0.6f;
            }
            ticksToPickHit = (int)Math.Round(100f / speed);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToPickHit, "ticksToPickHit");
        }
    }
}
