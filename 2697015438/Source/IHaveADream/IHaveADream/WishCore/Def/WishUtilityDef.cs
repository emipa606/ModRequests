using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class WishUtilityDef : Def
    {

        public List<float> ChancePerHourExpectationToGetTimeWish;
        public float factorChancePerOtherTimeWish;
        public SimpleCurve factorPerPawn;

        public float dayToGetNoWishDepression;
        public float dayToUpDepression;
        public float newWishBufferDepressionStartInDay; 

        public float dayToDismiss; 

        public string tierKeySingular;
        public string tierKeyPlural;

        public List<string> tierSingular;
        public List<string> tierPlural;

        public List<TraitDef> noWishTrait;

        public float inspirationMTBFactorPerWishSucceed;
    }
}
