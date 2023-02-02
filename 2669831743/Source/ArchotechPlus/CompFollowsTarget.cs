using Verse;

namespace ArchotechPlus
{
    public class CompFollowsTarget : ThingComp
    {
        public Thing Target { get; set; }
        public CompProperties_FollowsTarget Props => (CompProperties_FollowsTarget) props;

        public override void CompTick()
        {
            if (Target == null)
            {
                return;
            }

            parent.Position = Target.PositionHeld;
        }
    }
    
}