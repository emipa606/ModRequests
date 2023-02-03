using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BaseRobot
{
	// Token: 0x02000005 RID: 5
	[StaticConstructorOnStartup]
	public class Building_BaseRobotRechargeStation : Building
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000009 RID: 9 RVA: 0x0000224C File Offset: 0x0000044C
		public override Graphic Graphic
		{
			get
			{
				var flag = PrimaryGraphic == null;
				if (flag)
				{
					GetGraphics();
					var flag2 = PrimaryGraphic == null;
					if (flag2)
					{
						return base.Graphic;
					}
				}
				var flag3 = robot == null && !robotIsDestroyed;
				Graphic result;
				if (flag3)
				{
					result = PrimaryGraphic;
				}
				else
				{
					var flag4 = SecondaryGraphic == null;
					if (flag4)
					{
						GetGraphics();
						var flag5 = SecondaryGraphic == null;
						if (flag5)
						{
							result = PrimaryGraphic;
						}
						else
						{
							result = SecondaryGraphic;
						}
					}
					else
					{
						result = SecondaryGraphic;
					}
				}
				return result;
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000022FC File Offset: 0x000004FC
		public void AddRobotToContainer(ArcBaseRobot bot)
		{
			var flag = bot.HasAttachment(ThingDefOf.Fire);
			if (flag)
			{
				bot.GetAttachment(ThingDefOf.Fire).Destroy(DestroyMode.Vanish);
			}
			bot.stances.CancelBusyStanceHard();
			bot.jobs.StopAll(false);
			bot.pather.StopDead();
			var drafted = bot.Drafted;
			if (drafted)
			{
				bot.drafter.Drafted = false;
			}
			var flag2 = !container.Contains(bot);
			if (flag2)
			{
				container.Add(bot);
			}
			List<Map> maps = Find.Maps;
			for (var i = 0; i < maps.Count; i++)
			{
				maps[i].designationManager.RemoveAllDesignationsOn(bot, false);
			}
			DespawnRobot(bot, false);
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000023C8 File Offset: 0x000005C8
		private void Button_CallAllBotsForShutdown()
		{
			var list = Map.listerThings.AllThings.OfType<Building_BaseRobotRechargeStation>().ToList();
			for (var i = list.Count; i > 0; i--)
			{
				Building_BaseRobotRechargeStation building_BaseRobotRechargeStation = list[i - 1];
				building_BaseRobotRechargeStation.Notify_CallBotForShutdown();
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002418 File Offset: 0x00000618
		private void Button_CallBotForShutdown()
		{
			SpawnRobotAfterRecharge = false;
			if (robot != null && !robotIsDestroyed && robot.Spawned)
			{
				var flag = robot.jobs == null;
				if (flag)
				{
					Log.Error("Robot has no job driver!", false);
				}
				var jobGiver_RechargeEnergy = new JobGiver_RechargeEnergy();
				ThinkResult thinkResult = jobGiver_RechargeEnergy.TryIssueJobPackage(robot, default);
				if (thinkResult.Job != null && robot.CurJob.def != thinkResult.Job.def)
				{
					robot.jobs.StopAll(false);
					var flag2 = robot.drafter == null;
					if (flag2)
					{
						robot.drafter = new Pawn_DraftController(robot);
					}
					robot.drafter.Drafted = false;
					robot.jobs.StartJob(thinkResult.Job, JobCondition.None, null, false, true, null, null, false);
				}
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002548 File Offset: 0x00000748
		private void Button_ResetDestroyedRobot()
		{
			var flag = robot != null && !robot.Destroyed;
			if (flag)
			{
				robot.Destroy(DestroyMode.Vanish);
			}
			robot = null;
			robotIsDestroyed = false;
			Button_SpawnBot();
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002598 File Offset: 0x00000798
		private void Button_SpawnAllAvailableBots()
		{
			var list = Map.listerThings.AllThings.OfType<Building_BaseRobotRechargeStation>().ToList();
			for (var i = list.Count; i > 0; i--)
			{
				Building_BaseRobotRechargeStation building_BaseRobotRechargeStation = list[i - 1];
				building_BaseRobotRechargeStation.Notify_SpawnBot();
			}
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000025E8 File Offset: 0x000007E8
		private void Button_SpawnBot()
		{
			if (robot == null && !robotIsDestroyed)
			{
				var flag = spawnThingDef.NullOrEmpty();
				if (flag)
				{
					Log.Error("Robot Recharge Station: Wanted to spawn robot, but spawnThingDef is null or empty!", false);
				}
				else
				{
					var flag2 = !IsRobotInContainer();
					ArcBaseRobot arcBaseRobot;
					if (flag2)
					{
						arcBaseRobot = Building_BaseRobotCreator.CreateRobot(spawnThingDef, Position, Map, Faction.OfPlayer);
					}
					else
					{
						arcBaseRobot = container[0];
						container.Remove(arcBaseRobot);
						arcBaseRobot = GenSpawn.Spawn(arcBaseRobot, Position, Map, WipeMode.Vanish) as ArcBaseRobot;
					}
					robot = arcBaseRobot;
					robot.rechargeStation = this;
					robotSpawnedOnce = true;
					SpawnRobotAfterRecharge = true;
				}
			}
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000026B8 File Offset: 0x000008B8
		private void ClearContainer()
		{
			var flag = container != null && container.Count > 0;
			if (flag)
			{
				container.Clear();
			}
			container = new List<ArcBaseRobot>();
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002700 File Offset: 0x00000900
		public void DespawnRobot(ArcBaseRobot bot, bool destroying = false)
		{
			var flag = bot != null && !bot.Destroyed;
			if (flag)
			{
				if (destroying)
				{
					bot.Destroy(DestroyMode.Vanish);
				}
				else
				{
					var spawned = bot.Spawned;
					if (spawned)
					{
						bot.DeSpawn(DestroyMode.Vanish);
					}
				}
			}
			robot = null;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002753 File Offset: 0x00000953
		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			DespawnRobot(robot, true);
			ClearContainer();
			base.Destroy(mode);
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002770 File Offset: 0x00000970
		public override void ExposeData()
		{
			try
			{
				base.ExposeData();
				Scribe_Values.Look(ref robotSpawnedOnce, "robotSpawned", false, false);
				Scribe_Values.Look(ref robotIsDestroyed, "robotDestroyed", false, false);
				Scribe_Values.Look(ref SpawnRobotAfterRecharge, "autospawn", true, false);
				try
				{
					Scribe_References.Look(ref robot, "robot", true);
				}
				catch (Exception ex)
				{
					Log.Warning("Building_BaseRobot_RechargeStation -- Error while loading 'robot': " + ex.Message + "\t" + ex.StackTrace, false);
				}
				try
				{
					Scribe_Collections.Look(ref container, "container", LookMode.Deep, null);
				}
				catch (Exception ex2)
				{
					Log.Warning("Building_BaseRobot_RechargeStation -- Error while loading 'container': " + ex2.Message + "\t" + ex2.StackTrace, false);
				}
			}
			catch (Exception ex3)
			{
				Log.Error("Building_BaseRobot_RechargeStation -- Unknown error while loading: " + ex3.Message + "\t" + ex3.StackTrace, false);
			}
			var flag = Scribe.mode == LoadSaveMode.PostLoadInit;
			if (flag)
			{
				updateGraphicForceNeeded = true;
				var flag2 = container == null;
				if (flag2)
				{
					ClearContainer();
				}
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000028B8 File Offset: 0x00000AB8
		public override IEnumerable<Gizmo> GetGizmos()
		{
			var num = 31367676;
			var flag = robot == null && !robotIsDestroyed;
			if (flag)
			{
                var command_Action = new Command_Action
                {
                    defaultLabel = Translator.Translate(lbSpawnOwner),
                    defaultDesc = Translator.Translate(txtSpawnOwner),
                    icon = UI_ButtonStart,
                    hotKey = KeyBindingDefOf.Misc4,
                    activateSound = SoundDef.Named("Click"),
                    action = new Action(Button_SpawnBot),
                    disabled = powerComp != null && !powerComp.PowerOn,
                    disabledReason = Translator.Translate(txtNoPower),
                    groupKey = num + 1
                };
                yield return command_Action;
				command_Action = null;
			}
			var flag2 = (robot != null || robotSpawnedOnce) && !robotIsDestroyed;
			if (flag2)
			{
                var command_Action2 = new Command_Action
                {
                    defaultLabel = Translator.Translate(lbSendOwnerToRecharge),
                    defaultDesc = Translator.Translate(txtSendOwnerToRecharge),
                    icon = UI_ButtonForceRecharge,
                    hotKey = KeyBindingDefOf.Misc7,
                    activateSound = SoundDef.Named("Click"),
                    action = new Action(Button_CallBotForShutdown),
                    disabled = powerComp != null && !powerComp.PowerOn,
                    disabledReason = Translator.Translate(txtNoPower),
                    groupKey = num + 2
                };
                yield return command_Action2;
				command_Action2 = null;
			}
            var command_Action3 = new Command_Action
            {
                defaultLabel = Translator.Translate(lbRecallAllRobots),
                defaultDesc = Translator.Translate(txtRecallAllRobots),
                icon = UI_ButtonForceRechargeAll,
                hotKey = KeyBindingDefOf.Misc8,
                activateSound = SoundDef.Named("Click"),
                action = new Action(Button_CallAllBotsForShutdown),
                disabled = powerComp != null && !powerComp.PowerOn,
                disabledReason = Translator.Translate(txtNoPower),
                groupKey = num + 3
            };
            yield return command_Action3;
			command_Action3 = null;
            var command_Action4 = new Command_Action
            {
                defaultLabel = Translator.Translate(lbActivateAllRobots),
                defaultDesc = Translator.Translate(txtActivateAllRobots),
                icon = UI_ButtonForceActivateAll,
                hotKey = KeyBindingDefOf.Misc10,
                activateSound = SoundDef.Named("Click"),
                action = new Action(Button_SpawnAllAvailableBots),
                disabled = powerComp != null && !powerComp.PowerOn,
                disabledReason = Translator.Translate(txtNoPower),
                groupKey = num + 4
            };
            yield return command_Action4;
			command_Action4 = null;
			var godMode = DebugSettings.godMode;
			if (godMode)
			{
                var command_Action5 = new Command_Action
                {
                    defaultLabel = "(DEBUG) Reset destroyed robot",
                    defaultDesc = "",
                    icon = BaseContent.BadTex,
                    hotKey = null,
                    activateSound = SoundDef.Named("Click"),
                    action = new Action(Button_ResetDestroyedRobot),
                    disabled = false,
                    disabledReason = "",
                    groupKey = num + 9
                };
                yield return command_Action5;
				command_Action5 = null;
			}
			yield break;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000028DC File Offset: 0x00000ADC
		public void GetGraphics()
		{
			var flag = def2 == null;
			if (flag)
			{
				ReadXMLData();
			}
			var flag2 = PrimaryGraphic == null || PrimaryGraphic == BaseContent.BadGraphic;
			if (flag2)
			{
				PrimaryGraphic = base.Graphic;
			}
			var flag3 = SecondaryGraphic == null || SecondaryGraphic == BaseContent.BadGraphic;
			if (flag3)
			{
				SecondaryGraphic = GraphicDatabase.Get<Graphic_Multi>(SecondaryGraphicPath, def2.graphic.Shader, def2.graphic.drawSize, def2.graphic.Color, def2.graphic.ColorTwo);
			}
			var flag4 = UI_ButtonForceRecharge == null;
			if (flag4)
			{
                UI_ButtonForceRecharge = ContentFinder<Texture2D>.Get("UI/Commands/Robots/UI_ShutDown", true);
			}
			var flag5 = UI_ButtonStart == null;
			if (flag5)
			{
                UI_ButtonStart = ContentFinder<Texture2D>.Get("UI/Commands/Robots/UI_Start", true);
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000029E8 File Offset: 0x00000BE8
		public override string GetInspectString()
		{
			var stringBuilder = new StringBuilder();
			var inspectString = base.GetInspectString();
			var flag = inspectString.NullOrEmpty();
			if (flag)
			{
				stringBuilder.Append("...");
			}
			else
			{
				stringBuilder.Append(base.GetInspectString());
			}
			var flag2 = robot != null && robot.Spawned;
			if (flag2)
			{
				stringBuilder.AppendLine().Append(Translator.Translate("AIRobot_RobotIs") + " " + robot.LabelShort);
			}
			else
			{
				var flag3 = robotIsDestroyed;
				if (flag3)
				{
					stringBuilder.AppendLine().Append(Translator.Translate("AIRobot_RobotIsDestroyed"));
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002AA8 File Offset: 0x00000CA8
		private bool IsRobotInContainer()
		{
			var flag = container == null;
			bool result;
			if (flag)
			{
				ClearContainer();
				result = false;
			}
			else
			{
				var flag2 = spawnThingDef.NullOrEmpty();
				if (flag2)
				{
					result = false;
				}
				else
				{
					var flag3 = thingDefSpawn == null;
					if (flag3)
					{
						thingDefSpawn = DefDatabase<ThingDef>.GetNamedSilentFail(spawnThingDef);
					}
					var flag4 = thingDefSpawn == null || NumContained(container, thingDefSpawn) == 0;
					result = !flag4;
				}
			}
			return result;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002B38 File Offset: 0x00000D38
		public void Notify_CallBotForShutdown()
		{
			Button_CallBotForShutdown();
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002B40 File Offset: 0x00000D40
		public void Notify_SpawnBot()
		{
			Button_SpawnBot();
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002B48 File Offset: 0x00000D48
		public int NumContained(List<ArcBaseRobot> workList, ThingDef def)
		{
			var num = 0;
			for (var i = 0; i < workList.Count; i++)
			{
				var flag = workList[i].def == def;
				if (flag)
				{
					num += workList[i].stackCount;
				}
			}
			return num;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002B94 File Offset: 0x00000D94
		public override void PostMake()
		{
			base.PostMake();
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002B9C File Offset: 0x00000D9C
		private void ReadXMLData()
		{
			def2 = def as ThingDef_BaseRobot_Building_RechargeStation;
			if (def2 != null)
			{
				spawnThingDef = def2.spawnThingDef;
				SecondaryGraphicPath = def2.secondaryGraphicPath;
				rechargeEfficiency = def2.rechargeEfficiency;
			}
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002BFD File Offset: 0x00000DFD
		public void Setup_Part2()
		{
			GetGraphics();
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002C08 File Offset: 0x00000E08
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			ReadXMLData();
			base.SpawnSetup(map, respawningAfterLoad);
			powerComp = GetComp<CompPowerTrader>();
			var flag = container == null;
			if (flag)
			{
				ClearContainer();
			}
			LongEventHandler.ExecuteWhenFinished(new Action(Setup_Part2));
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002C58 File Offset: 0x00000E58
		public override void Tick()
		{
			base.Tick();
			UpdateGraphic();
			if (robot == null && IsRobotInContainer())
			{
				ArcBaseRobot arcBaseRobot = container[0];
				if (arcBaseRobot == null)
				{
					container.Remove(container[0]);
				}
				else
				{
					Need_Battery need_Battery = arcBaseRobot.needs.TryGetNeed<Need_Battery>();
					if (SpawnRobotAfterRecharge && need_Battery.CurLevel >= 0.99)
					{
						Button_SpawnBot();
					}
					else if (need_Battery.CurLevel < 1f)
					{
						need_Battery.CurLevel += 4E-05f * rechargeEfficiency;
						if (need_Battery.CurLevel > 1f)
						{
							need_Battery.CurLevel = 1f;
						}
						TryThrowBatteryMote(arcBaseRobot);
					}
				}
			}
			else if (!robotIsDestroyed && (robot != null || (robotSpawnedOnce && IsRobotInContainer())))
			{
				if (robotSpawnedOnce && !IsRobotInContainer() && (robot == null || robot.Destroyed || robot.Dead))
				{
					if ((robot.Destroyed || robot.Dead) && robot.Corpse != null)
					{
						robot.Corpse.Destroy(DestroyMode.Vanish);
					}
					robotIsDestroyed = true;
				}
				else
				{
					checkDistanceAndEnergyTicks--;
					if (checkDistanceAndEnergyTicks <= 0)
					{
						checkDistanceAndEnergyTicks = 192;
						Need_Battery need_Battery2 = robot.needs.TryGetNeed<Need_Battery>();
						if (need_Battery2.CurLevel < 0.4 && !robot.Drafted && !BaseRobot_Helper.IsInDistance(Position, robot.Position, 50f))
						{
							Button_CallBotForShutdown();
							SpawnRobotAfterRecharge = true;
						}
					}
				}
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002E84 File Offset: 0x00001084
		private void TryThrowBatteryMote(ArcBaseRobot robot)
		{
			if (robot != null)
			{
				timerMoteThrow--;
				if (timerMoteThrow <= 0)
				{
					timerMoteThrow = 300;
					var curLevel = robot.needs.TryGetNeed<Need_Battery>().CurLevel;
					if (curLevel <= 0.99)
					{
						var flag = curLevel > 0.9;
						if (flag)
						{
							MoteThrowHelper.ThrowBatteryGreen(Position.ToVector3(), Map, 0.8f);
						}
						else
						{
							var flag2 = curLevel > 0.7;
							if (flag2)
							{
								MoteThrowHelper.ThrowBatteryYellowYellow(Position.ToVector3(), Map, 0.8f);
							}
							else
							{
								var flag3 = curLevel > 0.35;
								if (flag3)
								{
									MoteThrowHelper.ThrowBatteryYellow(Position.ToVector3(), Map, 0.8f);
								}
								else
								{
									MoteThrowHelper.ThrowBatteryRed(Position.ToVector3(), Map, 0.8f);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002FBC File Offset: 0x000011BC
		private void UpdateGraphic()
		{
			var flag = Graphic != graphicOld || updateGraphicForceNeeded;
			if (flag)
			{
				updateGraphicForceNeeded = false;
				graphicOld = Graphic;
				Notify_ColorChanged();
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things, true, false);
			}
		}

		// Token: 0x04000005 RID: 5
		private static Texture2D UI_ButtonForceRecharge = ContentFinder<Texture2D>.Get("UI/Commands/Robots/UI_ShutDown", true);

		// Token: 0x04000006 RID: 6
		private static readonly Texture2D UI_ButtonForceActivateAll = ContentFinder<Texture2D>.Get("UI/Commands/Robots/UI_StartAll", true);

		// Token: 0x04000007 RID: 7
		private static readonly Texture2D UI_ButtonForceRechargeAll = ContentFinder<Texture2D>.Get("UI/Commands/Robots/UI_ShutDownAll", true);

		// Token: 0x04000008 RID: 8
		private static Texture2D UI_ButtonStart = ContentFinder<Texture2D>.Get("UI/Commands/Robots/UI_Start", true);

		// Token: 0x04000009 RID: 9
		private bool robotSpawnedOnce = false;

		// Token: 0x0400000A RID: 10
		public ArcBaseRobot robot;

		// Token: 0x0400000B RID: 11
		public List<ArcBaseRobot> container;

		// Token: 0x0400000C RID: 12
		public ThingDef_BaseRobot_Building_RechargeStation def2 = null;

		// Token: 0x0400000D RID: 13
		private bool robotIsDestroyed = false;

		// Token: 0x0400000E RID: 14
		private ThingDef thingDefSpawn;

		// Token: 0x0400000F RID: 15
		private int timerMoteThrow = 0;

		// Token: 0x04000010 RID: 16
		private bool updateGraphicForceNeeded = false;

		// Token: 0x04000011 RID: 17
		private Graphic graphicOld;

		// Token: 0x04000012 RID: 18
		public CompPowerTrader powerComp;

		// Token: 0x04000013 RID: 19
		private int checkDistanceAndEnergyTicks;

		// Token: 0x04000014 RID: 20
		private float rechargeEfficiency = 1f;

		// Token: 0x04000015 RID: 21
		public bool SpawnRobotAfterRecharge = true;

		// Token: 0x04000016 RID: 22
		private string spawnThingDef = "";

		// Token: 0x04000017 RID: 23
		private readonly string txtSendOwnerToRecharge = "AIRobot_SendOwnerToRecharge";

		// Token: 0x04000018 RID: 24
		private readonly string lbSendOwnerToRecharge = "AIRobot_Label_SendOwnerToRecharge";

		// Token: 0x04000019 RID: 25
		private readonly string txtSpawnOwner = "AIRobot_SpawnRobot";

		// Token: 0x0400001A RID: 26
		private readonly string lbSpawnOwner = "AIRobot_Label_SpawnRobot";

		// Token: 0x0400001B RID: 27
		private readonly string txtNoPower = "AIRobot_NoPower";

		// Token: 0x0400001C RID: 28
		private readonly string lbRecallAllRobots = "AIRobot_Label_RecallAllRobots";

		// Token: 0x0400001D RID: 29
		public string SecondaryGraphicPath;

		// Token: 0x0400001E RID: 30
		public Graphic SecondaryGraphic;

		// Token: 0x0400001F RID: 31
		public Graphic PrimaryGraphic;

		// Token: 0x04000020 RID: 32
		private readonly string txtActivateAllRobots = "AIRobot_ActivateAllRobots";

		// Token: 0x04000021 RID: 33
		private readonly string lbActivateAllRobots = "AIRobot_Label_ActivateAllRobots";

		// Token: 0x04000022 RID: 34
		private readonly string txtRecallAllRobots = "AIRobot_RecallAllRobots";
	}
}
