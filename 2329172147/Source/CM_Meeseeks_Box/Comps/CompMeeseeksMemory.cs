using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public class CompMeeseeksMemory : ThingComp
    {
        private static List<JobDef> freeJobs = new List<JobDef>();//{ JobDefOf.Equip, JobDefOf.Goto };
        public static List<JobDef> noContinueJobs = new List<JobDef>();// { JobDefOf.Goto };
        private static int maxQueueOrderTicks = 300;

        public bool givenTask = false;
        public bool startedTask = false;
        public bool taskCompleted = false;

        public bool temporarilyBlockTask = false;

        public SavedJob savedJob = null;
        public JobDef lastStartedJobDef = null;
        public LocalTargetInfo lastStartedJobTarget = null;
        public int lastStartedJobTick = 0;

        public bool jobStuck = false;

        public List<SavedTargetInfo> jobTargets = new List<SavedTargetInfo>();

        public int givenTaskTick = -1;
        public int acquiredEquipmentTick = -1;

        private bool destroyed = false;

        private Voice voice = new Voice();
        public Voice Voice => voice;

        private bool playedAcceptSound = false;

        private QualityCategory quality = QualityCategory.Awful;
        public QualityCategory Quality => quality;
        public int QualityInt => (int)quality;
        public int QualityPlusOneInt => ((int)quality + 1);

        private List<string> jobList = new List<string>();
        private List<string> jobResults = new List<string>();

        private Dictionary<WorkGiver, Thing> potentialTargetCache = new Dictionary<WorkGiver, Thing>();

        public CompProperties_MeeseeksMemory Props => (CompProperties_MeeseeksMemory)props;

        private Pawn creator = null;
        public Pawn Creator => creator;

        private List<Pawn> createdMeeseeks = new List<Pawn>();
        public List<Pawn> CreatedMeeseeks => createdMeeseeks;

        public bool GivenTask => givenTask;

        public IntVec3 guardPosition = IntVec3.Invalid;

        private Pawn meeseeks = null;
        public Pawn Meeseeks
        {
            get
            {
                if (meeseeks == null)
                    meeseeks = this.parent as Pawn;
                return meeseeks;
            }
        }

        public string GetDebugInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Jobs started: ");

            for (int i = 0; i < Math.Max(jobList.Count, jobResults.Count); ++i)
            {
                string jobName = "???";
                if (i < jobList.Count)
                    jobName = jobList[i];

                string jobResult = "???";
                if (i < jobResults.Count)
                    jobResult = jobResults[i];

                stringBuilder.AppendLine(jobName + " - " + jobResult);
            }

            return stringBuilder.ToString();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (Meeseeks != null)
            {
                bool hasPainHediff = meeseeks.health.hediffSet.HasHediff(MeeseeksDefOf.CM_Meeseeks_Box_Hediff_Existence_Is_Pain);
                if (!hasPainHediff)
                {
                    Hediff painHediff = HediffMaker.MakeHediff(MeeseeksDefOf.CM_Meeseeks_Box_Hediff_Existence_Is_Pain, meeseeks);
                    meeseeks.health.AddHediff(painHediff);
                }
            }

            //clothingPartPriority = new List<BodyPartGroupDef>();
            //clothingPartPriority.Add(BodyPartGroupDefOf.FullHead);
            //clothingPartPriority.Add(BodyPartGroupDefOf.UpperHead);
            //clothingPartPriority.Add(BodyPartGroupDefOf.Eyes);
            //clothingPartPriority.Add(BodyPartGroupDefOf.Torso);
            //clothingPartPriority.Add(BodyPartGroupDefOf.Legs);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<bool>(ref this.givenTask, "givenTask", false);
            Scribe_Values.Look<bool>(ref this.startedTask, "startedTask", false);
            Scribe_Values.Look<bool>(ref this.taskCompleted, "taskCompleted", false);

            Scribe_Values.Look<bool>(ref this.playedAcceptSound, "playedAcceptSound", false);

            Scribe_References.Look(ref this.creator, "creator");
            Scribe_Values.Look(ref this.quality, "quality");

            Scribe_Values.Look<int>(ref this.givenTaskTick, "givenTaskTick", -1);
            Scribe_Values.Look<int>(ref this.acquiredEquipmentTick, "acquiredWeaponTick", -1);

            Scribe_Deep.Look(ref savedJob, "savedJob");
            Scribe_Defs.Look(ref lastStartedJobDef, "lastStartedJobDef");
            Scribe_TargetInfo.Look(ref lastStartedJobTarget, "lastStartedJobTarget");
            Scribe_Values.Look<int>(ref lastStartedJobTick, "lastStartedJobTick");

            Scribe_Deep.Look(ref voice, "Voice");

            Scribe_Collections.Look(ref jobList, "jobList");
            Scribe_Collections.Look(ref jobResults, "jobResult");
            Scribe_Collections.Look(ref jobTargets, "jobTargets", LookMode.Deep);

            Scribe_Collections.Look(ref createdMeeseeks, "createdMeeseeks", LookMode.Reference);

            if (jobList == null)
                jobList = new List<string>();
            if (jobResults == null)
                jobResults = new List<string>();
            if (jobTargets == null)
                jobTargets = new List<SavedTargetInfo>();

            if (Scribe.mode == LoadSaveMode.PostLoadInit && voice == null)
            {
                voice = new Voice();
            }
        }

        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);

            // Awful quality -> take normal damage, Legendary -> take a quarter
            float IncomingDamageDivisor = (1.0f + ((float)(QualityInt) * 0.5f));
            float damageAmount = dinfo.Amount / IncomingDamageDivisor;

            //Logger.MessageFormat(this, "Incoming damaged divided: {0} / {1} = {2}", dinfo.Amount, IncomingDamageDivisor, damageAmount);

            dinfo.SetAmount(damageAmount);
        }

        public void CleanupMemory()
        {
            jobList = new List<string>();
            jobResults = new List<string>();
            jobTargets = new List<SavedTargetInfo>();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (Meeseeks.Downed && !destroyed)
            {
                destroyed = true;
                Logger.MessageFormat(this, "Meeseeks downed. Vanishing.");
                MeeseeksUtility.DespawnMeeseeks(Meeseeks);
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);

            if (Meeseeks.Dead && !destroyed)
            {
                destroyed = true;
                Logger.MessageFormat(this, "Meeseeks dead. Vanishing.");
                MeeseeksUtility.DespawnMeeseeks(Meeseeks);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            Logger.MessageFormat(this, "Meeseeks despawned \n{0}", this.GetDebugInfo());
        }

        public void SetQuality(QualityCategory setQuality)
        {
            quality = setQuality;
        }

        public void SetCreator(Pawn creatingPawn)
        {
            CompMeeseeksMemory creatorMemory = creatingPawn.GetComp<CompMeeseeksMemory>();
            if (creatorMemory != null)
            {
                creatorMemory.AddChildMeeseeks(Meeseeks);
            }

            creator = creatingPawn;
        }

        public void AddChildMeeseeks(Pawn child)
        {
            createdMeeseeks.Add(child);
        }

        public void CopyJobDataFrom(CompMeeseeksMemory otherMemory)
        {
            if (otherMemory.givenTask)
            {
                jobTargets = new List<SavedTargetInfo>(otherMemory.jobTargets);

                givenTask = otherMemory.givenTask;
                startedTask = otherMemory.startedTask;
                taskCompleted = otherMemory.taskCompleted;

                savedJob = new SavedJob(otherMemory.savedJob.MakeJob());

                givenTaskTick = Find.TickManager.TicksGame;

                guardPosition = otherMemory.guardPosition;
                if (guardPosition.IsValid)
                    ((Pawn)parent).drafter.Drafted = true;

                MeeseeksUtility.PlayAcceptTaskSound(this.parent, voice);
            }
        }

        public void ForceNewJob(Job newJob, SavedTargetInfo targetInfo)
        {
            jobTargets = new List<SavedTargetInfo> { targetInfo };

            givenTask = true;
            startedTask = true;
            taskCompleted = false;

            savedJob = new SavedJob(newJob);

            givenTaskTick = Find.TickManager.TicksGame;

            potentialTargetCache.Clear();
        }

        public void AddToPotentialTargetCache(WorkGiver workGiver, Thing thing)
        {
            potentialTargetCache[workGiver] = thing;
        }

        public void AddJobTarget(SavedTargetInfo target, bool firstTarget = false)
        {
            if (jobTargets.Contains(target))
                return;

            if (target.HasThing && savedJob.bill != null)
            {
                MeeseeksBillStorage billStorage = Current.Game.World.GetComponent<MeeseeksBillStorage>();

                if (firstTarget)
                {
                    billStorage.SaveBill(savedJob.bill);
                    savedJob.bill = billStorage.GetDuplicateBillFromOriginal(savedJob.bill);
                    savedJob.bill.billStack.billGiver = target.Thing as IBillGiver;
                }

                WorkGiver_Scanner workGiverScanner = savedJob.workGiverDef.Worker as WorkGiver_Scanner;
                if (workGiverScanner != null)
                {
                    Job job = workGiverScanner.JobOnThing(Meeseeks, target.Thing, true);
                    if (job != null && job.bill != null)
                    {
                        billStorage.SaveBill(job.bill);
                        target.bill = billStorage.GetDuplicateBillFromOriginal(job.bill);
                        target.bill.billStack.billGiver = target.Thing as IBillGiver;
                    }
                }
            }
            else if (target.HasThing && savedJob.IsConstruction)
            {
                ThingDef thingDefToBuild = null;
                TerrainDef terrainDefToBuild = null;
                ThingDef stuffDefToUse = null;

                Blueprint_Build blueprint = target.Thing as Blueprint_Build;
                Frame frame = target.Thing as Frame;

                if (blueprint != null)
                {
                    thingDefToBuild = blueprint.def.entityDefToBuild as ThingDef;
                    terrainDefToBuild = blueprint.def.entityDefToBuild as TerrainDef;
                    stuffDefToUse = blueprint.stuffToUse;
                }
                else if (frame != null)
                {
                    thingDefToBuild = frame.def.entityDefToBuild as ThingDef;
                    terrainDefToBuild = frame.def.entityDefToBuild as TerrainDef;
                    stuffDefToUse = frame.Stuff;
                }

                if (thingDefToBuild != null)
                {
                    target.blueprintThingDef = thingDefToBuild;
                }
                else if (terrainDefToBuild != null)
                {
                    target.blueprintTerrainDef = terrainDefToBuild;
                }

                target.blueprintStuff = stuffDefToUse;
                target.blueprintRotation = target.Thing.Rotation;

                // Redirect job to the cell so that we can continue various construction phases and replace blueprint if needed
                target.target = target.Cell;
            }
            else if (target.HasThing && savedJob.IsTraining)
            {
                target.trainable = (target.Thing as Pawn).training.NextTrainableToTrain();
            }

            jobTargets.Add(target);
        }

        public void SortJobTargets()
        {
            if (jobTargets.Count == 0)
                return;

            jobTargets.Sort((a, b) => (int)(Meeseeks.PositionHeld.DistanceToSquared(a.Cell) - Meeseeks.PositionHeld.DistanceToSquared(b.Cell)));

            // If it is a kill job, kill self last
            if (savedJob != null && savedJob.def == MeeseeksDefOf.CM_Meeseeks_Box_Job_Kill)
            {
                int indexOfSelf = jobTargets.FindIndex(target => target.Thing == Meeseeks);
                if (indexOfSelf >= 0)
                {
                    SavedTargetInfo selfTarget = jobTargets[indexOfSelf];
                    jobTargets.RemoveAt(indexOfSelf);
                    jobTargets.Add(selfTarget);
                }
            }
        }

        public bool HasTimeToQueueNewJob()
        {
            if (temporarilyBlockTask)
                return false;

            int ticksSinceOrder = Find.TickManager.TicksGame - givenTaskTick;
            return (givenTask && ticksSinceOrder < maxQueueOrderTicks && savedJob != null && !noContinueJobs.Contains(savedJob.def));
        }

        public bool CanTakeOrders()
        {
            if (temporarilyBlockTask)
                return false;

            if (!givenTask)
                return true;

            return false;

            //bool canQueueNewJob = (KeyBindingDefOf.QueueOrder.IsDownEvent && HasTimeToQueueNewJob());
            //return canQueueNewJob;
        }

        public bool CreatedByMeeseeks()
        {
            if (creator == null)
                return false;

            if (creator.GetComp<CompMeeseeksMemory>() == null)
                return false;

            return true;
        }

        public bool CanTakeOrderedJob(Job job)
        {
            if (!givenTask)
                return true;

            bool canQueueNewJob = (KeyBindingDefOf.QueueOrder.IsDownEvent && HasTimeToQueueNewJob());
            bool jobIsSame = (job.def == savedJob.def);

            return (canQueueNewJob && jobIsSame);
        }

        public void PreTryTakeOrderedJob(Job job)
        {
            // This allows Mr Meeseeks to know what clothes he could wear if he does take the job
            if (!givenTask)
            {
                Meeseeks.mindState.nextApparelOptimizeTick = Find.TickManager.TicksGame - 1;
                savedJob = new SavedJob(job);
            }
        }

        // This sometimes can get called out of order from normal flow, by Achtung mod for example
        public void PostTryTakeOrderedJob(bool success, Job job)
        {
            // If he didn't take the job and hasn't been officially given one, clear out the saved job
            if (!success && !givenTask)
                savedJob = null;

            if (success)
                JobStarted(job);
        }

        public void PreStartJob(Job job, JobDriver driver)
        {
            if (job != null)
            {
                job.ignoreDesignations = true;
                job.ignoreForbidden = true;
            }
        }

        public void PostStartJob(Job job, JobDriver driver)
        {
            if (job == null)
            {
                Logger.MessageFormat(this, "Meeseeks attempting to start null job...");
                return;
            }
            string jobName = job.def.defName;

            jobStuck = (lastStartedJobDef == job.def && lastStartedJobTarget == job.targetA && lastStartedJobTick == Find.TickManager.TicksGame);

            lastStartedJobDef = job.def;
            lastStartedJobTarget = job.targetA;
            lastStartedJobTick = Find.TickManager.TicksGame;

            jobList.Add(jobName);

            JobStarted(job);
        }

        private void JobStarted(Job job)
        {
            if (givenTask || !job.playerForced || freeJobs.Contains(job.def))
                return;


            givenTask = true;
            startedTask = true;
            givenTaskTick = Find.TickManager.TicksGame;

            if (!playedAcceptSound)
            {
                MeeseeksUtility.PlayAcceptTaskSound(this.parent, voice);
                playedAcceptSound = true;
            }

            savedJob = new SavedJob(job);

            if (job.workGiverDef != null && job.workGiverDef.Worker != null && potentialTargetCache.ContainsKey(job.workGiverDef.Worker))
            {
                AddJobTarget(new SavedTargetInfo(potentialTargetCache[job.workGiverDef.Worker]), true);
            }
            else
            {
                TargetIndex targetIndex = GetJobPrimaryTarget(job);

                if (targetIndex != TargetIndex.None)
                {
                    AddJobTarget(new SavedTargetInfo(job.GetTarget(targetIndex)), true);
                }
                else
                {
                    Logger.MessageFormat(this, "No target found for {0}", job.def.defName);
                }
            }

            potentialTargetCache.Clear();
        }

        public bool JobStuckRepeat(Job newJob)
        {
            if (jobStuck)
            {
                jobStuck = false;

                if (newJob != null && newJob.def == lastStartedJobDef && newJob.targetA == lastStartedJobTarget && Find.TickManager.TicksGame == lastStartedJobTick)
                {
                    return true;
                }
            }

            return false;
        }

        private TargetIndex GetJobPrimaryTarget(Job job)
        {
            TargetIndex result = TargetIndex.None;

            if (job.workGiverDef != null && job.workGiverDef.Worker != null)
            {
                WorkGiver_Scanner workGiverScanner = job.workGiverDef.Worker as WorkGiver_Scanner;
                if (workGiverScanner != null)
                {
                    if (this.HasSameJobOnThing(job, workGiverScanner, job.targetA))
                        return TargetIndex.A;
                    if (this.HasSameJobOnThing(job, workGiverScanner, job.targetB))
                        return TargetIndex.B;
                    if (this.HasSameJobOnThing(job, workGiverScanner, job.targetC))
                        return TargetIndex.C;

                    if (this.HasSameJobOnCell(job, workGiverScanner, job.targetA))
                        return TargetIndex.A;
                    if (this.HasSameJobOnCell(job, workGiverScanner, job.targetB))
                        return TargetIndex.B;
                    if (this.HasSameJobOnCell(job, workGiverScanner, job.targetC))
                        return TargetIndex.C;
                }
                else
                {
                    //Logger.MessageFormat(this, "Non scanner WorkGiver", job.workGiverDef.defName);
                }
            }
            else
            {
                //Logger.MessageFormat(this, "No workGiverDef found for {0}", job.def.defName);
            }

            if (result == TargetIndex.None)
            {
                // Let the hacks begin!
                // First, hauling to a construction job
                if (job.def == JobDefOf.HaulToContainer && WorkerDefUtility.constructionDefs.Contains(job.workGiverDef))
                {
                    if (job.targetC.IsValid)
                        result = TargetIndex.C;
                }

                if (result == TargetIndex.None)
                {
                    // There shold only be a few non scanner, non workGiver jobs that a Meeseeks could get, lets default to whatever targets we have if possible
                    if (HasValidTarget(job.targetA))
                        result = TargetIndex.A;
                    if (HasValidTarget(job.targetB))
                        result = TargetIndex.B;
                    if (HasValidTarget(job.targetC))
                        result = TargetIndex.C;


                    if (result != TargetIndex.None)
                        Logger.WarningFormat(this, "Had to default to any target for job {0}", job.def.defName);
                    else
                        Logger.WarningFormat(this, "Could not find any target at all for job {0}", job.def.defName);
                }
            }

            return result;
        }

        private bool HasSameJobOnThing(Job job, WorkGiver_Scanner workGiverScanner, LocalTargetInfo targetInfo)
        {
            if (HasValidTarget(targetInfo) && targetInfo.HasThing)
            {
                Job getJob = workGiverScanner.JobOnThing(Meeseeks, targetInfo.Thing, true);
                if (getJob != null && getJob.def == job.def)
                    return true;
            }

            return false;
        }

        private bool HasSameJobOnCell(Job job, WorkGiver_Scanner workGiverScanner, LocalTargetInfo targetInfo)
        {
            if (HasValidTarget(targetInfo) && !targetInfo.HasThing)
            {
                Job getJob = workGiverScanner.JobOnCell(Meeseeks, targetInfo.Cell, true);
                if (getJob != null && getJob.def == job.def)
                    return true;
            }

            return false;
        }

        private bool HasValidTarget(LocalTargetInfo targetInfo)
        {
            return (targetInfo != null && this.TargetValid(targetInfo));
        }

        private bool HasInvalidTarget(LocalTargetInfo targetInfo)
        {
            return (targetInfo != null && !this.TargetValid(targetInfo));
        }

        private bool TargetValid(LocalTargetInfo targetInfo)
        {
            if (!targetInfo.IsValid)
                return false;

            if (!targetInfo.HasThing )
                return true;

            Thing target = targetInfo.Thing;

            if (target.Destroyed)
                return false;

            //Pawn pawn = targetInfo.Pawn;
            //if (pawn != null && pawn.Dead)
            //    return false;

            return true;
        }

        public void EndCurrentJob(Job job, JobDriver jobDriver, JobCondition condition)
        {
            //Logger.MessageFormat(this, "Meeseeks has finished job: {0}, {1}", job.def.defName, condition.ToString());

            jobResults.Add(condition.ToString());
        }

        public string GetSummary()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(Meeseeks.GetUniqueLoadID());

            stringBuilder.AppendLine("Jobs started: ");
            foreach (string jobName in jobList)
            {
                stringBuilder.AppendLine(jobName);
            }

            return stringBuilder.ToString();
        }
    }
}
