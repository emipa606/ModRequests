using System;
using RimWorld;
using Verse;

namespace Crystosentient
{
	// Token: 0x0200000E RID: 14
	public class JobGiver_CrystalDefendSelf : JobGiver_AIDefendPawn
	{
		// Token: 0x06000058 RID: 88 RVA: 0x00003E58 File Offset: 0x00002058
		protected override Pawn GetDefendee(Pawn pawn)
		{
			return pawn;
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00003E6C File Offset: 0x0000206C
		protected override float GetFlagRadius(Pawn pawn)
		{
			return 22f;
		}
	}
}
