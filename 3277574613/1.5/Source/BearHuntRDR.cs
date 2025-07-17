using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace RimRedRedemption
{
    [DefOf]
    public static class RDR_DefOf
    {
        public static QuestScriptDef RDR_LegendaryBearHunt;
        // Add other RDR-specific QuestScriptDefs here as needed
        public static PawnKindDef RDR_LegendBearPawn;
        public static ThingDef RDR_LegendBearDef;
    }

    public class WorldComponent_LegendaryHunts : WorldComponent
    {
        public int tickCounter;
        public int ticksToNextHunt = 30000 * 55; // About 55 days

        public WorldComponent_LegendaryHunts(World world) : base(world) { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (tickCounter > ticksToNextHunt)
            {
                List<QuestScriptDef> questList = new List<QuestScriptDef>()
                {
                    RDR_DefOf.RDR_LegendaryBearHunt
                    // Add other legendary hunts here as you develop them
                };

                Slate slate = new Slate();
                Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questList.RandomElement(), slate);
                QuestUtility.SendLetterQuestAvailable(quest);

                ticksToNextHunt = (int)(60000 * Rand.RangeInclusive(50, 70));
                tickCounter = 0;
            }
            tickCounter++;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.tickCounter, nameof(this.tickCounter));
            Scribe_Values.Look(ref this.ticksToNextHunt, nameof(this.ticksToNextHunt));
        }
    }
}