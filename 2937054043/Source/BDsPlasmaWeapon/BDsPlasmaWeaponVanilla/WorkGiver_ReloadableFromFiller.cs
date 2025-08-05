using System.Collections.Generic;
using PipeSystem;
using RimWorld;
using Verse;
using Verse.AI;

namespace BDsPlasmaWeaponVanilla
{
    public class WorkGiver_ReloadableFromFillerLarge : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.BDP_LizionFillerLarge);

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!t.Spawned || t.IsForbidden(pawn))
            {
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            CompResource compLizionCellFiller = t.TryGetComp<CompResource>();
            List<Apparel> apparels = pawn.apparel.WornApparel;
            CompReloadableFromFiller compReloadableFromFiller = null;
            foreach (Apparel apparel in apparels)
            {
                CompReloadableFromFiller a = apparel.TryGetComp<CompReloadableFromFiller>();
                if (a != null && a.RemainingCharges < a.MaxCharges)
                {
                    compReloadableFromFiller = a;
                }
            }
            if (compLizionCellFiller == null || compLizionCellFiller.PipeNet.Stored < 1)
            {
                return false;
            }
            if (compReloadableFromFiller == null)
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompResource compLizionCellFiller = t.TryGetComp<CompResource>();
            List<Apparel> apparels = pawn.apparel.WornApparel;
            CompReloadableFromFiller compReloadableFromFiller = null;
            foreach (Apparel apparel in apparels)
            {
                CompReloadableFromFiller a = apparel.TryGetComp<CompReloadableFromFiller>();
                if (a != null && a.RemainingCharges < a.MaxCharges)
                {
                    compReloadableFromFiller = a;
                }
            }
            if (compLizionCellFiller == null || compLizionCellFiller.PipeNet.Stored < 1)
            {
                return null;
            }
            if (compReloadableFromFiller == null)
            {
                return null;
            }
            Job job = JobMaker.MakeJob(JobDefOf.BDP_JobDefRefillFromFiller, compReloadableFromFiller.parent);
            job.targetB = t;
            return job;
        }
    }
}