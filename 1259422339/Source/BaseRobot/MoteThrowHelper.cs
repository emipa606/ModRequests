using UnityEngine;
using Verse;

namespace BaseRobot
{
    // Token: 0x02000003 RID: 3
    public static class MoteThrowHelper
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
		public static Mote ThrowBatteryGreen(Vector3 loc, Map map, float scale)
		{
			return ThrowBatteryXYZ(DefDatabase<ThingDef>.GetNamed("Mote_BatteryGreen", true), loc, map, scale);
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000206D File Offset: 0x0000026D
		public static Mote ThrowBatteryRed(Vector3 loc, Map map, float scale)
		{
			return ThrowBatteryXYZ(DefDatabase<ThingDef>.GetNamed("Mote_BatteryRed", true), loc, map, scale);
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002084 File Offset: 0x00000284
		public static Mote ThrowBatteryXYZ(ThingDef moteDef, Vector3 loc, Map map, float scale)
		{
			Mote result;
			if (!GenView.ShouldSpawnMotesAt(loc, map) || map.moteCounter.Saturated)
			{
				result = null;
			}
			else
			{
				var moteThrown = (MoteThrown)ThingMaker.MakeThing(moteDef, null);
				moteThrown.Scale = scale;
				moteThrown.rotationRate = Rand.Range(-1, 1);
				moteThrown.exactPosition = loc;
				moteThrown.exactPosition += new Vector3(THIRTY_FIVE, 0f, THIRTY_FIVE);
				moteThrown.exactPosition += new Vector3(Rand.Value, 0f, Rand.Value) * POINT_TEN;
				moteThrown.SetVelocity(Rand.Range(30, 60), Rand.Range(THIRTY_FIVE, FIFTY_FIVE));
				GenSpawn.Spawn(moteThrown, IntVec3Utility.ToIntVec3(loc), map, WipeMode.Vanish);
				result = moteThrown;
			}
			return result;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002162 File Offset: 0x00000362
		public static Mote ThrowBatteryYellow(Vector3 loc, Map map, float scale)
		{
			return ThrowBatteryXYZ(DefDatabase<ThingDef>.GetNamed("Mote_BatteryYellow", true), loc, map, scale);
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002177 File Offset: 0x00000377
		public static Mote ThrowBatteryYellowYellow(Vector3 loc, Map map, float scale)
		{
			return ThrowBatteryXYZ(DefDatabase<ThingDef>.GetNamed("Mote_BatteryYellowYellow", true), loc, map, scale);
		}

		// Token: 0x04000001 RID: 1
		private const float THIRTY_FIVE = 0.35f;

		// Token: 0x04000002 RID: 2
		private const float FIFTY_FIVE = 0.55f;

		// Token: 0x04000003 RID: 3
		private const float POINT_TEN = 0.1f;
	}
}
