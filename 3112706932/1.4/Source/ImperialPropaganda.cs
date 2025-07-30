using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ImperialFunctionality
{
    public class ImperialPropaganda : ThingWithComps
    {
        public static float offset = 0.05f;
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            var index = this.overrideGraphicIndex;
            this.overrideGraphicIndex = 0;
            var indices = new List<int> { 0, 2, 0, 2};
            Rand.PushState(this.thingIDNumber);
            for (var i = 0; i < 4; i++)
            {
                var pos = ItemCenterAt(i, Position, def.Altitude);
                this.overrideGraphicIndex = indices[i];
                base.DrawAt(pos, flip);
            }
            Rand.PopState();
            this.overrideGraphicIndex = index;
        }

        private static Vector3 ItemCenterAt(int num, IntVec3 c, float altitude)
        {
            Vector3 vector3 = c.ToVector3Shifted();
            return new Vector3(vector3.x + ((float)num * offset), altitude + num, vector3.z + ((float)num * offset));
        }
    }
}
