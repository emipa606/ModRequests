using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BeatingsContinue
{
    class BeatingsDefsOf
    {
        public static DesignationDef designationDef => DefDatabase<DesignationDef>.GetNamed("Suppress");
        public static JobDef jobDefBeat => DefDatabase<JobDef>.GetNamed("BeatAttack");
        public static JobDef jobDefUnarmed => DefDatabase<JobDef>.GetNamed("UnarmedAttack");
    }
}
