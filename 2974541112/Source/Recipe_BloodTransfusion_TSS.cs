using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace zed_0xff.CPS;

// count hemogen packs inside the TSS, not on map
public class Recipe_BloodTransfusion_TSS : Recipe_Surgery {

	public override bool CompletableEver(Pawn surgeryTarget) {
		if (!surgeryTarget.health.hediffSet.HasHediff(HediffDefOf.BloodLoss)) {
			return false;
		}
		return base.CompletableEver(surgeryTarget);
	}

	public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null) {
		if (thing.MapHeld == null) {
			return false;
		}
		if (thing is Pawn pawn) {
            if( !pawn.health.hediffSet.HasHediff(HediffDefOf.BloodLoss))
                return false;
            if( !(pawn.ParentHolder is Building_TSS tss) )
                return false;
            if( !tss.HasAnyHemogen() )
                return false;
		}
		return base.AvailableOnNow(thing, part);
	}

	public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map) {
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (!ModsConfig.BiotechActive) {
			return;
		}
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < ingredients.Count; i++) {
			if (!ingredients[i].def.IsMedicine) {
				num += Recipe_BloodTransfusion.BloodlossHealedPerPack * (float)ingredients[i].stackCount;
				num2 += JobGiver_GetHemogen.HemogenPackHemogenGain * (float)ingredients[i].stackCount;
			}
		}
		if (num > 0f) {
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
			if (firstHediffOfDef != null) {
				firstHediffOfDef.Severity -= num;
			}
		}
		if (num2 > 0f && pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>() != null) {
			GeneUtility.OffsetHemogen(pawn, num2);
		}
		for (int j = 0; j < ingredients.Count; j++) {
			ingredients[j].Destroy();
		}
	}

	public override float GetIngredientCount(IngredientCount ing, Bill bill) {
		return 1;
	}
}
