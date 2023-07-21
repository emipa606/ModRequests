using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace yayoPlanet
{

    public class GameCondition_yyCold : GameCondition
    {

        public override void Init()
        {
            base.Init();
            Messages.Message("nt_cold".Translate(), MessageTypeDefOf.NeutralEvent);
        }

        public override void End()
        {
            base.End();
            util.threatMultiply = 1f;
        }

        public override void GameConditionTick()
        {
            base.GameConditionTick();

            List<Map> affectedMaps = base.AffectedMaps;
            util.threatMultiply = 1f - (active_factor * modBase.val_threrat);

            if (active_tick >= 0)
            {
                for (int j = 0; j < this.overlays.Count; j++)
                {
                    for (int k = 0; k < affectedMaps.Count; k++)
                    {
                        this.overlays[j].TickOverlay(affectedMaps[k]);
                    }

                }
            }


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
            return active_factor * modBase.val_yyCold;

        }

        // 하늘 색깔
        public override SkyTarget? SkyTarget(Map map)
        {
            /*SkyColorSet SkyColors = new SkyColorSet(
                new Color(0.65f - active_factor * 0.5f, 0.65f - active_factor * 0.5f, 0.85f + active_factor * 0.15f), //sky
                new Color(0.75f - active_factor * 0.6f, 0.75f - active_factor * 0.6f, 0.95f), // shadow
                new Color(0.75f - active_factor * 0.4f, 0.75f - active_factor * 0.4f, 0.9f + active_factor * 0.1f), 1f); // overay
                */
            SkyColorSet SkyColors = new SkyColorSet(
                new Color(0.2f, 0.2f, 0.85f), //sky
                new Color(0.1f, 0.1f, 0.95f), // shadow
                new Color(0.15f, 0.15f, 0.9f), 1f); // overay
            return new SkyTarget(0.75f, SkyColors, 1f, 1f);
        }
        
        // 하늘 색깔
        public override float SkyTargetLerpFactor(Map map)
        {
            return active_factor;
        }











        public override float AnimalDensityFactor(Map map)
        {

            return Mathf.Clamp01(1f - (Mathf.Abs(map.mapTemperature.OutdoorTemp) / 75f));
        }


        // 셀 데미지
        public override void DoCellSteadyEffects(IntVec3 cell, Map map)
        {
            float temp = cell.GetTemperature(map);
            if (cell.GetRoof(map) == null && temp < -130f)
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
                            Pawn p = thing as Pawn;
                            if(p != null)
                            {
                                if (Rand.Value < Mathf.Clamp01(-(temp - p.SafeTemperatureRange().min) / 100f))
                                {
                                    float dmg = 0f;
                                    if (p.def.race.FleshType == FleshTypeDefOf.Normal || p.def.race.FleshType == FleshTypeDefOf.Insectoid)
                                    {
                                        dmg = 15f;
                                    }else if(p.def.race.FleshType == FleshTypeDefOf.Mechanoid)
                                    {
                                        dmg = 1f;
                                    }
                                    if (Rand.Value < 0.25f) thing.TakeDamage(new DamageInfo(DamageDefOf.Frostbite, dmg, 0, -1, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
                                }
                                

                                
                                
                            }
                            
                            break;

                    }

                }
            }
        }




        public override void GameConditionDraw(Map map)
        {
            for (int i = 0; i < this.overlays.Count; i++)
            {
                this.overlays[i].DrawOverlay(map);
            }
        }

        public override List<SkyOverlay> SkyOverlays(Map map)
        {
            return this.overlays;

        }

        private List<SkyOverlay> overlays
        {
            get
            {
                List<SkyOverlay> ar = new List<SkyOverlay>();
                if (active_tick > 0)
                {

                    SkyOverlay sky = new WeatherOverlay_fastWind();
                    sky.OverlayColor = new Color(0.6f, 0.6f, 0.9f, 0.25f); // 바람 색깔
                    ar.Add(sky);

                }
                return ar;
            }

        }



    }


}