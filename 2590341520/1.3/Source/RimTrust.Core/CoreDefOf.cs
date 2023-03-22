using RimWorld;
using Verse;

namespace RimTrust.Core
{
    [DefOf]
    public static class CoreDefOf
    {
        public static JobDef UseBankTerminal;
        public static ThingDef TrustLedgerConsole;
        public static JobDef UseNeuralChair;
        public static ThingDef NeuralInterfaceChair;
        public static JobDef UseNeuralChair_TII;
        public static ThingDef NeuralInterfaceChair_TII;
        public static ThingDef NutrientTube;
        public static ThingDef MealNutrientSupplementPill;
        public static JobDef FillNutrientTube;
        public static JobDef EmptyNutrientTube;
        public static ThingDef RawRice;
        public static ThingDef RawCorn;
 
        //public static ThoughtDef Ascension;

        static CoreDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CoreDefOf));
        }

    }
}