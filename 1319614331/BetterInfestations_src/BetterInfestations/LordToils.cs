using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using System.Collections.Generic;

namespace BetterInfestations
{
    public class LordToil_DefendAndExpandHive : LordToil
    {
        public float distToHiveToAttack = 30f;
        public override void UpdateAllDuties()
        {
            if (lord != null && !lord.ownedPawns.NullOrEmpty())
            {
                for (int i = 0; i < lord.ownedPawns.Count; i++)
                {
                    Hive hive = HiveUtility.GetHive(lord.ownedPawns[i]);
                    if (hive == null) return;

                    PawnDuty duty = new PawnDuty(DutyDefOf.BI_DefendAndExpandHive, hive, distToHiveToAttack);
                    if (duty == null) return;

                    lord.ownedPawns[i].mindState.duty = duty;
                }
            }
        }
    }
    public class LordToil_DefendHiveAggressively : LordToil
    {
        public float distToHiveToAttack = 80f;
        public override void UpdateAllDuties()
        {
            if (lord != null && !lord.ownedPawns.NullOrEmpty())
            {
                for (int i = 0; i < lord.ownedPawns.Count; i++)
                {
                    Hive hive = HiveUtility.GetHive(lord.ownedPawns[i]);
                    if (hive == null) return;

                    PawnDuty duty = new PawnDuty(DutyDefOf.BI_DefendHiveAggressively, hive, distToHiveToAttack);
                    if (duty == null) return;

                    lord.ownedPawns[i].mindState.duty = duty;
                }
            }
        }
    }
    public class LordToil_HiveHunters : LordToil
    {
        public override void UpdateAllDuties()
        {
            if (lord != null && !lord.ownedPawns.NullOrEmpty())
            {
                for (int i = 0; i < lord.ownedPawns.Count; i++)
                {
                    PawnDuty duty = new PawnDuty(DutyDefOf.BI_HiveHunters, null);
                    if (duty == null) return;

                    lord.ownedPawns[i].mindState.duty = duty;
                }
            }
        }
    }
    public class LordToil_AssaultColony : LordToil
    {
        private bool attackDownedIfStarving;
        public override bool ForceHighStoryDanger => true;
        public override bool AllowSatisfyLongNeeds => false;
        public LordToil_AssaultColony(bool attackDownedIfStarving = false)
        {
            this.attackDownedIfStarving = attackDownedIfStarving;
        }
        public override void Init()
        {
            base.Init();
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.Drafting, OpportunityType.Critical);
        }
        public override void UpdateAllDuties()
        {
            if (lord != null && !lord.ownedPawns.NullOrEmpty())
            {
                for (int i = 0; i < lord.ownedPawns.Count; i++)
                {
                    PawnDuty duty = new PawnDuty(RimWorld.DutyDefOf.AssaultColony);
                    if (duty == null) return;

                    lord.ownedPawns[i].mindState.duty = duty;
                    lord.ownedPawns[i].mindState.duty.attackDownedIfStarving = attackDownedIfStarving;
                }
            }
        }
    }
}