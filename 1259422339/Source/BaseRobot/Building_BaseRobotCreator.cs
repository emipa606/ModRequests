using System;
using RimWorld;
using Verse;

namespace BaseRobot
{
	// Token: 0x02000014 RID: 20
	public class Building_BaseRobotCreator : Building
	{
		// Token: 0x0600005C RID: 92 RVA: 0x00005320 File Offset: 0x00003520
		public static ArcBaseRobot CreateRobot(string pawnDefName, IntVec3 position, Map map)
		{
			return CreateRobot(pawnDefName, position, map, Faction.OfPlayer);
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00005330 File Offset: 0x00003530
		public static ArcBaseRobot CreateRobot(string pawnDefName, IntVec3 position, Map map, Faction faction)
		{
			PawnKindDef named = DefDatabase<PawnKindDef>.GetNamed(pawnDefName, true);
			var request = new PawnGenerationRequest(named, faction, PawnGenerationContext.NonPlayer, -1, true, true, false, false, false, false, 0f, false, false, true, false, false, false, false, false, 0, null, 1, null, null, null,null, new float?(0f), new float?(0f), new float?(0f), new Gender?(Gender.None), new float?(0f), null);
			var newThing = (ArcBaseRobot)PawnGenerator.GeneratePawn(request);
			return (ArcBaseRobot)Spawn(newThing, position, map);
		}

		// Token: 0x0600005E RID: 94 RVA: 0x000053AC File Offset: 0x000035AC
		public static Thing Spawn(Thing newThing, IntVec3 loc, Map map)
		{
			return Spawn(newThing, loc, map, Rot4.South);
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000053BB File Offset: 0x000035BB
		public static Thing Spawn(Thing newThing, IntVec3 loc, Map map, Rot4 rot)
		{
			newThing = GenSpawn.Spawn(newThing, loc, map, rot, WipeMode.Vanish, false);
			newThing.Position = loc;
			return newThing;
		}

		// Token: 0x06000060 RID: 96 RVA: 0x000053D4 File Offset: 0x000035D4
		public override void Tick()
		{
			var flag = destroy;
			if (flag)
			{
				Destroy(DestroyMode.Vanish);
			}
			else
			{
				try
				{
                    CreateRobot("BaseRobot_Hauler", Position, Map);
				}
				catch (Exception ex)
				{
					Log.Error("Error while creating Robot." + ex.Message + "STACK: " + ex.StackTrace, false);
				}
				destroy = true;
			}
		}

		// Token: 0x0400003B RID: 59
		private bool destroy = false;
	}
}
