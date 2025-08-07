using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Stranger_Danger
{
    [StaticConstructorOnStartup]
    public static class CompToHumanlikes_SD
    {
        static CompToHumanlikes_SD()
        {
            AddCompToHumanlikes();
        }

        public static void AddCompToHumanlikes()
        {
            foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (allDef.race != null && allDef.race.Humanlike)
                {
                    allDef.comps.Add(new CompProperties_StrangerDanger());
                }
            }
        }
    }

    public class CompProperties_StrangerDanger : CompProperties
    {
        public CompProperties_StrangerDanger()
        {
            compClass = typeof(CompStrangerDanger);
        }
    }

    public class CompStrangerDanger : ThingComp
    {
        public bool FormerInnocent;
        public bool FormerSlave;
        public bool FormerColonist;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref FormerInnocent, "SD_Innocent", false);
            Scribe_Values.Look(ref FormerSlave, "SD_Slave", false);
            Scribe_Values.Look(ref FormerColonist, "SD_Colonist", false);
        }
    }

    public class DisabledWorkClass
    {
        public List<WorkTypeDef> DisabledWorkTypes;
        public WorkTags DisabledWorkTags;
    }
}
