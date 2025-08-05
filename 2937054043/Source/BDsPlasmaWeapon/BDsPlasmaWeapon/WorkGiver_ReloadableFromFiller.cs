using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace BDsPlasmaWeapon
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
            CompLizionCellFiller compLizionCellFiller = t.TryGetComp<CompLizionCellFiller>();
            List<Apparel> apparels = pawn.apparel.WornApparel;
            CompReloadableFromFiller compReloadableFromFiller = null;
            foreach (Apparel apparel in apparels)
            {
                CompReloadableFromFiller a = apparel.TryGetComp<CompReloadableFromFiller>();
                if (a != null && a.remainingCharges < a.MaxCharges)
                {
                    compReloadableFromFiller = a;
                }
            }
            if (compLizionCellFiller == null || compLizionCellFiller.PipeNet.Stored < 1 || !compLizionCellFiller.IsAvaliable())
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
            CompLizionCellFiller compLizionCellFiller = t.TryGetComp<CompLizionCellFiller>();
            List<Apparel> apparels = pawn.apparel.WornApparel;
            CompReloadableFromFiller compReloadableFromFiller = null;
            foreach (Apparel apparel in apparels)
            {
                CompReloadableFromFiller a = apparel.TryGetComp<CompReloadableFromFiller>();
                if (a != null && a.remainingCharges < a.MaxCharges)
                {
                    compReloadableFromFiller = a;
                }
            }
            if (compLizionCellFiller == null || compLizionCellFiller.PipeNet.Stored < 1 || !compLizionCellFiller.IsAvaliable())
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

    public class WorkGiver_ReloadableFromFillerSmall : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.BDP_LizionFillerSmall);

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
            CompLizionCellFiller compLizionCellFiller = t.TryGetComp<CompLizionCellFiller>();
            List<Apparel> apparels = pawn.apparel.WornApparel;
            CompReloadableFromFiller compReloadableFromFiller = null;
            foreach (Apparel apparel in apparels)
            {
                CompReloadableFromFiller a = apparel.TryGetComp<CompReloadableFromFiller>();
                if (a != null && a.remainingCharges < a.MaxCharges)
                {
                    compReloadableFromFiller = a;
                }
            }
            if (compLizionCellFiller == null || compLizionCellFiller.PipeNet.Stored < 1 || !compLizionCellFiller.IsAvaliable())
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
            CompLizionCellFiller compLizionCellFiller = t.TryGetComp<CompLizionCellFiller>();
            List<Apparel> apparels = pawn.apparel.WornApparel;
            CompReloadableFromFiller compReloadableFromFiller = null;
            foreach (Apparel apparel in apparels)
            {
                CompReloadableFromFiller a = apparel.TryGetComp<CompReloadableFromFiller>();
                if (a != null && a.remainingCharges < a.MaxCharges)
                {
                    compReloadableFromFiller = a;
                }
            }
            if (compLizionCellFiller == null || compLizionCellFiller.PipeNet.Stored < 1 || !compLizionCellFiller.IsAvaliable())
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