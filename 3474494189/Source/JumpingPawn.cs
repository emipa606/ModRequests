using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using VFECore.Abilities;

namespace AddieSolarHunter
{
    public class JumpingPawn : AbilityPawnFlyer
    {
        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            FlyingPawn.Drawer.renderer.DynamicDrawPhaseAt(phase, drawLoc, Rotation, true);
        }

        public override void Tick()
        {
            base.Tick();
            if (Map != null && Find.TickManager.TicksGame % 3 == 0)
            {
                var map = Map;
                var data = FleckMaker.GetDataStatic(GetDrawPos(), map, VPESH_DefOf.AirPuff);
                data.rotation = Rand.Range(0f, 360f);
                map.flecks.CreateFleck(data);
            }
        }

        private Vector3 GetDrawPos()
        {
            var x = ticksFlying / (float)ticksFlightTime;
            var drawPos = DrawPos;
            drawPos.y = AltitudeLayer.Skyfaller.AltitudeFor();
            return drawPos + Vector3.forward * (x - Mathf.Pow(x, 2)) * 15f;
        }

        protected override void RespawnPawn()
        {
            var flyingPawn = FlyingPawn;
            base.RespawnPawn();
            VPESH_DefOf.JumpPackLand.PlayOneShot(flyingPawn);
            FleckMaker.ThrowSmoke(flyingPawn.DrawPos, flyingPawn.Map, 1f);
            FleckMaker.ThrowDustPuffThick(flyingPawn.DrawPos, flyingPawn.Map, 2f, new Color(1f, 1f, 1f, 2.5f));
        }
    }
}