using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BaseRobot
{
	// Token: 0x02000011 RID: 17
	public class ArcBaseRobot : Pawn
	{
		// Token: 0x0600004B RID: 75 RVA: 0x00004A44 File Offset: 0x00002C44
		public static Building_BaseRobotRechargeStation TryFindRechargeStation(ArcBaseRobot bot, Map map)
		{
			if (map == null && bot.rechargeStation != null)
			{
				map = bot.rechargeStation.Map;
			}
			if (map == null)
			{
				map = bot.Map;
			}
			Building_BaseRobotRechargeStation result;
			if (map == null)
			{
				result = null;
			}
			else
			{
				IEnumerable<Building_BaseRobotRechargeStation> enumerable = map.listerBuildings.AllBuildingsColonistOfClass<Building_BaseRobotRechargeStation>();
				if (enumerable == null)
				{
					result = null;
				}
				else
				{
					Building_BaseRobotRechargeStation building_BaseRobotRechargeStation = (from t in enumerable
					where t.robot == bot
					select t).FirstOrDefault();
					result = building_BaseRobotRechargeStation;
				}
			}
			return result;
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00004ADC File Offset: 0x00002CDC
		public bool CanDoWorkType(WorkTypeDef workTypeDef)
		{
			bool result;
			if (def2 == null)
			{
				result = false;
			}
			else
			{
				int num;
				if (workTypeDef == null || workTypeDef.relevantSkills == null)
				{
					num = 0;
				}
				else
				{
					num = workTypeDef.relevantSkills.Count;
				}
				foreach (SkillDef skillDef in workTypeDef.relevantSkills)
				{
					foreach (ThingDef_BaseRobot.RobotSkills robotSkills in def2.robotSkills)
					{
						if (robotSkills.skillDef == skillDef)
						{
							num--;
						}
					}
					if (num == 0)
					{
						break;
					}
				}
				WorkTags workTags = def2.RobotWorkTags & workTypeDef.workTags;
				result = num == 0 && workTags > WorkTags.None;
			}
			return result;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00004BF4 File Offset: 0x00002DF4
		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			IntVec3 loc = (!(Position != IntVec3.Invalid)) ? PositionHeld : Position;
			Map map = Map ?? MapHeld;
			Building_BaseRobotRechargeStation rechargestation = rechargeStation;
			ThingDef thingDef = null;
			if (this != null && def2 != null && def2.destroyedDef != null)
			{
				thingDef = def2.destroyedDef;
			}
			base.Destroy(DestroyMode.Vanish);
			if (thingDef != null)
			{
				var baseRobot_disabled = (BaseRobot_disabled)GenSpawn.Spawn(thingDef, loc, map, WipeMode.Vanish);
				baseRobot_disabled.stackCount = 1;
				baseRobot_disabled.rechargestation = rechargestation;
			}
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00004CA8 File Offset: 0x00002EA8
		private int GetPriority(WorkTypeDef workTypeDef)
		{
			if (def2 != null)
			{
				foreach (ThingDef_BaseRobot.RobotWorkTypes robotWorkTypes in def2.robotWorkTypes)
				{
					if (robotWorkTypes.workTypeDef == workTypeDef)
					{
						return robotWorkTypes.priority;
					}
				}
				return 0;
			}
			return 0;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00004D34 File Offset: 0x00002F34
		public List<WorkGiver> GetWorkGivers(bool emergency)
		{
			List<WorkGiver> result;
			if (emergency && workGiversEmergencyCache != null)
			{
				result = workGiversEmergencyCache;
			}
			else if (!emergency && workGiversNonEmergencyCache != null)
			{
				result = workGiversNonEmergencyCache;
			}
			else
			{
				var list = new List<WorkTypeDef>();
				List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
				var num = 999;
				Predicate<WorkGiverDef> predicate = null;
				for (var i = 0; i < allDefsListForReading.Count; i++)
				{
					WorkTypeDef workTypeDef = allDefsListForReading[i];
					var priority = GetPriority(workTypeDef);
					if (priority > 0)
					{
						if (priority < num)
						{
							List<WorkGiverDef> workGiversByPriority = workTypeDef.workGiversByPriority;
							Predicate<WorkGiverDef> predicate2;
							if ((predicate2 = predicate) == null)
							{
								predicate = predicate2 = (WorkGiverDef wg) => wg.emergency == emergency;
							}
							if (workGiversByPriority.Any(predicate2))
							{
								num = priority;
							}
						}
						list.Add(workTypeDef);
					}
				}
				list.InsertionSort(delegate(WorkTypeDef a, WorkTypeDef b)
				{
					var value = (float)(a.naturalPriority + ((4 - GetPriority(a)) * 100000));
					return ((float)(b.naturalPriority + ((4 - GetPriority(b)) * 100000))).CompareTo(value);
				});
				var list2 = new List<WorkGiver>();
				for (var j = 0; j < list.Count; j++)
				{
					WorkTypeDef workTypeDef2 = list[j];
					for (var k = 0; k < workTypeDef2.workGiversByPriority.Count; k++)
					{
						WorkGiver worker = workTypeDef2.workGiversByPriority[k].Worker;
						list2.Add(worker);
					}
				}
				if (emergency)
				{
					workGiversEmergencyCache = list2;
				}
				else
				{
					workGiversNonEmergencyCache = list2;
				}
				result = list2;
			}
			return result;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00004ED0 File Offset: 0x000030D0
		private void InitPawn_Setup()
		{
			if (Scribe.mode <= LoadSaveMode.Inactive)
			{
                Name = new NameSingle(Label, false);
				equipment = new Pawn_EquipmentTracker(this);
				apparel = new Pawn_ApparelTracker(this);
				skills = new Pawn_SkillTracker(this);
				SetSkills();
                story = new Pawn_StoryTracker(this)
                {
                    bodyType = (gender != Gender.Female) ? BodyTypeDefOf.Female : BodyTypeDefOf.Male,
                    crownType = CrownType.Average
                };
                Drawer.renderer.graphics.ResolveApparelGraphics();
			}
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00004F7C File Offset: 0x0000317C
		private void SetSkills()
		{
			if (def2 != null)
			{
				foreach (SkillRecord skillRecord in skills.skills)
				{
					foreach (ThingDef_BaseRobot.RobotSkills robotSkills in def2.robotSkills)
					{
						if (skillRecord.def == robotSkills.skillDef)
						{
							skillRecord.levelInt = robotSkills.level;
							skillRecord.passion = robotSkills.passion;
						}
					}
				}
			}
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00005054 File Offset: 0x00003254
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			def2 = def as ThingDef_BaseRobot;
			if (def2 == null)
			{
				Log.Error("BaseRobot -- def2 is null. Missing class definition in xml file?", false);
			}
			LongEventHandler.ExecuteWhenFinished(new Action(InitPawn_Setup));
		}

		// Token: 0x06000053 RID: 83 RVA: 0x000050A4 File Offset: 0x000032A4
		public override void Tick()
		{
			base.Tick();
			if (def2 == null || !def2.allowLearning)
			{
				foreach (SkillRecord skillRecord in skills.skills)
				{
					if (skillRecord.xpSinceLastLevel > 1f)
					{
						skillRecord.xpSinceLastLevel = 1f;
						skillRecord.xpSinceMidnight = 1f;
					}
				}
			}
			Need_Battery need_Battery = needs.TryGetNeed<Need_Battery>();
			if (Spawned && (Dead || Downed || need_Battery.CurLevel <= 0.02))
			{
				Destroy(DestroyMode.Vanish);
			}
			else if (Spawned && rechargeStation == null)
			{
				rechargeStation = TryFindRechargeStation(this, Map);
			}
		}

		// Token: 0x04000033 RID: 51
		public Building_BaseRobotRechargeStation rechargeStation;

		// Token: 0x04000034 RID: 52
		public ThingDef_BaseRobot def2;

		// Token: 0x04000035 RID: 53
		private List<WorkGiver> workGiversEmergencyCache = null;

		// Token: 0x04000036 RID: 54
		private List<WorkGiver> workGiversNonEmergencyCache = null;
	}
}
