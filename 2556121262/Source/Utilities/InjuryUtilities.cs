using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HalfDragons
{
    public static class InjuryUtilities
    {
        public static bool IsScar(this Hediff hediff)
        {
            if(hediff is Hediff_Injury injury)
            {
                return injury.IsPermanent() &&
                injury.TryGetComp<HediffComp_GetsPermanent>() != null;
            }
            else
            {
                return false;
            }
        }

        public static bool IsTendable(this Hediff hediff)
        {
            return hediff.def.tendable;
        }
    }
}
