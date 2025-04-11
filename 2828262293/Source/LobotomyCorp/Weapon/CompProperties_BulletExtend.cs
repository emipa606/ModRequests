using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LobotomyCorp.Weapon
{
    public class CompProperties_BulletExtend : CompProperties
    {
        public int radius = 0;
        private AOEType aoe;

        public bool healingColonist = false;
        public float healValue = 3;

        public CompProperties_BulletExtend()
        {
            this.compClass = typeof(CompBulletExtend);
        }

        public List<IntVec3> AffectCellsOf(IntVec3 pos)
        {
            if (radius == 0) return new List<IntVec3>() { pos };

            if (aoe == null) 
                aoe = new AOEType() { 
                    multi =Multi.Circle, radius = this.radius 
                };

            return aoe.AffectCellsOf(pos);
        }
    }


    public class CompBulletExtend : ThingComp
    {

    }

}
