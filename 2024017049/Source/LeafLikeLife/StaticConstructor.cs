using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace LeafLikeLife
{
  [StaticConstructorOnStartup]
  public static class LLLStartUp
  {
    static LLLStartUp()
    {
      // Loads right before main menu
      LLLUtility.UpdateAllChanges();
    }
  }

  public class LLLUtility
  {
    public static void UpdateAllChanges()
    {
      // ThingDefOf.SmokeleafJoint.comps.Where(c => c is CompProperties_Drug).FirstOrDefault()
      //HediffDef.Named("SmokeleafHigh").stages.Where((HediffStage stage) => stage.capMods.Any((PawnCapacityModifier mod) => mod.capacity == PawnCapacityDefOf.Consciousness)).First().capMods.Where((PawnCapacityModifier mod) => mod.capacity == PawnCapacityDefOf.Consciousness).First().offset = LLLModSettings.amountPenaltyConsciousness;
      
      HediffDef.Named("SmokeleafHigh").stages.Where(s => s.capMods.Any(m => m.capacity == PawnCapacityDefOf.Consciousness)).First().capMods.Where(m => m.capacity == PawnCapacityDefOf.Consciousness).First().offset = LLLModSettings.amountPenaltyConsciousness;
      HediffDef.Named("SmokeleafHigh").stages.Where(s => s.capMods.Any(m => m.capacity == PawnCapacityDefOf.Moving)).First().capMods.Where(m => m.capacity == PawnCapacityDefOf.Moving).First().offset = LLLModSettings.amountPenaltyMovement;

      IngestionOutcomeDoer_OffsetNeed restOffset = (IngestionOutcomeDoer_OffsetNeed)ThingDefOf.SmokeleafJoint.ingestible.outcomeDoers.Find(o => o is IngestionOutcomeDoer_OffsetNeed);
      restOffset.offset = LLLModSettings.amountPenaltyRest;

      HediffDef.Named("SmokeleafHigh").stages[0].hungerRateFactorOffset = LLLModSettings.amountHungerRate;

      CompProperties_Drug comDrug = ThingDefOf.SmokeleafJoint.GetCompProperties<CompProperties_Drug>() as CompProperties_Drug;

      comDrug.addictiveness = LLLModSettings.amountAddictiveness;
      comDrug.minToleranceToAddict = LLLModSettings.amountToleranceToAddict;

      HediffGiver_RandomDrugEffect hediffAsthma = HediffDef.Named("SmokeleafTolerance").hediffGivers[0] as HediffGiver_RandomDrugEffect;
      HediffGiver_RandomDrugEffect hediffCancer = HediffDef.Named("SmokeleafTolerance").hediffGivers[1] as HediffGiver_RandomDrugEffect;

      hediffAsthma.baseMtbDays = LLLModSettings.amountAsthmaDaysChance;
      hediffCancer.baseMtbDays = LLLModSettings.amountCancerDaysChance;

      ThoughtDef.Named("SmokeleafWithdrawal").stages[1].baseMoodEffect = LLLModSettings.amountPenaltyWithdrawal;
    }
  }
}
