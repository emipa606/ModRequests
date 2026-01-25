using RimFridge;
using Verse;

namespace MoreFrostyRimFridgeDrinks
{
    internal class CompAlwaysFrosty : CompFrosty
    {
        public override void PostIngested(Pawn ingester) 
            => ingester.needs.mood.thoughts.memories.TryGainMemory(Props.thought);

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            temperature = 0f;
        }

        public override void PostSplitOff(Thing piece)
        { }

        public override void CompTickRare()
        { }

        public override string CompInspectStringExtra() => "Frosty";
    }
}
