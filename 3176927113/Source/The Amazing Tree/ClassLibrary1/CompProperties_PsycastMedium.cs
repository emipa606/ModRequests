using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace The_Amazing_Tree
{
    public class CompProperties_PsycastMedium : CompProperties
    {
        public CompProperties_PsycastMedium()
        {
            compClass = typeof(CompPsycastMedium);

        }
        public int minSight;
        public int maxSight;
        public int daysBeforeShrink;
    }

    public class CompPsycastMedium : ThingComp
    {
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref sight, "Sight");
            Scribe_Values.Look(ref meditationStrength, "MeditationStrength");
            Scribe_Values.Look(ref daysSinceLastReduction, "DaysSinceLastReduction");

        }
        public override void PostPostMake()
        {
            base.PostPostMake();
            sight = Props.minSight;
        }

        //Reduce 1 radius every X days
        public override void CompTickLong()
        {
            if (GenLocalDate.DayTick(this.parent.Map) < 2000)
            {
                Log.Message("DayTick");
                daysSinceLastReduction++;
                if (daysSinceLastReduction >= Props.daysBeforeShrink)
                {
                    daysSinceLastReduction = 0;
                    sight = Mathf.Max(sight - 1, Props.minSight);
                }
            }
        }

        //Add up medium power
        public void AddPower()
        {
            meditationStrength += 0.00005f;
            if (meditationStrength >= 1f)
            {
                meditationStrength = 0f;
                sight = Mathf.Min(sight + 1, Props.maxSight);
            }
        }

        public override string CompInspectStringExtra()
        {
            if (sight < Props.maxSight)
            {
                return "CurrentRadius".Translate(sight) + "\n" + "ProgressToNext".Translate(meditationStrength * 100) + "\n" + "DaysBeforeNextShrink".Translate(Props.daysBeforeShrink - daysSinceLastReduction);
            }
            else
            {
                return "CurrentRadius".Translate(sight) + "\n" + "DaysBeforeNextShrink".Translate(Props.daysBeforeShrink - daysSinceLastReduction);
            }

        }

        public CompProperties_PsycastMedium Props
        {
            get
            {
                return (CompProperties_PsycastMedium)this.props;
            }
        }

        public List<MeditationFocusDef> MedicationFocuses
        {
            get
            {
                CompMeditationFocus comp =parent.GetComp<CompMeditationFocus>();
                if (comp == null)
                {
                    return new List<MeditationFocusDef>();
                }
                return comp.Props.focusTypes;
            }
        }
        public float Sight
        {
            get
            {
                if(sight < Props.minSight)
                {
                    return Props.minSight;
                }
                else
                {
                    return sight;
                }
            }
        }
        float sight;
        float meditationStrength;
        int daysSinceLastReduction;
    }

}
