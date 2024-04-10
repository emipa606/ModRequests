using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace BetterInfestations
{
    class JobDriver_Butcher : JobDriver
    {
        private Thing thing
        {
            get
            {
                return job.GetTarget(TargetIndex.A).Thing;
            }
        }
        private float ChewDurationMultiplier
        {
            get
            {
                Corpse corpse = thing as Corpse;
                if (thing != null && corpse == null)
                {
                    return 4f / pawn.GetStatValue(StatDefOf.EatingSpeed, true);
                }
                return 1f / pawn.GetStatValue(StatDefOf.EatingSpeed, true);
            }
        }
        public override string GetReport()
        {
            if (!job.GetTarget(TargetIndex.A).Thing.def.IsCorpse)
            {
                string txt = "converting " + TargetA.Label.ToLower() + " to jelly.";
                return ReportStringProcessed(txt).CapitalizeFirst();
            }
            return base.GetReport();
        }
        public override void ExposeData()
        {
            base.ExposeData();
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (thing != null && thing.Spawned && !pawn.Reserve(thing, job, 1, -1, null)) return false;

            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            Toil chew = ChewCorpse(pawn, ChewDurationMultiplier, TargetIndex.A).FailOn((Toil x) => !thing.Spawned).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return chew;
            yield return FinalizeToil();
        }
        public static Toil ChewCorpse(Pawn chewer, float durationMultiplier, TargetIndex ind)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Thing thing = actor.CurJob.GetTarget(ind).Thing;
                actor.jobs.curDriver.ticksLeftThisToil = Mathf.RoundToInt(500 * durationMultiplier);
                if (thing != null && thing.Spawned)
                {
                    thing.Map.physicalInteractionReservationManager.Reserve(chewer, actor.CurJob, thing);
                }
            };
            toil.tickAction = delegate
            {
                if (chewer != toil.actor)
                {
                    toil.actor.rotationTracker.FaceCell(chewer.Position);
                }
                else
                {
                    Thing thing = toil.actor.CurJob.GetTarget(ind).Thing;
                    if (thing != null && thing.Spawned)
                    {
                        toil.actor.rotationTracker.FaceCell(thing.Position);
                    }
                }
            };
            toil.WithProgressBar(ind, delegate
            {
                Pawn actor = toil.actor;
                Thing thing = actor.CurJob.GetTarget(ind).Thing;
                if (thing == null)
                {
                    return 1f;
                }
                return 1f - toil.actor.jobs.curDriver.ticksLeftThisToil / Mathf.Round(500 * durationMultiplier);
            }, false, -0.5f);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnDestroyedOrNull(ind);
            toil.AddFinishAction(delegate
            {
                if (chewer == null) return;
                if (chewer.CurJob == null) return;

                Thing thing = chewer.CurJob.GetTarget(ind).Thing;
                if (thing == null) return;

                if (chewer.Map.physicalInteractionReservationManager.IsReservedBy(chewer, thing))
                {
                    chewer.Map.physicalInteractionReservationManager.Release(chewer, toil.actor.CurJob, thing);
                }
            });
            toil.handlingFacing = true;
            AddChewEffects(toil, chewer, ind);
            return toil;
        }
        public static Toil AddChewEffects(Toil toil, Pawn chewer, TargetIndex ind)
        {
            toil.WithEffect(delegate
            {
                Pawn actor = toil.actor;
                Thing thing = actor.CurJob.GetTarget(ind).Thing;
                EffecterDef result;
                if (thing != null && thing.def.ingestible != null)
                {
                    result = thing.def.ingestible.ingestEffect;
                }
                else
                {
                    result = DefDatabase<EffecterDef>.GetNamed("EatVegetarian", true);
                }
                return result;
            }, delegate
            {
                if (!toil.actor.CurJob.GetTarget(ind).HasThing) return null;

                Thing thing = toil.actor.CurJob.GetTarget(ind).Thing;
                if (chewer != toil.actor) return chewer;

                return thing;
            });
            toil.PlaySustainerOrSound(delegate
            {
                SoundDef result = null;
                LocalTargetInfo target = toil.actor.CurJob.GetTarget(ind);
                if (target != null && target.HasThing)
                {
                    result = DefDatabase<SoundDef>.GetNamed("RawMeat_Eat", true);
                }
                return result;
            });
            return toil;
        }
        public Toil FinalizeToil()
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                ConvertToJelly();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
        public void ConvertToJelly()
        {
            Log.Message("converting");
            if (thing != null)
            {
                Corpse corpse = thing as Corpse;
                if (corpse != null)
                {
                    ButcherProducts(corpse);
                    if (!corpse.Destroyed)
                    {
                        corpse.Destroy(DestroyMode.Vanish);
                    }
                }
                else
                {
                    Thing thing2 = ThingMaker.MakeThing(RimWorld.ThingDefOf.InsectJelly, null);
                    if (thing2 != null)
                    {
                        thing2.stackCount = (int)Math.Ceiling((double)thing.stackCount * BetterInfestationsMod.settings.jellyMultiplier);
                        GenPlace.TryPlaceThing(thing2, thing.Position, thing.Map, ThingPlaceMode.Near, out Thing jelly, null);
                        if (jelly != null)
                        {
                            jelly.SetForbidden(true);
                        }
                        if (!thing.Destroyed)
                        {
                            thing.Destroy(DestroyMode.Vanish);
                        }
                    }
                }
            }
        }
        public void ButcherProducts(Corpse corpse)
        {
            if (corpse == null) return;
            if (corpse.Map == null) return;
            if (corpse.InnerPawn == null) return;
            if (corpse.InnerPawn.RaceProps == null) return;

            if (corpse.InnerPawn.RaceProps.Humanlike)
            {
                IntVec3 pos = corpse.PositionHeld;
                if (corpse.InnerPawn.equipment != null)
                {
                    corpse.InnerPawn.equipment.DropAllEquipment(pos, true);
                }
                if (corpse.InnerPawn.apparel != null)
                {
                    corpse.InnerPawn.apparel.DropAll(pos, true);
                }
                if (corpse.InnerPawn.inventory != null)
                {
                    corpse.InnerPawn.inventory.DropAllNearPawn(pos, true);
                }
            }
            ButcherMakeFilth(corpse);
            ButcherMeat(corpse);
            ButcherLeather(corpse);
        }
        public void ButcherMakeFilth(Corpse corpse)
        {
            if (corpse == null) return;
            if (corpse.Map == null) return;
            if (corpse.InnerPawn == null) return;
            if (corpse.InnerPawn.RaceProps == null) return;
            if (corpse.InnerPawn.RaceProps.BloodDef == null) return;

            FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, corpse.InnerPawn.RaceProps.BloodDef, corpse.InnerPawn.LabelIndefinite(), 1);
        }
        public void ButcherMeat(Corpse corpse)
        {
            if (corpse == null) return;
            if (corpse.Map == null) return;
            if (corpse.InnerPawn == null) return;
            if (corpse.InnerPawn.RaceProps == null) return;
            if (corpse.InnerPawn.RaceProps.meatDef == null) return;

            Map map = corpse.Map;
            IntVec3 pos = corpse.Position;
            ThingDef meatDef = corpse.InnerPawn.RaceProps.meatDef;
            int count = GenMath.RoundRandom(corpse.InnerPawn.GetStatValue(StatDefOf.MeatAmount, true));
            if (count > 0)
            {
                if (count >= 2)
                {
                    RotStage rotStage = corpse.GetRotStage();
                    if (rotStage == RotStage.Rotting)
                    {
                        count /= 2;
                    }
                }
                Thing thing = ThingMaker.MakeThing(meatDef, null);
                if (thing != null)
                {
                    int stacks = (int)Math.Ceiling((double)count / thing.def.stackLimit);
                    int tempCount;
                    for (int i = 0; i < stacks; i++)
                    {
                        if (thing != null)
                        {
                            tempCount = count;
                            if (count > thing.def.stackLimit)
                            {
                                tempCount = thing.def.stackLimit;
                            }
                            thing.stackCount = tempCount;
                            GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near, out Thing meat, null);
                            if (meat != null)
                            {
                                meat.SetForbidden(true);
                            }
                            count -= tempCount;
                            thing = ThingMaker.MakeThing(meatDef, null);
                        }
                    }
                }
            }
        }
        public void ButcherLeather(Corpse corpse)
        {
            if (BetterInfestationsMod.settings == null) return;
            if (!BetterInfestationsMod.settings.produceLeather) return;
            if (corpse == null) return;
            if (corpse.Map == null) return;
            if (corpse.InnerPawn == null) return;
            if (corpse.InnerPawn.RaceProps == null) return;
            if (corpse.InnerPawn.RaceProps.leatherDef == null) return;

            Map map = corpse.Map;
            IntVec3 pos = corpse.Position;
            ThingDef leatherDef = corpse.InnerPawn.RaceProps.leatherDef;
            int count = GenMath.RoundRandom(corpse.InnerPawn.GetStatValue(StatDefOf.LeatherAmount, true));
            if (count > 0)
            {
                if (count >= 2)
                {
                    RotStage rotStage = corpse.GetRotStage();
                    if (rotStage == RotStage.Rotting)
                    {
                        count /= 2;
                    }
                }
                Thing thing = ThingMaker.MakeThing(leatherDef, null);
                if (thing != null)
                {
                    int stacks = (int)Math.Ceiling((double)count / thing.def.stackLimit);
                    int tempCount;
                    for (int i = 0; i < stacks; i++)
                    {
                        if (thing != null)
                        {
                            tempCount = count;
                            if (count > thing.def.stackLimit)
                            {
                                tempCount = thing.def.stackLimit;
                            }
                            thing.stackCount = tempCount;
                            GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near, out Thing leather, null);
                            if (leather != null)
                            {
                                leather.SetForbidden(true);
                            }
                            count -= tempCount;
                            thing = ThingMaker.MakeThing(leatherDef, null);
                        }
                    }
                }
            }
        }
    }
    public class JobDriver_Maintain : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil toil = Toils_General.Wait(180);
            toil.WithProgressBarToilDelay(TargetIndex.A);
            toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return toil;
            Toil maintain = new Toil();
            maintain.initAction = delegate
            {
                maintain.actor.CurJob.targetA.Thing.TryGetComp<CompMaintainable>().Maintained();
            };
            maintain.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return maintain;
        }
    }
    public class JobDriver_GotoSpawnHive : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            yield return FinalizeToil();

            Toil FinalizeToil()
            {
                Toil toil = new Toil();
                toil.initAction = delegate
                {
                    if (IntVec3Utility.ManhattanDistanceFlat(pawn.Position, job.targetA.Cell) <= 3 && SpawnHive(pawn))
                    {
                        Queen queen = pawn as Queen;
                        if (queen != null)
                        {
                            queen.CalculateNextHiveSpawnTick();
                        }
                    }
                };
                toil.defaultCompleteMode = ToilCompleteMode.Instant;
                return toil;
            }
        }
        public bool SpawnHive(Pawn pawn)
        {
            if (BetterInfestationsMod.settings == null) return false;
            if (HiveUtility.TotalSpawnedHivesCount(pawn.Map) >= BetterInfestationsMod.settings.maxHivesPerMap) return false;

            IntVec3 c = CellFinder.RandomClosewalkCellNear(pawn.Position, pawn.Map, 1);
            Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.BI_Hive), c, pawn.Map);
            if (hive == null) return false;

            hive.SetFaction(Faction.OfInsects);
            hive.questTags = HiveUtility.GetHive(pawn).questTags;
            Messages.Message("A bug queen has created a new hive.", hive, MessageTypeDefOf.NegativeEvent);
            return true;
        }
    }
    public class JobDriver_GotoPatrol : JobDriver
    {
        public int nextDriverTick = 0;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            toil.FailOn(() => PreyFound(pawn));
            yield return toil;
            Toil toil2 = new Toil();
            toil2.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil2;
        }

        private bool PreyFound(Pawn pawn)
        {
            if (Find.TickManager.TicksGame >= nextDriverTick)
            {
                nextDriverTick = Find.TickManager.TicksGame + 60;
                Predicate<Thing> validator = delegate (Thing t)
                {
                    if (t != null && t.def.category == ThingCategory.Item && !t.def.IsCorpse && t.IngestibleNow && !t.IsBurning() && !t.Fogged())
                    {
                        return true;
                    }
                    Pawn p = t as Pawn;
                    if (p != null && (p.Faction == null || (p.Faction != null && p.Faction != pawn.Faction && p.Faction.def.defName != "VFEI_Insect")) && !p.IsBurning() && !p.Fogged())
                    {
                        return true;
                    }
                    Corpse c = t as Corpse;
                    if (c != null && c.InnerPawn != null && c.InnerPawn.RaceProps.IsFlesh && c.GetRotStage() != RotStage.Dessicated && !c.IsBurning() && !c.Fogged())
                    {
                        if (c.InnerPawn.Faction == null || (c.InnerPawn.Faction != null && c.InnerPawn.Faction != pawn.Faction && c.InnerPawn.Faction.def.defName != "VFEI_Insect"))
                        {
                            return true;
                        }
                    }
                    return false;
                };
                Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, true, true, true), 8, validator);
                if (thing != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public class JobDriver_ChewConduits : JobDriver
    {
        private int numMeleeAttacksMade;
        public int nextDriverTick = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref numMeleeAttacksMade, "numMeleeAttacksMade", 0);
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            IAttackTarget attackTarget = job.targetA.Thing as IAttackTarget;
            if (attackTarget != null)
            {
                pawn.Map.attackTargetReservationManager.Reserve(pawn, job, attackTarget);
            }
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, delegate
            {
                Thing thing = job.GetTarget(TargetIndex.A).Thing;
                if (pawn.meleeVerbs.TryMeleeAttack(thing, job.verbToUse) && pawn.CurJob != null && pawn.jobs.curDriver == this)
                {
                    numMeleeAttacksMade++;
                    if (numMeleeAttacksMade >= job.maxNumMeleeAttacks)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                }
            }).FailOnDespawnedOrNull(TargetIndex.A).FailOn(() => FoundThreat(pawn));
        }
        private bool FoundThreat(Pawn pawn)
        {
            if (Find.TickManager.TicksGame >= nextDriverTick)
            {
                nextDriverTick = Find.TickManager.TicksGame + 60;
                Predicate<Thing> validator = delegate (Thing t)
                {
                    Building_Turret b = t as Building_Turret;
                    if (!b.DestroyedOrNull() && b.PowerComp.PowerNet.HasActivePowerSource && !b.Fogged())
                    {
                        return true;
                    }
                    Pawn p = t as Pawn;
                    if (!p.DestroyedOrNull() && p.Faction != null && p.Faction != pawn.Faction && p.Faction.def.defName != "VFEI_Insect" && !p.Downed && !p.Fogged())
                    {
                        return true;
                    }
                    return false;
                };
                Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, true, true, true), 24, validator);
                if (thing != null)
                {
                    return true;
                }
            }
            return false;
        }
        public override void Notify_PatherFailed()
        {
            base.Notify_PatherFailed();
        }
        public override bool IsContinuation(Job j)
        {
            return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }
    }
}