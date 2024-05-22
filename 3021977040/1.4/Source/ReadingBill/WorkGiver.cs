using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using VanillaBooksExpanded;
using Verse.AI;
using UnityEngine;
using System.Reflection;

namespace ReadingBill
{
    [DefOf]
    public static class Defs
    {
        public static JobDef VBE_ReadBook_Bill;
    }

    public class WorkGiver_ReadBook : WorkGiver_DoBill
    {
        private class DefCountList
        {
            private List<ThingDef> defs = new List<ThingDef>();

            private List<float> counts = new List<float>();

            public int Count => defs.Count;

            public float this[ThingDef def]
            {
                get
                {
                    int num = defs.IndexOf(def);
                    if (num < 0)
                    {
                        return 0f;
                    }
                    return counts[num];
                }
                set
                {
                    int num = defs.IndexOf(def);
                    if (num < 0)
                    {
                        defs.Add(def);
                        counts.Add(value);
                        num = defs.Count - 1;
                    }
                    else
                    {
                        counts[num] = value;
                    }
                    CheckRemove(num);
                }
            }

            public float GetCount(int index)
            {
                return counts[index];
            }

            public void SetCount(int index, float val)
            {
                counts[index] = val;
                CheckRemove(index);
            }

            public ThingDef GetDef(int index)
            {
                return defs[index];
            }

            private void CheckRemove(int index)
            {
                if (counts[index] == 0f)
                {
                    counts.RemoveAt(index);
                    defs.RemoveAt(index);
                }
            }

            public void Clear()
            {
                defs.Clear();
                counts.Clear();
            }

            public void GenerateFrom(List<Thing> things)
            {
                Clear();
                for (int i = 0; i < things.Count; i++)
                {
                    this[things[i].def] += things[i].stackCount;
                }
            }
        }

        private List<ThingCount> chosenIngThings = new List<ThingCount>();

        private static List<IngredientCount> missingIngredients = new List<IngredientCount>();

        private static List<Thing> tmpMissingUniqueIngredients = new List<Thing>();

        private static readonly IntRange ReCheckFailedBillTicksRange = new IntRange(500, 600);

        private static List<Thing> relevantThings = new List<Thing>();

        private static HashSet<Thing> processedThings = new HashSet<Thing>();

        private static List<Thing> newRelevantThings = new List<Thing>();

        private static DefCountList availableCounts = new DefCountList();

