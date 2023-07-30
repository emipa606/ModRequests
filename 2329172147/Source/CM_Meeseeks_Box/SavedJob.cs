using System;
using System.Collections.Generic;

using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace CM_Meeseeks_Box
{
    public class SavedJob : IExposable
    {
        public JobDef def;

        public LocalTargetInfo targetA = LocalTargetInfo.Invalid;

        public LocalTargetInfo targetB = LocalTargetInfo.Invalid;

        public LocalTargetInfo targetC = LocalTargetInfo.Invalid;

        public List<LocalTargetInfo> targetQueueA;

        public List<LocalTargetInfo> targetQueueB;

        public GlobalTargetInfo globalTarget = GlobalTargetInfo.Invalid;

        public int count = -1;

        public List<int> countQueue;

        public int startTick = -1;

        public int expiryInterval = -1;

        public bool checkOverrideOnExpire;

        public bool playerForced;

        public List<ThingCountClass> placedThings;

        public int maxNumMeleeAttacks = int.MaxValue;

        public int maxNumStaticAttacks = int.MaxValue;

        public LocomotionUrgency locomotionUrgency = LocomotionUrgency.Jog;

        public HaulMode haulMode;

        public Bill bill;

        public ICommunicable commTarget;

        public ThingDef plantDefToSow;

        public Verb verbToUse;

        public bool haulOpportunisticDuplicates;

        public bool exitMapOnArrival;

        public bool failIfCantJoinOrCreateCaravan;

        public bool killIncappedTarget;

        public bool ignoreForbidden;

        public bool ignoreDesignations;

        public bool canBash;

        public bool canUseRangedWeapon = true;

        public bool haulDroppedApparel;

        public bool restUntilHealed;

        public bool ignoreJoyTimeAssignment;

        public bool doUntilGatheringEnded;

        public bool overeat;

        public bool attackDoorIfTargetLost;

        public int takeExtraIngestibles;

        public bool expireRequiresEnemiesNearby;

        public Lord lord;

        public bool collideWithPawns;

        public bool forceSleep;

        public InteractionDef interaction;

        public bool endIfCantShootTargetFromCurPos;

        public bool endIfCantShootInMelee;

        public bool checkEncumbrance;

        public float followRadius;

        public bool endAfterTendedOnce;

        public Quest quest;

        public Mote mote;

        public float psyfocusTargetLast = -1f;

        public bool wasOnMeditationTimeAssignment;

        public bool reactingToMeleeThreat;

        public WorkGiverDef workGiverDef;

        public Ability ability;

        public WorkGiver_Scanner WorkGiverScanner => workGiverDef?.Worker as WorkGiver_Scanner;

        public bool IsConstruction => (workGiverDef != null && WorkerDefUtility.constructionDefs.Contains(workGiverDef));

        public bool IsDoBill => UsesWorkGiver<WorkGiver_DoBill>();

        public bool IsTaming => UsesWorkGiver<WorkGiver_Tame>();

        public bool IsTraining => UsesWorkGiver<WorkGiver_Train>();

        public bool UsesWorkGiver<T>()
        {
            return (workGiverDef != null && workGiverDef.Worker != null && workGiverDef.Worker is T);
        }

        public SavedJob()
        {

        }

        public LocalTargetInfo GetTarget(TargetIndex ind)
        {
            switch (ind)
            {
                case TargetIndex.A: return targetA;
                case TargetIndex.B: return targetB;
                case TargetIndex.C: return targetC;
                default: return null;
            }
        }

        public SavedJob(Job job)
        {
            def = job.def;
            targetA = job.targetA;
            targetB = job.targetB;
            targetC = job.targetC;
            targetQueueA = job.targetQueueA;
            targetQueueB = job.targetQueueB;
            count = job.count;
            countQueue = job.countQueue;
            startTick = job.startTick;
            expiryInterval = job.expiryInterval;
            checkOverrideOnExpire = job.checkOverrideOnExpire;
            playerForced = job.playerForced;
            placedThings = job.placedThings;
            maxNumMeleeAttacks = job.maxNumMeleeAttacks;
            maxNumStaticAttacks = job.maxNumStaticAttacks;
            locomotionUrgency = job.locomotionUrgency;
            haulMode = job.haulMode;
            bill = job.bill;
            commTarget = job.commTarget;
            plantDefToSow = job.plantDefToSow;
            verbToUse = job.verbToUse;
            haulOpportunisticDuplicates = job.haulOpportunisticDuplicates;
            exitMapOnArrival = job.exitMapOnArrival;
            failIfCantJoinOrCreateCaravan = job.failIfCantJoinOrCreateCaravan;
            killIncappedTarget = job.killIncappedTarget;
            ignoreForbidden = job.ignoreForbidden;
            ignoreDesignations = job.ignoreDesignations;
            canBash = job.canBash;
            canUseRangedWeapon = job.canUseRangedWeapon;
            haulDroppedApparel = job.haulDroppedApparel;
            restUntilHealed = job.restUntilHealed;
            ignoreJoyTimeAssignment = job.ignoreJoyTimeAssignment;
            doUntilGatheringEnded = job.doUntilGatheringEnded;
            overeat = job.overeat;
            attackDoorIfTargetLost = job.attackDoorIfTargetLost;
            takeExtraIngestibles = job.takeExtraIngestibles;
            expireRequiresEnemiesNearby = job.expireRequiresEnemiesNearby;
            lord = job.lord;
            collideWithPawns = job.collideWithPawns;
            forceSleep = job.forceSleep;
            interaction = job.interaction;
            endIfCantShootTargetFromCurPos = job.endIfCantShootTargetFromCurPos;
            endIfCantShootInMelee = job.endIfCantShootInMelee;
            checkEncumbrance = job.checkEncumbrance;
            followRadius = job.followRadius;
            endAfterTendedOnce = job.endAfterTendedOnce;
            quest = job.quest;
            mote = job.mote;
            reactingToMeleeThreat = job.reactingToMeleeThreat;
            wasOnMeditationTimeAssignment = job.wasOnMeditationTimeAssignment;
            psyfocusTargetLast = job.psyfocusTargetLast;
            //jobGiverThinkTree = job.jobGiverThinkTree;
            //jobGiver = job.jobGiver;
            workGiverDef = job.workGiverDef;
            ability = job.ability;
        }

        public Job MakeJob()
        {
            Job newJob = JobMaker.MakeJob();

            newJob.def = def;
            newJob.targetA = targetA;
            newJob.targetB = targetB;
            newJob.targetC = targetC;
            newJob.targetQueueA = targetQueueA;
            newJob.targetQueueB = targetQueueB;
            newJob.count = count;
            newJob.countQueue = countQueue;
            newJob.startTick = startTick;
            newJob.expiryInterval = expiryInterval;
            newJob.checkOverrideOnExpire = checkOverrideOnExpire;
            newJob.playerForced = playerForced;
            newJob.placedThings = placedThings;
            newJob.maxNumMeleeAttacks = maxNumMeleeAttacks;
            newJob.maxNumStaticAttacks = maxNumStaticAttacks;
            newJob.locomotionUrgency = locomotionUrgency;
            newJob.haulMode = haulMode;
            newJob.bill = bill;
            newJob.commTarget = commTarget;
            newJob.plantDefToSow = plantDefToSow;
            newJob.verbToUse = verbToUse;
            newJob.haulOpportunisticDuplicates = haulOpportunisticDuplicates;
            newJob.exitMapOnArrival = exitMapOnArrival;
            newJob.failIfCantJoinOrCreateCaravan = failIfCantJoinOrCreateCaravan;
            newJob.killIncappedTarget = killIncappedTarget;
            newJob.ignoreForbidden = ignoreForbidden;
            newJob.ignoreDesignations = ignoreDesignations;
            newJob.canBash = canBash;
            newJob.canUseRangedWeapon = canUseRangedWeapon;
            newJob.haulDroppedApparel = haulDroppedApparel;
            newJob.restUntilHealed = restUntilHealed;
            newJob.ignoreJoyTimeAssignment = ignoreJoyTimeAssignment;
            newJob.doUntilGatheringEnded = doUntilGatheringEnded;
            newJob.overeat = overeat;
            newJob.attackDoorIfTargetLost = attackDoorIfTargetLost;
            newJob.takeExtraIngestibles = takeExtraIngestibles;
            newJob.expireRequiresEnemiesNearby = expireRequiresEnemiesNearby;
            newJob.lord = lord;
            newJob.collideWithPawns = collideWithPawns;
            newJob.forceSleep = forceSleep;
            newJob.interaction = interaction;
            newJob.endIfCantShootTargetFromCurPos = endIfCantShootTargetFromCurPos;
            newJob.endIfCantShootInMelee = endIfCantShootInMelee;
            newJob.checkEncumbrance = checkEncumbrance;
            newJob.followRadius = followRadius;
            newJob.endAfterTendedOnce = endAfterTendedOnce;
            newJob.quest = quest;
            newJob.mote = mote;
            newJob.reactingToMeleeThreat = reactingToMeleeThreat;
            newJob.wasOnMeditationTimeAssignment = wasOnMeditationTimeAssignment;
            newJob.psyfocusTargetLast = psyfocusTargetLast;
            //newJob.jobGiverThinkTree = jobGiverThinkTree;
            //newJob.jobGiver = jobGiver;
            newJob.workGiverDef = workGiverDef;
            newJob.ability = ability;

            return newJob;
        }

        public void ExposeData()
        {
            ILoadReferenceable refee = (ILoadReferenceable)commTarget;
            Scribe_References.Look(ref refee, "commTarget");
            commTarget = (ICommunicable)refee;
            Scribe_References.Look(ref verbToUse, "verbToUse");
            Scribe_References.Look(ref bill, "bill");
            Scribe_References.Look(ref lord, "lord");
            Scribe_References.Look(ref quest, "quest");
            Scribe_Defs.Look(ref def, "def");
            //Scribe_Values.Look(ref loadID, "loadID", 0);
            Scribe_TargetInfo.Look(ref targetA, "targetA");
            Scribe_TargetInfo.Look(ref targetB, "targetB");
            Scribe_TargetInfo.Look(ref targetC, "targetC");
            Scribe_TargetInfo.Look(ref globalTarget, "globalTarget");
            Scribe_Collections.Look(ref targetQueueA, "targetQueueA", LookMode.Undefined);
            Scribe_Collections.Look(ref targetQueueB, "targetQueueB", LookMode.Undefined);
            Scribe_Values.Look(ref count, "count", -1);
            Scribe_Collections.Look(ref countQueue, "countQueue", LookMode.Undefined);
            Scribe_Values.Look(ref startTick, "startTick", -1);
            Scribe_Values.Look(ref expiryInterval, "expiryInterval", -1);
            Scribe_Values.Look(ref checkOverrideOnExpire, "checkOverrideOnExpire", defaultValue: false);
            Scribe_Values.Look(ref playerForced, "playerForced", defaultValue: false);
            Scribe_Collections.Look(ref placedThings, "placedThings", LookMode.Undefined);
            Scribe_Values.Look(ref maxNumMeleeAttacks, "maxNumMeleeAttacks", int.MaxValue);
            Scribe_Values.Look(ref maxNumStaticAttacks, "maxNumStaticAttacks", int.MaxValue);
            Scribe_Values.Look(ref exitMapOnArrival, "exitMapOnArrival", defaultValue: false);
            Scribe_Values.Look(ref failIfCantJoinOrCreateCaravan, "failIfCantJoinOrCreateCaravan", defaultValue: false);
            Scribe_Values.Look(ref killIncappedTarget, "killIncappedTarget", defaultValue: false);
            Scribe_Values.Look(ref haulOpportunisticDuplicates, "haulOpportunisticDuplicates", defaultValue: false);
            Scribe_Values.Look(ref haulMode, "haulMode", HaulMode.Undefined);
            Scribe_Defs.Look(ref plantDefToSow, "plantDefToSow");
            Scribe_Values.Look(ref locomotionUrgency, "locomotionUrgency", LocomotionUrgency.Jog);
            Scribe_Values.Look(ref ignoreDesignations, "ignoreDesignations", defaultValue: false);
            Scribe_Values.Look(ref canBash, "canBash", defaultValue: false);
            Scribe_Values.Look(ref canUseRangedWeapon, "canUseRangedWeapon", defaultValue: true);
            Scribe_Values.Look(ref haulDroppedApparel, "haulDroppedApparel", defaultValue: false);
            Scribe_Values.Look(ref restUntilHealed, "restUntilHealed", defaultValue: false);
            Scribe_Values.Look(ref ignoreJoyTimeAssignment, "ignoreJoyTimeAssignment", defaultValue: false);
            Scribe_Values.Look(ref overeat, "overeat", defaultValue: false);
            Scribe_Values.Look(ref attackDoorIfTargetLost, "attackDoorIfTargetLost", defaultValue: false);
            Scribe_Values.Look(ref takeExtraIngestibles, "takeExtraIngestibles", 0);
            Scribe_Values.Look(ref expireRequiresEnemiesNearby, "expireRequiresEnemiesNearby", defaultValue: false);
            Scribe_Values.Look(ref collideWithPawns, "collideWithPawns", defaultValue: false);
            Scribe_Values.Look(ref forceSleep, "forceSleep", defaultValue: false);
            Scribe_Defs.Look(ref interaction, "interaction");
            Scribe_Values.Look(ref endIfCantShootTargetFromCurPos, "endIfCantShootTargetFromCurPos", defaultValue: false);
            Scribe_Values.Look(ref endIfCantShootInMelee, "endIfCantShootInMelee", defaultValue: false);
            Scribe_Values.Look(ref checkEncumbrance, "checkEncumbrance", defaultValue: false);
            Scribe_Values.Look(ref followRadius, "followRadius", 0f);
            Scribe_Values.Look(ref endAfterTendedOnce, "endAfterTendedOnce", defaultValue: false);
            Scribe_Defs.Look(ref workGiverDef, "workGiverDef");
            //Scribe_Defs.Look(ref jobGiverThinkTree, "jobGiverThinkTree");
            Scribe_Values.Look(ref doUntilGatheringEnded, "doUntilGatheringEnded", defaultValue: false);
            Scribe_Values.Look(ref psyfocusTargetLast, "psyfocusTargetLast", 0f);
            Scribe_Values.Look(ref wasOnMeditationTimeAssignment, "wasOnMeditationTimeAssignment", defaultValue: false);
            Scribe_Values.Look(ref reactingToMeleeThreat, "reactingToMeleeThreat", defaultValue: false);
            Scribe_References.Look(ref ability, "ability");
            //if (Scribe.mode == LoadSaveMode.Saving)
            //{
            //    jobGiverKey = ((jobGiver != null) ? jobGiver.UniqueSaveKey : (-1));
            //}
            //Scribe_Values.Look(ref jobGiverKey, "lastJobGiverKey", -1);
            //if (Scribe.mode == LoadSaveMode.PostLoadInit && jobGiverKey != -1 && !jobGiverThinkTree.TryGetThinkNodeWithSaveKey(jobGiverKey, out jobGiver))
            //{
            //    Log.Warning("Could not find think node with key " + jobGiverKey);
            //}
            if (Scribe.mode == LoadSaveMode.PostLoadInit && verbToUse != null && verbToUse.BuggedAfterLoading)
            {
                verbToUse = null;
                Log.Warning(string.Concat(GetType(), " had a bugged verbToUse after loading."));
            }
        }
    }
}
