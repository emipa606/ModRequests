﻿using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace JecsTools
{
    public interface ICaravanJobEndable
    {
        Caravan GetActor();

        void AddEndCondition(Func<JobCondition> newEndCondition);
    }

    //Duplicate of RimWorld's Toil -- made for Caravans
    public class CaravanToil : ICaravanJobEndable
    {
        public Caravan actor;

        public bool atomicWithPrevious;

        public ToilCompleteMode defaultCompleteMode = ToilCompleteMode.Instant;

        public int defaultDuration;

        public List<Func<JobCondition>> endConditions = new List<Func<JobCondition>>();

        public List<Action> finishActions;

        public bool handlingFacing;

        public Action initAction;

        public List<Action> preInitActions;

        public List<Action> preTickActions;

        public RandomSocialMode socialMode = RandomSocialMode.Normal;

        public Action tickAction;

        public Caravan GetActor()
        {
            return actor;
        }

        public void AddEndCondition(Func<JobCondition> newEndCondition)
        {
            endConditions.Add(newEndCondition);
        }

        public void Cleanup()
        {
            if (finishActions != null)
                for (var i = 0; i < finishActions.Count; i++)
                    try
                    {
                        finishActions[i]();
                    }
                    catch (Exception ex)
                    {
                        var curJob = CaravanJobsUtility.GetCaravanJobGiver().CurJob(actor);
                        Log.Error($"Pawn {actor} threw exception while executing toil's finish action ({i}), curJob={curJob}: {ex}");
                    }
        }

        public void AddFailCondition(Func<bool> newFailCondition)
        {
            endConditions.Add(() => newFailCondition() ? JobCondition.Incompletable : JobCondition.Ongoing);
        }

        public void AddPreInitAction(Action newAct)
        {
            preInitActions ??= new List<Action>();
            preInitActions.Add(newAct);
        }

        public void AddPreTickAction(Action newAct)
        {
            preTickActions ??= new List<Action>();
            preTickActions.Add(newAct);
        }

        public void AddFinishAction(Action newAct)
        {
            finishActions ??= new List<Action>();
            finishActions.Add(newAct);
        }
    }
}
