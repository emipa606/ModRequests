using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace HLVRMonsters
{
	// Token: 0x02000A38 RID: 2616
	public class IncidentWorker_AntlionInfestation : IncidentWorker
	{
		// Token: 0x06003E96 RID: 16022 RVA: 0x0014A888 File Offset: 0x00148A88
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec;
			return base.CanFireNowSub(parms) && HiveUtility.TotalSpawnedHivesCount(map) < 30 && InfestationCellFinder.TryFindCell(out intVec, map);
		}

		// Token: 0x06003E97 RID: 16023 RVA: 0x0014A8C0 File Offset: 0x00148AC0
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Thing t = AntlionInfestationUtility.SpawnTunnels(Mathf.Max(GenMath.RoundRandom(parms.points / 220f), 1), map, false, false, null);
			base.SendStandardLetter(parms, t, Array.Empty<NamedArgument>());
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}

		// Token: 0x04002558 RID: 9560
		public const float HivePoints = 220f;
	}
}