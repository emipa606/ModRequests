using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DrugScheduleWhileWorking;

[StaticConstructorOnStartup]
public class DrugScheduleUtils
{
    public static readonly IEnumerable<ThingDef> AllDrugs = DefDatabase<ThingDef>.AllDefs.Where(thingDef => thingDef.IsDrug);
}