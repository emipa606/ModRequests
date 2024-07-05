using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using System.Text;
namespace RimWorldRealFoW
{
	[StaticConstructorOnStartup]
    public class Building_SurveillanceCamera : Building
    {

		public override void ExposeData()
		{
			base.ExposeData();

		}

		public Building_SurveillanceCamera()
        {

        }

		public bool isPowered()
        {
			return powerComp.PowerOn;
        }

        public override string GetInspectString()
        {
            StringBuilder inspect = new StringBuilder();
            inspect.Append(base.GetInspectString());
			if(mapComp.workingCameraConsole)
			{
            inspect.AppendInNewLine("Revealing".Translate());
			} else {
				inspect.AppendInNewLine("NoCameraConsole".Translate());

			}

            return inspect.ToString();
        }
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.powerComp = base.GetComp<CompPowerTrader>();
			this.mapComp = MapUtils.getMapComponentSeenFog(map);
			this.mapComp.RegisterSurveillanceCamera(this);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002DAC File Offset: 0x00000FAC
		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(mode);
			this.mapComp.DeregisterSurveillanceCamera(this);
		}

		CompPowerTrader powerComp;
		MapComponentSeenFog mapComp;
	}
}
