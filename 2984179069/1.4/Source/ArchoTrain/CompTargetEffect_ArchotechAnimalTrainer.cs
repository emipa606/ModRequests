using RimWorld;
using Verse;

namespace ArchoTrain
{
	public class CompTargetEffect_ArchotechAnimalTrainer : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (pawn.Faction != Faction.OfPlayer || !pawn.AnimalOrWildMan() || pawn.Dead)
			{
				return;
			}
			bool flag = false;
			foreach (TrainableDef allDef in DefDatabase<TrainableDef>.AllDefs)
			{
				if (pawn.training.GetWanted(allDef))
				{
					pawn.training.Train(allDef, null, complete: true);
					bool flag2 = true;
				}
			}
			if (flag)
			{
				return;
			}
			foreach (TrainableDef allDef2 in DefDatabase<TrainableDef>.AllDefs)
			{
				if (pawn.training.CanAssignToTrain(allDef2).Accepted)
				{
					pawn.training.Train(allDef2, null, complete: true);
				}
			}
		}
	}
}
