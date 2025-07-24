using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI.Group;
using KCSG;

namespace liberator_stuff
{
	public class TLRSShipPart : Building
	{
		// Token: 0x0600005A RID: 90 RVA: 0x00005374 File Offset: 0x00003574
		public override bool ClaimableBy(Faction by, StringBuilder reason = null)
		{
			return false;
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00005388 File Offset: 0x00003588
		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(ref dinfo, out absorbed);
			bool flag = this.def != TLRSDefOf.TLRS_TrooperStorage && base.Map != null;
			if (flag)
			{
				IEnumerable<TrooperStorage> source = from TrooperStorage x in base.Map.listerThings.ThingsOfDef(TLRSDefOf.TLRS_TrooperStorage)
													 where x.CanSpawnTroopers
													 select x;
				bool flag2 = source.Count<TrooperStorage>() > 0;
				if (flag2)
				{
					TrooperStorage trooperStorage = (from y in source
													 orderby y.Position.DistanceTo(base.Position)
													 select y).First<TrooperStorage>();
					trooperStorage.ReleaseTroopers();
				}
			}
		}
	}
}

