using Verse;

namespace SkinTones
{

    public class SkinTonesSettings : ModSettings
    {
        //Define Setting
        public static bool ApplySkinShader = true;
        
        //Write Setting
        public override void ExposeData()
        {
            Scribe_Values.Look(ref ApplySkinShader, "ApplySkinShader");
            base.ExposeData();
        }
    }
}