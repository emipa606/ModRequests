using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StuffableCore.SCComps
{
    public class HediffCompProperties_Stuffable : HediffCompProperties
    {
        public HediffCompProperties_Stuffable() => this.compClass = typeof(HediffCompStuffable);
    }
}
