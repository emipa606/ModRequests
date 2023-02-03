using Verse;

namespace ImprovedInsectoids
{
    public class CompProperties_MentalStatePusher : CompProperties
    {
        public MentalStateDef mentalState;

        public int radius;

        public float chance;

        public int tickInterval = 2500;

        public bool affectOwnFaction = false;

        public MentalStateDef exceptionForPlayerFaction = null;

        public CompProperties_MentalStatePusher()
        {
            this.compClass = typeof(CompMentalStatePusher);
        }
    }
}
