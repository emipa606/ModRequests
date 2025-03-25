using RimWorld;
using Verse;

namespace SimpleSlaveryCollars
{
	public class ThoughtWorker_SlaveCollar : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn pawn)
		{
			float num = pawn.records.GetValue(SS_RecordDefOf.TimeAsSlave);
			float stage1 = 60000f * SimpleSlaveryCollarsSetting.Slavestage1Period;
			float stage2 = 60000f * SimpleSlaveryCollarsSetting.Slavestage2Period;
			float stage3 = 60000f * SimpleSlaveryCollarsSetting.Slavestage3Period;
			if (SlaveUtility.HasSlaveCollar(pawn) && !(SlaveUtility.GetSlaveCollar(pawn).def.thingClass == typeof(SlaveCollar_Explosive)))
			{
				if (pawn.story.traits.HasTrait(TraitDef.Named("Masochist"))) return ThoughtState.ActiveAtStage(2);
				else if (pawn.IsColonist && !pawn.IsSlaveOfColony) return ThoughtState.ActiveAtStage(3);
				return num < stage1 + stage2 + stage3 ? ThoughtState.ActiveAtStage(0) : ThoughtState.ActiveAtStage(1);
			}
			return ThoughtState.Inactive;
		}
	}
}