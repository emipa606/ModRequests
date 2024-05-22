using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlienRace;
using Verse;

namespace TakeOffIndoor
{
    public class HARWrapper
    {
        public static void GetHARNames(out List<string> defNameList, out List<string> labelList)
        {

            defNameList = new List<string>();
            labelList = new List<string>();
            foreach (ThingDef_AlienRace race in DefDatabase<ThingDef_AlienRace>.AllDefsListForReading)
            {
                defNameList.Add(race.defName);
                labelList.Add(race.label);
            }
        }


        public static ThingDef GetByName(string defName)
        {
            return DefDatabase<ThingDef_AlienRace>.GetNamed(defName, false);

        }
    }
}
