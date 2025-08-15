using AlienRace;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace StylishRim
{
    internal class StylishAtlasManager
    {
        // バニラのキャッシュ機能がそもそもヒトという単一の種族を格納する前提だから高解像度のキャッシュは想定されてない。
        // なのでキャッシュを種族別に分けるとメモリの使用量が跳ね上がる問題が発生。
        // HARはどうにかメモリの使用量を抑えようとした？のかキャッシュを一部の種族で共用しようとしてBorerScaleのことを忘れたのか処理を誤ってサイズバグが発生。

        // ちなみにこれを作ってる当時はHARのサイズバグとか知りませんでした。
        // 種族別に設定できたほうがいいよなーとか目的で種族別にキャッシュ分ければ処理も高速化するじゃんとか
        // 色々あってキャッシュを種族別に分けたんですが、結果として知らないうちにサイズバグが無くなりました。
        // つまり、副産物です。
        public static List<PawnTextureAtlas> pawnTextureAtlases;
        public static List<PawnTextureAtlas> PawnTextureAtlases
        {
            get
            {
                if (pawnTextureAtlases == null) pawnTextureAtlases = AccessTools.StaticFieldRefAccess<List<PawnTextureAtlas>>(typeof(GlobalTextureAtlasManager), "pawnTextureAtlases");
                return pawnTextureAtlases;
            }
        }
        public static readonly string TEXT_CORPSE = "Stylish Corpse";
        public static readonly float BORDER_CORPSE = 1;

        private static readonly int COUNT_MAX = 300;
        private static readonly int COUNT_1 = 100;
        private static readonly int COUNT_2 = 200;
        public static Dictionary<string, List<PawnTextureAtlas>> AtlasMap = new Dictionary<string, List<PawnTextureAtlas>>();
        public static Dictionary<string, int> DrawModeDic = new Dictionary<string, int>();

        public static List<string> disableCacheIDList = new List<string>();
        public static List<string> disableCacheColonistList = new List<string>();


        public static int GCCount = 0;
        public static int index = 0;
        public static bool defaultAtlasCancel = false;

        // Expose fields
        public static bool disableCacheColonist = false;
        public static bool leaveOneCache = false;
        public static bool preCreateCache = false;
        public static bool forCorpseCache = false;
        public static bool disableZoomCacheOff = false;

        private static bool runOnce = false;

        public static bool enableAnimalGear = false;
        public static bool disableAnimalCache = false;

        public static void RemoveDrawMode(string thingId)
        {
            if (DrawModeDic.ContainsKey(thingId)) DrawModeDic.Remove(thingId);
        }
        public static bool DrawModeIs(string thingId, PawnDrawMode drawMode)
        {
            if (!DrawModeDic.ContainsKey(thingId))
            {
                DrawModeDic.Add(thingId, (int)drawMode);
            }
            else
            {
                if (DrawModeDic[thingId] == (int)drawMode)
                {
                    return true;
                }

                DrawModeDic[thingId] = (int)drawMode;
            }
            return false;
        }
        public static float DisableZoomCacheOff(float f)
        {
            if (disableZoomCacheOff)
            {
                f = 19;
            }
            return f;
        }
        public static void ClearDisableCacheList()
        {
            disableCacheColonistList.Clear();
            disableCacheIDList.Clear();
        }
        public static void AddDisableCacheByID(string thingId)
        {
            if (!disableCacheIDList.Contains(thingId)) disableCacheIDList.Add(thingId);
        }
        public static void RemoveDisableCache(string thingId_raceName)
        {
            if (disableCacheIDList.Contains(thingId_raceName)) disableCacheIDList.Remove(thingId_raceName);
        }
        public static void AddDisableCachColonist()
        {
            if (disableCacheColonist)
            {
                foreach (string id in PawnsFinder.AllMaps_FreeColonists.Select(x => x.ThingID))
                {
                    if (!disableCacheColonistList.Contains(id)) disableCacheColonistList.Add(id);
                }
            }
        }
        public static bool IsDisableCache(Pawn pawn)
        {
            return IsDisableCacheColonist(pawn.ThingID) || IsDisableCacheSetting(pawn);
        }
        public static bool IsDisableCacheSetting(Pawn pawn)
        {
            return disableCacheIDList.Contains(pawn.def.defName) || disableCacheIDList.Contains(pawn.ThingID);
        }
        public static bool IsDisableCacheColonist(string thingId)
        {
            if (disableCacheColonist)
            {
                if (disableCacheColonistList.Count == 0) AddDisableCachColonist();
            }
            return disableCacheColonistList.Contains(thingId);
        }


        public static bool GCCountCheck()
        {
            if (GCCount < COUNT_MAX)
            {
                if (GCCount == 0)
                {
                    if (!runOnce)
                    {
                        PreCreateCacheLeaveRaces();
                        runOnce = true;
                    }
                }
                else if (GCCount == COUNT_1)
                {
                    OrganizeAtlasMap();
                }
                else if (GCCount == COUNT_2)
                {
                    DestroyEmptyAtlasBetweenGC();
                }
                GCCount++;
                return false;
            }
            RemoveNullAtlas();
            index++;
            GCCount = 0;
            return true;
        }
        public static string AtlasKey
        {
            get
            {
                if (AtlasMap.Count <= index)
                {
                    index = 0;
                }
                return AtlasMap.ToList()[index].Key;
            }
        }
        public static PawnTextureAtlas NowAtlas
        {
            get
            {
                if (PawnTextureAtlases.Count == 0) return null;
                if (PawnTextureAtlases.Count <= index)
                {
                    index = 0;
                }
                return PawnTextureAtlases[index];
            }
        }
        internal static void PreCreateCacheLeaveRaces()
        {
            if (!preCreateCache) return;

            if (forCorpseCache && !AtlasMap.ContainsKey(TEXT_CORPSE))
            {
                AtlasMap.Add(TEXT_CORPSE, new List<PawnTextureAtlas>());
            }
            foreach (string key in AtlasMap.Keys)
            {
                PreCreateCache(key);
            }
            foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values.Where(x => x.IsRace && x.leaveOneCache))
            {
                if (DefDatabase<ThingDef_AlienRace>.AllDefs.Where(x => x.defName == pss.key).EnumerableCount() > 0)
                {
                    if (!AtlasMap.ContainsKey(pss.key))
                    {
                        AtlasMap.Add(pss.key, new List<PawnTextureAtlas>());
                    }
                    PreCreateCache(pss.key, pss);
                }
            }
        }
        public static void PreCreateCache(string key, PawnStyleSet pss = null)
        {
            if (Find.Maps == null) return;
            if (!DefDatabase<ThingDef_AlienRace>.AllDefsListForReading.Any(x => x.defName == key)) return;
            if (!AtlasMap.ContainsKey(key))
            {
                AtlasMap.Add(key, new List<PawnTextureAtlas>());
            }
            else if (AtlasMap[key] == null)
            {
                AtlasMap[key] = new List<PawnTextureAtlas>();
            }
            if (AtlasMap[key].Count == 0)
            {
                if (key == TEXT_CORPSE)
                {
                    PawnTextureAtlas atlas = CorpseCache;
                    PawnTextureAtlases.Add(atlas);
                    AtlasMap[key].Add(atlas);
                }
                else
                {
                    if (pss == null) pss = PawnStyleSet.Get(key);
                    if (pss != null && pss.leaveOneCache)
                    {
                        int cacheX = 8;
                        int cacheY = 8;
                        for (int i = pss.CacheSize; i > 0; i--)
                        {
                            if (i % 2 == 1)
                            {
                                cacheX /= 2;
                            }
                            else
                            {
                                cacheY /= 2;
                            }
                        }
                        PawnTextureAtlas atlas = CreateAtlas(pss.Calc.atlasScale, pss.Calc.borderScale, cacheX, cacheY, pss.CacheWidth);
                        PawnTextureAtlases.Add(atlas);
                        AtlasMap[pss.key].Add(atlas);
                    }
                }
            }
        }
        private static void RemoveNullAtlas()
        {
            for (int i = PawnTextureAtlases.Count - 1; i >= 0; i--)
            {
                if (PawnTextureAtlases[i] == null)
                {
                    PawnTextureAtlases.RemoveAt(i);
                }
            }
            List<string> nullKeys = new List<string>();
            foreach (string key in AtlasMap.Keys)
            {
                for (int j = AtlasMap[key].Count - 1; j >= 0; j--)
                {
                    if (AtlasMap[key][j] == null)
                    {
                        RemoveAtlas(key, j);
                    }
                }
                if (AtlasMap[key] == null)
                {
                    nullKeys.Add(key);
                }
            }
            foreach (string key in nullKeys)
            {
                AtlasMap[key] = new List<PawnTextureAtlas>();
            }
        }
        public static void DestroyEmptyAtlasBetweenGC()
        {
            bool removePawn(PawnTextureAtlas atlas, bool remove)
            {
                List<Pawn> tempPawns = new List<Pawn>();
                if (disableCacheColonist)
                {
                    foreach (Pawn p in (atlas as StylishTextureAtlas).FrameAssignments.Keys)
                    {
                        if (disableCacheColonistList.Contains(p.ThingID)) tempPawns.Add(p);
                    }
                }
                foreach (Pawn p in (atlas as StylishTextureAtlas).FrameAssignments.Keys)
                {
                    if (disableCacheIDList.Contains(p.ThingID) && !tempPawns.Contains(p)) tempPawns.Add(p);
                }
                foreach (Pawn p in tempPawns)
                {
                    RemoveFrameSetByPawn(atlas, p);
                }
                tempPawns.Clear();
                return remove && (atlas as StylishTextureAtlas).UsedCount == 0;
            }

            AddDisableCachColonist();

            if (PawnTextureAtlases.Count > 0)
            {
                PawnTextureAtlas atlas = NowAtlas;
                string key = GetAtlasKeyAndIllegalCheck(atlas);
                if (key != null)
                {
                    List<PawnTextureAtlas> list = AtlasMap[key];
                    if (key == TEXT_CORPSE)
                    {
                        if (removePawn(atlas, list.Count > 1))
                        {
                            RemoveAtlas(key, list.IndexOf(atlas));
                        }
                    }
                    else
                    {
                        bool leaveOneCache = PawnStyleSet.Get(key)?.leaveOneCache ?? false;
                        if (removePawn(atlas, !leaveOneCache || list.Count > 1))
                        {
                            RemoveAtlas(key, list.IndexOf(atlas));
                        }
                    }
                }
            }
            foreach (PawnStyleSet pss in PawnStyleSet.Styles.Values.Where(x => x.IsRace))
            {
                pss.RemoveAdjusterByUnfindedPawn();
                if (pss.disableCache)
                {
                    if (AtlasMap.ContainsKey(pss.key))
                    {
                        RemoveAtlas(pss.key);
                    }
                    AtlasMap.Remove(pss.key);
                }
            }
        }
        private static string GetAtlasKeyAndIllegalCheck(PawnTextureAtlas atlas)
        {
            // 渡されたAtlasの種族名をGet
            string key = null;
            foreach (string race in AtlasMap.Keys)
            {
                if (AtlasMap[race].Contains(atlas))
                {
                    key = race;
                    break;
                }
            }
            if (key == null)
            {
                // もしMap格納していないAtlasなら処理する
                if (atlas == null)
                {
                    Log.Warning("[Stylish Rim] Null atlas remained.");
                    PawnTextureAtlases.Remove(atlas);
                }
                else
                {
                    Log.Warning("[Stylish Rim] Atlas not included in AtlasMap.");
                    PawnTextureAtlases.Remove(atlas);
                    atlas.Destroy();
                }
            }
            return key;
        }
        private static void OrganizeAtlasMap()
        {
            if (PawnTextureAtlases.Count > 0)
            {
                string key = GetAtlasKeyAndIllegalCheck(NowAtlas);
                if (key != null)
                {
                    if (AtlasMap[key] != null)
                    {
                        OrganizeAtlas(AtlasMap[key]);
                    }
                    else
                    {
                        AtlasMap[key] = new List<PawnTextureAtlas>();
                    }
                }
            }
        }
        private static void OrganizeAtlas(List<PawnTextureAtlas> list)
        {
            if (list.Count > 1)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] == null) list.RemoveAt(i);
                }
                //対象のキャッシュリストが二つ以上であればまず空きが多い順に並び替える
                list.OrderByDescending(x => x?.FreeCount ?? 0);
                for (int i = list.Count - 1; i > 0; i--)
                {
                    List<Pawn> tempPawns = new List<Pawn>();
                    try
                    {
                        if (list[0].FreeCount > (list[i] as StylishTextureAtlas).UsedCount * 1.5)
                        {

                            foreach (Pawn p in (list[i] as StylishTextureAtlas).FrameAssignments.Keys)
                            {
                                tempPawns.Add(p);
                            }
                            foreach (Pawn p in tempPawns)
                            {
                                RemoveFrameSetByPawn(list[i], p);
                                if (!list[0].ContainsFrameSet(p))
                                {
                                    list[0].TryGetFrameSet(p, out PawnTextureAtlasFrameSet frameSet, out bool createdNew);
                                }
                            }
                        }
                        else
                        {
                            foreach (Pawn p in (list[i] as StylishTextureAtlas).FrameAssignments.Keys)
                            {
                                tempPawns.Add(p);
                            }
                            foreach (Pawn p in tempPawns)
                            {
                                if (!list[0].ContainsFrameSet(p))
                                {
                                    RemoveFrameSetByPawn(list[i], p);
                                }
                            }
                        }
                    }
                    finally
                    {
                        tempPawns.Clear();
                    }
                }
            }
        }
        private static void RemoveAtlas(PawnTextureAtlas atlas)
        {
            RemoveAtlas(atlas, GetAtlasKeyAndIllegalCheck(atlas));
        }
        private static void RemoveAtlas(PawnTextureAtlas atlas, string race)
        {
            if (race == null) return;
            RemoveAtlas(race, AtlasMap[race].IndexOf(atlas));
        }
        private static void RemoveAtlas(string race)
        {
            //よくわからなくなったので対象種族のアトラス問答無用ぶっ潰しマン
            if (!AtlasMap.ContainsKey(race)) return;
            for (int i = AtlasMap[race].Count - 1; i >= 0; i--)
            {
                RemoveAtlas(race, i);
            }
            PortraitsCache.Clear();
        }
        private static void ClearAtlas(PawnTextureAtlas atlas)
        {
            if (atlas == null) return;
            StylishTextureAtlas sta = atlas as StylishTextureAtlas;
            foreach (PawnTextureAtlasFrameSet frameset in sta.FrameAssignments.Values)
            {
                sta.FreeFrameSet.Add(frameset);
            }
            sta.FrameAssignments.Clear();
        }
        private static void RemoveAtlas(string race, int index)
        {
            //確実にアトラスぶっ潰しマン
            PawnTextureAtlases.Remove(AtlasMap[race][index]);
            AtlasMap[race][index].Destroy();
            AtlasMap[race][index] = null;
            AtlasMap[race].RemoveAt(index);
        }
        public static bool PawnCheck(Pawn pawn)
        {
            return pawn.SpawnedOrAnyParentSpawned;
        }
        public static PawnTextureAtlas GetAtlas(Pawn pawn, bool corpse = false)
        {
            string race = corpse ? TEXT_CORPSE : pawn.def.defName;
            if (AtlasMap.ContainsKey(race))
            {
                foreach (PawnTextureAtlas atlas in AtlasMap[race])
                {
                    if (atlas.ContainsFrameSet(pawn)) return atlas;
                }
                foreach (PawnTextureAtlas atlas in AtlasMap[race])
                {
                    if (atlas.FreeCount > 0) return atlas;
                }
            }
            return null;
        }

        // せっかくなのでこっちもリストを対象の種族のみに絞って最適化
        // 重複しないように更に絞り込む。
        public static List<PawnTextureAtlas> OptimizeList(Pawn pawn)
        {
            if (forCorpseCache && pawn.Dead)
            {
                //死体キャッシュ作成する場合
                if (!AtlasMap.ContainsKey(TEXT_CORPSE))
                {
                    AtlasMap.Add(TEXT_CORPSE, new List<PawnTextureAtlas>());
                }
                foreach (PawnTextureAtlas a in AtlasMap[TEXT_CORPSE])
                {
                    //すでに死体キャッシュに格納されていればそいつを返す
                    if (a.ContainsFrameSet(pawn))
                    {
                        return new List<PawnTextureAtlas> { a };
                    }
                }
                //格納されていなければ格納元から消して
                RemoveFrameSetByPawn(GetAtlas(pawn), pawn);
                foreach (PawnTextureAtlas a in AtlasMap[TEXT_CORPSE])
                {
                    //空きのあるキャッシュを返す
                    if (a.FreeCount > 0)
                    {
                        return new List<PawnTextureAtlas> { a };
                    }
                }
                return new List<PawnTextureAtlas>();
            }
            PawnTextureAtlas atlas = GetAtlas(pawn);
            if (atlas == null)
            {
                return new List<PawnTextureAtlas>();
            }
            return new List<PawnTextureAtlas> { atlas };
        }
        public static void AddAtlasMap(PawnTextureAtlas atlas, Pawn pawn)
        {
            string race = forCorpseCache && pawn.Dead ? TEXT_CORPSE : pawn.def.defName;
            if (!AtlasMap.ContainsKey(race))
            {
                AtlasMap.Add(race, new List<PawnTextureAtlas>() { atlas });
            }
            else
            {
                AtlasMap[race].Add(atlas);
            }
        }
        public static PawnTextureAtlas CorpseCache => CreateAtlas(1, 1, 1, 1, 16, true);
        public static PawnTextureAtlas CreateAtlasByPawn(Pawn pawn)
        {
            if (forCorpseCache && pawn.Dead) return CorpseCache;
            PawnStyleSet pss = PawnStyleSet.Get(pawn);
            if (pss != null)
            {
                int cacheX = 8;
                int cacheY = 8;
                for (int i = pss.CacheSize; i > 0; i--)
                {
                    if (i % 2 == 1)
                    {
                        cacheX /= 2;
                    }
                    else
                    {
                        cacheY /= 2;
                    }
                }
                return CreateAtlas(pss.Calc.atlasScale, pss.Calc.borderScale, cacheX, cacheY, pss.CacheWidth);
            }
            return new StylishTextureAtlas(StylishAdjuster.GetPawnAtlasScale(pawn), StylishAdjuster.GetPawnBorderScale(pawn));
        }
        public static PawnTextureAtlas CreateAtlas(int atlasScale, float borderScale, int cacheX = 1, int cacheY = 1, int cacheWidth = 16, bool forCorpse = false)
        {
            defaultAtlasCancel = true;
            PawnTextureAtlas atlas = new StylishTextureAtlas(atlasScale, borderScale, cacheX, cacheY, cacheWidth, forCorpse);
            defaultAtlasCancel = false;
            if (atlas?.RawTexture == null) atlas = new PawnTextureAtlas();
            return atlas;
        }


        // スタイリッシュなアトラスマネージャー
        // 更新したらキャッシュ消さないとってやつ
        public static void TargetRaceRebuild(string raceName)
        {
            // 種族全員のキャッシュ消す
            RemoveAtlas(raceName);
            PreCreateCache(raceName);

            // キャッシュ作るだけのアトラス生成
            foreach (Pawn p in PawnsFinder.HomeMaps_FreeColonistsSpawned.Where(x => x.def.defName == raceName))
            {
                GlobalTextureAtlasManager.TryGetPawnFrameSet(p, out PawnTextureAtlasFrameSet frameSet, out bool createdNew);
            }
        }
        public static void TargetPawnRebuild(string thingID)
        {
            // 対象のフレームセットぶっ壊す
            foreach (Pawn p in PawnsFinder.AllMaps.Where(x => x.ThingID == thingID))
            {
                DestroyAtlasByPawn(p);
            }

            // キャッシュ作るだけのアトラス生成
            foreach (Pawn p in PawnsFinder.HomeMaps_FreeColonistsSpawned.Where(x => x.ThingID == thingID))
            {
                GlobalTextureAtlasManager.TryGetPawnFrameSet(p, out PawnTextureAtlasFrameSet frameSet, out bool createdNew);
            }
        }
        public static void DestroyAtlasByPawn(Pawn pawn)
        {
            // ターゲットの入っているフレームセット絶対壊すマン
            if (!AtlasMap.ContainsKey(pawn.def.defName))
            {
                if (!AtlasMap.ContainsKey(TEXT_CORPSE))
                {
                    return;
                }
                RemoveFrameSetByPawn(AtlasMap[TEXT_CORPSE].Where(x => x.ContainsFrameSet(pawn)).FirstOrDefault(), pawn);
            }
            else
            {
                RemoveFrameSetByPawn(AtlasMap[pawn.def.defName].Where(x => x.ContainsFrameSet(pawn)).FirstOrDefault(), pawn);
            }
            PortraitsCache.Clear();
        }
        private static void RemoveFrameSetByPawn(PawnTextureAtlas atlas, Pawn pawn)
        {
            // ターゲットのフレームセット絶対壊すマン
            if (atlas == null || !atlas.ContainsFrameSet(pawn)) return;
            (atlas as StylishTextureAtlas).FreeFrameSet.Add((atlas as StylishTextureAtlas).FrameAssignments[pawn]);
            (atlas as StylishTextureAtlas).FrameAssignments.Remove(pawn);
            RemoveDrawMode(pawn.ThingID);
            PawnStyleSet.Get(pawn)?.RemoveAdjuster(pawn.ThingID);
        }

        public static void ForceRebuild()
        {
            // リビルドついでにガベージコレクションも行うマン
            GCCount = COUNT_MAX;
            GlobalTextureAtlasManager.rebakeAtlas = true;
            DrawModeDic.Clear();
            GlobalTextureAtlasManager.GlobalTextureAtlasManagerUpdate();
        }
        internal static void SendDebug()
        {
            // AtlasMap確認マン
            Log.Error($"[Stylish Rim] AtlasMap / Atlases Count: {AtlasMap.Count} / {AccessTools.StaticFieldRefAccess<List<PawnTextureAtlas>>(typeof(GlobalTextureAtlasManager), "pawnTextureAtlases").Count}.");
            int i = 0;
            foreach (KeyValuePair<string, List<PawnTextureAtlas>> pair in AtlasMap)
            {
                Log.Warning($"[Stylish Rim] AtlasMap[{i}] key: {pair.Key}, List Count: {pair.Value.Count}.");
                foreach (PawnTextureAtlas atlas in pair.Value)
                {

                    Log.Message($"[Stylish Rim] frameAssignments Count: {CachedData.pawnTextureAtlasFrameAssignments(atlas).Count}, freeFrameSet Count: {atlas.FreeCount}, Texture: {atlas.RawTexture.width} x {atlas.RawTexture.height}.");
                    foreach (KeyValuePair<Pawn, PawnTextureAtlasFrameSet> p in CachedData.pawnTextureAtlasFrameAssignments(atlas))
                    {
                        Log.Message($"[Stylish Rim] key: {p.Key}, race: {p.Key.def.defName}, Size: {p.Value.meshes[0].vertices[0]}.");
                    }
                }
                i++;
            }
        }
    }
    public class StylishTextureAtlas : PawnTextureAtlas
    {
        public static bool disableAdjustCache = false;
        public static bool ignoreHeadOnly = false;
        public static bool IgnoreHeadOnly => !disableAdjustCache && ignoreHeadOnly;
        public Dictionary<Pawn, PawnTextureAtlasFrameSet> FrameAssignments => Traverse.Create(this).Field<Dictionary<Pawn, PawnTextureAtlasFrameSet>>("frameAssignments").Value;
        public int UsedCount => FrameAssignments.Count;
        public int TotalCount => FrameAssignments.Count + FreeCount;
        public List<PawnTextureAtlasFrameSet> FreeFrameSet => Traverse.Create(this).Field<List<PawnTextureAtlasFrameSet>>("freeFrameSets").Value;
        internal StylishTextureAtlas(int atlas, float border, int cacheX = 1, int cacheY = 1, int cacheWidth = 16, bool forCorpse = false)
        {
            Traverse tra = Traverse.Create(this);
            tra.Field<Dictionary<Pawn, PawnTextureAtlasFrameSet>>("frameAssignments").Value = new Dictionary<Pawn, PawnTextureAtlasFrameSet>();
            tra.Field<List<PawnTextureAtlasFrameSet>>("freeFrameSets").Value = new List<PawnTextureAtlasFrameSet>();
            AccessTools.StaticFieldRefAccess<List<Pawn>>(typeof(PawnTextureAtlas), "tmpPawnsToFree") = new List<Pawn>();
            int atlasX;
            int atlasY;
            int atlasOneX;
            int atlasOneY;
            if (!forCorpse)
            {
                atlasX = 2048 * atlas / (disableAdjustCache ? 1 : cacheX) / 16 * cacheWidth;
                atlasY = 2048 * atlas / (disableAdjustCache ? 1 : cacheY);
                atlasOneX = 128 * atlas / 16 * cacheWidth;
                atlasOneY = 128 * atlas;
            }
            else
            {
                atlasX = 2048;
                atlasY = 2048;
                atlasOneX = 64;
                atlasOneY = 64;
                border = StylishAtlasManager.BORDER_CORPSE;
                cacheWidth = 16;
            }
            float w = (float)atlasOneX / atlasX;
            float h = (float)atlasOneY / atlasY;

            tra.Field<RenderTexture>("texture").Value = new RenderTexture(atlasX, atlasY, 24, forCorpse ? RenderTextureFormat.ARGB1555 : RenderTextureFormat.ARGB32, 0)
            {
                name = "PawnTextureAtlas_" + atlasX
            };
            List<Rect> list = new List<Rect>();
            for (int i = 0; i < atlasX; i += atlasOneX)
            {
                for (int j = 0; j < atlasY; j += atlasOneY)
                {
                    list.Add(new Rect((float)i / atlasX, (float)j / atlasY, w, h));
                }
            }
            while (list.Count >= (forCorpse ? 1 : IgnoreHeadOnly ? 4 : 8))
            {
                int count = list.Count;
                PawnTextureAtlasFrameSet pawnTextureAtlasFrameSet = new PawnTextureAtlasFrameSet();
                if (forCorpse)
                {
                    pawnTextureAtlasFrameSet.uvRects = new Rect[8]
                    {
                        list[count - 1],
                        list[count - 1],
                        list[count - 1],
                        list[count - 1],
                        list[count - 1],
                        list[count - 1],
                        list[count - 1],
                        list.Pop()
                    };
                }
                else if (IgnoreHeadOnly)
                {
                    pawnTextureAtlasFrameSet.uvRects = new Rect[8]
                    {
                        list[count - 1],
                        list[count - 2],
                        list[count - 3],
                        list[count - 4],
                        list.Pop(),
                        list.Pop(),
                        list.Pop(),
                        list.Pop()
                    };
                }
                else
                {
                    pawnTextureAtlasFrameSet.uvRects = new Rect[8]
                    {
                        list.Pop(),
                        list.Pop(),
                        list.Pop(),
                        list.Pop(),
                        list.Pop(),
                        list.Pop(),
                        list.Pop(),
                        list.Pop()
                    };
                }
                pawnTextureAtlasFrameSet.isDirty = new bool[8];
                pawnTextureAtlasFrameSet.meshes = (from u in pawnTextureAtlasFrameSet.uvRects
                                                   select CreateMeshForUV(u, border / 16 * cacheWidth, border)).ToArray();
                pawnTextureAtlasFrameSet.atlas = tra.Field<RenderTexture>("texture").Value;
                tra.Field<List<PawnTextureAtlasFrameSet>>("freeFrameSets").Value.Add(pawnTextureAtlasFrameSet);
            }
            Mesh CreateMeshForUV(Rect r, float x, float y)
            {
                Mesh mesh = new Mesh
                {
                    vertices = new Vector3[4]
                    {
                        new Vector3(-1f * x, 0f, -1f * y),
                        new Vector3(-1f * x, 0f, 1f * y),
                        new Vector3(1f * x, 0f, 1f * y),
                        new Vector3(1f * x, 0f, -1f * y)
                    },
                    normals = new Vector3[4]
                    {
                        Vector3.up,
                        Vector3.up,
                        Vector3.up,
                        Vector3.up
                    },
                    uv = new Vector2[4]
                    {
                        r.min,
                        new Vector2(r.xMin, r.yMax),
                        r.max,
                        new Vector2(r.xMax, r.yMin)
                    },
                    triangles = new int[6]
                    {
                        0,
                        1,
                        2,
                        2,
                        3,
                        0
                    }
                };
                return mesh;

            }
        }
    }
}
