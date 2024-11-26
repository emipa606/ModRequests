using Verse;

namespace Ehi_FatLarrysGoldenShower
{
    public class Hediff_HungerRage : HediffWithComps
    {
        #region Overrides
        public override void Tick()
        {
            base.Tick();

            Severity = 1f - pawn.needs.food.CurLevel;
        }
        #endregion Overrides
    }
}
