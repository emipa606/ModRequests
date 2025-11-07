using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class ProjectileGuidedPenetratingBulletCE : PenetratingBulletCE
    {
        public float Distance
        {
            get
            {
                return distance;
            }
        }

        public Vector3 Origin
        {
            get { return launcherPos; }
        }
        private Vector3 Direction
        {
            get
            {
                dicrectionInt.x = TargetPos.x - launcherPos.x;
                dicrectionInt.y = TargetPos.y - launcherPos.y;

                return dicrectionInt.normalized;
            }
        }

        public Vector3 TargetPos
        {
            get
            {
                targetPosInt.x = intendedTargetThing.DrawPos.x;
                targetPosInt.y = intendedTargetThing.DrawPos.z;
                return targetPosInt;
            }
        }

        private Vector3 GuidedDestination
        {
            get
            {
                return launcherPos + (Direction * distance);
            }
        }

        private float distance;
        private Vector3 targetPosInt;
        private Vector3 dicrectionInt;
        private Vector3 launcherPos;
        public ProjectileGuidedPenetratingBulletCE()
        {
            targetPosInt = new Vector3();
            dicrectionInt = new Vector3();
            this.launcherPos = new Vector3();
        }

        public override void Launch(Thing launcher, Vector2 origin, float shotAngle, float shotRotation, float shotHeight = 0, float shotSpeed = -1, Thing equipment = null, float distance = -1)
        {
            this.launcherPos.x = origin.x;
            this.launcherPos.y = origin.y;
            base.Launch(launcher, origin, shotAngle, shotRotation, shotHeight, shotSpeed);
            double x = this.Destination.x - origin.x;
            double y = this.Destination.y - origin.y;
            distance = (float)Math.Sqrt(x * x + y * y);
        }

        public override void Tick()
        {
            //Log.Message("Dest:" + destinationInt + ", Guided:" + GuidedDestination + ", Origin:" + origin + ", Dis:" + distance + ", Direct:" + Direction);
            if (intendedTargetThing != null)
            {
                this.Destination.x = GuidedDestination.x;
                this.Destination.y = GuidedDestination.y;
            }
            base.Tick();
        }
    }
}
