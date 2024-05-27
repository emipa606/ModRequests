using System;
using Verse;
using RimWorld;
using HLVR;


namespace RimWorld
{
	// Token: 0x02000B59 RID: 2905
	public class DeathActionWorker_SnarkNestPop : DeathActionWorker
	{
		// Token: 0x17000C2F RID: 3119
		// (get) Token: 0x060044EE RID: 17646 RVA: 0x00170B2A File Offset: 0x0016ED2A
		public override RulePackDef DeathRules
		{
			get
			{
				return RulePackDefOf.Transition_DiedExplosive;
			}
		}

		// Token: 0x17000C30 RID: 3120
		// (get) Token: 0x060044EF RID: 17647 RVA: 0x00010451 File Offset: 0x0000E651
		public override bool DangerousInMelee
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060044F0 RID: 17648 RVA: 0x00170BC4 File Offset: 0x0016EDC4
		public override void PawnDied(Corpse corpse)
		{
			for (int i = 0; i <= 8; i++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(HLVRDefOf.Monster_Snark, null);
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(corpse.Position, corpse.Map, 2, null);
				GenSpawn.Spawn(pawn, loc, corpse.Map, corpse.Rotation, WipeMode.Vanish, false);
			}
			corpse.Destroy();
		}
	}
}
