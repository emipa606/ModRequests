using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace BaseRobot
{
	// Token: 0x02000009 RID: 9
	public class JobGiver_Work : ThinkNode_JobGiver
	{
		// Token: 0x0600002F RID: 47 RVA: 0x00003B6C File Offset: 0x00001D6C
		public override float GetPriority(Pawn pawn)
		{
			TimeAssignmentDef timeAssignmentDef = (pawn.timetable == null) ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment;
			float result;
			if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
			{
				result = 5.5f;
			}
			else if (timeAssignmentDef == TimeAssignmentDefOf.Work)
			{
				result = 9f;
			}
			else if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
			{
				result = 2f;
			}
			else
			{
				if (timeAssignmentDef != TimeAssignmentDefOf.Joy)
				{
					throw new NotImplementedException();
				}
				result = 2f;
			}
			return result;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00003BF4 File Offset: 0x00001DF4
		private Job GiverTryGiveJobPrioritized(Pawn pawn, WorkGiver giver, IntVec3 cell)
        {
            if (!PawnCanUseWorkGiver(pawn, giver))
            {
                return null;
            }
            try
            {
                Job job = giver.NonScanJob(pawn);
                if (job != null)
                {
                    return job;
                }
                if (giver is WorkGiver_Scanner scanner)
                {
                    if (giver.def.scanThings)
                    {
                        bool predicate(Thing t)
                        {
                            return !t.IsForbidden(pawn) && scanner.HasJobOnThing(pawn, t, false);
                        }

                        List<Thing> thingList = cell.GetThingList(pawn.Map);
                        for (var i = 0; i < thingList.Count; i++)
                        {
                            Thing thing = thingList[i];
                            if (scanner.PotentialWorkThingRequest.Accepts(thing) && predicate(thing))
                            {
                                Job job2 = scanner.JobOnThing(pawn, thing, false);
                                if (job2 != null)
                                {
                                    job2.workGiverDef = giver.def;
                                }
                                return job2;
                            }
                        }
                    }
                    if (giver.def.scanCells && !cell.IsForbidden(pawn) && scanner.HasJobOnCell(pawn, cell, false))
                    {
                        Job job3 = scanner.JobOnCell(pawn, cell, false);
                        if (job3 != null)
                        {
                            job3.workGiverDef = giver.def;
                        }
                        return job3;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Concat(new object[]
                {
                        pawn,
                        " threw exception in GiverTryGiveJobTargeted on WorkGiver ",
                        giver.def.defName,
                        ": ",
                        ex.ToString()
                }), false);
            }
            return null;
        }

        // Token: 0x06000031 RID: 49 RVA: 0x00003E10 File Offset: 0x00002010
        private bool PawnCanUseWorkGiver(Pawn pawn, WorkGiver giver)
		{
			return giver.MissingRequiredCapacity(pawn) == null && !giver.ShouldSkip(pawn, false);
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00003E2C File Offset: 0x0000202C
		protected override Job TryGiveJob(Pawn pawn)
		{
            Job result;
            if (!(pawn is ArcBaseRobot arcBaseRobot))
			{
				result = null;
			}
			else
			{
				List<WorkGiver> workGivers = arcBaseRobot.GetWorkGivers(false);
				var num = -999;
				TargetInfo targetInfo = TargetInfo.Invalid;
				WorkGiver_Scanner workGiver_Scanner = null;
				for (var i = 0; i < workGivers.Count; i++)
				{
					WorkGiver workGiver = workGivers[i];
					if (workGiver.def.priorityInType != num && targetInfo.IsValid)
					{
						break;
					}
					if (PawnCanUseWorkGiver(pawn, workGiver))
					{
						try
						{
							Job job = workGiver.NonScanJob(pawn);
							if (job != null)
							{
								return job;
							}
                            if (workGiver is WorkGiver_Scanner scanner)
                            {
                                if (workGiver.def.scanThings)
                                {
                                    bool predicate(Thing t)
                                    {
                                        return !t.IsForbidden(pawn) && scanner.HasJobOnThing(pawn, t, false);
                                    }

                                    IEnumerable<Thing> enumerable = scanner.PotentialWorkThingsGlobal(pawn);
                                    Thing thing;
                                    if (scanner.Prioritized)
                                    {
                                        IEnumerable<Thing> enumerable2 = enumerable;
                                        if (enumerable2 == null)
                                        {
                                            enumerable2 = pawn.Map.listerThings.ThingsMatching(scanner.PotentialWorkThingRequest);
                                        }
                                        Predicate<Thing> validator = predicate;
                                        thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, enumerable2, scanner.PathEndMode, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, (Thing x) => scanner.GetPriority(pawn, x));
                                    }
                                    else
                                    {
                                        Predicate<Thing> validator2 = predicate;
                                        var forceAllowGlobalSearch = enumerable != null;
                                        thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, scanner.PotentialWorkThingRequest, scanner.PathEndMode, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator2, enumerable, 0, scanner.MaxRegionsToScanBeforeGlobalSearch, forceAllowGlobalSearch, RegionType.Set_Passable, false);
                                    }
                                    if (thing != null)
                                    {
                                        targetInfo = thing;
                                        workGiver_Scanner = scanner;
                                    }
                                }
                                if (workGiver.def.scanCells)
                                {
                                    IntVec3 position = pawn.Position;
                                    var num2 = 99999f;
                                    var num3 = float.MinValue;
                                    var prioritized = scanner.Prioritized;
                                    foreach (IntVec3 intVec in scanner.PotentialWorkCellsGlobal(pawn))
                                    {
                                        var flag = false;
                                        var num4 = (float)(intVec - position).LengthHorizontalSquared;
                                        if (prioritized)
                                        {
                                            if (!intVec.IsForbidden(pawn) && scanner.HasJobOnCell(pawn, intVec, false))
                                            {
                                                var priority = scanner.GetPriority(pawn, intVec);
                                                if (priority > num3 || (priority == num3 && num4 < num2))
                                                {
                                                    flag = true;
                                                    num3 = priority;
                                                }
                                            }
                                        }
                                        else if (num4 < num2 && !intVec.IsForbidden(pawn) && scanner.HasJobOnCell(pawn, intVec, false))
                                        {
                                            flag = true;
                                        }
                                        if (flag)
                                        {
                                            targetInfo = new TargetInfo(intVec, pawn.Map, false);
                                            workGiver_Scanner = scanner;
                                            num2 = num4;
                                        }
                                    }
                                }
                            }
                        }
						catch (Exception ex)
						{
							Log.Error(string.Concat(new object[]
							{
								pawn,
								" threw exception in WorkGiver ",
								workGiver.def.defName,
								": ",
								ex.ToString()
							}), false);
						}
						if (targetInfo.IsValid)
						{
                            Job job2 = workGiver_Scanner.JobOnThing(pawn, targetInfo.Thing, false);
                            if (job2 != null)
                            {
                                job2.workGiverDef = workGiver.def;
                            }
                            return job2;
						}
						num = workGiver.def.priorityInType;
					}
				}
				result = null;
			}
			return result;
		}
	}
}
