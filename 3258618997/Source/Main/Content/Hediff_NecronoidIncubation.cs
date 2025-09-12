using System;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Necro
{
	
	public class Hediff_NecronoidIncubation : HediffWithComps
	{
		
		public static void SpawnNecronoid(Pawn pawn)
		{
			DefDatabase<HediffDef>.GetNamed("Necronoid_Incubation", true);
			pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).First((BodyPartRecord bpr) => bpr.def == DefDatabase<BodyPartDef>.GetNamed("Brain", false));
			int num = UnityEngine.Random.Range(1, 1);
			for (int i = 0; i < num; i++)
			{
				GenSpawn.Spawn(PawnGenerator.GeneratePawn(NecroDefOf.Necronoid_FetidParasite, Find.FactionManager.FirstFactionOfDef(FactionDefOf.Necronoid)), pawn.Position, pawn.MapHeld, WipeMode.Vanish);
			}
			pawn.health.DropBloodFilth();
			pawn.TakeDamage(new DamageInfo(NecroDefOf.GapingWound, 1000f, 0f, -1000f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
		}

		
		public override void Tick()
		{
			this.iCounter++;
			if (this.iCounter > 10)
			{
				Hediff_NecronoidIncubation.SpawnNecronoid(this.pawn);
			}
			base.Tick();
		}

		
		public Hediff_NecronoidIncubation()
		{
		}

		
		public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
		{
			base.Notify_PawnDied(dinfo, culprit);
			if (this.pawn.Corpse.Spawned)
			{
				this.pawn.Corpse.Destroy(DestroyMode.Vanish);
				return;
			}
			NecroDefOf.FleshPod_Gore.PlayOneShot(new TargetInfo(this.pawn.Position, this.pawn.Map, false));
		}

		
		private int iCounter;
        
	}
}
