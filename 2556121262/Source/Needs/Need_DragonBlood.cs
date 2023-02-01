using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;

namespace HalfDragons
{
    public class Need_DragonBlood : Need
    {
        int age;
        int AgeInTicks => age * 150;

        List<BodyPartRecord> bodyPartsToHeal = new List<BodyPartRecord>();

        public Need_DragonBlood() { }

        public Need_DragonBlood(Pawn pawn)
        {
            this.pawn = pawn;
        }

        /*protected override bool IsFrozen
        {
            get
            {
                return base.IsFrozen || pawn.health.hediffSet.HasHediff(HD_HediffDefOf.HD_regenerativeExhaustion);
            }
        }*/

        public override void NeedInterval()
        {
            if (IsFrozen)
            {
                //Log.Message("dragonblood need frozen");
                return;
            }
            age++;
            //Log.Message("needincrease interval: " + SettingsAccess.needIncreaseInterval);
            if (AgeInTicks % SettingsAccess.needIncreaseInterval == 0)
            {
                CurLevel += SettingsAccess.needIncreaseValue;
            }
            DetermineBodyPartsToHeal(); // add new parts below healing threshold
            RemoveHealedParts();    // remove old parts that have been healed above healing threshold
            if (bodyPartsToHeal.Count > 0)
            {
                DoDragonBloodHealing();
            }
            if(CurLevel == 0)
            {
                HediffDef regenExhaustDef = HD_HediffDefOf.HD_regenerativeExhaustion;
                if (!pawn.health.hediffSet.HasHediff(regenExhaustDef))
                {
                    Hediff regenExhaustion = HediffMaker.MakeHediff(regenExhaustDef, pawn);
                    regenExhaustion.PostMake();
                    pawn.health.AddHediff(regenExhaustion);
                }
            }
        }

        private void DoDragonBloodHealing()
        {
            //Log.Message("Body parts to heal: " + string.Join(", ", bodyPartsToHeal.ConvertAll(part => part.def.defName)));
            BodyPartRecord partToHeal = bodyPartsToHeal.RandomElement();
            IEnumerable<Hediff_Injury> injuries = pawn.health.hediffSet.GetHediffs<Hediff_Injury>().Where(hediff => hediff.Part == partToHeal);
            Hediff_Injury injuryToHeal = injuries.RandomElement();
            injuryToHeal.Heal(SettingsAccess.injuryHealingPoints);
            CurLevel -= SettingsAccess.injuryHealingCost;
        }

        private void RemoveHealedParts()
        {
            foreach(BodyPartRecord part in bodyPartsToHeal.ToList())
            {
                float thresholdToBeConsideredHealed = part.def.GetMaxHealth(pawn) * SettingsAccess.thresholdToBeConsideredHealed;
                if (pawn.health.hediffSet.GetPartHealth(part) == thresholdToBeConsideredHealed)
                {
                    bodyPartsToHeal.Remove(part);
                }
            }
        }

        private void DetermineBodyPartsToHeal()
        {
            HediffSet hediffs = pawn.health?.hediffSet;
            IEnumerable<BodyPartRecord> damagedBodyParts = hediffs?.GetInjuredParts();
            if (damagedBodyParts.EnumerableNullOrEmpty()){
                return;
            }
            //Log.Message("damaged body parts: " + string.Join(", ", damagedBodyParts.Select(part => part.def.defName)));
            damagedBodyParts = damagedBodyParts.Where(
                // filter to all parts, that are at 50% or lower
                part => hediffs.GetPartHealth(part) <= part.def.GetMaxHealth(pawn) * SettingsAccess.thresholdToBeConsideredDamaged
            );

            //Log.Message("damaged body parts below 50%: " + string.Join(", ", damagedBodyParts.Select(part => part.def.defName)));
            foreach(BodyPartRecord part in damagedBodyParts)
            {
                if (!bodyPartsToHeal.Contains(part))
                {
                    bodyPartsToHeal.Add(part);
                }
            }
        }
    }
}
