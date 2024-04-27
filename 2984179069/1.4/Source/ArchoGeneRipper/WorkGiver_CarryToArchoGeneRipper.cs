using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MoreArchotechGarbage
{
    public class WorkGiver_CarryToArchoGeneRipper : WorkGiver_CarryToBuilding
    {
        public override ThingRequest ThingRequest => ThingRequest.ForDef(MAG_DefOf.MAG_ArchotechGeneRipper);
        public override bool ShouldSkip(Pawn pawn, bool forced = false) => !ModsConfig.BiotechActive;
    }
}
