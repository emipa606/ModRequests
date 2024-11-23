using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public class WishTierDef : Def
    {
        public int scale;

        public List<ExpectationFactor> expectationFactors;

        public float GetExpectationFactor(ExpectationDef expectation)
        {
            for (int i = 0; i < expectationFactors.Count; i++)
            {
                if (expectationFactors[i].expectation == expectation) return expectationFactors[i].factor;
            }
            return expectationFactors[expectationFactors.Count - 1].factor;
        }

    }
    public class ExpectationFactor
    {
        public ExpectationDef expectation;
        public float factor;

    }
}
