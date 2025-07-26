using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace ResistanceRestraintsMod
{
    public class HediffSeveritySetting
    {
        public HediffDef hediffDef;
        public float minSeverity = 0f;
        public bool removeOnExit = true;
        public bool onlyApplyToNonRecruitable = false; // New flag
    }


    public class CompProperties_PrisonerRestrainedHediff : CompProperties
    {
        public List<HediffSeveritySetting> hediffsToApply = new List<HediffSeveritySetting>(); // List of hediffs with settings

        public CompProperties_PrisonerRestrainedHediff()
        {
            this.compClass = typeof(CompPrisonerRestrainedHediff);
        }
    }

    public class CompPrisonerRestrainedHediff : ThingComp
    {
        private Building_Bed bed => this.parent as Building_Bed;
        private Dictionary<Pawn, List<HediffSeveritySetting>> affectedPawns = new Dictionary<Pawn, List<HediffSeveritySetting>>();

        public CompProperties_PrisonerRestrainedHediff Props => (CompProperties_PrisonerRestrainedHediff)props;

        public override void CompTick()
        {
            base.CompTick();

            if (bed == null || Props.hediffsToApply.NullOrEmpty()) return;

            List<Pawn> currentOccupants = bed.CurOccupants?.ToList() ?? new List<Pawn>();

            // Apply hediffs to current occupants
            foreach (Pawn pawn in currentOccupants)
            {
                if (pawn == null || pawn.Dead) continue;

                if (!affectedPawns.ContainsKey(pawn))
                {
                    affectedPawns[pawn] = new List<HediffSeveritySetting>();
                }

                foreach (HediffSeveritySetting setting in Props.hediffsToApply)
                {
                    // Check if the pawn is a prisoner and if they are recruitable
                    bool isNonRecruitablePrisoner = pawn.guest?.IsPrisoner == true && !pawn.guest.Recruitable;

                    // If onlyApplyToNonRecruitable is true, only apply if pawn is non-recruitable
                    if (setting.onlyApplyToNonRecruitable && !isNonRecruitablePrisoner)
                    {
                        continue; // Skip applying the hediff
                    }

                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(setting.hediffDef);

                    if (hediff == null)
                    {
                        hediff = HediffMaker.MakeHediff(setting.hediffDef, pawn);
                        hediff.Severity = setting.minSeverity;
                        pawn.health.AddHediff(hediff);
                        affectedPawns[pawn].Add(setting);
                    }
                    else if (hediff.Severity < setting.minSeverity)
                    {
                        hediff.Severity = setting.minSeverity;
                    }
                }

            }

            // Remove hediffs from pawns who left, if they are set to be removed
            foreach (Pawn pawn in affectedPawns.Keys.ToList()) // Copy to avoid modification issues
            {
                if (!currentOccupants.Contains(pawn))
                {
                    foreach (HediffSeveritySetting setting in affectedPawns[pawn])
                    {
                        if (setting.removeOnExit)
                        {
                            Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(setting.hediffDef);
                            if (existingHediff != null)
                            {
                                pawn.health.RemoveHediff(existingHediff);
                            }
                        }
                    }
                    affectedPawns.Remove(pawn);
                }
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            // Remove all hediffs that are set to be removed on exit
            foreach (Pawn pawn in affectedPawns.Keys)
            {
                foreach (HediffSeveritySetting setting in affectedPawns[pawn])
                {
                    if (setting.removeOnExit)
                    {
                        Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(setting.hediffDef);
                        if (existingHediff != null)
                        {
                            pawn.health.RemoveHediff(existingHediff);
                        }
                    }
                }
            }

            affectedPawns.Clear();
        }
    }
}