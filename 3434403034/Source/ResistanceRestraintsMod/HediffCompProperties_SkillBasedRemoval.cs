using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_SkillBasedRemoval : HediffCompProperties
    {
        public int minTicks = 600; // Minimum interval
        public int maxTicks = 1200; // Maximum interval
        public float baseRemovalChance = 0.1f; // Base chance per tick
        public string replacementHediffDef; // Hediff to apply on removal
        public List<string> restrictedHediffs = new List<string>(); // List of hediffs that prevent replacement

        public HediffCompProperties_SkillBasedRemoval()
        {
            this.compClass = typeof(HediffComp_SkillBasedRemoval);
        }
    }

    public class HediffComp_SkillBasedRemoval : HediffComp
    {
        private int nextCheckTick;

        public HediffCompProperties_SkillBasedRemoval Props => (HediffCompProperties_SkillBasedRemoval)this.props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            ScheduleNextCheck();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn == null || Pawn.Dead || Find.TickManager.TicksGame < nextCheckTick)
                return;

            TryRemoveHediff();

            ScheduleNextCheck();
        }

        private void TryRemoveHediff()
        {
            if (Pawn == null || Pawn.Dead)
                return;

            float skillFactor = GetAverageRelevantSkill();
            float removalChance = Props.baseRemovalChance * skillFactor;

            if (Rand.Value < removalChance)
            {
                Pawn.health.RemoveHediff(parent);

                if (ShouldApplyReplacementHediff())
                {
                    ApplyReplacementHediff();
                }
            }
        }

        private bool ShouldApplyReplacementHediff()
        {
            if (string.IsNullOrEmpty(Props.replacementHediffDef))
                return false;

            // Check if pawn is a prisoner and is recruitable
            bool isRecruitablePrisoner = Pawn.guest?.IsPrisoner == true && Pawn.guest.Recruitable;

            // Check if pawn has restricted hediffs
            bool hasRestrictedHediffs = false;
            foreach (string hediffDefName in Props.restrictedHediffs)
            {
                HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(hediffDefName);
                if (hediffDef != null && Pawn.health.hediffSet.HasHediff(hediffDef))
                {
                    hasRestrictedHediffs = true;
                    break;
                }
            }

            // Apply if recruitable prisoner OR does NOT have restricted hediffs
            return isRecruitablePrisoner || !hasRestrictedHediffs;
        }

        private void ApplyReplacementHediff()
        {
            HediffDef newHediff = DefDatabase<HediffDef>.GetNamedSilentFail(Props.replacementHediffDef);
            if (newHediff != null)
            {
                Pawn.health.AddHediff(newHediff);
            }
        }

        private float GetAverageRelevantSkill()
        {
            if (Pawn?.skills == null)
                return 1.0f; // Default to 1 if no skills exist (e.g., animals, mechanoids)

            float totalSkill = 0;
            int count = 0;

            foreach (SkillDef skill in DefDatabase<SkillDef>.AllDefs)
            {
                SkillRecord skillRecord = Pawn.skills.GetSkill(skill);
                if (skillRecord != null && skillRecord.Level > 0)
                {
                    totalSkill += skillRecord.Level;
                    count++;
                }
            }

            return count > 0 ? totalSkill / count / 20f : 0.1f; // Scale between 0.1 and 1 based on skill level (20 is max skill)
        }

        private void ScheduleNextCheck()
        {
            nextCheckTick = Find.TickManager.TicksGame + Rand.RangeInclusive(Props.minTicks, Props.maxTicks);
        }

        // === UI DISPLAY FUNCTION ===
        public override string CompTipStringExtra
        {
            get
            {
                if (Pawn == null) return base.CompTipStringExtra;

                float skillFactor = GetAverageRelevantSkill();
                float removalChance = Props.baseRemovalChance * skillFactor;
                int ticksUntilEscape = Math.Max(0, nextCheckTick - Find.TickManager.TicksGame);
                string timeLeft = ticksUntilEscape.ToStringTicksToPeriod();

                return $"Escape Chance: {removalChance:P}\nTime Until Attempt: {timeLeft}";
            }
        }
    }
}
