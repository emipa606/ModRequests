using Verse;
using RimWorld;

namespace RandomChance
{
    [DefOf]
    public class RandomChance_DefOf
    {
        public static ThingDef ElectricStove;
        public static ThingDef FueledStove;
        public static ThingDef MealLavish;

        public static EffecterDef RC_ElectricShockBonesEffect;

        public static DamageDef RC_ElectricShockDamage;

        public static HediffDef RC_ElectricShockHediff;

        public static ShaderTypeDef TransparentPostLight;

        public static RecipeDef CookMealSimpleBulk;
        public static RecipeDef CookMealFine;
        public static RecipeDef CookMealFine_Veg;
        public static RecipeDef CookMealFine_Meat;
        public static RecipeDef CookMealFineBulk;
        public static RecipeDef CookMealFineBulk_Meat;
        public static RecipeDef CookMealFineBulk_Veg;
        public static RecipeDef CookMealLavish;
        public static RecipeDef CookMealLavish_Meat;
        public static RecipeDef CookMealLavish_Veg;
        public static RecipeDef CookMealLavishBulk;
        public static RecipeDef CookMealLavishBulk_Veg;
        public static RecipeDef CookMealLavishBulk_Meat;
        public static RecipeDef ButcherCorpseFlesh;
        public static RecipeDef CremateCorpse;

        static RandomChance_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RandomChance_DefOf));
        }
    }
}
