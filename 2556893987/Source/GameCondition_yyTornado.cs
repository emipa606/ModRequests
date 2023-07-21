using UnityEngine;
using Verse;
using RimWorld;
using System;
using System.Collections.Generic;


namespace yayoPlanet
{

    public class GameCondition_yyTornado : GameCondition
    {
        public override void Init()
        {
            base.Init();
            Messages.Message("nt_tornado".Translate(), MessageTypeDefOf.NeutralEvent);
        }

        public override void End()
        {
            base.End();
            util.threatMultiply = 1f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.nextTick, "nextTick", 0, false);
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
                return Mathf.Clamp((float)Mathf.Max(active_tick - GenDate.TicksPerDay, 0) / (float)GenDate.TicksPerQuadrum, 0f, 1f);
            }
        }


        int nextTick = 0;

        public override void GameConditionTick()
        {
            util.threatMultiply = 1f - (active_factor * modBase.val_threrat);

            List<Map> affectedMaps = base.AffectedMaps;

            if (active_tick >= 0 && Find.TickManager.TicksAbs >= nextTick && active_tick <= GenDate.TicksPerQuadrum - GenDate.TicksPerDay)
            {
                nextTick = Find.TickManager.TicksAbs + (int)(500 + (GenDate.TicksPerDay * 0.5f * (1f - active_factor)));

                foreach (Map map in affectedMaps)
                {
                    IntVec3 c = CellFinder.RandomEdgeCell(map);
                    Spawn(c, map);

                }
                

            }

            if(active_tick >= 0)
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
                    //sky.OverlayColor = new Color(1f, 1f, 1f, 0.6f);
                    sky.OverlayColor = new Color(0.7f, 0.3f, 0.2f, 0.7f * active_factor);
                    ar.Add(sky);

                }
                return ar;
            }

        }


        // 하늘 색깔
        SkyColorSet SkyColors = new SkyColorSet(
                new Color(0.7f, 0.4f, 0.25f), //sky
                new Color(0.6f, 0.25f, 0.15f), // shadow
                new Color(0.7f, 0.4f, 0.25f), 1f); // overay
        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget?(new SkyTarget(1f, SkyColors, 1f, 1f));
        }
        
        // 하늘 색깔
        public override float SkyTargetLerpFactor(Map map)
        {
            return Mathf.Clamp(active_factor * 1.5f, 0f, 1f);
        }

        

        private bool Spawn(IntVec3 loc, Map map)
        {
            IntVec3 intVec;
            if (!this.TryFindCell(out intVec, map))
            {
                //Log.Message("can not find tornado spawn position");
                return false;
            }

            //Log.Message("tornado spawned");

            GenSpawn.Spawn(ThingDef.Named("yy_Tornado"), intVec, map, 0);
            return true;
        }



        private bool TryFindCell(out IntVec3 cell, Map map)
        {
            int maxMineables = ThingSetMaker_Meteorite.MineablesCountRange.max;
            return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 10, default(IntVec3), -1, false, false, false, false, true, true, delegate (IntVec3 x)
            {
                int num = Mathf.CeilToInt(Mathf.Sqrt((float)maxMineables)) + 2;
                CellRect cellRect = CellRect.CenteredOn(x, num, num);
                int num2 = 0;
                foreach (IntVec3 c in cellRect)
                {
                    if (c.InBounds(map) && c.Standable(map))
                    {
                        num2++;
                    }
                }
                return num2 >= maxMineables;
            });
        }



    }


}
 