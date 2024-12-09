using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Psycasts_Electrified
{
    public class CompProperties_ElectricalOverload : CompProperties
    {
        public CompProperties_ElectricalOverload()
        {
            this.compClass = typeof(CompElectricalOverload);
        }
    }
}
