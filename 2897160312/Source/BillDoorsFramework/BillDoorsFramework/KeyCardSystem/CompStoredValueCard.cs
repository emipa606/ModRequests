using RimWorld;
using Verse;

namespace BillDoorsFramework
{
    public class CompStoredValueCard : ThingComp
    {
        private int remainingCredits;

        public CompProperties_StoredValueCard Props => props as CompProperties_StoredValueCard;

        public int RemainingCredits => remainingCredits;

        public bool CanBeUsed => remainingCredits > 0;

        public override void PostPostMake()
        {
            base.PostPostMake();
            remainingCredits = (int)Props.initialCreditRange.Evaluate(Rand.Value);
        }

        public override string CompInspectStringExtra()
        {
            return "BDkeycard_UsesRemaining".Translate(Props.ChargeNounArgument) + ": " + remainingCredits;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref remainingCredits, "remainingCharges", -999);
        }

        public void UsedOnce()
        {
            Used(1);
        }

        public Thing deactivate()
        {
            return ThingMaker.MakeThing(Props.deactivationRemain);
        }

        public void Used(int charge)
        {
            remainingCredits -= charge;
        }
    }

    public class CompProperties_StoredValueCard : CompProperties
    {
        public ThingDef deactivationRemain;

        public SimpleCurve initialCreditRange;

        [MustTranslate]
        public string chargeNoun = "credit";

        public NamedArgument ChargeNounArgument => chargeNoun.Named("CHARGENOUN");

        public CompProperties_StoredValueCard()
        {
            compClass = typeof(CompStoredValueCard);
        }
    }
}
