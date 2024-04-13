using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;


namespace Beam
{
    public class BeamHediffComp_SeverityPerDay : HediffComp_SeverityModifierBase
    {
        private float severityPerDay;

        private BeamHediffCompProperties_SeverityPerDay Props => (BeamHediffCompProperties_SeverityPerDay)props;

        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (Props.showHoursToRecover && SeverityChangePerDay() < 0f)
                {
                    return Mathf.RoundToInt(parent.Severity / Mathf.Abs(SeverityChangePerDay()) * 24f) + (string)"LetterHour".Translate();
                }
                return null;
            }
        }

        public override string CompTipStringExtra
        {
            get
            {
                if (Props.showDaysToRecover && SeverityChangePerDay() < 0f)
                {
                    return "DaysToRecover".Translate((parent.Severity / Mathf.Abs(SeverityChangePerDay())).ToString("0.0"));
                }
                return null;
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
        }

        public override void CompPostInjuryHeal(float amount)
        {
            severityPerDay = amount;
            base.CompPostInjuryHeal(0);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref severityPerDay, "severityPerDay", 0f);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && severityPerDay == 0f && Props.severityPerDay != 0f && Props.severityPerDayRange == FloatRange.Zero)
            {
                //severityPerDay = Props.CalculateSeverityPerDay();
                Log.Warning("Hediff " + parent.Label + " had severityPerDay not matching parent.");
            }
        }

        public override float SeverityChangePerDay()
        {
            if (base.Pawn.ageTracker.AgeBiologicalYearsFloat < Props.minAge)
            {
                return 0f;
            }
            float num = severityPerDay;
            if (ModsConfig.BiotechActive && MechanitorUtility.IsMechanitor(base.Pawn))
            {
                num *= Props.mechanitorFactor;
            }
            return num;
        }
    }
}