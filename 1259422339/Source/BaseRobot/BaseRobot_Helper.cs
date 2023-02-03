using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;
using Verse.AI;

namespace BaseRobot
{
	// Token: 0x0200000F RID: 15
	[StaticConstructorOnStartup]
	public class BaseRobot_Helper
	{
		// Token: 0x0600003F RID: 63 RVA: 0x00004559 File Offset: 0x00002759
		public static Building_BaseRobotRechargeStation FindMedicalRechargeStationFor(ArcBaseRobot p)
		{
			return FindRechargeStationFor(p, p, false, false, true);
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00004565 File Offset: 0x00002765
		public static Building_BaseRobotRechargeStation FindRechargeStationFor(ArcBaseRobot p)
		{
			return FindRechargeStationFor(p, p, false, false, false);
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00004574 File Offset: 0x00002774
		public static Building_BaseRobotRechargeStation FindRechargeStationFor(ArcBaseRobot sleeper, ArcBaseRobot traveler, bool sleeperWillBePrisoner, bool checkSocialProperness, bool medicalBedNeeded = false)
		{
            bool predicate(Thing t)
            {
                var flag3 = !traveler.CanReserveAndReach(t, PathEndMode.OnCell, Danger.Some, 1, -1, null, false);
                bool result;
                if (flag3)
                {
                    result = false;
                }
                else
                {
                    var building_BaseRobotRechargeStation = t as Building_BaseRobotRechargeStation;
                    var flag4 = building_BaseRobotRechargeStation == null;
                    if (flag4)
                    {
                        result = false;
                    }
                    else
                    {
                        var flag5 = building_BaseRobotRechargeStation.robot != null && building_BaseRobotRechargeStation.robot != sleeper;
                        if (flag5)
                        {
                            result = false;
                        }
                        else
                        {
                            var flag6 = building_BaseRobotRechargeStation.IsForbidden(traveler);
                            if (flag6)
                            {
                                result = false;
                            }
                            else
                            {
                                var flag7 = building_BaseRobotRechargeStation.IsBurning();
                                result = !flag7;
                            }
                        }
                    }
                }
                return result;
            }
            var flag = sleeper.rechargeStation != null && predicate(sleeper.rechargeStation);
			if (flag)
			{
				Building_BaseRobotRechargeStation rechargeStation = sleeper.rechargeStation;
				var flag2 = rechargeStation != null;
				if (flag2)
				{
					return rechargeStation;
				}
			}
			return null;
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000045F4 File Offset: 0x000027F4
		public static List<List<T>> GetAllCombos<T>(List<T> initialList, bool includeEmpty = false, bool includeInitList = true)
		{
			var list = new List<List<T>>();
			int num;
			if (includeEmpty)
			{
				num = Convert.ToInt32(Math.Pow(2.0, initialList.Count()));
			}
			else
			{
				num = Convert.ToInt32(Math.Pow(2.0, initialList.Count())) - 1;
			}
			int num2;
			if (includeEmpty)
			{
				num2 = 0;
			}
			else
			{
				num2 = 1;
			}
			for (var i = num2; i < num; i++)
			{
				var list2 = new List<T>();
				for (var j = 0; j < initialList.Count(); j++)
				{
					var num3 = 1 << j;
					var flag = (i & num3) == num3;
					if (flag)
					{
						list2.Add(initialList[j]);
					}
				}
				list.Add(list2);
			}
			if (includeInitList)
			{
				list.Add(initialList);
			}
			return list;
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000046CC File Offset: 0x000028CC
		public static double GetDistance(IntVec3 p1, IntVec3 p2)
		{
			var num = Math.Abs(p1.x - p2.x);
			var num2 = Math.Abs(p1.y - p2.y);
			var num3 = Math.Abs(p1.z - p2.z);
			return Math.Sqrt((num * num) + (num2 * num2) + (num3 * num3));
		}

		// Token: 0x06000044 RID: 68 RVA: 0x0000472C File Offset: 0x0000292C
		public static bool IsInDistance(IntVec3 p1, IntVec3 p2, float distance)
		{
			var num = Math.Abs(p1.x - p2.x);
			var num2 = Math.Abs(p1.y - p2.y);
			var num3 = Math.Abs(p1.z - p2.z);
			return (num * num) + (num2 * num2) + (num3 * num3) <= distance * distance;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x0000478C File Offset: 0x0000298C
		public static void ReApplyThingToListerThings(IntVec3 cell, Thing thing)
		{
			if (!(cell == IntVec3.Invalid) && thing != null && thing.Map != null && thing.Spawned)
			{
				Map map = thing.Map;
				RegionGrid regionGrid = map.regionGrid;
				Region region = null;
				var flag = cell.InBounds(map);
				if (flag)
				{
					region = regionGrid.GetValidRegionAt(cell);
				}
				var flag2 = region != null;
				if (flag2)
				{
					var flag3 = !region.ListerThings.Contains(thing);
					if (flag3)
					{
						region.ListerThings.Add(thing);
					}
				}
			}
		}

		// Token: 0x06000046 RID: 70 RVA: 0x0000482C File Offset: 0x00002A2C
		public static void UpdateBaseShieldingWhileRecharging(Pawn pawn, bool inRechargeStation, string shieldingDefName)
		{
			if (inRechargeStation)
			{
				foreach (Apparel apparel in pawn.apparel.WornApparel)
				{
					var flag = apparel.def.defName == shieldingDefName && apparel.HitPoints < apparel.MaxHitPoints * 0.95;
					if (flag)
					{
						Apparel apparel2 = apparel;
						apparel2.HitPoints++;
					}
				}
				if (pawn.apparel.WornApparelCount == 0)
				{
					ThingDef named = DefDatabase<ThingDef>.GetNamed(shieldingDefName, true);
					var apparel3 = (Apparel)ThingMaker.MakeThing(named, null);
					apparel3.HitPoints = (int)(apparel3.MaxHitPoints * 0.05);
					var flag2 = ApparelUtility.HasPartsToWear(pawn, apparel3.def);
					if (flag2)
					{
						pawn.apparel.Wear(apparel3, false);
					}
				}
			}
		}

		// Token: 0x02000010 RID: 16
		[CompilerGenerated]
		[Serializable]
		public sealed class Something
		{
			// Token: 0x06000048 RID: 72 RVA: 0x00004958 File Offset: 0x00002B58
			internal bool RemoveCommUnit(BodyPartRecord p)
			{
				return p.def.defName == "AIRobot_CommUnit";
			}

			// Token: 0x04000031 RID: 49
			public static readonly Something blah = new Something();

			// Token: 0x04000032 RID: 50
			public static Func<BodyPartRecord, bool> part;
		}
	}
}
