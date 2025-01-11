using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace HumanResources
{
    using static ModBaseHumanResources;
    class WorkGiver_LearnWeapon : WorkGiver_Knowledge
    {
        public List<ThingCount> chosenIngThings = new List<ThingCount>();
        protected MethodInfo BestIngredientsInfo = AccessTools.Method(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients");
        protected FieldInfo rangeInfo = AccessTools.Field(typeof(WorkGiver_DoBill), "ReCheckFailedBillTicksRange");

        public static bool ShouldReserve(Pawn p, LocalTargetInfo target, int maxPawns = 1, int stackCount = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false)
        {
            if (p.TryGetComp<CompKnowledge>().knownWeapons.Contains(target.Thing.def))
            {
                return false;
            }
            return p.CanReserve(target, maxPawns, stackCount, layer, ignoreOtherReservations);
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            int tick = Find.TickManager.TicksGame;
            if (actualJob == null || lastVerifiedJobTick != tick || Find.TickManager.Paused)
            {
                actualJob = null;
                IBillGiver billGiver = thing as IBillGiver;
                if (billGiver == null || !this.ThingIsUsableBillGiver(thing) || !billGiver.BillStack.AnyShouldDoNow
                    || !billGiver.UsableForBillsAfterFueling() || !pawn.CanReserve(thing, 1, -1, null, forced)
                    || thing.IsBurning() || thing.IsForbidden(pawn)
                    || (thing.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(thing.InteractionCell, forced)))
                {
                    return null;
                }
                if (IsRangeClear(thing)) //check is shooting area is clear if it exists.
                {
                    billGiver.BillStack.RemoveIncompletableBills();
                    foreach (Bill bill in RelevantBills(thing, pawn))
                    {
                        if (ValidateChosenWeapons(bill, pawn, billGiver)) //check bill filter
                        {
                            actualJob = StartBillJob(pawn, billGiver, bill);
                            lastVerifiedJobTick = tick;
                            break;
                        }
                    }
                }
            }
            return actualJob;
        }

        protected virtual bool IsRangeClear(Thing target)
        {
            CompShootingArea comp = target.TryGetComp<CompShootingArea>();
            if (comp == null) return true;
            var check = ShootingRangeUtility.AreaClear(comp.RangeArea, target.Map);
            if (check.Accepted) return true;
            if (!JobFailReason.HaveReason) JobFailReason.Is(check.Reason);
            return false;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            IEnumerable<ThingDef> knownWeapons = pawn.TryGetComp<CompKnowledge>()?.knownWeapons;
            if (knownWeapons != null)
            {
                IEnumerable<ThingDef> available = unlocked.hardWeapons;
                IEnumerable<ThingDef> studyMaterial = available.Except(knownWeapons);
                return !studyMaterial.Any();
            }
            return true;
        }

        protected virtual IEnumerable<ThingDef> StudyWeapons(Bill bill, Pawn pawn)
        {
            CompKnowledge techComp = pawn.TryGetComp<CompKnowledge>();
            IEnumerable<ThingDef> known = techComp.knownWeapons;
            IEnumerable<ThingDef> craftable = techComp.craftableWeapons;
            IEnumerable<ThingDef> allowed = unlocked.hardWeapons.Concat(craftable);
            IEnumerable<ThingDef> chosen = bill.ingredientFilter.AllowedThingDefs;
            IEnumerable<ThingDef> viable = chosen.Intersect(allowed).Except(known);
            IEnumerable<ThingDef> unavailable = chosen.Except(viable);
            if (viable.EnumerableNullOrEmpty() && !unavailable.EnumerableNullOrEmpty())
            {
                string thoseWeapons = "ThoseWeapons".Translate();
                string listing = (unavailable.EnumerableCount() < 10) ? unavailable.Select(x => x.label).ToStringSafeEnumerable() : thoseWeapons;
                JobFailReason.Is("MissingRequirementToLearnWeapon".Translate(pawn, listing));
            }
            return viable;
        }

        protected Job StartBillJob(Pawn pawn, IBillGiver giver, Bill bill)
        {
            if ((Find.TickManager.TicksGame > bill.nextTickToSearchForIngredients || FloatMenuMakerMap.makingFor == pawn)
                && bill.ShouldDoNow() && bill.PawnAllowedToStartAnew(pawn))
            {
                Job result = TryStartNewDoBillJob(pawn, bill, giver);
                chosenIngThings.Clear();
                return result;
            }
            chosenIngThings.Clear();
            return null;
        }

        protected virtual Job TryStartNewDoBillJob(Pawn pawn, Bill bill, IBillGiver giver)
        {
            Job haulOffJob = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, giver, null);
            if (haulOffJob != null)
            {
                return haulOffJob;
            }
            Job job = new Job(TechJobDefOf.TrainWeapon, (Thing)giver);
            job.targetQueueB = new List<LocalTargetInfo>(chosenIngThings.Count);
            job.countQueue = new List<int>(chosenIngThings.Count);
            for (int i = 0; i < chosenIngThings.Count; i++)
            {
                job.targetQueueB.Add(chosenIngThings[i].Thing);
                job.countQueue.Add(chosenIngThings[i].Count);
            }
            job.haulMode = HaulMode.ToCellNonStorage;
            job.bill = bill;
            return job;
        }

        protected virtual bool ValidateChosenWeapons(Bill bill, Pawn pawn, IBillGiver giver)
        {
            if ((bool)BestIngredientsInfo.Invoke(this, new object[] { bill, pawn, giver, chosenIngThings, new List<IngredientCount>() }))
            {
                var studyWeapons = StudyWeapons(bill, pawn);
                chosenIngThings.RemoveAll(x => !studyWeapons.Contains(x.Thing.def));
                if (chosenIngThings.Any())
                {
                    if (!JobFailReason.HaveReason) JobFailReason.Is("NoWeaponToLearn".Translate(pawn), null);
                    return studyWeapons.Any();
                }
            }
            if (!JobFailReason.HaveReason) JobFailReason.Is("NoWeaponsFoundToLearn".Translate(pawn), null);
            if (FloatMenuMakerMap.makingFor != pawn)
            {
                IntRange range = (IntRange)rangeInfo.GetValue(null);
                bill.nextTickToSearchForIngredients = Find.TickManager.TicksGame + range.RandomInRange;
            }
            return false;
        }
    }
}