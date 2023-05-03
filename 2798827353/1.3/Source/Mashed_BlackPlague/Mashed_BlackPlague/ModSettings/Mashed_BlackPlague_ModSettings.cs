using Verse;

namespace Mashed_BlackPlague
{
    class Mashed_BlackPlague_ModSettings : ModSettings
    {
        private static Mashed_BlackPlague_ModSettings _instance;

        public static bool Enable_VesselMentalBreak
        {
            get
            {
                return _instance.BlackPlague_Enable_VesselMentalBreak;
            }
        }

        public static float Chance_InfectedTouch
        {
            get
            {
                return _instance.BlackPlague_Chance_InfectedTouch;
            }
        }

        public bool BlackPlague_Enable_VesselMentalBreak = true;
        public float BlackPlague_Chance_InfectedTouch = 0.1f;

        public Mashed_BlackPlague_ModSettings()
        {
            _instance = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref BlackPlague_Enable_VesselMentalBreak, "BlackPlague_Enable_VesselMentalBreak", true);
            Scribe_Values.Look(ref BlackPlague_Chance_InfectedTouch, "BlackPlague_Chance_InfectedTouch", 0.1f);
        }
    }
}
