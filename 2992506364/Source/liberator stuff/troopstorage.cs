using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace liberator_stuff
{
	public class TrooperStorage : TLRSShipPart
	{
		// Token: 0x06000081 RID: 129 RVA: 0x000066E0 File Offset: 0x000048E0
		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(ref dinfo, out absorbed);
			bool canSpawnTroopers = this.CanSpawnTroopers;
			if (canSpawnTroopers)
			{
				this.ReleaseTroopers();
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000082 RID: 130 RVA: 0x0000670A File Offset: 0x0000490A
		public bool CanSpawnTroopers
		{
			get
			{
				return !this.troopersAreReleased && base.Map != null;
			}
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00006720 File Offset: 0x00004920
		public void ReleaseTroopers()
		{
			Faction faction = Find.FactionManager.FirstFactionOfDef(TLRSDefOf.TLRS_LiberatorsFaction);
			IEnumerable<Pawn> enumerable = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
			{
				tile = base.Map.Tile,
				faction = faction,
				points = 300f,
				groupKind = PawnGroupKindDefOf.Combat
			}, true);
			foreach (Pawn pawn in enumerable)
			{
				GenPlace.TryPlaceThing(pawn, base.Position, base.Map, ThingPlaceMode.Near, null, null, default(Rot4));
				MechUtils.CreateOrAddToAssaultLord(pawn, null, false, false, false, false, false);
			}
			this.troopersAreReleased = true;
		}

		// Token: 0x06000084 RID: 132 RVA: 0x000067EC File Offset: 0x000049EC
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.troopersAreReleased, "troopersAreReleased", false, false);
		}

		// Token: 0x04000044 RID: 68
		private bool troopersAreReleased;
	}
}
