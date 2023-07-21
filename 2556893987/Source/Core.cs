using UnityEngine;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;
using System.Collections.Generic;


namespace yayoPlanet
{

    public class modBase : ModBase
    {
        public override string ModIdentifier => "yayoPlanet";

        public static int tickOfYear = 0;

        private SettingHandle<int> eventStart_s;
        public static int eventStart = 1;

        private SettingHandle<int> eventCycle_s;
        public static int eventCycle = 2;



        private SettingHandle<bool> bl_randomType_s;
        public static bool bl_randomType = false;

        private SettingHandle<bool> bl_yyCold_s;
        public static bool bl_yyCold = true;

        private SettingHandle<float> val_yyCold_s;
        public static float val_yyCold = 0f;




        private SettingHandle<bool> bl_yyHot_s;
        public static bool bl_yyHot = true;

        private SettingHandle<float> val_yyHot_s;
        public static float val_yyHot = 0f;





        private SettingHandle<bool> bl_yyTornado_s;
        public static bool bl_yyTornado = true;

        private SettingHandle<float> val_yyTornado_s;
        public static float val_yyTornado = 1f;




        private SettingHandle<float> val_threat_s;
        public static float val_threrat = 0.5f;



        // -----------------------------------------


        static public List<GameConditionDef> ar_gc;
        GameConditionDef gc_cold => GameConditionDef.Named("yyCold");
        GameConditionDef gc_hot => GameConditionDef.Named("yyHot");
        GameConditionDef gc_Tornado => GameConditionDef.Named("yyTornado");


        // -----------------------------------------

        public override void DefsLoaded()
        {
            eventStart_s = Settings.GetHandle<int>("eventStart", "eventStart_t".Translate(), "eventStart_d".Translate(), 1);

            eventCycle_s = Settings.GetHandle<int>("eventCycle", "eventCycle_t".Translate(), "eventCycle_d".Translate(), 2);


            bl_randomType_s = Settings.GetHandle<bool>("bl_randomType", "bl_randomType_t".Translate(), "bl_randomType_d".Translate(), false);



            bl_yyCold_s = Settings.GetHandle<bool>("bl_yyCold", "bl_yyCold_t".Translate(), "bl_yyCold_d".Translate(), true);

            val_yyCold_s = Settings.GetHandle<float>("val_yyCold", "val_yyCold_t".Translate(), "val_yyCold_d".Translate(), -270f);


            bl_yyHot_s = Settings.GetHandle<bool>("bl_yyHot", "bl_yyHot_t".Translate(), "bl_yyHot_d".Translate(), true);

            val_yyHot_s = Settings.GetHandle<float>("val_yyHot", "val_yyHot_t".Translate(), "val_yyHot_d".Translate(), 200f);


            bl_yyTornado_s = Settings.GetHandle<bool>("bl_yyTornado", "bl_yyTornado_t".Translate(), "bl_yyTornado_d".Translate(), true);

            val_yyTornado_s = Settings.GetHandle<float>("val_yyTornado", "val_yyTornado_t".Translate(), "val_yyTornado_d".Translate(), 1f);


            val_threat_s = Settings.GetHandle<float>("val_threrat", "val_threrat_t".Translate(), "val_threrat_d".Translate(), 0.5f);

            SettingsChanged();
        }

        public override void SettingsChanged()
        {
            ar_gc = new List<GameConditionDef>();

            eventStart = eventStart_s.Value;
            eventCycle = Mathf.Clamp(eventCycle_s.Value, 1, 10);


            bl_randomType = bl_randomType_s.Value;


            bl_yyCold = bl_yyCold_s.Value;
            if (bl_yyCold) ar_gc.Add(gc_cold);

            val_yyCold_s.Value = Mathf.Clamp(val_yyCold_s.Value, -500f, -1f);
            val_yyCold = val_yyCold_s.Value;



            bl_yyHot = bl_yyHot_s.Value;
            if (bl_yyHot) ar_gc.Add(gc_hot);

            val_yyHot_s.Value = Mathf.Clamp(val_yyHot_s.Value, 1f, 1500f);
            val_yyHot = val_yyHot_s.Value;




            bl_yyTornado = bl_yyTornado_s.Value;
            if (bl_yyTornado) ar_gc.Add(gc_Tornado);

            val_yyTornado_s.Value = Mathf.Clamp(val_yyTornado_s.Value, 0.2f, 3f);
            val_yyTornado = val_yyTornado_s.Value;




            val_threat_s.Value = Mathf.Clamp(val_threat_s.Value, 0f, 0.9f);
            val_threrat = val_threat_s.Value;

        }



        


