using RimWorld;
using Verse;
using Verse.AI;

namespace BDsPlasmaWeapon
{
    public class WorkGiver_EmptyLizionCellFillerLarge : WorkGiver_Scanner
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
            if (compLizionCellFiller == null || !compLizionCellFiller.IsAvaliable())
            {
                return false;
            }
            CompEggContainer compEggContainer = t.TryGetComp<CompEggContainer>();
            if (compEggContainer == null || compEggContainer.ContainedThing == null)
            {
                return false;
            }
            if (!compEggContainer.CanEmpty && !forced)
            {
                return false;
            }
            if (!StoreUtility.TryFindBestBetterStorageFor(compEggContainer.ContainedThing, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var _, out var _, needAccurateResult: false))
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompEggContainer compEggContainer = t.TryGetComp<CompEggContainer>();
            if (compEggContainer == null || compEggContainer.ContainedThing == null)
            {
                return null;
            }
            if (!compEggContainer.CanEmpty && !forced)
            {
                return null;
            }
            if (!StoreUtility.TryFindBestBetterStorageFor(compEggContainer.ContainedThing, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var foundCell, out var _))
            {
                JobFailReason.Is(HaulAIUtility.NoEmptyPlaceLowerTrans);
                return null;
            }
            Job job = JobMaker.MakeJob(RimWorld.JobDefOf.EmptyThingContainer, t, compEggContainer.ContainedThing, foundCell);
            job.count = compEggContainer.ContainedThing.stackCount;
            return job;
        }
    }

    public class WorkGiver_EmptyLizionCellFillerSmall : WorkGiver_Scanner
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
            if (compLizionCellFiller == null || !compLizionCellFiller.IsAvaliable())
            {
                return false;
            }
            CompEggContainer compEggContainer = t.TryGetComp<CompEggContainer>();
            if (compEggContainer == null || compEggContainer.ContainedThing == null)
            {
                return false;
            }
            if (!compEggContainer.CanEmpty && !forced)
            {
                return false;
            }
            if (!StoreUtility.TryFindBestBetterStorageFor(compEggContainer.ContainedThing, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var _, out var _, needAccurateResult: false))
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompEggContainer compEggContainer = t.TryGetComp<CompEggContainer>();
            if (compEggContainer == null || compEggContainer.ContainedThing == null)
            {
                return null;
            }
            if (!compEggContainer.CanEmpty && !forced)
            {
                return null;
            }
            if (!StoreUtility.TryFindBestBetterStorageFor(compEggContainer.ContainedThing, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var foundCell, out var _))
            {
                JobFailReason.Is(HaulAIUtility.NoEmptyPlaceLowerTrans);
                return null;
            }
            Job job = JobMaker.MakeJob(RimWorld.JobDefOf.EmptyThingContainer, t, compEggContainer.ContainedThing, foundCell);
            job.count = compEggContainer.ContainedThing.stackCount;
            return job;
        }
    }
}