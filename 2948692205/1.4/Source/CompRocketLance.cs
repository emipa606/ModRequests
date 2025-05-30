using Verse;

namespace IndustrialArmory
{
    public class CompRocketLance : ThingComp
    {
        public bool rocketsEnabled;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref rocketsEnabled, "rocketsEnabled");
        }
    }
}
