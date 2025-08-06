using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MaliExtinguishRefuelables
{
    //public static class BetterPyromania_PyroUtility
    //{
    //    public static readonly HashSet<ThingDef> containedFireSources;

    //    static BetterPyromania_PyroUtility()
    //    {
    //        containedFireSources = new HashSet<ThingDef>();
    //        foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
    //        {
    //            if (((item.CompDefForAssignableFrom<CompFireOverlayExtinguishableBase>() != null) || (item.CompDefForAssignableFrom<CompFireOverlayBase>() != null)) && item.CompDefForAssignableFrom<CompGlower>() != null)
    //            {
    //                containedFireSources.Add(item);
    //            }
    //        }
    //    }

    //    public static (bool, bool) IsFireAt(IntVec3 cell, Map map)
    //    {
    //        foreach (Thing thing in cell.GetThingList(map))
    //        {
    //            if (thing.IsBurning())
    //            {
    //                return (true, true);
    //            }
    //            if (containedFireSources.Contains(thing.def))
    //            {
    //                CompGlower compGlower = thing.TryGetComp<CompGlower>();
    //                if (compGlower != null && compGlower.Glows)
    //                {
    //                    return (true, false);
    //                }
    //            }
    //        }
    //        return (false, false);
    //    }
    //}
}