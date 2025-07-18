using RimWorld;
using Verse;
using Verse.AI;

namespace Kraltech_Industries;

public class JobGiver_UnbridledRage : ThinkNode_JobGiver
{
    private const float WaitChance = 0.5f;

    private const int WaitTicks = 90;

    private const int MinMeleeChaseTicks = 420;

    private const int MaxMeleeChaseTicks = 900;

    private float maxAttackDistance = 40f;

    protected override Job TryGiveJob(Pawn pawn)
    {
        var mentalState_UnbridledRage = pawn.MentalState as MentalState_UnbridledRage;
        Job result;
        if (mentalState_UnbridledRage == null || !mentalState_UnbridledRage.IsTargetStillValidAndReachable())
        {
            result = null;
        }
        else if (pawn.TryGetAttackVerb(null) == null)
        {
            result = null;
        }
        else
        {
            var spawnedParentOrMe = mentalState_UnbridledRage.target.SpawnedParentOrMe;
            var job = JobMaker.MakeJob(JobDefOf.AttackMelee, spawnedParentOrMe);
            job.canBashDoors = true;
            job.killIncappedTarget = true;
            if (spawnedParentOrMe != mentalState_UnbridledRage.target) job.maxNumMeleeAttacks = 10;
            var pawn2 = FindPawnTarget(pawn);
            if (pawn2 != null)
            {
                var job2 = JobMaker.MakeJob(JobDefOf.AttackMelee, pawn2);
                job2.maxNumMeleeAttacks = 1;
                job2.expiryInterval = Rand.Range(420, 900);
                job2.canBashDoors = true;
                result = job2;
            }
            else
            {
                result = job;
            }
        }

        return result;
    }

    private Pawn FindPawnTarget(Pawn pawn)
    {
        return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, delegate(Thing x)
        {
            Pawn pawn2;
            return (pawn2 = x as Pawn) != null && pawn2.Spawned && !pawn2.Downed && !pawn2.IsPsychologicallyInvisible();
        }, 0f, maxAttackDistance, default, float.MaxValue, true);
    }

    public override ThinkNode DeepCopy(bool resolve = true)
    {
        var jobGiver_UnbridledRage = (JobGiver_UnbridledRage)base.DeepCopy(resolve);
        jobGiver_UnbridledRage.maxAttackDistance = maxAttackDistance;
        return jobGiver_UnbridledRage;
    }
}