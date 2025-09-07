using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Verse;
using RimWorld;
using UnityEngine;
using PipeSystem;

namespace valkyrim.livingwalls
{
    class Livingwallbase : Building
    {
       public override void Tick()
        { 
            
            var wallbase =  GetComp<complivingwalltilebase>();
            
            if (wallbase.livingwallfueled == true)
            {
                HealLivingWall(wallbase.Tier);
            }
        }
      public int healtick = 0;
        public void HealLivingWall(float tier)
        {
         
            if (HitPoints < MaxHitPoints)
            {
                if (healtick < 250)
                {
                    healtick = healtick + 1 ;
                }
                if (healtick >= 250)
                {
                    healtick = 0;
                    HitPoints += (int)Math.Ceiling(MaxHitPoints * 0.05 * tier);
                    if (HitPoints > MaxHitPoints)
                        HitPoints = MaxHitPoints;
                }
            }
        }
    }
    public class complivingwalltilebase : ThingComp

    {
        public CompPropertieslivingwalltilebase Props => (CompPropertieslivingwalltilebase)props;

        public float Tier => Props.tier;
        public bool livingwallfueled = true;
        public override void Initialize(CompProperties props)
        {
            //Log.Message("compexists");
            base.Initialize(props);
        }
    }
    public class CompPropertieslivingwalltilebase : CompProperties
    {
        public float tier;
        
        public CompPropertieslivingwalltilebase()
        {
            compClass = typeof(complivingwalltilebase);
        }
        public CompPropertieslivingwalltilebase(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }

    }

}


