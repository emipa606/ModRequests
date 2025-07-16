using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using System.Reflection;

namespace Shoo
{
    class JobDriver_Shoo : JobDriver
    {
        private readonly SimpleCurve relativeBodySizeFactor = new SimpleCurve
        {
            new CurvePoint(0.2f, 5f), // squirrel
            new CurvePoint(1f, 1f), // deer, big cat
            new CurvePoint(2f, 1f), // elk, camel
            new CurvePoint(4f, 0.1f), // elephant
        };

        private readonly SimpleCurve wildnessFactor = new SimpleCurve
        {
            new CurvePoint(0f, 5f), // housepets
            new CurvePoint(0.25f, 2f), // alpaca, dromedary
            new CurvePoint(0.75f, 1f), // deer, wolf
            new CurvePoint(1f, 0.1f), // thrumbo
        };

        protected const TargetIndex AnimalInd = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnDowned(TargetIndex.A);
            this.FailOnNotCasualInterruptible(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil shoo = new Toil();
            shoo.initAction = delegate
            {
                Pawn pawn = (Pawn)shoo.actor.CurJob.GetTarget(TargetIndex.A);
                pawn.mindState.lastAssignedInteractTime = Find.TickManager.TicksGame;
                float successOdds = shoo.actor.GetStatValue(StatDefOf.TameAnimalChance, true); // base odds
                successOdds *= relativeBodySizeFactor.Evaluate(pawn.BodySize / shoo.actor.BodySize); // size factor
                successOdds *= wildnessFactor.Evaluate(pawn.RaceProps.wildness); //wildness factor
                successOdds = Mathf.Clamp01(successOdds);
                //successOdds = 1f; //hi guys I'm the debug flag

                if (Rand.Chance(successOdds))
                {
                    //Log.Message("Successful shoo attempt with odds " + successOdds);
                    pawn.Map.designationManager.RemoveDesignation(pawn.Map.designationManager.DesignationOn(pawn, ShooDefOf.Shoo));
                    if (FindShooFleeCell(pawn, out IntVec3 c))
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
                        pawn.jobs.TryTakeOrderedJob(new Job(ShooDefOf.FleeBecauseShoo, c, shoo.actor) { canBashDoors = true }); // SOMEHOW canBash makes it work for herd animals. Don't ask me to explain it, because I can't.
                        MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, "ShooSuccess".Translate());               // No, I mean it, Mehni and I went through the pathfinder line by line. We don't know why it won't work without that.
                    }
                    else
                    {
                        Log.Warning("Could not find a valid shoo location for " + pawn.LabelShort);
                    }
                }
                else
                {
                    //Log.Message("Failed shoo attempt with odds " + successOdds);
                    MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, "ShooFailure".Translate(new object[] { successOdds.ToStringPercent() }));
                    if (Rand.Chance(pawn.RaceProps.manhunterOnTameFailChance / 2f))
                    {
                        pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
                        string label = "ManhuntOnFailedShooLabel".Translate(new object[] { pawn.LabelShort });
                        string text = "ManhuntOnFailedShoo".Translate(new object[]{ pawn.LabelShort });
                        Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.ThreatSmall, pawn);
                    }
                }
            };
            yield return shoo;
        }

        private bool FindShooFleeCell(Pawn p, out IntVec3 cell, int minFleeDist = 30, int maxFleeDist = 50)
        {
            //try to find cells between 30 and 50 cells away that are not inside of a building
            //if we fail to do so, at least try to find one that's walkable
            IntVec3 root = p.Position;
            for (int i = 0; i < 100; i++)
            {
                cell = root + IntVec3.FromVector3(Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.RangeInclusive(minFleeDist, maxFleeDist));
                Room room = cell.GetRoom(pawn.Map);
                if (cell.Walkable(pawn.Map) && (i > 50 || room == null || room.IsHuge))
                {
                    //Log.Message("found a shoo destination cell at: " + cell.ToString() + " vs our position " + root.ToString() + " on attempt " + i);
                    return true;
                }
            }

            //Log.Message("failed to find a shoo destination cell");
            cell = p.Position;
            return false;
        }
    }
}
