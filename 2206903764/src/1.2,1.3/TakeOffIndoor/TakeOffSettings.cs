using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace TakeOffIndoor
{

    public enum ResolveGraphicsMode
    {
        Test = -1,
        Normal = 0,
        AlwaysAll = 1,
        AlwaysApparel = 2,
        Legacy = 3,
        Legacy2 = 4,
        Legacy3 = 5
    }
    public class TakeOffSettings : ModSettings
    {
        public static TakeOffSettings Default = new TakeOffSettings();

        public static bool useTakeOff = true;
        public static bool dontUsePersonalSettings = false;
        public static bool dontShowDuplicateWaring = false;

        public static ulong settingsBK;
        public static bool settingToggle = false;
        public static bool usePrivateRoom = true;
        public static int roomCheckInterval = 60;

        public static TakeOffData data = new TakeOffData();

        public static bool useRemoveAnimal = true;
        public static bool useRemoveMech = true;

        public static int verdate = 0;
        public static int nowVerdate = 202101028; //TakeOffWorldComponentも修正
        public static int latestSaveVerdate = 0;

        public static bool dontApplyPortrait = false;
        public static bool hatsOnlyOnMap = false;

        public static int updateTicks = 5;

        public static bool patchCanDrawAddon = false;
        public static bool showLatestUpdates = true;

        public static ResolveGraphicsMode graphicsMode = ResolveGraphicsMode.Normal;

        public static bool portraitOption = false;


        public static bool useInvisible = false;
        public static bool UseInvisible
        {
            get
            {
                return useInvisible;
            }
        }
        public static byte useInvisibleCount = 0;


        public static List<string> neverDefNameList;
        public static List<string> invisibleDefNameList;
        public static List<string> visibleDefNameList;
        public static List<string> forceDefNameList;
        public static Dictionary<string,VisibleMode> defNameList;
        public static bool LegacyMode
        {
            get
            {
                return graphicsMode == ResolveGraphicsMode.Legacy || graphicsMode == ResolveGraphicsMode.Legacy2;
            }
        }

        public static bool DontUsePersonalSettings
        {
            get
            {
                return !useTakeOff || dontUsePersonalSettings;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref useTakeOff, "useTakeOff",true);
            Scribe_Values.Look(ref dontUsePersonalSettings, "dontUsePersonalSettings", false);
            Scribe_Values.Look(ref dontApplyPortrait, "dontPortrait", false);

            Scribe_Values.Look(ref dontShowDuplicateWaring,"duplicateWarning",false);

            Scribe_Deep.Look(ref data, "TakeOffSettings",null);
            if (data == null)
            {
                data = new TakeOffData();
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                svl(ref verdate, "ver");
                if (verdate == 0)
                {
                    svl(ref data.useTakeOffDrafted, "useTakeOffDrafted");
                    svl(ref data.useTakeOffHatsIndoor, "useTakeOffHatsIndoor");
                    svl(ref data.hideHatsInBed, "hideHatsInBed");
                    svl(ref data.useTemp, "useTemp");
                    svl(ref data.useTempHotWhileIndoor, "HotIndoor");
                    svl(ref data.useTempHotWhileOutdoor, "HotOutdoor");
                    svl(ref data.useHotTempDefault, "useHotDefault");
                    svl(ref data.hotTemp, "hotTemp");
                    svl(ref data.useTempColdWhileIndoor, "ColdIndoor");
                    svl(ref data.useTempColdWhileOutdoor, "ColdOutdoor");
                    svl(ref data.useColdTempDefault, "useColdDefault");
                    svl(ref data.coldTemp, "coldTemp");
                    //svl(ref alwaysShowCoatInPortrait, "showPortrait");

                    svl(ref data.indoorLayer, "indoorLayer");
                    svl(ref data.privateRoomLayer, "privateRoomLayer");
                    svl(ref data.beltAsLayer, "beltAsLayer");
                    svl(ref data.hotLayer, "hotLayer");
                    svl(ref data.coldLayer, "coldLayer");

                    svl(ref data.useHatsAsLayer, "useHatsAs");
                    svl(ref data.hatsLayer, "hatsLayer");
                }
                if(verdate < 20210416)
                {
                    Scribe_Values.Look(ref hatsOnlyOnMap, "hatsPnlyOnMap", false);
                }
                else
                {
                    Scribe_Values.Look(ref hatsOnlyOnMap, "hatsOnlyOnMap", false);
                }
            }
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                verdate = nowVerdate;
                Scribe_Values.Look(ref verdate, "ver",0);
                Scribe_Values.Look(ref hatsOnlyOnMap, "hatsOnlyOnMap", false);
            }


            Scribe_Values.Look(ref usePrivateRoom, "usePrivateRoom", true);
            Scribe_Values.Look(ref roomCheckInterval, "roomCheckInterval", 60);
            //Scribe_Values.Look(ref useRemoveAnimal, "removeAnimal", true);
            //Scribe_Values.Look(ref useRemoveMech, "removeMech", true);
            Scribe_Values.Look(ref updateTicks, "updateTicks", 5);
            Scribe_Values.Look(ref dontApplyPortrait, "dontApplyPortrait", false);

            portraitOption = dontApplyPortrait || hatsOnlyOnMap;

            Scribe_Values.Look(ref graphicsMode, "resolveGraphicsMode");

            //debug.WriteLine(graphicsMode.ToString());


            Scribe_Values.Look(ref patchCanDrawAddon, "patchCanDrawAddon", false);


            Scribe_Values.Look(ref showLatestUpdates, "showLatestUpdates", true);



            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Values.Look(ref latestSaveVerdate, "latestSaveVerdate", 0);
                if (Util.TempLoadInt("toc_lsv", out int tmpint))
                {
                    if (latestSaveVerdate < tmpint)
                    {
                        latestSaveVerdate = tmpint;
                    }
                }
            }
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (Util.TempLoadInt("toc_lsv", out int tmpint, true))
                {
                    if (latestSaveVerdate < tmpint)
                    {
                        latestSaveVerdate = tmpint;
                    }
                }
                Scribe_Values.Look(ref latestSaveVerdate, "latestSaveVerdate", 0);
            }

            Scribe_Values.Look(ref useInvisible, "useInvisible", false);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                neverDefNameList = ApparelList.NeverDefNameList;
                invisibleDefNameList = ApparelList.InvisibleDefNameList;
                visibleDefNameList = ApparelList.VisibleDefNameList;
                forceDefNameList = ApparelList.ForceVisibleDefNameList;
            }
            Scribe_Collections.Look(ref neverDefNameList, "neverList", LookMode.Value, new List<string>());
            Scribe_Collections.Look(ref invisibleDefNameList, "invisibleList",LookMode.Value,new List<string>());
            Scribe_Collections.Look(ref visibleDefNameList, "visibleList", LookMode.Value, new List<string>());
            Scribe_Collections.Look(ref forceDefNameList, "forceList", LookMode.Value, new List<string>());
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                defNameList = new Dictionary<string, VisibleMode>();
                defNameList.AddRangeFromList(neverDefNameList, VisibleMode.Never);
                defNameList.AddRangeFromList(invisibleDefNameList, VisibleMode.Invisible);
                defNameList.AddRangeFromList(visibleDefNameList, VisibleMode.Visible);
                defNameList.AddRangeFromList(forceDefNameList, VisibleMode.Force);
            }

            base.ExposeData();
        }



        private static void svl<T>(ref T value, string label) //Scribeの初期値にクラスの初期値を使用
        {
            Scribe_Values.Look(ref value, label, value);
        }

        public static ulong CalcSettingToULong()
        {
            ulong ret = (ulong)data.LongValue;
            int ind = TakeOffData.LongValueSize;
            ret += Util.bitOnLong(useTakeOff, ++ind); //32
            ret += Util.bitOnLong(dontApplyPortrait, ++ind);
            ret += Util.bitOnLong(hatsOnlyOnMap, ++ind); //34
            ret += Util.bitOnLong((ulong)graphicsMode, ind);ind += 3; //37
            ret += Util.bitOnLong(useInvisible, ++ind); //38
            ret += Util.bitOnLong((ulong)(useInvisibleCount%4) , ind); ind += 2; //40

            return ret;
        }

        public static void SetToDefault()
        {
            useTakeOff = true;
            dontUsePersonalSettings = false;
            dontShowDuplicateWaring = false;
            data.SetToDefault();

            settingToggle = false;
            usePrivateRoom = true;
            roomCheckInterval = 60;

            dontApplyPortrait = false;
            hatsOnlyOnMap = false;

            patchCanDrawAddon = false;
            showLatestUpdates = true;

            graphicsMode = ResolveGraphicsMode.Normal;

            portraitOption = false;
            useInvisible = false;
    }
        public static bool ResolveAlwaysAll
        {
            get
            {
                return graphicsMode == ResolveGraphicsMode.AlwaysAll;
            }
        }

        public static bool ResolveAlwaysApparel
        {
            get
            {
                return graphicsMode == ResolveGraphicsMode.AlwaysApparel;
            }
        }
        public static bool ResolveNormal
        {
            get
            {
                return graphicsMode == ResolveGraphicsMode.Normal;
            }
        }
        public static bool ResolveLegacy
        {
            get
            {
                return graphicsMode == ResolveGraphicsMode.Legacy;
            }
        }
        public static bool ResolveLegacy2
        {
            get
            {
                return graphicsMode == ResolveGraphicsMode.Legacy2;
            }
        }
        public static bool ResolveLegacy3
        {
            get
            {
                return graphicsMode == ResolveGraphicsMode.Legacy3;
            }
        }

        public static bool ResolveTest
        {
            get
            {
                return graphicsMode == ResolveGraphicsMode.Test;
            }
        }
    }
}
