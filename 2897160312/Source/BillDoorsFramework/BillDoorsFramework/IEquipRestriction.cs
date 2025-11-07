using System.Linq;
using Verse;

namespace BillDoorsFramework
{
    public interface IEquipRestriction
    {
        bool CannotEquip(Pawn pawn, out string reason);
    }

    public static class EquipRestrictionUtil
    {
        public static bool CannotEquip(Pawn p, Thing thing, out string reason)
        {
            if (thing is IEquipRestriction ie)
            {
                if (ie.CannotEquip(p, out reason)) return true;
            }
            if (thing.def.modExtensions != null)
            {
                foreach (var ext in thing.def.modExtensions)
                {
                    if (ext is IEquipRestriction iee)
                    {
                        if (iee.CannotEquip(p, out reason)) return true;
                    }
                }
            }
            if (thing is ThingWithComps twc)
            {
                foreach (var c in twc.AllComps)
                {
                    if (c is IEquipRestriction iec)
                    {
                        if (iec.CannotEquip(p, out reason)) return true;
                    }
                }
            }
            reason = null;
            return false;
        }
    }
}
