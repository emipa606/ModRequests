using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Unity;


namespace HLVR
{
    class CompCombineThumper : ThingComp
    {
        private static int ThumpInterval = 180;
        private int ThumpCounter = 0;

        public override void CompTick()
        {
            base.CompTick();
            if(ThumpCounter>=ThumpInterval)
            {
                GenExplosion.DoExplosion(this.parent.Position, this.parent.Map, 5.0f ,DamageDefOf.Smoke, this.parent, -1, -1, SoundDefOf.Bombardment_PreImpact, null, null, null, null, 0f, 1,GasType.Unused, false, null, 0f, 1, 0f, false, null, null);
                ThumpCounter = 0;
            }
            ThumpCounter++;

        }
    }
}
