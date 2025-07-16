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
                float successOdds = shoo.actor.GetStatValue(StatDefOf.TameAnimalChance, true) * 10f; // base odds
                successOdds *= Mathf.Max(0.05f, GenMath.LerpDouble(0f, 2f, 1.5f, 0.5f, pawn.BodySize / shoo.actor.BodySize)); // size factor
                successOdds *= Mathf.Max(0.05f, GenMath.LerpDouble(0f, 1f, 2f, 0f, pawn.RaceProps.wildness)); //wildness factor
                successOdds = Mathf.Clamp01(successOdds); // le clamp
                //successOdds = 1f; //hi guys I'm the debug flag

                if (Rand.Chance(successOdds))
                {
                    //Log.Message("Successful shoo attempt with odds " + successOdds);
                    pawn.Map.designationManager.RemoveDesignation(pawn.Map.designationManager.DesignationOn(pawn, ShooDefOf.Shoo));
                    //if((pawn.Map.areaManager.Home[pawn.Position] && RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out c)) ||
                    //    RCellFinder.TryFindDirectFleeDestination(pawn.Position, 30f, pawn, out c))
                    if (FindShooFleeCell(pawn, out IntVec3 c))
                    {
                        //JobTag.
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
                //shoo.actor.jobs.EndCurrentJob(JobCondition.Succeeded);
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