        // AssignedWorkbenches compatible
        private static MethodInfo AllowedToWorkBench = AppDomain.CurrentDomain
            .GetAssemblies().FirstOrDefault(t => t.FullName.StartsWith("AssignedWorkbenches"))?
            .GetType("AssignedWorkbenches.AssignedWorkbenchesComp")
            .GetMethod("AllowedToWorkBench");

        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                if (def.fixedBillGiverDefs != null && def.fixedBillGiverDefs.Count == 1)
                {
                    return ThingRequest.ForDef(def.fixedBillGiverDefs[0]);
                }
                return ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver);
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Some;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.PotentialBillGiver);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is IBillGiver billGiver && billGiver != pawn && ThingIsUsableBillGiver(list[i]) && billGiver.BillStack.AnyShouldDoNow)
                {
                    return false;
                }
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            // vanilla
            if (!(thing is IBillGiver billGiver) || !ThingIsUsableBillGiver(thing) || !billGiver.BillStack.AnyShouldDoNow || !pawn.CanReserve(thing, 1, -1, null, forced) || thing.IsBurning() || thing.IsForbidden(pawn) || (thing.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(thing.InteractionCell, forced)))
            {
                return null;
            }
            // only colonist
            if (!pawn.IsColonist)
            {
                return null;
            }
            // table & seat check
            Building giver = thing as Building;
            if (!giver.Position.HasEatSurface(pawn.Map)){
                JobFailReason.Is("MustOnTable".Translate());
                return null;
            }
            Building seat = giver.InteractionCell.GetEdifice(pawn.Map);
            if (seat == null
            || !seat.def.building.isSittable
            || (seat.def.rotatable && seat.Rotation != giver.Rotation))
            {
                JobFailReason.Is("MustHaveSeat".Translate());
                return null;
            }
            // AssignedWorkbenches compatible
            if (AllowedToWorkBench != null && !(bool)AllowedToWorkBench.Invoke(null, new object[] { pawn, thing }))
            {
                return null;
            }

            billGiver.BillStack.RemoveIncompletableBills();
            return StartOrResumeBillJob(pawn, billGiver);
        }

        private Job StartOrResumeBillJob(Pawn pawn, IBillGiver giver)
        {
            bool flag = FloatMenuMakerMap.makingFor == pawn;
            for (int i = 0; i < giver.BillStack.Count; i++)
            {
                Bill bill = giver.BillStack[i];
                if ((Find.TickManager.TicksGame <= bill.nextTickToSearchForIngredients && FloatMenuMakerMap.makingFor != pawn) || !bill.ShouldDoNow() || !bill.PawnAllowedToStartAnew(pawn))
                {
                    continue;
                }
                SkillRequirement skillRequirement = bill.recipe.FirstSkillRequirementPawnDoesntSatisfy(pawn);
                if (skillRequirement != null)
                {
                    JobFailReason.Is("UnderRequiredSkill".Translate(skillRequirement.minLevel), bill.Label);
                    continue;
                }
                List<IngredientCount> list = null;
                if (flag)
                {
                    list = missingIngredients;
                    list.Clear();
                    tmpMissingUniqueIngredients.Clear();
                }
                if (!TryFindBestBillIngredients(bill, pawn, (Thing)giver, chosenIngThings, list) || !tmpMissingUniqueIngredients.NullOrEmpty())
                {
                    if (FloatMenuMakerMap.makingFor != pawn)
                    {
                        bill.nextTickToSearchForIngredients = Find.TickManager.TicksGame + ReCheckFailedBillTicksRange.RandomInRange;
                    }
                    else if (flag)
                    {
                        string text = list.Select((IngredientCount missing) => missing.Summary).Concat(tmpMissingUniqueIngredients.Select((Thing t) => t.Label)).ToCommaList();
                        JobFailReason.Is("MissingMaterials".Translate(text), bill.Label);
                        flag = false;
                    }
                    chosenIngThings.Clear();
                    continue;
                }
                SkillBook book = chosenIngThings[0].Thing as SkillBook;
                if (!book.CanLearnFromBook(pawn))
                {
                    JobFailReason.Is("VBE.CantReadSkillBookTooSimple".Translate());
                    chosenIngThings.Clear();
                    continue;
                }
                flag = false;
                Job haulOffJob;
                Job result = TryStartNewDoBillJob(pawn, bill, giver, chosenIngThings, out haulOffJob);
                chosenIngThings.Clear();
                return result;
            }
            chosenIngThings.Clear();
            return null;
        }

        public static new Job TryStartNewDoBillJob(Pawn pawn, Bill bill, IBillGiver giver, List<ThingCount> chosenIngThings, out Job haulOffJob, bool dontCreateJobIfHaulOffRequired = true)
        {
            haulOffJob = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, giver, null);
            if (haulOffJob != null && dontCreateJobIfHaulOffRequired)
            {
                return haulOffJob;
            }
            Job job = JobMaker.MakeJob(Defs.VBE_ReadBook_Bill, (Thing)giver);
            job.targetB = chosenIngThings[0].Thing;
            job.haulMode = HaulMode.ToCellNonStorage;
            job.bill = bill;
            return job;
        }

        private static bool IsUsableIngredient(Thing t, Bill bill)
        {
            if (!bill.IsFixedOrAllowedIngredient(t))
            {
                return false;
            }
            foreach (IngredientCount ingredient in bill.recipe.ingredients)
            {
                if (ingredient.filter.Allows(t))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool TryFindBestBillIngredients(Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen, List<IngredientCount> missingIngredients)
        {
            return TryFindBestIngredientsHelper((Thing t) => IsUsableIngredient(t, bill), (List<Thing> foundThings) => TryFindBestBillIngredientsInSet(foundThings, bill, chosen, GetBillGiverRootCell(billGiver, pawn), billGiver is Pawn, missingIngredients), bill.recipe.ingredients, pawn, billGiver, chosen, bill.ingredientSearchRadius);
        }

        private static bool TryFindBestIngredientsHelper(Predicate<Thing> thingValidator, Predicate<List<Thing>> foundAllIngredientsAndChoose, List<IngredientCount> ingredients, Pawn pawn, Thing billGiver, List<ThingCount> chosen, float searchRadius)
        {
            chosen.Clear();
            newRelevantThings.Clear();
            if (ingredients.Count == 0)
            {
                return true;
            }
            IntVec3 billGiverRootCell = GetBillGiverRootCell(billGiver, pawn);
            Region rootReg = billGiverRootCell.GetRegion(pawn.Map);
            if (rootReg == null)
            {
                return false;
            }
            relevantThings.Clear();
            processedThings.Clear();
            bool foundAll = false;
            float radiusSq = searchRadius * searchRadius;
            Predicate<Thing> baseValidator = (Thing t) => t.Spawned
                && thingValidator(t) 
                && (float)(t.Position - billGiver.Position).LengthHorizontalSquared < radiusSq
                && !t.IsForbidden(pawn)
                && pawn.CanReserve(t)
                && (t as SkillBook).CanLearnFromBook(pawn);
            TraverseParms traverseParams = TraverseParms.For(pawn);
            RegionEntryPredicate entryCondition = null;
            if (Math.Abs(999f - searchRadius) >= 1f)
            {
                entryCondition = delegate (Region from, Region r)
                {
                    if (!r.Allows(traverseParams, isDestination: false))
                    {
                        return false;
                    }
                    CellRect extentsClose = r.extentsClose;
                    int num2 = Math.Abs(billGiver.Position.x - Math.Max(extentsClose.minX, Math.Min(billGiver.Position.x, extentsClose.maxX)));
                    if ((float)num2 > searchRadius)
                    {
                        return false;
                    }
                    int num3 = Math.Abs(billGiver.Position.z - Math.Max(extentsClose.minZ, Math.Min(billGiver.Position.z, extentsClose.maxZ)));
                    return !((float)num3 > searchRadius) && (float)(num2 * num2 + num3 * num3) <= radiusSq;
                };
            }
            else
            {
                entryCondition = (Region from, Region r) => r.Allows(traverseParams, isDestination: false);
            }
            int adjacentRegionsAvailable = rootReg.Neighbors.Count((Region region) => entryCondition(rootReg, region));
            int regionsProcessed = 0;
            processedThings.AddRange(relevantThings);
            foundAllIngredientsAndChoose(relevantThings);
            RegionProcessor regionProcessor = delegate (Region r)
            {
                List<Thing> list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                for (int i = 0; i < list.Count; i++)
                {
                    Thing thing = list[i];
                    if (!processedThings.Contains(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, PathEndMode.ClosestTouch, pawn) && baseValidator(thing))
                    {
                        newRelevantThings.Add(thing);
                        processedThings.Add(thing);
                    }
                }
                int num = regionsProcessed + 1;
                regionsProcessed = num;
                if (newRelevantThings.Count > 0 && regionsProcessed > adjacentRegionsAvailable)
                {
                    relevantThings.AddRange(newRelevantThings);
                    newRelevantThings.Clear();
                    if (foundAllIngredientsAndChoose(relevantThings))
                    {
                        foundAll = true;
                        return true;
                    }
                }
                return false;
            };
            RegionTraverser.BreadthFirstTraverse(rootReg, entryCondition, regionProcessor, 99999);
            relevantThings.Clear();
            newRelevantThings.Clear();
            processedThings.Clear();
            return foundAll;
        }

        private static IntVec3 GetBillGiverRootCell(Thing billGiver, Pawn forPawn)
        {
            if (billGiver is Building building)
            {
                if (building.def.hasInteractionCell)
                {
                    return building.InteractionCell;
                }
                Log.Error(string.Concat("Tried to find bill ingredients for ", billGiver, " which has no interaction cell."));
                return forPawn.Position;
            }
            return billGiver.Position;
        }

        private static bool TryFindBestBillIngredientsInSet(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted, List<IngredientCount> missingIngredients)
        {
            if (!alreadySorted)
            {
                Comparison<Thing> comparison = delegate (Thing t1, Thing t2)
                {
                    float num4 = (t1.PositionHeld - rootCell).LengthHorizontalSquared;
                    float value = (t2.PositionHeld - rootCell).LengthHorizontalSquared;
                    return num4.CompareTo(value);
                };
                availableThings.Sort(comparison);
            }
            chosen.Clear();
            availableCounts.Clear();
            missingIngredients?.Clear();
            availableCounts.GenerateFrom(availableThings);
            List<IngredientCount> ingredients = bill.recipe.ingredients;
            for (int i = 0; i < ingredients.Count; i++)
            {
                IngredientCount ingredientCount = ingredients[i];
                bool flag = false;
                for (int j = 0; j < availableCounts.Count; j++)
                {
                    if (!ingredientCount.filter.Allows(availableCounts.GetDef(j)))
                    {
                        continue;
                    }
                    float num = ingredientCount.CountRequiredOfFor(availableCounts.GetDef(j), bill.recipe, bill);
                    for (int k = 0; k < availableThings.Count; k++)
                    {
                        if (availableThings[k].def != availableCounts.GetDef(j))
                        {
                            continue;
                        }
                        int num2 = availableThings[k].stackCount - ThingCountUtility.CountOf(chosen, availableThings[k]);
                        if (num2 > 0)
                        {
                            int num3 = Mathf.Min(Mathf.FloorToInt(num), num2);
                            ThingCountUtility.AddToList(chosen, availableThings[k], num3);
                            num -= (float)num3;
                            if (num < 0.001f)
                            {
                                flag = true;
                                float count = availableCounts.GetCount(j);
                                count -= num;
                                availableCounts.SetCount(j, count);
                                break;
                            }
                        }
                    }
                    if (flag)
                    {
                        break;
                    }
                }
                if (!flag)
                {
                    if (missingIngredients == null)
                    {
                        return false;
                    }
                    missingIngredients.Add(ingredientCount);
                }
            }
            if (missingIngredients != null)
            {
                return missingIngredients.Count == 0;
            }
            return true;
        }
    }
}
