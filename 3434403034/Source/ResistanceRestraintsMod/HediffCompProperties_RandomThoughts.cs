using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ResistanceRestraintsMod
{
    public class HediffCompProperties_RandomThoughts : HediffCompProperties
    {
        public List<ThoughtDef> possibleThoughts = new List<ThoughtDef>(); // List of possible thoughts to add
        public int minTicksBetweenThoughts = 6000; // Min time before applying a thought (1 in-game hour)
        public int maxTicksBetweenThoughts = 36000; // Max time before applying a thought (6 in-game hours)

        public HediffCompProperties_RandomThoughts()
        {
            this.compClass = typeof(HediffComp_RandomThoughts);
        }
    }

    public class HediffComp_RandomThoughts : HediffComp
    {
        private int nextThoughtTick = 0; // Next time to apply a thought

        public HediffCompProperties_RandomThoughts Props => (HediffCompProperties_RandomThoughts)this.props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            ScheduleNextThought();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn == null || Props.possibleThoughts.NullOrEmpty())
                return;

            if (Find.TickManager.TicksGame >= nextThoughtTick)
            {
                TryApplyRandomThought();
                ScheduleNextThought();
            }
        }

        private void ScheduleNextThought()
        {
            nextThoughtTick = Find.TickManager.TicksGame + Rand.Range(Props.minTicksBetweenThoughts, Props.maxTicksBetweenThoughts);
        }

        private void TryApplyRandomThought()
        {
            if (Pawn.needs?.mood == null) return;

            List<ThoughtDef> validThoughts = Props.possibleThoughts.FindAll(t => ThoughtUtility.CanGetThought(Pawn, t));
            if (validThoughts.NullOrEmpty()) return;

            ThoughtDef selectedThought = validThoughts.RandomElement();
            Pawn.needs.mood.thoughts.memories.TryGainMemory(selectedThought);

        }
    }
}
