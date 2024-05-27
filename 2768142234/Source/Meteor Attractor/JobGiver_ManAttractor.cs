using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MeteorAttractor
{
    public class JobGiver_OperateDriver : ThinkNode_JobGiver
    {

        public float maxDistFromPoint = -1f;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_OperateDriver obj = (JobGiver_OperateDriver)base.DeepCopy(resolve);
            obj.maxDistFromPoint = maxDistFromPoint;
            return obj;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Predicate<Thing> validator = delegate (Thing t)
            {
                if (!t.def.hasInteractionCell)
                {
                    return false;
                }
                if (!t.def.HasComp(typeof(CompMannable)))
                {
                    return false;
                }
                if (!pawn.CanReserve(t))
                {
                    return false;
                }
                if (t.Faction != pawn.Faction)
                    return false;
                return true;
            };
            Thing thing = GenClosest.ClosestThingReachable(GetRoot(pawn), pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.InteractionCell, TraverseParms.For(pawn), maxDistFromPoint, validator);
            if (thing != null)
            {
                Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("OperateAsteroidAttractor"), thing);
                job.expiryInterval = 2000;
                job.checkOverrideOnExpire = true;
                return job;
            }
            return null;
        }

        protected IntVec3 GetRoot(Pawn pawn)
        {
            return pawn.Position;
        }
    }
}
