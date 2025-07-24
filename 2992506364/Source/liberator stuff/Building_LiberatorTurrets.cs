using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace liberator_stuff
{
	public class Building_LiberatorTurrets : Building_TurretGun
	{
		// Token: 0x06000004 RID: 4 RVA: 0x00002100 File Offset: 0x00000300
		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			foreach (TrooperStorage TrooperStorage in from TrooperStorage b in base.Map.listerThings.ThingsOfDef(TLRSDefOf.TLRS_TrooperStorage)
																				where b.CanSpawnTroopers
													  select b)
			{
				TrooperStorage.ReleaseTroopers();
			}
			base.PreApplyDamage(ref dinfo, out absorbed);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002194 File Offset: 0x00000394
		public override bool ClaimableBy(Faction by, StringBuilder reason = null)
		{
			return false;
		}
	}
}
