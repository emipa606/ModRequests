﻿using System;
using Verse;
using RimWorld;


namespace HLVRMonsters
{
	
	public class DeathActionWorker_AntlionWorkerExplosion : DeathActionWorker
	{
		
		public override RulePackDef DeathRules
		{
			get
			{
				return RulePackDefOf.Transition_DiedExplosive;
			}
		}

		
		public override bool DangerousInMelee
		{
			get
			{
				return true;
			}
		}

		
		public override void PawnDied(Corpse corpse)
		{
			GenExplosion.DoExplosion(corpse.Position, corpse.Map, 1.9f, HLVRDefOf.HLVR_WorkerBurst, corpse.InnerPawn, -1, -1f, null, null, null, null, null, 0f, 1,GasType.Unused, false, null, 0f, 1, 0f, false, null, null);
			corpse.Destroy();
		}
	}
}
