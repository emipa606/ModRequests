using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using Verse;
using Reload.Defs;
using Reload.Data;
using Reload.Components;

namespace Reload.Jobs
{
    public class WorkGiver_Scanner_Fetch : WorkGiver_Scanner
    {
        ThingDef _targetDef = null;

        int _count = 0;

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            try
            {
                if (!Setting.EnableAmmo)
                    return true;
                if (pawn?.inventory == null)
                    return true;

                Dictionary<ThingDef, int> ammoNeededQuantity = new Dictionary<ThingDef, int>();
                List<Thing> equipments = ReloadUtils.GetReloadableEquipments(pawn);
                Stock stock = Base.Instance.StockDataStorage.GetStock(pawn);

                foreach (var item in stock.Items)
                {
                    ThingDef targetDef = item.Key;
                    int amount = item.Value;
                    if (!ammoNeededQuantity.ContainsKey(targetDef))
                        ammoNeededQuantity.Add(targetDef, 0);
                    ammoNeededQuantity[targetDef] += amount;
                }
                if (!equipments.NullOrEmpty())
                {
                    foreach (var equipment in equipments)
                    {
                        var comp = equipment.TryGetComp<CompReload>();
                        if (!comp.NeedsReload())
                            continue;
                        if (!ammoNeededQuantity.ContainsKey(comp.Props.ammoDef))
                            ammoNeededQuantity[comp.Props.ammoDef] = 0;
                        ammoNeededQuantity[comp.Props.ammoDef] += comp.MagazineCapacity - comp.remainingAmmo;
                    }
                }

                foreach(var item in ammoNeededQuantity)
                {
                    ThingDef ammoDef = item.Key;
                    int quantity = item.Value;
                    int quantityInInventory = pawn.inventory.innerContainer.Where(x => x.def == ammoDef).Sum(x => x.stackCount);
                    if(quantityInInventory < quantity)
                    {
                        _count = quantity - quantityInInventory;
                        _targetDef = ammoDef;
                        return false;
                    }
                }
            }
            catch
            {
                Main.LogMessage("Error in WorkGiver_Scanner_Stock.ShouldSkip");
            }
            return true;
        }
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerThings.AllThings
                .Where(x => x?.def != null && x.def == _targetDef);
        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return !ForbidUtility.IsForbidden(t, pawn) && ReservationUtility.CanReserve(pawn, t, 1, -1, null, forced);
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Job job = JobMaker.MakeJob(ReloadJobDefOf.R_FetchThings, pawn);
            job.targetQueueB = (from thing in new List<Thing> { t }
                                select new LocalTargetInfo(thing)).ToList();
            job.playerForced = forced;
            job.count = _count;
            return job;
        }
    }
}