using Verse;

namespace Ehi_FatLarrysGoldenShower
{
    public class Weapon_GoldenShower : ThingWithComps
    {
        #region Overrides
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            if (pawn != null && DefOfs.Ehi_Hediff_HungerRage != null)
            {
                Hediff hungerRageHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(DefOfs.Ehi_Hediff_HungerRage);

                if (hungerRageHediff == null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(DefOfs.Ehi_Hediff_HungerRage, pawn);
                    hediff.Severity = 1f - pawn.needs.food.CurLevel;
                    pawn.health.AddHediff(hediff);
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (pawn != null && DefOfs.Ehi_Hediff_HungerRage != null)
            {
                Hediff hungerRageHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(DefOfs.Ehi_Hediff_HungerRage);
                if (hungerRageHediff != null)
                {
                    pawn.health.RemoveHediff(hungerRageHediff);
                }
            }
        }
        #endregion Overrides
    }
}
