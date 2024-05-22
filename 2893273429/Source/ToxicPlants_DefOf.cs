using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ToxicPlants
{
    [DefOf]
    public static class ToxicPlants_DefOf
    {
        public static ThingDef Plant_BloodRose;
        static ToxicPlants_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ToxicPlants_DefOf));
        }
    }
}
