using HugsLib.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace KriilMod_CD
{
	public class Building_CombatDummy : Building
	{
		[Flags]
		public enum TrainingTypes
		{
			None = 0x0,
			Melee = 0x1,
			Ranged = 0x2,
			Any = 0x3
		}
		 
		public TrainingTypes trainingType = TrainingTypes.None;

		public override void PostMapInit()
		{
			if (HugsLibUtility.HasDesignation((Thing)this, CombatTrainingDefOf.TrainCombatDesignation))
			{
				trainingType = TrainingTypes.Any;
			}
			else if (HugsLibUtility.HasDesignation((Thing)this, CombatTrainingDefOf.TrainCombatDesignationMeleeOnly))
			{
				trainingType = TrainingTypes.Melee;
			}
			else if (HugsLibUtility.HasDesignation((Thing)this, CombatTrainingDefOf.TrainCombatDesignationRangedOnly))
			{
				trainingType = TrainingTypes.Ranged;
			}
		}

		public void determineDesignation()
		{
			HugsLibUtility.ToggleDesignation((Thing)this, CombatTrainingDefOf.TrainCombatDesignation, false);
			HugsLibUtility.ToggleDesignation((Thing)this, CombatTrainingDefOf.TrainCombatDesignationMeleeOnly, false);
			HugsLibUtility.ToggleDesignation((Thing)this, CombatTrainingDefOf.TrainCombatDesignationRangedOnly, false);
			switch (trainingType)
			{
			case TrainingTypes.None:
				break;
			case TrainingTypes.Melee:
				HugsLibUtility.ToggleDesignation((Thing)this, CombatTrainingDefOf.TrainCombatDesignationMeleeOnly, true);
				break;
			case TrainingTypes.Ranged:
				HugsLibUtility.ToggleDesignation((Thing)this, CombatTrainingDefOf.TrainCombatDesignationRangedOnly, true);
				break;
			case TrainingTypes.Any:
				HugsLibUtility.ToggleDesignation((Thing)this, CombatTrainingDefOf.TrainCombatDesignation, true);
				break;
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return new Command_Toggle
			{
				isActive = (() => (trainingType & TrainingTypes.Melee) != 0),
				toggleAction = delegate
				{
					trainingType ^= TrainingTypes.Melee;
					determineDesignation();
				},
				defaultDesc = "CommandTrainCombatMeleeOnlyDesc".Translate(),
				icon = TexCommand.AttackMelee,
				defaultLabel = "CommandTrainCombatMeleeOnlyLabel".Translate()
			};
			yield return new Command_Toggle
			{
				isActive = (() => (trainingType & TrainingTypes.Ranged) != 0),
				toggleAction = delegate
				{
					trainingType ^= TrainingTypes.Ranged;
					determineDesignation();
				},
				defaultDesc = "CommandTrainCombatRangedOnlyDesc".Translate(),
				icon = TexCommand.Attack,
				defaultLabel = "CommandTrainCombatRangedOnlyLabel".Translate()
			};
		}

	}
}
