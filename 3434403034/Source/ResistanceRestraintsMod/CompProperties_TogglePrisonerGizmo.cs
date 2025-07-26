using System.Linq;
using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;
using RimWorld.Planet;

namespace ResistanceRestraintsMod
{
    public class CompProperties_TogglePrisonerGizmo : CompProperties
    {
        public bool onlyNonRecruitablePrisoners = false;
        public bool onlyRecruitablePrisoners = false;
        public bool recruitableOrSpecificHediff = false;
        public List<HediffDef> excludedHediffs = new List<HediffDef>();
        public List<HediffDef> includedHediffs = new List<HediffDef>();

        public CompProperties_TogglePrisonerGizmo()
        {
            this.compClass = typeof(CompTogglePrisonerGizmo);
        }
    }

    public class CompTogglePrisonerGizmo : ThingComp
    {
        public CompProperties_TogglePrisonerGizmo Props => (CompProperties_TogglePrisonerGizmo)this.props;
        private CompRefuelable compRefuelable => parent.GetComp<CompRefuelable>();
        private CompPowerTrader compPowerTrader => parent.GetComp<CompPowerTrader>();

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Building_Bed bed = parent as Building_Bed;
            if (bed == null)
                yield break;

            // Check if the building has both fuel and power
            bool hasFuel = compRefuelable?.HasFuel ?? true; // Default to true if no fuel component
            bool hasPower = compPowerTrader?.PowerOn ?? true; // Default to true if no power component

            if (!hasFuel || !hasPower)
                yield break; // Hide gizmos if out of fuel or power

            bool isSensoryCollapser = parent.def.defName == "SilkCircuit_SensoryCollapser";

            if (!bed.CurOccupants.Any())
            {
                yield return new Command_Action
                {
                    defaultLabel = isSensoryCollapser ? "Select unwavering prisoner" : "Select prisoner",
                    defaultDesc = isSensoryCollapser ?
                        "Select an unwaveringly loyal prisoner for a warden to escort to this sensory collapser. A successful sensory collapse will trigger the onset of early-stage stockholm syndrome." :
                        "Select a recruitable prisoner or a prisoner with stockholm syndrome for a warden to escort to this restraining device.",
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/Execute"),
                    action = ShowPrisonerSelectionMenu
                };
            }
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "Release prisoner",
                    defaultDesc = isSensoryCollapser ?
                        "Cancel the sensory collapse and order a warden to release the prisoner from restraints." :
                        "Order a warden to release the prisoner from their restraints.",
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/Banish"),
                    action = ReleasePrisoner
                };
            }
        }

        private void ShowPrisonerSelectionMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            CompProperties_TogglePrisonerGizmo props = (CompProperties_TogglePrisonerGizmo)this.props;

            List<Pawn> prisoners = Find.CurrentMap.mapPawns.PrisonersOfColony
                .Where(p =>
                    !p.Downed &&
                    (!props.onlyNonRecruitablePrisoners || !p.guest.Recruitable) &&
                    (!props.onlyRecruitablePrisoners || p.guest.Recruitable) &&
                    !HasExcludedHediff(p, props.excludedHediffs) &&
                    (props.recruitableOrSpecificHediff ? (p.guest.Recruitable || HasIncludedHediff(p, props.includedHediffs)) : true)
                )
                .ToList();

            if (prisoners.NullOrEmpty())
            {
                options.Add(new FloatMenuOption("No available prisoners", null));
            }
            else
            {
                foreach (Pawn prisoner in prisoners)
                {
                    options.Add(new FloatMenuOption(prisoner.LabelShort, () => AssignPrisoner(prisoner)));
                }
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private bool HasIncludedHediff(Pawn pawn, List<HediffDef> includedHediffs)
        {
            if (includedHediffs.NullOrEmpty())
                return false;

            return pawn.health.hediffSet.hediffs.Any(hediff => includedHediffs.Contains(hediff.def));
        }

        private bool HasExcludedHediff(Pawn pawn, List<HediffDef> excludedHediffs)
        {
            if (excludedHediffs.NullOrEmpty())
                return false;

            return pawn.health.hediffSet.hediffs.Any(hediff => excludedHediffs.Contains(hediff.def));
        }

        private void AssignPrisoner(Pawn prisoner)
        {
            if (prisoner == null || !prisoner.IsPrisoner || prisoner.Downed)
                return;

            Pawn warden = FindWarden(prisoner);
            if (warden == null)
            {
                Messages.Message("No available warden right now. Job queued.", MessageTypeDefOf.NeutralEvent, false);
                return;
            }

            Job job = JobMaker.MakeJob(JobDefOf.EscortPrisonerToBed, prisoner, parent);
            job.count = 1;

            if (warden.jobs.curJob != null)
            {
                warden.jobs.jobQueue.EnqueueFirst(job); // Ensures the job is executed after their current one
            }
            else
            {
                warden.jobs.TryTakeOrderedJob(job);
            }
        }


        private void ReleasePrisoner()
        {
            Building_Bed bed = parent as Building_Bed;
            if (bed == null)
                return;

            Pawn prisoner = bed.CurOccupants.FirstOrDefault();
            if (prisoner == null || !prisoner.IsPrisoner)
                return;

            Pawn warden = FindWarden(prisoner);

            // Create the job regardless of warden availability
            Job job = JobMaker.MakeJob(JobDefOf_SilkCircuit.SilkCircuit_RemoveImmobility, prisoner);

            if (warden != null)
            {
                // Assign the job to the available warden
                if (warden.jobs.curJob != null)
                {
                    warden.jobs.jobQueue.EnqueueFirst(job);
                }
                else
                {
                    warden.jobs.TryTakeOrderedJob(job);
                }
            }
            else
            {
                // No available warden now? Queue the job globally.
                prisoner.Map.pawnDestinationReservationManager.Reserve(null, job, prisoner.Position);
                prisoner.jobs.StartJob(job, JobCondition.InterruptForced, null, true);
                Messages.Message("No available warden now. The job has been queued and will be assigned when a warden is free.", MessageTypeDefOf.NeutralEvent, false);
            }
        }


        private Pawn FindWarden(Pawn prisoner)
        {
            return prisoner.Map.mapPawns.FreeColonists
                .Where(p => p.workSettings.WorkIsActive(WorkTypeDefOf.Warden))
                .OrderBy(p => p.jobs.curJob != null) // Prefer idle wardens first
                .ThenByDescending(p => p.skills.GetSkill(SkillDefOf.Social).Level) // Then by skill level
                .FirstOrDefault();
        }


    }
}
