using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace TakeOffIndoor
{
    public enum LayerPriority
    {
        Null=0,
        Headgear = 1,
        Naked = 2,
        Underwear = 3,
        OnSkin = 4,
        Middle = 5,
        Shell = 6,
        Other = 7,
        All = 8, //互換用
        Midlle = 5//互換用
    }

    public class TakeOffComp : ThingComp
    {
        private bool usePersonalSettings = false;

        private static TakeOffComp Default = new TakeOffComp();

        static int serialInt = 0;
        static int loopInt = 0;
        //public int tickShift;
        public int calcShift;
        private int lastRoomCheck = 0;
        private int lastOwnRoomCheck = 0;
        private bool indoorBk = false;
        private bool privateRoomBk = false;
        private bool inBedBk = false;
        private bool DraftedBK = false;
        private bool tooHotBk = false;
        private bool tooColdBk = false;
        private Room _nowRoom;
        private Room _ownRoom = null;

        public bool chkRoomFlg = true;
        public bool hatsInvisibleFlg = false;

        public bool initialized = false;

        public ulong personalSettingsBk;
        public ulong settingsBk;
        //public bool settingToggle = false;
        public ulong nowPersonalSettings;

        public LayerPriority nowLayer = LayerPriority.Other;

        public TakeOffData data;
        public TakeOffData personalData;

        public bool portrait = false;
        public bool tempHatsInvisible = false;

        public float lastTick = 0;

        public float frame = 0;
        public bool tempExecute = false;

        public List<Apparel> wornApparelCash = null;
        public List<Apparel> wornApparelPortraitCash = null;

        public bool first = true;

        public Building_Bed nowBed = null;
        //public int updateCount = 0;

        //Compは互換用に古いメンバも入れておいた方が良いのかもしれない
        //AwesomeInventory、Rimdeed、WorkTab等で呼び出される？ → 大丈夫っぽい
        //public LayerPriority indoorLayer = LayerPriority.Middle;
        //public LayerPriority privateRoomLayer = LayerPriority.OnSkin;
        //public LayerPriority beltAsLayer = LayerPriority.Shell;
        //public LayerPriority hotLayer = LayerPriority.Midlle;
        //public LayerPriority coldLayer = LayerPriority.Shell;
        //public bool useTakeOffDrafted = false;
        //public bool useTakeOffHatsIndoor = false;
        //public bool hideHatsInBed = true;
        //public bool useTemp = false;
        //public bool useTempHotWhileIndoor = true;
        //public bool useTempColdWhileIndoor = true;
        //public bool useTempHotWhileOutdoor = false;
        //public bool useTempColdWhileOutdoor = false;
        //public bool useHotTempDefault = true;
        //public bool useColdTempDefault = true;
        //public int hotTemp = 100;
        //public int coldTemp = -100;
        //public static int hotTempDefault = 26;
        //public static int coldTempDefault = 16;
        //public bool useHatsAsLayer = true;
        //public LayerPriority hatsLayer = LayerPriority.Shell;


        public TakeOffComp()
        {
        }

        public bool UsePersonalSettings
        {
            get
            {
                return usePersonalSettings && !TakeOffSettings.dontUsePersonalSettings;
            }
            set
            {
                usePersonalSettings = value;
                if (usePersonalSettings)
                {
                    //debug.WriteLine("ups1");
                    if (ReferenceEquals(personalData,null))
                    {
                        personalData = TakeOffSettings.data.MakeClone();

                        //debug.WriteLine("persinit " + personalData.hotTemp.ToString());
                    }
                    else
                    {
                        if (!personalData.init)
                        {
                            if(personalData == TakeOffData.Default)
                            {
                                personalData = TakeOffSettings.data.MakeClone();
                            }
                            personalData.init = true;
                        }
                    }
                    data = personalData;
                }
                else
                {
                    //debug.WriteLine("ups2");
                    data = TakeOffSettings.data;
                }
            }
        }

        private int SerialInt
        {
            get
            {
                return serialInt++;
            }
        }
        private int LoopInt
        {
            get
            {
                if (loopInt++ > 2)
                {
                    loopInt = 0;
                }
                return loopInt;
            }
        }

        public void Initialize()
        {
            
            RoomCheck();
            initialized = true;
            UsePersonalSettings = false;
            nowLayer = GetShowLayerLevel();
        }

        public Pawn Pawn
        {
            get
            {
                return parent as Pawn;
            }
        }

        public bool IsTargetPawn
        {
            get
            {
                if (!TakeOffSettings.useTakeOff) return false;
                Pawn pawn = Pawn;
                return  !pawn.IsPrisoner && pawn.IsColonistPlayerControlled && !pawn.Dead && pawn.IsColonist;
            }
        }
        

        //徴兵判定及び機能を使うかどうか
        //trueなら徴兵されていないor徴兵でも機能を使用
        public bool IsDraftedTargetPawn
        {
            get
            {
                return !Pawn.Drafted || UseTakeOffDrafted;
            }
        }
        public LayerPriority PrivateRoomLayer
        {
            get
            {
                return data.privateRoomLayer; 
            }
        }

        public LayerPriority SleepingLayer
        {
            get
            {
                return data.sleepingLayer;
            }
        }
        public LayerPriority IndoorLayer
        {
            get
            {
                return data.indoorLayer;
            }
        }
        public LayerPriority BeltAsLayer
        {
            get
            {
                return data.beltAsLayer;
            }
        }
        public LayerPriority HotLayer
        {
            get
            {
                return data.hotLayer;
            }
        }
        public LayerPriority ColdLayer
        {
            get
            {
                return data.coldLayer;
            }
        }
        public LayerPriority HatsLayer
        {
            get
            {
                return data.hatsLayer;
            }
        }
        public bool UseTakeOffDrafted
        {
            get
            {
                return data.useTakeOffDrafted;
            }
        }
        public bool HideHatsInBed
        {
            get
            {
                return data.hideHatsInBed;
            }
        }
        public bool UseTakeOffHatsIndoor
        {
            get
            {
                return data.useTakeOffHatsIndoor;
            }
        }
        public bool UseTemp
        {
            get
            {
                return data.useTemp;
            }
        }
        public bool UseTempHotWhileIndoor
        {
            get
            {
                return data.useTempHotWhileIndoor;
            }
        }
        public bool UseTempColdWhileIndoor
        {
            get
            {
                return data.useTempColdWhileIndoor;
            }
        }
        public bool UseTempHotWhileOutdoor
        {
            get
            {
                return data.useTempHotWhileOutdoor;
            }
        }
        public bool UseTempColdWhileOutdoor
        {
            get
            {
                return data.useTempColdWhileOutdoor;
            }
        }
        public bool UseHotTempDefault
        {
            get
            {
                return data.useHotTempDefault;
            }
        }
        public bool UseColdTempDefault
        {
            get
            {
                return data.useColdTempDefault;
            }
        }

        public int HotTemp
        {
            get
            {
                return data.hotTemp;
            }
        }
        public int ColdTemp
        {
            get
            {
                return data.coldTemp;
            }
        }

        public bool UseHatsAsLayer
        {
            get
            {
                return data.useHatsAsLayer;
            }
        }


        public LayerPriority GetShowLayerLevel()
        {
            if (!IsTargetPawn)
            {
                return LayerPriority.All;
            }

            if (indoor)
            {
                LayerPriority lp = IndoorLayer;

                if (TakeOffSettings.usePrivateRoom && privateRoom)
                {
                    lp = PrivateRoomLayer;
                }

                if (inBed)
                {
                    if (!AtSleepingSpot)
                    {
                        lp = SleepingLayer;
                    }
                }

                if (UseTemp) 
                { 
                    if (UseTempHotWhileIndoor && TooHot)
                    {
                        if((int)lp > (int)HotLayer)
                        {
                            lp = HotLayer;
                        }
                    }
                    if (UseTempColdWhileIndoor && TooCold)
                    {
                        if ((int)lp < (int)ColdLayer)
                        {
                            lp = ColdLayer;
                        }
                    }
                }

                return lp;
            }
            else
            {
                if (UseTemp)
                {
                    if (UseTempHotWhileOutdoor && TooHot)
                    {
                        return HotLayer;
                    }
                    if (UseTempColdWhileOutdoor && TooCold)
                    {
                        return ColdLayer;
                    }
                }
            }

            return LayerPriority.Shell;
        }

        public bool ChkPersonalSettingsUpdate
        {
            get
            {
                if (nowPersonalSettings == personalSettingsBk)
                {
                    return false;
                }
                personalSettingsBk = nowPersonalSettings;
                return true;
            }
        }
        public bool ChkSettingsUpdate
        {
            get
            {
                if (settingsBk == TakeOffSettings.settingsBK)
                {
                    return false;
                }
                settingsBk = TakeOffSettings.settingsBK;
                //if (settingToggle == TakeOffSettings.settingToggle)
                //{
                //    return false;
                //}
                //settingToggle = TakeOffSettings.settingToggle;
                return true;
            }
        }

        public ulong CalcSettingToULong()
        {
            ulong ret = 0;
            ret += Util.bitOnLong(usePersonalSettings, TakeOffData.LongValueSize + 1);
            if (usePersonalSettings)
            {
                ret += (ulong)data.LongValue;
            }
            SetChkRoomFlg();
            return ret;
        }

        public void SetChkRoomFlg()
        {
            if (PrivateRoomLayer == IndoorLayer)
            {
                chkRoomFlg = false;
            }
            else
            {
                if (TakeOffSettings.usePrivateRoom)
                {
                    chkRoomFlg = true;
                }
            }
        }

        public bool ChkLayerLevelChanged
        {
            get
            {
                if (first) //初期化用の項目はここ
                {
                    Firstpass();
                }
                //競合するケースがある様なので機能停止RenderPawnInternalの外で描画してるMODがありそうだが原因不明
                if (lastTick < Find.TickManager.TicksGame || (Pawn.Drafted != DraftedBK)) //徴兵の変更は常に反映 
                {
                    lastTick = Find.TickManager.TicksGame + TakeOffSettings.updateTicks; //timeSpeedで倍率かけようかと思ったが、SlowSpeedと競合しそうなのでやめた
                    if ((Pawn.Drafted != DraftedBK) || (indoorBk != Indoor) || (privateRoomBk != PrivateRoom) || (inBedBk != InBed) 
                        || (UseTemp && ((tooHotBk != TooHot) || (tooColdBk != TooCold)))
                        || first)
                    {
                        first = false;
                        indoorBk = Indoor;
                        privateRoomBk = PrivateRoom;
                        inBedBk = InBed;
                        DraftedBK = Pawn.Drafted;
                        if (UseTemp)
                        {
                            tooHotBk = TooHot;
                            tooColdBk = TooCold;
                        }
                        UpdateShowLayerLevel();
                        //nowLayerBK = nowLayer;

                        return true;
                    }
                }
                //if(nowLayerBK != nowLayer)
                //{
                //    nowLayerBK = nowLayer;
                //    return true;
                //}
                return false;
            }
        }
        public void UpdateShowLayerLevel()
        {
            nowLayer = GetShowLayerLevel();
            if (HatsInvisible)
            {
                hatsInvisibleFlg = true;
            }
            else
            {
                hatsInvisibleFlg = false;
            }
        }

        public bool indoor
        {
            get
            {
                return indoorBk;
            }
        }
        public bool Indoor
        {
            get
            {
                return Pawn.Spawned && !Pawn.Position.UsesOutdoorTemperature(Pawn.Map);
            }

        }
        public bool privateRoom
        {
            get
            {
                return privateRoomBk;
            }
        }

        public Room OwnedRoom
        {
            get
            {
                if (data.applyBarracks)
                {
                    if ((Find.TickManager.TicksGame) >= lastOwnRoomCheck + TakeOffSettings.roomCheckInterval)
                    {
                        _ownRoom = Pawn.ownership?.OwnedBed?.GetRoom();
                        lastOwnRoomCheck = Find.TickManager.TicksGame;
                    }
                    return _ownRoom;
                }
                else
                {
                    return Pawn.ownership?.OwnedRoom;

                }
            }
        }
        public bool PrivateRoom
        {
            get
            {
                if (!chkRoomFlg) //private=indoorの場合確認不要
                {
                    return false;
                }


                if (!indoor || OwnedRoom == null)
                {
                    return false;
                }
                //debug.WriteLine(this.Pawn.Name.ToStringShort + " PR NowRoom:" + NowRoom.ID + ",OwnedRoom:" + OwnedRoom.ID + ",RoomCheck" + Pawn.GetRoom().ID);

                return NowRoom == OwnedRoom;
            }
        }

        public bool inBed
        {
            get
            {
                return inBedBk;
            }
        }
        public bool InBed
        {
            get
            {
                bool flg = Pawn.InBed() && !Pawn.Downed;
                if (flg)
                {
                    nowBed = Pawn.CurrentBed();
                }
                return flg;
            }
        }

        public bool OnlyUseInBed
        {
            get
            {
                return data.onlyUseInBed;
            }
        }
        public bool AtSleepingSpot
        {
            get
            {
                if (OnlyUseInBed && InBed && nowBed != null)
                {
                    return nowBed.def.building.bed_showSleeperBody;
                }
                return false;
            }
        }

        public Room NowRoom
        {
            get{
                //if ((Find.TickManager.TicksGame+tickShift) >= lastRoomCheck + TakeOffSettings.roomCheckInterval)
                //debug.WriteLine("ticksGame:" + Find.TickManager.TicksGame + " lastCheck:" + lastRoomCheck + " interval:" + TakeOffSettings.roomCheckInterval);
                if ((Find.TickManager.TicksGame) >= lastRoomCheck + TakeOffSettings.roomCheckInterval)
                {
                    RoomCheck();
                }
                return _nowRoom;
            }
        }
        private void RoomCheck()
        {
            if (!IsTargetPawn)
            {
                return;
            }
            //debug.WriteLine(Pawn.Name.ToStringShort + " RoomCheck");
            _nowRoom = Pawn.GetRoom();
            //lastRoomCheck = Find.TickManager.TicksGame + tickShift;
            lastRoomCheck = Find.TickManager.TicksGame;
            return;
        }

        public int NowHotTemp
        {
            get
            {
                if (UseHotTempDefault)
                {
                    return TakeOffData.hotTempDefault;
                }
                return HotTemp;
            }
        }
        public bool TooHot
        {
            get
            {
                if (tooHotBk)
                {
                    if ((int)Pawn.AmbientTemperature > NowHotTemp - 2) //フリッカ防止
                    {
                        return true;
                    }
                }
                else
                {
                    //debug.WriteLine(Pawn.Name.ToStringShort + Pawn.AmbientTemperature.ToString()  + " "+NowHotTemp.ToString());
                    if ((int)Pawn.AmbientTemperature > NowHotTemp)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public int NowColdTemp
        {
            get
            {
                if (UseColdTempDefault)
                {
                    return TakeOffData.coldTempDefault;
                }
                return ColdTemp;
            }
        }
        public bool TooCold
        {
            get
            {
                if (tooColdBk)
                {
                    if ((int)Pawn.AmbientTemperature < NowColdTemp + 2) //フリッカ防止
                    {
                        return true;
                    }
                }
                else
                {
                    if ((int)Pawn.AmbientTemperature < NowColdTemp)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool HatsInvisible
        {
            get
            {
                if ((HideHatsInBed && inBed)
                    || (UseTakeOffHatsIndoor && indoor))
                {
                    return true;
                }
                if (UseHatsAsLayer && (HatsLayer > nowLayer))
                {
                    return true;
                }
                return false;
            }
        }

        public bool DontBeNaked
        {
            get
            {
                return data.dontBeNaked;
            }
        }

        public LayerPriority CalcLayerPriority(Apparel apparel)
        {
            ApparelLayerDef ald = apparel.def.apparel.LastLayer;

            if(CustomLayerDictionary.DefsDic.TryGetValue(ald.defName,out LayerPriority lp))
            {
                return lp;
            }

            if (ald.Equals(ApparelLayerDefOf.Overhead))
            {
                //debug.WriteLine(apparel.def.defName+" is headgear");
                return LayerPriority.Headgear;
            }
            if (ald.Equals(ApparelLayerDefOf.Belt))
            {
                return data.beltAsLayer;
            }

            if (ald.drawOrder >= ApparelLayerDefOf.Shell.drawOrder)
            {
                return LayerPriority.Shell;
            }
            if (ald.drawOrder >= ApparelLayerDefOf.Middle.drawOrder)
            {
                return LayerPriority.Middle;
            }
            if (ald.drawOrder >= ApparelLayerDefOf.OnSkin.drawOrder)
            {
                return LayerPriority.OnSkin;
            }
            if (ald.drawOrder < ApparelLayerDefOf.OnSkin.drawOrder)
            {
                return LayerPriority.Underwear;
            }
            return LayerPriority.Other; //実際には通らない
        }

        public bool HasGraphic(Apparel apparel)
        {
            return Util.HasGraphic(apparel.def);
        }

        public bool IsIncludeTorso(Apparel apparel)
        {
            if (apparel.def.apparel.bodyPartGroups!=null && apparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
            {
                return true;
            }
            return false;
        }

        public static void RemoveFromAnimals()
        {
            IEnumerable<ThingDef> things = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.race?.Animal??false);

            foreach(ThingDef thing in things)
            {
                //debug.WriteLine(thing.defName);
                if (thing.comps != null)
                {
                    for (int i = 0; i < thing.comps.Count; i++)
                    {
                        if (thing.comps[i].compClass == typeof(TakeOffComp))
                        {
                            thing.comps.RemoveAt(i);
                            //debug.WriteLine("Removed");
                            break;
                        }
                    }
                }
                else
                {
                    //debug.WriteLine("no comps");
                }
            }
        }

        public static void RemoveFromMechanoid()
        {
            IEnumerable<ThingDef> things = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.race?.IsMechanoid??false);

            foreach (ThingDef thing in things)
            {
                if (thing.comps != null)
                {
                    for (int i = 0; i < thing.comps.Count; i++)
                    {
                        if (thing.comps[i].compClass == typeof(TakeOffComp))
                        {
                            thing.comps.RemoveAt(i);
                            //debug.WriteLine(thing.defName);
                            break;
                        }
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            if (Pawn == null || Pawn.RaceProps == null || !Pawn.RaceProps.Humanlike)
            {
                return;
            }
            if (!Pawn.IsColonist)
            {
                return;
            }

            base.PostExposeData();

            int ver;
            //debug.WriteLine("toc0"+ " " +Pawn.Name?.ToStringShort+":"+Pawn.ThingID);
            TakeOffWorldComponent wc = Find.World.GetComponent<TakeOffWorldComponent>(); //一部のPawnだとFindでコンポーネントが取得出来ない場合がある 仕様が謎 誘拐されたりするとWorldから除外になってParent的なのが消滅したりする？
            if (wc == null)
            {
                //debug.WriteLine("world is null");
                ver = 0;
                return;
            }
            else
            {
                ver = wc.verdate;
            }

            //debug.WriteLine(Pawn.Name?.ToStringShort +" " + Scribe.mode.ToString() + ":" + ver.ToString());

            bool usePersonal = usePersonalSettings;

            if (Scribe.mode == LoadSaveMode.Saving || ver!= 0)
            {
                //debug.WriteLine("toc1");
                Scribe_Values.Look(ref this.initialized, "TOCInit", false);

                if (this.initialized)
                {
                    //debug.WriteLine("toc1-1");
                    Scribe_Values.Look(ref usePersonal, "TOCPers", false);


                    if(!(Scribe.mode == LoadSaveMode.Saving && ReferenceEquals(personalData,null))) //null値保存しない
                    {
                        Scribe_Deep.Look(ref personalData, "TOC", null);
                    }
                }
            }
            else
            {
                //debug.WriteLine("toc2");
                Scribe_Values.Look(ref this.initialized, "TakeOffComp.initialized", false);


                if (this.initialized)
                {
                    //debug.WriteLine("toc2-1");
                    Scribe_Values.Look(ref usePersonal, "TakeOffComp.usePersonalSettings", false);

                    personalData = new TakeOffData();
                    //debug.WriteLine("toc2-2");

                    Scribe_Values.Look(ref usePersonalSettings, "TakeOffComp.usePersonalSettings", Default.usePersonalSettings);
                    Scribe_Values.Look(ref personalData.useTakeOffDrafted, "TakeOffComp.useTakeOffDrafted", TakeOffData.Default.useTakeOffDrafted);
                    Scribe_Values.Look(ref personalData.hideHatsInBed, "TakeOffComp.hideHatsInBed", TakeOffData.Default.hideHatsInBed);
                    Scribe_Values.Look(ref personalData.useTakeOffHatsIndoor, "TakeOffComp.useTakeOffHatsIndoor", TakeOffData.Default.useTakeOffHatsIndoor);
                    Scribe_Values.Look(ref personalData.useTemp, "TakeOffComp.useTemp", TakeOffData.Default.useTemp);
                    Scribe_Values.Look(ref personalData.useTempHotWhileIndoor, "TakeOffComp.TempHotIndoor", TakeOffData.Default.useTempHotWhileIndoor);
                    Scribe_Values.Look(ref personalData.useTempColdWhileIndoor, "TakeOffComp.TempColdIndoor", TakeOffData.Default.useTempColdWhileIndoor);
                    Scribe_Values.Look(ref personalData.useTempHotWhileOutdoor, "TakeOffComp.TempHotOutdoor", TakeOffData.Default.useTempHotWhileOutdoor);
                    Scribe_Values.Look(ref personalData.useTempColdWhileOutdoor, "TakeOffComp.TempColdOutdoor", TakeOffData.Default.useTempColdWhileOutdoor);
                    Scribe_Values.Look(ref personalData.useHotTempDefault, "TakeOffComp.UseHotDefault", TakeOffData.Default.useHotTempDefault);
                    Scribe_Values.Look(ref personalData.useColdTempDefault, "TakeOffComp.UseColdDefault", TakeOffData.Default.useColdTempDefault);
                    Scribe_Values.Look(ref personalData.hotTemp, "TakeOffComp.hotTemp", TakeOffData.Default.hotTemp);
                    Scribe_Values.Look(ref personalData.coldTemp, "TakeOffComp.coldTemp", TakeOffData.Default.coldTemp);

                    Scribe_Values.Look(ref personalData.indoorLayer, "TakeOffComp.indoorLayer", TakeOffData.Default.indoorLayer);
                    Scribe_Values.Look(ref personalData.privateRoomLayer, "TakeOffComp.privateRoomLayer", TakeOffData.Default.privateRoomLayer);
                    Scribe_Values.Look(ref personalData.beltAsLayer, "TakeOffComp.beltAsLayer", TakeOffData.Default.beltAsLayer);
                    Scribe_Values.Look(ref personalData.hotLayer, "TakeOffComp.hotLayer", TakeOffData.Default.hotLayer);
                    Scribe_Values.Look(ref personalData.coldLayer, "TakeOffComp.coldLayer", TakeOffData.Default.coldLayer);

                    Scribe_Values.Look(ref personalData.useHatsAsLayer, "TakeOffComp.useHatsAs", TakeOffData.Default.useHatsAsLayer);
                    Scribe_Values.Look(ref personalData.hatsLayer, "TakeOffComp.hatsLayer", TakeOffData.Default.hatsLayer);

                    if (usePersonal || !(personalData.Equals(TakeOffData.Default)))
                    {
                        personalData.init = true;
                    }

                }
            }
            //debug.WriteLine("toc3");

            UsePersonalSettings = usePersonal;

            //debug.WriteLine("toc4");
            //debug.WriteLine("toc5");
        }
        public void Firstpass()
        {
            //hotTempDefault = (int)Pawn.ComfortableTemperatureRange().max;
            //coldTempDefault = (int)Pawn.ComfortableTemperatureRange().min;
            //hotTempDefault = (int)Pawn.GetStatValue(StatDefOf.ComfyTemperatureMax);
            //coldTempDefault = (int)Pawn.GetStatValue(StatDefOf.ComfyTemperatureMin);
            //hotTempDefault = (int)Pawn.def.statBases.GetStatValueFromList(StatDefOf.ComfyTemperatureMax, 99);
            //coldTempDefault = (int)Pawn.def.statBases.GetStatValueFromList(StatDefOf.ComfyTemperatureMin, -99);
            //debug.WriteLine(Pawn.Name.ToStringShort+" hot:" + hotTempDefault.ToString()+" cold:"+coldTempDefault.ToString());

            //tickShift = SerialInt;
            //calcShift = LoopInt;
            RoomCheck();
            UsePersonalSettings = usePersonalSettings;

            SetChkRoomFlg();
            //UpdateShowLayerLevel();
        }


        public void Update(bool forced = false)
        {

            PawnRenderer rend = Pawn.Drawer.renderer;

            UpdateShowLayerLevel();

            if (ChkLayerLevelChanged || forced)
            {
                if (TakeOffSettings.dontApplyPortrait)
                {
                    portrait = true;
                    tempHatsInvisible = TakeOffSettings.hatsOnlyOnMap;

                    ResolveGraphics(rend);

                    //rend.RenderPortrait();
                    portrait = false;
                    tempHatsInvisible = false;

                    ResolveGraphics(rend);
                }
                else
                {
                    if (TakeOffSettings.hatsOnlyOnMap)
                    {
                        tempHatsInvisible = TakeOffSettings.hatsOnlyOnMap;
                        ResolveGraphics(rend);
                        //rend.RenderPortrait();
                        tempHatsInvisible = false;
                    }


                    ResolveGraphics(rend,!TakeOffSettings.ResolveAlwaysApparel && forced);
                    if (forced)
                    {
                        rend.graphics.ResolveAllGraphics();
                    }
                    else
                    {
                        rend.graphics.ResolveApparelGraphics();
                    }


                    if (!TakeOffSettings.hatsOnlyOnMap)
                    {
                        PortraitsCache.SetDirty(Pawn);
                    }
                }
            }
        }

        public void ResolveGraphics(PawnRenderer rend,bool force=false)
        {
            if (TakeOffSettings.ResolveAlwaysAll || force)
            {
                rend.graphics.ResolveAllGraphics();
            }
            else
            {
                rend.graphics.ResolveApparelGraphics();
            }
        }
        public void WornApparel(ref List<Apparel> list)
        {
            TakeOffComp comp = this;

            //デバッグコードが残ってるとmisc robotでエラーが出る(Pawnだが名前が無い)
            //debug.WriteLine(apparelTracker.pawn.Name.ToStringShort + " GetWornApparel" + ((comp == null) ? " null" : ""));
            if (!comp.initialized)
            {
                if (Pawn.IsColonist)
                {
                    comp.Initialize();
                }
                else
                {
                    return;
                }
            }

            //debug.WriteLine("ind:" + comp.indoor.ToString() + " pvr:" + comp.privateRoom.ToString() + " inb:" + comp.inBed.ToString() + " nowL:" + comp.nowLayer.ToString() + " itp:" + comp.IsTargetPawn.ToString() + "hif:" + comp.hatsInvisibleFlg);
            if (comp.IsTargetPawn)
            {
                list = list.ListFullCopy<Apparel>();

                if (TakeOffSettings.UseInvisible)
                {
                    if (!comp.portrait)
                    {
                        RemoveLayersForceInvisible(comp, ref list); //強制非表示設定を反映
                    }
                }

                if(TakeOffSettings.ResolveLegacy || TakeOffSettings.ResolveLegacy2)
                {
                    LayerPriority nowLayer = comp.nowLayer;
                    if (comp.IsDraftedTargetPawn)
                    {
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            if (TakeOffSettings.UseInvisible) //強制表示設定は個別に判定
                            {
                                if (ApparelList.IsForceVisible(list[i].def, Pawn.Drafted))
                                {
                                    continue;
                                }
                            }
                            LayerPriority lp = comp.CalcLayerPriority(list[i]);
                            if ((lp == LayerPriority.Headgear && comp.hatsInvisibleFlg) || lp > nowLayer)
                            {
                                list.Remove(list[i]);
                            }
                        }
                    }
                }
                else
                {
                    if (comp.portrait || comp.tempHatsInvisible) //徴兵中
                    {
                        if (comp.tempHatsInvisible) //帽子非表示の場合帽子だけ除外
                        {
                            RemoveLayersHatsOnly(comp, ref list);
                        }
                        if (comp.portrait) //ポートレイトの場合抜ける、そうでない場合通常の処理も行う
                        {
                            return;
                        }
                    }

                    if (comp.IsDraftedTargetPawn)
                    {
                        if (comp.DontBeNaked)
                        {
                            RemoveLayersNoNaked(comp, ref list);
                        }
                        else
                        {
                            RemoveLayers(comp, ref list);
                        }
                    }
                }
            }
            return;
        }

        public static void RemoveLayersHatsOnly(TakeOffComp comp, ref List<Apparel> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (TakeOffSettings.UseInvisible)
                {
                    if (ApparelList.IsForceVisible(list[i].def, comp.Pawn.Drafted))
                    {
                        continue;
                    }
                }
                LayerPriority rlp = comp.CalcLayerPriority(list[i]);
                if ((rlp == LayerPriority.Headgear && comp.tempHatsInvisible))
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static void RemoveLayers(TakeOffComp comp, ref List<Apparel> list)
        {

            LayerPriority lp = comp.nowLayer;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (TakeOffSettings.UseInvisible)
                {
                    if (ApparelList.IsForceVisible(list[i].def, comp.Pawn.Drafted))
                    {
                        continue;
                    }
                }

                LayerPriority rlp = comp.CalcLayerPriority(list[i]);
                if ((((rlp == LayerPriority.Headgear) && (comp.hatsInvisibleFlg || comp.tempHatsInvisible)))
                    || (rlp > lp))
                {
                    //debug.WriteLine("rem:" + list[i].Label + " rlp:" + rlp.ToString()) ;
                    list.RemoveAt(i);
                }
                else
                {
                    //debug.WriteLine("add:" + list[i].Label + " rlp:" + rlp.ToString());
                }
            }
        }

        public static void RemoveLayersNoNaked(TakeOffComp comp, ref List<Apparel> list)
        {
            List<int> remlist = new List<int>();
            LayerPriority lp = comp.nowLayer;
            LayerPriority minLp = LayerPriority.Other;
            bool minIsTorso = false;
            int min = -1;
            bool hasTorso = false;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                bool flg = true;
                if (TakeOffSettings.UseInvisible)
                {
                    if (ApparelList.IsForceVisible(list[i].def, comp.Pawn.Drafted))
                    {
                        flg = false;
                    }
                }

                LayerPriority rlp = comp.CalcLayerPriority(list[i]);
                if (flg)
                {
                    if ((((rlp == LayerPriority.Headgear) && (comp.hatsInvisibleFlg || comp.tempHatsInvisible)))
                        || (rlp > lp))
                    {
                        remlist.Add(i);
                    }
                }
                if (comp.HasGraphic(list[i]))
                {
                    if (rlp > LayerPriority.Naked)
                    {
                        if (rlp <= minLp)
                        {
                            hasTorso = comp.IsIncludeTorso(list[i]);
                            if (hasTorso)
                            {
                                minLp = rlp;
                                min = i;
                                minIsTorso = true;
                            }
                            else
                            {
                                if (!minIsTorso) //現在の最小レイヤーに胴がない場合のみ更新
                                {
                                    minLp = rlp;
                                    min = i;
                                }
                            }
                        }
                    }
                }
            }

            foreach (int i in remlist) //大きい順に投入してるので大きい方から消す
            {
                if (i != min)
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static void RemoveLayersForceInvisible(TakeOffComp comp, ref List<Apparel> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (TakeOffSettings.UseInvisible)
                {
                    if (ApparelList.IsInvisible(list[i].def, comp.Pawn.Drafted))
                    {
                        list.Remove(list[i]);
                    }
                }
            }
        }

    }
}
