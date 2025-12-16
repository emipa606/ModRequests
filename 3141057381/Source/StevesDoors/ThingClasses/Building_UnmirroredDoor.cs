using RimWorld;
using Verse;
using UnityEngine;

namespace StevesDoors
{
    [StaticConstructorOnStartup]
    public class Building_UnmirroredDoor : Building_Door
    {
        public new float OpenPct => Mathf.Clamp01((float)ticksSinceOpen / (float)TicksToOpenNow);

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            Comps_PostDraw();
        }
    }
}
