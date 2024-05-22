using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TakeOffIndoor
{
    public class TakeOffData : IExposable
    {
        public static TakeOffData Default = new TakeOffData();

        public LayerPriority indoorLayer = LayerPriority.Middle;
        public LayerPriority privateRoomLayer = LayerPriority.OnSkin;
        public bool applyBarracks = false;
        public LayerPriority sleepingLayer = LayerPriority.OnSkin;
        public bool onlyUseInBed = true;
        public LayerPriority beltAsLayer = LayerPriority.Shell;
        public LayerPriority hotLayer = LayerPriority.Middle;
        public LayerPriority coldLayer = LayerPriority.Shell;
        public bool useTakeOffDrafted = false;
        public bool useTakeOffHatsIndoor = false;
        public bool hideHatsInBed = true;
        public bool useTemp = false;
        public bool useTempHotWhileIndoor = true;
        public bool useTempColdWhileIndoor = true;
        public bool useTempHotWhileOutdoor = false;
        public bool useTempColdWhileOutdoor = false;
        public bool useHotTempDefault = true;
        public bool useColdTempDefault = true;
        public int hotTemp = 99;
        public int coldTemp = -99;
        public static int hotTempDefault = 26;
        public static int coldTempDefault = 16;
        public bool useHatsAsLayer = true;
        public LayerPriority hatsLayer = LayerPriority.Shell;
        public bool dontBeNaked = true;

        public bool init = false;


        public void Initialize()
        {
            useTakeOffDrafted = Default.useTakeOffDrafted;
            hideHatsInBed = Default.hideHatsInBed;
            useTemp = Default.useTemp;
            indoorLayer = Default.indoorLayer;
            privateRoomLayer = Default.privateRoomLayer;
            sleepingLayer = Default.sleepingLayer;
            beltAsLayer = Default.beltAsLayer;
            hotLayer = Default.hotLayer;
            coldLayer = Default.coldLayer;
            hatsLayer = Default.hatsLayer;
        }

        public bool Equals(TakeOffData B)
        {
            if (ReferenceEquals(B, null))
            {
                return false;
            }

            if (this.indoorLayer!=B.indoorLayer) return false;
            if(this.privateRoomLayer!=B.privateRoomLayer) return false;
            if (this.applyBarracks != B.applyBarracks) return false;
            if (this.sleepingLayer != B.sleepingLayer) return false;
            if (this.onlyUseInBed != B.onlyUseInBed) return false;
            if (this.beltAsLayer!=B.beltAsLayer) return false;
            if(this.hotLayer!=B.hotLayer) return false;
            if(this.coldLayer!=B.coldLayer) return false;
            if(this.useTakeOffDrafted!=B.useTakeOffDrafted) return false;
            if(this.useTakeOffHatsIndoor!=B.useTakeOffHatsIndoor) return false;
            if(this.hideHatsInBed!=B.hideHatsInBed) return false;
            if(this.useTemp!=B.useTemp) return false;
            if(this.useTempHotWhileIndoor!=B.useTempHotWhileIndoor) return false;
            if(this.useTempColdWhileIndoor!=B.useTempColdWhileIndoor) return false;
            if(this.useTempHotWhileOutdoor!=B.useTempHotWhileOutdoor) return false;
            if(this.useTempColdWhileOutdoor!=B.useTempColdWhileOutdoor) return false;
            if(this.useHotTempDefault!=B.useHotTempDefault) return false;
            if(this.useColdTempDefault!=B.useColdTempDefault) return false;
            if(this.hotTemp!=B.hotTemp) return false;
            if(this.coldTemp!=B.coldTemp) return false;

            if(this.init!=B.init) return false;

            if(this.useHatsAsLayer!=B.useHatsAsLayer) return false;
            if(this.hatsLayer!=B.hatsLayer) return false;

            if (this.dontBeNaked != B.dontBeNaked) return false;
            
            return true;
        }

        public TakeOffData MakeClone()
        {
            return MakeCloneFrom(this);
        }
        public static TakeOffData MakeCloneFrom(TakeOffData src)
        {
            TakeOffData ret = new TakeOffData();

            ret.indoorLayer=src.indoorLayer;
            ret.privateRoomLayer=src.privateRoomLayer;
            ret.applyBarracks = src.applyBarracks;
            ret.sleepingLayer = src.sleepingLayer;
            ret.onlyUseInBed = src.onlyUseInBed;
            ret.beltAsLayer=src.beltAsLayer;
            ret.hotLayer=src.hotLayer;
            ret.coldLayer=src.coldLayer;
            ret.useTakeOffDrafted=src.useTakeOffDrafted;
            ret.useTakeOffHatsIndoor=src.useTakeOffHatsIndoor;
            ret.hideHatsInBed=src.hideHatsInBed;
            ret.useTemp=src.useTemp;
            ret.useTempHotWhileIndoor=src.useTempHotWhileIndoor;
            ret.useTempColdWhileIndoor=src.useTempColdWhileIndoor;
            ret.useTempHotWhileOutdoor=src.useTempHotWhileOutdoor;
            ret.useTempColdWhileOutdoor=src.useTempColdWhileOutdoor;
            ret.useHotTempDefault=src.useHotTempDefault;
            ret.useColdTempDefault=src.useColdTempDefault;
            ret.hotTemp=src.hotTemp;
            ret.coldTemp=src.coldTemp;
            ret.init=src.init;
            ret.useHatsAsLayer=src.useHatsAsLayer;
            ret.hatsLayer=src.hatsLayer;
            ret.dontBeNaked=src.dontBeNaked;

            return ret;
        }
        public void ExposeData() //longにして圧縮しようかとも思ったが100人も行かないだろうからそのまま保存
        {
            Scribe_Values.Look(ref init, "init", Default.init);
            Scribe_Values.Look(ref useTakeOffDrafted, "useDrafted", Default.useTakeOffDrafted);
            Scribe_Values.Look(ref hideHatsInBed, "hideHatsInBed", Default.hideHatsInBed);
            Scribe_Values.Look(ref useTakeOffHatsIndoor, "useHatsIn", Default.useTakeOffHatsIndoor);
            Scribe_Values.Look(ref useTemp, "useTemp", Default.useTemp);
            Scribe_Values.Look(ref useTempHotWhileIndoor, "hotIn", Default.useTempHotWhileIndoor);
            Scribe_Values.Look(ref useTempColdWhileIndoor, "coldIn", Default.useTempColdWhileIndoor);
            Scribe_Values.Look(ref useTempHotWhileOutdoor, "hotOut", Default.useTempHotWhileOutdoor);
            Scribe_Values.Look(ref useTempColdWhileOutdoor, "coldOut", Default.useTempColdWhileOutdoor);
            Scribe_Values.Look(ref useHotTempDefault, "useHotDef", Default.useHotTempDefault);
            Scribe_Values.Look(ref useColdTempDefault, "useColdDef", Default.useColdTempDefault);
            Scribe_Values.Look(ref hotTemp, "hotTemp", Default.hotTemp);
            Scribe_Values.Look(ref coldTemp, "coldTemp", Default.coldTemp);

            Scribe_Values.Look(ref indoorLayer, "indLay", Default.indoorLayer);
            Scribe_Values.Look(ref privateRoomLayer, "prvLay", Default.privateRoomLayer);
            Scribe_Values.Look(ref sleepingLayer, "slpLay", Default.sleepingLayer);
            Scribe_Values.Look(ref applyBarracks, "aplBarac", Default.applyBarracks);
            Scribe_Values.Look(ref onlyUseInBed, "onlyBed", Default.onlyUseInBed);
            Scribe_Values.Look(ref beltAsLayer, "beltLay", Default.beltAsLayer);
            Scribe_Values.Look(ref hotLayer, "hotLay", Default.hotLayer);
            Scribe_Values.Look(ref coldLayer, "coldLay", Default.coldLayer);

            Scribe_Values.Look(ref useHatsAsLayer, "useHatsAs", Default.useHatsAsLayer);
            Scribe_Values.Look(ref hatsLayer, "hatsLay", Default.hatsLayer);

            Scribe_Values.Look(ref dontBeNaked, "dontNaked", Default.dontBeNaked);

        }

        public void CorrectLegacyLayer()
        {
            if (indoorLayer == LayerPriority.All)
            {
                indoorLayer = LayerPriority.Other;
            }
            if (privateRoomLayer == LayerPriority.All)
            {
                privateRoomLayer = LayerPriority.Other;
            }
            if (beltAsLayer == LayerPriority.All)
            {
                beltAsLayer = LayerPriority.Other;
            }
            if (hatsLayer == LayerPriority.All)
            {
                hatsLayer = LayerPriority.Other;
            }
        }

        public const int LongValueSize = 36; //ulongではないので63まで
        public long LongValue
        {
            get
            {
                long ret = 0;

                ret += Util.bitOn(init, 1);
                ret += Util.bitOn(useTakeOffDrafted, 2);
                ret += Util.bitOn(useTakeOffHatsIndoor, 3);
                ret += Util.bitOn(hideHatsInBed, 4);
                ret += Util.bitOn(useTemp, 5);
                ret += Util.bitOn(useTempHotWhileIndoor, 6);
                ret += Util.bitOn(useTempColdWhileIndoor, 7);
                ret += Util.bitOn(useTempHotWhileOutdoor, 8);
                ret += Util.bitOn(useTempColdWhileOutdoor, 9);
                ret += Util.bitOn(useHotTempDefault, 10);
                ret += Util.bitOn(useColdTempDefault, 11);
                ret += Util.bitOn((uint)indoorLayer, 12);//0～7→3bit
                ret += Util.bitOn((uint)privateRoomLayer, 15);
                ret += Util.bitOn((uint)beltAsLayer, 18);
                ret += Util.bitOn((uint)hotLayer, 21);
                ret += Util.bitOn((uint)coldLayer, 24);
                ret += Util.bitOn(useHatsAsLayer, 27);
                ret += Util.bitOn((uint)hatsLayer, 28);
                ret += Util.bitOn(dontBeNaked, 31);
                ret += (long)Util.bitOnLong((ulong)sleepingLayer, 32);
                ret += (long)Util.bitOnLong(onlyUseInBed, 35);
                ret += (long)Util.bitOnLong(applyBarracks, 36);
                //以降追加する時はbitOnLong

                return ret;
            }
        }

        //public void SetFromLongValue(ulong value)
        //{

        //}

        public void SetToDefault()
        {
            indoorLayer = Default.indoorLayer;
            privateRoomLayer = Default.privateRoomLayer;
            applyBarracks = Default.applyBarracks;
            sleepingLayer = Default.sleepingLayer;
            onlyUseInBed = Default.onlyUseInBed;
            beltAsLayer = Default.beltAsLayer;
            hotLayer = Default.hotLayer;
            coldLayer = Default.coldLayer;
            useTakeOffDrafted = Default.useTakeOffDrafted;
            useTakeOffHatsIndoor = Default.useTakeOffHatsIndoor;
            hideHatsInBed = Default.hideHatsInBed;
            useTemp = Default.useTemp;
            useTempHotWhileIndoor = Default.useTempHotWhileIndoor;
            useTempColdWhileIndoor = Default.useTempColdWhileIndoor;
            useTempHotWhileOutdoor = Default.useTempHotWhileOutdoor;
            useTempColdWhileOutdoor = Default.useTempColdWhileOutdoor;
            useHotTempDefault = Default.useHotTempDefault;
            useColdTempDefault = Default.useColdTempDefault;
            hotTemp = Default.hotTemp;
            coldTemp = Default.coldTemp;
            init = Default.init;
            useHatsAsLayer = Default.useHatsAsLayer;
            hatsLayer = Default.hatsLayer;
            dontBeNaked = Default.dontBeNaked;
        }


    }

}