        /*
        public override void MapLoaded(Map map)
        {
            base.MapLoaded(map);

            

            if (!util.isYayoGC(map))
            {
                util.setRandomYayoGc(map);
            }
            

        }
        */


    }



    public class Core : MapComponent
    {
        //public int Today { get; private set; }
        int notice_tick = GenDate.TicksPerQuadrum;

        public override void MapComponentTick()
        {
            // 매 틱마다
            base.MapComponentTick();
            modBase.tickOfYear = Find.TickManager.TicksAbs % GenDate.TicksPerYear;
            

            //Log.Message("A " + Find.TickManager.TicksGame.ToString());
            /*
            if (Find.TickManager.TicksGame == 50 && modBase.tickOfYear > notice_tick)
            {
                if (!util.isYayoGC(map))
                {
                    util.setRandomYayoGc(map);
                }
            }
            */

            // 이벤트 종료
            if (modBase.tickOfYear == 0)
            {
                for(int i = map.gameConditionManager.ActiveConditions.Count -1; i >= 0; i--)
                {
                    map.gameConditionManager.ActiveConditions[i].End();
                }

            }

            // 이벤트 예고
            if (modBase.tickOfYear == notice_tick)
            {
                int passedYearAbs = Find.TickManager.TicksAbs / GenDate.TicksPerYear;
                if (!util.isYayoGC(map)
                    && passedYearAbs >= modBase.eventStart
                    && (passedYearAbs - modBase.eventStart) % modBase.eventCycle == 0)
                {
                    util.setRandomYayoGc(map);
                }

            }

            



        }

        public Core(Map map) : base(map)
        {
            /*
            if (!util.isYayoGC(map))
            {
                util.setRandomYayoGc(map);
            }
            */
        }
    }



    static public class util
    {
        static public float threatMultiply = 1f;

        static public bool isYayoGC(Map map)
        {
            bool result = false;
            /*
            foreach(GameConditionDef gc in modBase.ar_gc)
            {
                if (map.gameConditionManager.GetActiveCondition(gc) != null)
                {
                    result = true;
                }
            }
            */
            foreach (GameCondition gc in map.gameConditionManager.ActiveConditions)
            {
                if (gc.def.defName.Contains("yy"))
                {
                    result = true;
                }
            }
            return result;
        }

        static public GameConditionDef getRandomGcDef()
        {
            return modBase.ar_gc[Rand.Range(0, modBase.ar_gc.Count)];
        }


        static public void setRandomYayoGc(Map map)
        {
            if (modBase.bl_randomType)
            {
                GameCondition gc = GameConditionMaker.MakeConditionPermanent(getRandomGcDef());
                map.gameConditionManager.RegisterCondition(gc);
            }
            else
            {
                int passedYearAbs = Find.TickManager.TicksAbs / GenDate.TicksPerYear;
                GameCondition gc = GameConditionMaker.MakeConditionPermanent(modBase.ar_gc[(passedYearAbs - modBase.eventStart) / modBase.eventCycle % modBase.ar_gc.Count]);
                map.gameConditionManager.RegisterCondition(gc);
            }
            
        }



    }


    [StaticConstructorOnStartup]
    static public class util2
    {
        public static readonly Material TornadoMaterial = MaterialPool.MatFrom("Things/Ethereal/Tornado", ShaderDatabase.Transparent, MapMaterialRenderQueues.Tornado);
    }




}