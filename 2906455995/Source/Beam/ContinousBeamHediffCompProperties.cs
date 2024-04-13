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
    public class BeamHediffCompProperties_SeverityPerDay : HediffCompProperties
    {
        public float severityPerDay;

        public bool showDaysToRecover;

        public bool showHoursToRecover;

        public float mechanitorFactor = 1f;

        public float reverseSeverityChangeChance;

        public FloatRange severityPerDayRange = FloatRange.Zero;

        public float minAge;

        public BeamHediffCompProperties_SeverityPerDay()
        {
            compClass = typeof(BeamHediffComp_SeverityPerDay);
        }

        public float CalculateSeverityPerDay()
        {
            float num = severityPerDay + severityPerDayRange.RandomInRange;
            if (Rand.Chance(reverseSeverityChangeChance))
            { 
                num *= -1f;
            }
            return num;
        }
    }

}