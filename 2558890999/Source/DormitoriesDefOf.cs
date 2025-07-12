using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Dormitories
{
    /// <summary>
    /// All my Defs
    /// </summary>
    [DefOf]
    public static class DormitoriesDefOf
    {
        public static RoomRoleDef PrisonDormitory;
        public static RoomRoleDef Dormitory;
        public static ThoughtDef SleptInDormitory;

        static DormitoriesDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DormitoriesDefOf));
        }
    }
}
