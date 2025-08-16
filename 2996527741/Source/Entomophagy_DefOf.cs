using RimWorld;
using Verse;

namespace Entomophagy
{
    [DefOf]
    class Entomophagy_DefOf
    {
        public static RecipeDef Insect_CookMealSimple;
        public static RecipeDef Insect_CookMealSimpleBulk;
        public static RecipeDef Insect_CookMealFine;
        public static RecipeDef Insect_CookMealFineBulk;
        public static RecipeDef Insect_CookMealLavish;
        public static RecipeDef Insect_CookMealLavishBulk;
        public static RecipeDef Insect_CookMealSurvival;
        public static RecipeDef Insect_CookMealSurvivalBulk;
        public static RecipeDef Insect_MakePemmican;
        public static RecipeDef Insect_MakePemmicanBulk;

       // public static JobDef ForageForBugs;

        public static ThoughtDef AteInsectMeatAsEntomophagous;
        public static ThoughtDef ForagedForBugs;
        public static ThoughtDef ObservedForaging;

        static Entomophagy_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(Entomophagy_DefOf));
        }
    }
}
