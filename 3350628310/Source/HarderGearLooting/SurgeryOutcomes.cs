using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HarderGearLooting
{
    internal class SurgeryOutcome_Strip: SurgeryOutcome
    {
        public override bool Apply(
            float quality,
            RecipeDef recipe,
            Pawn surgeon,
            Pawn patient,
            BodyPartRecord part)
        {
            if (Rand.Chance(quality))
            {
                return false;
            }
            Hediff acidifier;
            if (!patient.health.hediffSet.TryGetHediff(HardGearLooting_DefOf.Acidifier, out acidifier)){
                Log.Error($"No acidifier found when performing surgery to remove it.");
            }
            patient.health.RemoveHediff(acidifier);
            patient.Strip();
            patient.health.AddHediff(acidifier, part);
            this.ApplyDamage(patient, part);
            if (!patient.Dead)
                patient.Kill(new DamageInfo?(), (Hediff)null);
            this.SendLetter(surgeon, patient, recipe);
            return true;
        }
    }

    internal class SurgeryOutcome_AcidifierActivated : SurgeryOutcome
    {
        public override bool Apply(
            float quality,
            RecipeDef recipe,
            Pawn surgeon,
            Pawn patient,
            BodyPartRecord part)
        {
            this.ApplyDamage(patient, part);
            if (!patient.Dead)
                patient.Kill(new DamageInfo?(), (Hediff)null);
            this.SendLetter(surgeon, patient, recipe);
            return true;
        }
    }

    public class SurgeryOutcomeComp_SettingsFactor : SurgeryOutcomeComp_Factor
    {
        public InspirationDef inspirationDef;

        public override void AffectQuality(
           RecipeDef recipe,
          Pawn surgeon,
          Pawn patient,
          List<Thing> ingredients,
          BodyPartRecord part,
          Bill bill,
          ref float quality)
        {
            quality *= HarderGearLootingSettings.acidifierRemovalSuccessFactor;
        }
    }
}
