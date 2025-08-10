using RimWorld;
using Verse;

namespace Komishne.SanguophageTweaks
{
    public class Utility
    {
        // Similar to how animal bloodfeeder bite ability reduces or increases the amount of blood loss if the body
        // size of the victim is large or small, reduce the bloodloss severity here based on body size.
        public static float BloodlossSeverityMultiplierFromBodySize(Pawn pawn)
        {
            // Using a simple 1/x multiplier, where x is the pawn's body size.
            // Ex: https://www.desmos.com/calculator/p8wbpnpgap

            float bodySize = pawn.BodySize;
            if (bodySize <= 0.0f)
                return 1.0f;

            if (bodySize < 0.2f)
                return 5.0f;
            if (bodySize > 10.0f)
                return 0.1f;
            return 1 / bodySize;
        }

        // Modifies the severity of a pawn's blood loss hediff by the indicated amount. Since this hediff is a measure
        // of how much blood the pawn has already lost, a positive bloodLossAmount means the pawn will lose more
        // blood, and a negative value will effectively give the pawn more blood.
        public static void AdjustBloodLoss(Pawn pawn, float bloodLossAmount, bool requiresAdjustmentForPawn = false)
        {
            if (pawn is null || bloodLossAmount == 0.0f)
                return;

            if (requiresAdjustmentForPawn)
                bloodLossAmount *= BloodlossSeverityMultiplierFromBodySize(pawn);

            if (bloodLossAmount < 0.0f)  // gaining blood back
            {
                Hediff firstBloodLossHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
                if (firstBloodLossHediff != null)
                    firstBloodLossHediff.Severity += bloodLossAmount;
            }

            if (bloodLossAmount > 0.0f)  // losing blood
            {
                Hediff newBloodLossHediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn);
                newBloodLossHediff.Severity = bloodLossAmount;
                pawn.health.AddHediff(newBloodLossHediff);
            }
        }

        public static void ApplySurgeryOnDeathHediff(Pawn surgeon, Pawn patient, RecipeDef recipe, BodyPartRecord bodyPart)
        {
            if (patient is null)
            {
                Log.Error(
                    $"[KOM.SanguophageTweaks] In ApplySurgeryOnDeathHediff, but patient is null. " +
                    $"Parameters: surgeon: {surgeon?.ToString() ?? "<null>"}, " +
                    $"patient: {patient?.ToString() ?? "<null>"}, " +
                    $"recipe: {recipe?.ToString() ?? "<null>"}, " +
                    $"bodyPart: {bodyPart?.ToString() ?? "<null>"}.");
                return;
            }

            Hediff mishapHediff = HediffMaker.MakeHediff(KOM_DefOf.KOM_FatalSurgeryMishap, patient, bodyPart);
            patient.health.AddHediff(mishapHediff);
        }
    }
}
