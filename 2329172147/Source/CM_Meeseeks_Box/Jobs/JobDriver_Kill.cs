using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CM_Meeseeks_Box
{
    public class JobDriver_Kill : JobDriver
    {
        private int jobStartTick = -1;

        private int targetCheckTick = -1;

        private int targetCheckInterval = 60;

        private const TargetIndex VictimInd = TargetIndex.A;

        public Pawn Victim => (Pawn)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil startJobToil = new Toil();
            startJobToil.initAction = delegate
            {
                jobStartTick = Find.TickManager.TicksGame;
            };
            yield return startJobToil;
            yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
            Toil startCollectCorpseLabel = Toils_General.Label();
            Toil slaughterLabel = Toils_General.Label();
            Toil meleeLabel = Toils_General.Label();

            Toil checkTargetToil = new Toil();
            checkTargetToil.initAction = delegate
            {
                targetCheckTick = Find.TickManager.TicksGame;

                //if (pawn.equipment.Primary == null || pawn.equipment.Primary.def.IsMeleeWeapon)
                //    pawn.jobs.curDriver.JumpToToil(meleeLabel);
            };
            yield return checkTargetToil;

            //Toil gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, true, 0.95f).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpseLabel).FailOn(() => Find.TickManager.TicksGame > jobStartTick + 5000);
            Toil gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, true, 4.0f)
                                           .JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpseLabel)
                                           .JumpIf(() => Find.TickManager.TicksGame > targetCheckTick + targetCheckInterval, checkTargetToil);
            yield return gotoCastPos;

            Toil selectAttack = new Toil();
            selectAttack.initAction = delegate
            {
                if (pawn.CanReachImmediate(TargetA, PathEndMode.Touch))
                {
                    pawn.jobs.curDriver.JumpToToil(meleeLabel);
                }
                else if (Victim.Downed)
                {
                    if (Victim.AnimalOrWildMan())
                        pawn.jobs.curDriver.JumpToToil(slaughterLabel);
                    else
                        pawn.jobs.curDriver.JumpToToil(meleeLabel);
                }
            };
            yield return selectAttack;

            // Ranged attack
            yield return Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, checkTargetToil);
            yield return Toils_Combat.CastVerb(TargetIndex.A, canHitNonTargetPawns: false).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpseLabel).FailOn(() => Find.TickManager.TicksGame > jobStartTick + 5000);
            yield return Toils_Jump.JumpIfTargetDespawnedOrNull(TargetIndex.A, startCollectCorpseLabel);
            yield return Toils_Jump.Jump(selectAttack);

            // Melee
            yield return meleeLabel;
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIf(() => Find.TickManager.TicksGame > targetCheckTick + targetCheckInterval, checkTargetToil);
            //yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell).JumpIf(() => Find.TickManager.TicksGame > targetCheckTick + targetCheckInterval, checkTargetToil);
            Toil meleeAttack = new Toil();
            meleeAttack.tickAction = delegate
            {
                if (Victim == null || !Victim.Spawned || Victim.IsForbidden(pawn) || !pawn.CanReachImmediate(TargetA, PathEndMode.Touch))
                {
                    pawn.jobs.curDriver.JumpToToil(checkTargetToil);
                }
                else if (Victim.Downed && Victim.AnimalOrWildMan())
                {
                    pawn.jobs.curDriver.JumpToToil(slaughterLabel);
                }

                pawn.pather.StopDead();
                pawn.meleeVerbs.TryMeleeAttack(Victim);
            };
            meleeAttack.activeSkill = () => SkillDefOf.Melee;
            meleeAttack.defaultCompleteMode = ToilCompleteMode.Never;
            yield return meleeAttack;

            yield return Toils_Jump.JumpIfCannotTouch(TargetIndex.A, PathEndMode.Touch, checkTargetToil);

            // Slaughter target
            yield return slaughterLabel;
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnMobile(TargetIndex.A);
            yield return Toils_General.WaitWith(TargetIndex.A, 180, useProgressBar: true).FailOnMobile(TargetIndex.A);
            yield return Toils_General.Do(delegate
            {
                if (!Victim.Dead)
                {
                    ExecutionUtility.DoExecutionByCut(pawn, Victim);
                    pawn.records.Increment(RecordDefOf.AnimalsSlaughtered);
                    if (pawn.InMentalState)
                    {
                        pawn.MentalState.Notify_SlaughteredAnimal();
                    }
                }
            });
            yield return Toils_Jump.Jump(startCollectCorpseLabel);
            yield return startCollectCorpseLabel;

            //yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            //yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, delegate
            //{
            //    Thing thing = job.GetTarget(TargetIndex.A).Thing;
            //    Pawn p;
            //    if (job.reactingToMeleeThreat && (p = thing as Pawn) != null && !p.Awake())
            //    {
            //        EndJobWith(JobCondition.InterruptForced);
            //    }
            //    else if (pawn.meleeVerbs.TryMeleeAttack(thing, job.verbToUse) && pawn.CurJob != null && pawn.jobs.curDriver == this)
            //    {
            //        numMeleeAttacksMade++;
            //        if (numMeleeAttacksMade >= job.maxNumMeleeAttacks)
            //        {
            //            EndJobWith(JobCondition.Succeeded);
            //        }
            //    }
            //}).FailOnDespawnedOrNull(TargetIndex.A);
        }
    }
}
