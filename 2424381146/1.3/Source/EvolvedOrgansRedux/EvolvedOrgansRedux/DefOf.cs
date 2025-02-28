using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolvedOrgansRedux {
    [RimWorld.DefOf]
    public static class DefOf {
        public static Verse.BodyPartDef Heart;
        public static Verse.BodyPartDef Lung;
        public static Verse.BodyPartDef Shoulder;
        public static Verse.BodyPartDef LowerShoulder;
        public static Verse.BodyPartDef Back;
        public static Verse.BodyPartDef BodyCavity1; //Left
        public static Verse.BodyPartDef BodyCavity2; //Right
        public static Verse.BodyPartDef BodyCavityA; //Abdominal
        static DefOf() {
            RimWorld.DefOfHelper.EnsureInitializedInCtor(typeof(RimWorld.BodyPartDefOf));
        }
    }
}
