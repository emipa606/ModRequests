using RimWorld;
using UnityEngine;
using Verse;

namespace AdditionalJoyObjects
{

    public class JobDriver_Dartboard : JobDriver_WatchBuilding
    {
        // Interval at which darts will be thrown
        private static int dartThrowInterval = 400;
        private static float dartSpeed = 20f;

       
        protected override void WatchTickAction()
        {
            if (base.pawn.IsHashIntervalTick(dartThrowInterval))
            {
                ThrowDart(pawn, TargetA.Cell);
            }
            base.WatchTickAction();
        }

        // Process the dart
        public static void ThrowDart(Pawn thrower, IntVec3 target)
        {
            // If the pawn can't spawn motes, do nothing
            if (!thrower.Position.ShouldSpawnMotesAt(thrower.Map) || thrower.Map.moteCounter.SaturatedLowPriority) return;
            Vector3 preciseTarget = target.ToVector3Shifted();
            // Dart color based on player
            ThingDef dart = AjoDefOf.AJO_Mote_GreenDart;
            if (target.x == thrower.Position.x - 1 || target.z == thrower.Position.z - 1) dart = AjoDefOf.AJO_Mote_RedDart;
            else if (target.x == thrower.Position.x + 1 || target.z == thrower.Position.z + 1) dart = AjoDefOf.AJO_Mote_BlueDart;
           
            // Transforms for the dart
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(dart, null);
            moteThrown.Scale = 1f;
            moteThrown.rotationRate = 0f;
            moteThrown.exactPosition = thrower.DrawPos;
            moteThrown.exactRotation = (preciseTarget - moteThrown.exactPosition).AngleFlat();
            moteThrown.SetVelocity((preciseTarget - moteThrown.exactPosition).AngleFlat(), dartSpeed);
            moteThrown.MoveAngle = (preciseTarget - moteThrown.exactPosition).AngleFlat();
            moteThrown.airTimeLeft = (moteThrown.exactPosition - preciseTarget).MagnitudeHorizontal() / dartSpeed;

            // Throw the dart lol
            GenSpawn.Spawn(moteThrown, thrower.Position, thrower.Map);
        }
    }
    [DefOf]
    public static class AjoDefOf
    {   // Dart defs
        public static ThingDef AJO_Mote_GreenDart, AJO_Mote_RedDart, AJO_Mote_BlueDart;
        // Chandelier def
        public static ThingDef ChanPart_1x1_Bowl;
        static AjoDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AjoDefOf));
                }
        
    }
}
