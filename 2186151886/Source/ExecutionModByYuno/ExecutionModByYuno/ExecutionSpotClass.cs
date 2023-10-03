using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;

namespace ExecutionModByYuno
{
    public class Building_SpotExecution : Building
    {
        ushort countdown = 0;

        ushort countdownEnd = 10;

        Pawn ExeCutor;

        Pawn Prisoner;

        public State currentState = State.rest;

        private int _WatchRadius = 5;

        public void ResetSpot()
        {
            this.currentState = State.rest;
            this.countdown = 0;
        }

        public Pawn GetPrisonerToExecute(Pawn Warden)
        {
            foreach (Pawn p in base.Map.mapPawns.FreeColonistsAndPrisoners)
            {
                bool flag = p.guest.interactionMode == PrisonerInteractionModeDefOf.Execution && Warden.CanReserve(p, 1, 1, null, false);
                if (flag)
                {
                    return p;
                }
            }
            return null;
        }

        public int WatchRadius
        {
            get
            {
                return _WatchRadius;
            }
        }

        public int SetWatchRadius
        {
            set
            {
                _WatchRadius = value;
            }
        }


        public override string GetInspectString()
        {
            string ReturnString = base.GetInspectString();
            ReturnString += $"State:{currentState.ToString()}";
            ReturnString += $"\nCountDown:{countdown}/{countdownEnd}";
            return ReturnString;

        }

        public void StartExecution(Pawn pawn, Pawn Takee, IntVec3 cell)
        {
            LordMaker.MakeNewLord(Faction.OfPlayer, new Lord_Job_Execution(cell,this), this.Map);
            ExeCutor = pawn;
            Prisoner = Takee; 
            currentState = State.preparation;
        }


        public void GiveWardenExecutionOrder()
        {
            if(!ExeCutor.Drafted)
            {
                Job ExecuteJob = new Job(JobDefOf.Execute2, Prisoner, this);

                ExeCutor.jobs.StartJob(ExecuteJob);
                countdown = 0;
            }
            else
            {
                Messages.Message("Warden is drafted, execution is canceled.", MessageTypeDefOf.RejectInput, true);
                this.ResetSpot();
            }
        }


        public override void TickRare()
        {
            
            if(currentState == State.inMotion)
            {
            
                countdown += 1;
                if(countdown >= countdownEnd)
                {
                    GiveWardenExecutionOrder();
                }
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref _WatchRadius, "Radius", 5, false);
            Scribe_Values.Look<ushort>(ref countdown, "countdown", 0, false);
        }

        public enum State
        {
            rest,

            preparation,

            inMotion,

            unavailable
        }


    }
   
    
       
}
