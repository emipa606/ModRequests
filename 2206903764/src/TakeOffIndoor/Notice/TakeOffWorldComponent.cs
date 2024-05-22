using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;


namespace TakeOffIndoor
{
    public class TakeOffWorldComponent : RimWorld.Planet.WorldComponent
    {
        public int verdate = 0; //セーブデータのバージョン
        public static int nowVerdate = 20221205; //TakeOffSettingsも修正
        public TakeOffWorldComponent(RimWorld.Planet.World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Values.Look(ref verdate, "TakeOffCoatVer", 0);

                //debug.WriteLine("WCLoad "+verdate.ToString());
                //if (verdate < nowVerdate)
                //{
                //    CompatibilityPricessing();
                //}
            }
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Scribe_Values.Look(ref nowVerdate, "TakeOffCoatVer");
            }
            base.ExposeData();
        }

        //public void CompatibilityPricessing()
        //{

        //}
        public override void FinalizeInit()
        {
            base.FinalizeInit();

            if (TakeOffSettings.showLatestUpdates)
            {
                if(verdate > TakeOffSettings.latestSaveVerdate)
                {
                    TakeOffSettings.latestSaveVerdate = verdate;
                }

                if(nowVerdate> TakeOffSettings.latestSaveVerdate)
                {
                    UpdateNotice.Notice(nowVerdate, TakeOffSettings.latestSaveVerdate);

                    TakeOffSettings.latestSaveVerdate = nowVerdate;

                    //Util.mod.WriteSettings(); //ここで処理すると何故か拠点が描画されないエラーが出て致命的バグになる

                    Util.TempSaveInt("toc_lsv", TakeOffSettings.latestSaveVerdate);
                }

            }

        }
    }
}
