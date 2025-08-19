using RimWorld;
using Verse;
using VFEPirates;

namespace ArmoredUp
{
    [StaticConstructorOnStartup]
    public static class PirateCompatibility
    {
        public static bool piratesLoaded = ModsConfig.IsActive("oskarpotocki.vfe.pirates");
        public static bool GetCasket(this Pawn pawn)
        {
            return pawn.IsWearingWarcasket();
        }

        public static bool IsCasketApparel(this Apparel apparel)
        {
            return apparel is Apparel_Warcasket;
        }
    }
}