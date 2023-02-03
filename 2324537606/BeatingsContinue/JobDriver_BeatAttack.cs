using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace BeatingsContinue
{
    class JobDriver_BeatAttack : JobDriver_AttackMelee
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.DoAtomic(delegate
            {
                Pawn pawn = job.targetA.Thing as Pawn;
                //if (pawn != null && pawn.Downed && base.pawn.mindState.duty != null && base.pawn.mindState.duty.attackDownedIfStarving && base.pawn.Starving())
                //{
                job.killIncappedTarget = true;
                //}
            });
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, delegate
            {
                Thing thing = job.GetTarget(TargetIndex.A).Thing;
                Pawn p;
                //if (job.reactingToMeleeThreat && (p = (thing as Pawn)) != null && !p.Awake())
                //{
                //    EndJobWith(JobCondition.InterruptForced);
                //}
                InteractionUtility.TryGetRandomVerbForSocialFight(base.pawn, out Verb verb);
                if (BeatDecider.shouldStopBeating(base.pawn, base.TargetA.Thing as Pawn))
                {
                    EndJobWith(JobCondition.Succeeded);
                }
                else
                {
                    pawn.meleeVerbs.TryMeleeAttack(thing, verb);
                }
            }).FailOnDespawnedOrNull(TargetIndex.A);
        }
    }
}
