using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using RimWorld;
using Verse;

namespace Celestials
{
    [DefOf]
    public static class CelestialsDefOf
    {
        static CelestialsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CelestialsDefOf));
        }

        public static HediffDef O21_CelestialHediff;
    }
}
