using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace yayoPlanet
{

    public class GameCondition_yyHot : GameCondition
    {
        public override void Init()
        {
            base.Init();
            Messages.Message("nt_hot".Translate(), MessageTypeDefOf.NeutralEvent);
        }

        public override void End()
        {
            base.End();
            util.threatMultiply = 1f;
        }

        public override void GameConditionTick()
        {
            base.GameConditionTick();
            util.threatMultiply = 1f - (active_factor * modBase.val_threrat);
        }

        public override int TransitionTicks
        {
            get
            {
                return 6000;
            }
        }


        int active_tick
        {
            get
            {
                return (Find.TickManager.TicksAbs % GenDate.TicksPerYear) - (GenDate.TicksPerQuadrum * 3);
            }
        }
        float active_factor
        {
            get
            {
                return Mathf.Clamp((float)Mathf.Max(active_tick, 0) / (float)GenDate.TicksPerQuadrum, 0f, 1f);
            }
        }

        // 오프셋 기온
        public override float TemperatureOffset()
        {

            //Log.Message(active_factor.ToString());
            return active_factor * modBase.val_yyHot;

        }

        // 하늘 색깔
        public override SkyTarget? SkyTarget(Map map)
        {
            SkyColorSet SkyColors = new SkyColorSet(
                new Color(0.8f, 0.25f, 0.25f), //sky
                new Color(0.95f, 0.15f, 0.15f), // shadow
                new Color(0.85f, 0.25f, 0.25f), 1f); // overay
            return new SkyTarget(1f, SkyColors, 1f, 1f);
        }
        
        // 하늘 색깔
        public override float SkyTargetLerpFactor(Map map)
        {
            return active_factor;
        }




        public override float AnimalDensityFactor(Map map)
        {

            return Mathf.Clamp01(1f - (Mathf.Abs(map.mapTemperature.OutdoorTemp) / 65f));
        }


        // 셀 데미지
        /*
        public override void DoCellSteadyEffects(IntVec3 cell, Map map)
        {
            
            if (cell.GetRoof(map) == null && map.mapTemperature.OutdoorTemp > 120f)
            {
                List<Thing> ar_thing = cell.GetThingList(map);
                for (int i = 0; i < ar_thing.Count; i++) 
                {
                    Thing thing = ar_thing[i];
                    switch (thing.def.category)
                    {
                        case ThingCategory.Item:
                            thing.TakeDamage(new DamageInfo(DamageDefOf.Rotting, active_factor * 2.5f, 0, -1, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
                            break;
                        case ThingCategory.Pawn:
                            float temp = cell.GetTemperature(map);
                            Pawn p = thing as Pawn;
                            if (p != null)
                            {

                                if (Rand.Value < Mathf.Clamp01(-(temp - p.SafeTemperatureRange().max) / 100f))
                                {
                                    if (Rand.Value < 0.25f) thing.TakeDamage(new DamageInfo(DamageDefOf.Burn, 20f, 0, -1, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
                                }

                            }
                            break;

                    }

                }
            }
        }
        */















    }


}