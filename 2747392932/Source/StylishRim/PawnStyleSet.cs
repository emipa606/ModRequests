using AlienRace;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace StylishRim
{
    public class PawnStyleSet : IExposable
    {

        //==============================Static=====================================//

        internal static Dictionary<string, PawnStyleSet> _styles;
        public static Dictionary<string, PawnStyleSet> Styles
        {
            get
            {
                if (_styles == null) _styles = new Dictionary<string, PawnStyleSet>();
                return _styles;
            }
        }
        static readonly StylishSet None = new StylishSet();

        private static ThingDef_AlienRace humanDef;

        public static ThingDef_AlienRace HumanDef
        {
            get
            {
                if (humanDef == null) humanDef = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.defName == StylishRimSettings.RACE_HUMAN).FirstOrDefault() as ThingDef_AlienRace;
                return humanDef;
            }
        }

        public static bool ContainsKey(string thingID_raceName) => thingID_raceName != null && Styles.ContainsKey(thingID_raceName);
        public static bool ContainsKey(Pawn pawn) => pawn != null && ContainsKey(pawn.ThingID) || ContainsKey(pawn.def.defName);
        public static PawnStyleSet Get(string thingID_raceName)
        {
            if (!ContainsKey(thingID_raceName)) return null;
            return Styles[thingID_raceName];
        }
        public static PawnStyleSet Get(Pawn pawn) => Get(pawn.ThingID) ?? Get(pawn.def.defName);

        public static void CalculateAll(bool force = false)
        {
            foreach (PawnStyleSet pss in Styles.Values)
            {
                if (!pss.IsPawn)
                {
                    pss.CalculateValue(force);
                }
            }
            foreach (PawnStyleSet pss in Styles.Values)
            {
                if (pss.IsPawn)
                {
                    if (PawnsFinder.AllMaps.Any(x => x.ThingID == pss.key))
                    {
                        pss.CalculateValue(force);
                    }
                }
            }
        }
        public void Apply()
        {
            if (ContainsKey(key))
            {
                _styles[key] = this;
            }
            else
            {
                _styles.Add(key, this);
            }
            ApplyValue();
            if (IsRace)
            {
                ApplyRace(key);
            }
        }
        private static void ApplyRace(string raceName)
        {
            foreach (PawnStyleSet pss in Styles.Values.Where(x => x.raceName == raceName))
            {
                pss.ApplyValue();
            }
        }
        public void ClearApparelCache()
        {
            if (IsRace)
            {
                ClearApparelCacheByRace();
            }
            else
            {
                ClearApparelCacheByPawn();
            }
        }
        private void ClearApparelCacheByRace()
        {
            foreach (Pawn p in PawnsFinder.AllMaps.Where(x => x.def.defName == key))
            {
                ClearApparelCache(p);
                AddClearCache(p);
            }
            RemoveAdjusterByUnfindedPawn();
        }
        private void ClearApparelCacheByPawn()
        {
            ClearApparelCache(key);
            AddClearCache(key);
        }
        private void AddClearCache(Pawn p)
        {
            AddClearCache(p.ThingID);
        }
        private void AddClearCache(string key)
        {
            if (clearCaches == null) clearCaches = new List<string> { key };
            if (!clearCaches.Contains(key))
            {
                clearCaches.Add(key);
            }
        }
        public static void ClearApparelCache(string ThingID)
        {
            foreach (Pawn p in PawnsFinder.AllMaps.Where(x => x.ThingID == ThingID))
            {
                ClearApparelCache(p);
            }
        }
        public static void ClearApparelCache(Pawn pawn)
        {
            pawn.Drawer.renderer.graphics.nakedGraphic = null;
            //new Traverse(pawn.Drawer.renderer.graphics).Field<int>("cachedMatsBodyBaseHash").Value = -1;
        }
        public static void Remove(string thingID_raceName)
        {
            if (ContainsKey(thingID_raceName))
            {
                Styles[thingID_raceName].ClearApparelCache();
                Styles[thingID_raceName].Remove();
            }
        }
        public void Remove()
        {
            if (Styles.ContainsValue(this))
            {
                Styles.Remove(key);
            }
        }
        //==============================Instance=====================================//

        // expose fields
        private Dictionary<string, StylishSet> hairs = new Dictionary<string, StylishSet>();
        private Dictionary<string, ApparelDict> apparels = new Dictionary<string, ApparelDict>();
        private Dictionary<string, StylishSet> equipments = new Dictionary<string, StylishSet>();
        private Dictionary<string, StylishSet> genes = new Dictionary<string, StylishSet>();
        public Dictionary<int, StylishAddon> addons;

        public string key = null;
        public string name = null;
        public string raceName = null;
        public Vector3 adjustBodySize = Vector3.one;
        public Vector3 adjustHeadSize = Vector3.one;
        public Vector3 adjustBodyAddonSize = Vector3.one;
        public Vector3 adjustHeadAddonSize = Vector3.one;

        public Vector2 adjustHeadOffset = Vector2.zero;
        public Vector2 adjustHeadOffsetVerticalDiff = Vector2.zero;
        public Vector2 adjustBodyAddonOffset = Vector2.zero;
        public Vector2 adjustHeadAddonOffset = Vector2.zero;

        public Vector2 adjustDrawLoc = Vector2.zero;
        public Vector3 adjustChangeBodySize = Vector3.one;
        public Vector3 adjustChangeBodyOffset = Vector3.zero;

        public Vector2 adjustChangeBodyAngle = Vector2.zero;
        public Vector3 adjustChangeBodyColor = Vector3.zero;

        public int adjustPortraitRotation = 2;
        public float adjustPortraitSize = 100f;
        public Vector2 adjustPortraitOffset = Vector2.zero;
        public bool adjustPortraitDisableHeadGear = false;
        public bool adjustPortraitDisableClothes = false;
        public float adjustPortraitAngle = 0f;
        public float adjustPortraitHeadOffsetMult = 100;

        public bool disableCache = false;
        public bool disableUtilityAdjust = false;

        public string defaultBodyTypeDefName = string.Empty;
        public string changeBodyRaceName = null;
        public string changeBodyTypeName = null;
        //public string changeShaderName = null;
        public string ChangeBodyRaceName { get => changeBodyRaceName ?? Get(raceName)?.changeBodyRaceName ?? null; set => changeBodyRaceName = value; }
        public bool showHair = false;
        public bool ShowHair => showHair || (Get(raceName)?.showHair ?? false);

        //race commonality
        public float borderScaleMult = 1f;
        public int cacheSize = 6;
        public int resolution = 0;

        public bool ignoreRaceRestriction = false;
        public bool IgnoreRaceRestriction
        {
            get { return IsRace ? ignoreRaceRestriction : Get(raceName)?.ignoreRaceRestriction ?? ignoreRaceRestriction; }
        }
        public bool leaveOneCache = false;
        public bool LeaveOneCache
        {
            get { return IsRace ? leaveOneCache : Get(raceName)?.leaveOneCache ?? leaveOneCache; }
        }
        public int cacheWidth = 16;
        public int CacheWidth { get => IsRace ? cacheWidth : Get(raceName)?.cacheWidth ?? cacheWidth; }


        public Vector2 raceDrawSize = Vector2.one;
        public Vector2 raceHeadDrawSize = Vector2.one;
        public Vector2 racePortraitDrawSize = Vector2.one;
        public Vector2 racePortraitHeadDrawSize = Vector2.one;


        // temp fields
        public Pawn pawn;
        public PawnStyleSet racePss;

        public ThingDef_AlienRace alienDef;
        public ThingDef_AlienRace AlienDef
        {
            get
            {
                if (alienDef == null) alienDef = Def as ThingDef_AlienRace;
                return alienDef ?? HumanDef;
            }
        }

        private ThingDef def;
        public ThingDef Def
        {
            get
            {
                if (def == null)
                {
                    if (IsPawn)
                    {
                        if (racePss == null) racePss = Get(raceName);
                        if (racePss?.def == null)
                        {
                            def = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => (raceName ?? key) == x.defName).FirstOrDefault();
                        }
                        else
                        {
                            return racePss.def;
                        }
                    }
                    else
                    {
                        def = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => (raceName ?? key) == x.defName).FirstOrDefault();
                    }
                }
                return def;
            }
            set => def = value;
        }

        public StylishBodyChanger changer;
        public StylishBodyChanger Changer
        {
            get => changer ?? Get(raceName)?.changer ?? null;
        }



        public Dictionary<string, Vector3> corrections;
        public Dictionary<string, Vector3> drawLocCorrections;
        public Dictionary<string, Vector3> Corrections
        {
            get => IsRace ? corrections : Get(raceName)?.corrections ?? corrections;
        }
        public Dictionary<string, Vector3> DrawLocCorrections
        {
            get => IsRace ? drawLocCorrections : Get(raceName)?.drawLocCorrections ?? drawLocCorrections;
        }
        public Vector3 GetCorrection(string bodyType)
        {
            return Corrections != null ? Corrections.ContainsKey(bodyType) ? Corrections[bodyType] : Vector3.zero : Vector3.zero;
        }
        public Vector3 GetDrawLocCorrection(string bodyType)
        {
            return DrawLocCorrections != null ? DrawLocCorrections.ContainsKey(bodyType) ? DrawLocCorrections[bodyType] : Vector3.zero : Vector3.zero;
        }

        public Dictionary<int, StylishAddon> Addons
        {
            get
            {
                if (addons == null) SetUpAddon();
                return addons;
            }
            set
            {
                addons = value;
            }
        }
        public void SetUpAddon() => StylishAddon.SetUpAddon(ref addons, AlienDef);
        public void SetUpAddon(List<BodyAddon> list)
        {
            StylishAddon.SetUpAddon(ref addons, list, raceName ?? key);
        }
        public void AddonsClear()
        {
            addons.Clear();
        }
        public bool changeFace = false;
        public bool useFaceAnim = false;

        public float largestValue = 1f;
        public float largestBodyValue = 1f;
        public float largestHeadValue = 1f;

        public bool loadedScale = false;
        public float raceBorderScale = 1f;
        public int raceAtlasScale = 1;


        public PawnStyleSet() { }
        public bool IsPawn { get => raceName != null; }
        public bool IsRace { get => raceName == null; }

        public Vector3 AdjustBodySize { get => Vector3.Scale(adjustBodySize, Get(raceName)?.adjustBodySize ?? Vector3.one); }
        public Vector3 AdjustHeadSize { get => Vector3.Scale(adjustHeadSize, Get(raceName)?.adjustHeadSize ?? Vector3.one); }
        public Vector3 AdjustBodyAddonSize { get => Vector3.Scale(adjustBodyAddonSize, Get(raceName)?.adjustBodyAddonSize ?? Vector3.one); }
        public Vector3 AdjustHeadAddonSize { get => Vector3.Scale(adjustHeadAddonSize, Get(raceName)?.adjustHeadAddonSize ?? Vector3.one); }

        public float AdjustBodySizeX { get => adjustBodySize.x * (Get(raceName)?.adjustBodySize.x ?? 1f); set => adjustBodySize.x = value; }
        public float AdjustBodySizeY { get => adjustBodySize.y * (Get(raceName)?.adjustBodySize.y ?? 1f); set => adjustBodySize.y = value; }
        public float AdjustBodySizeZ { get => adjustBodySize.z * (Get(raceName)?.adjustBodySize.z ?? 1f); set => adjustBodySize.z = value; }
        public float AdjustHeadSizeX { get => adjustHeadSize.x * (Get(raceName)?.adjustHeadSize.x ?? 1f); set => adjustHeadSize.x = value; }
        public float AdjustHeadSizeY { get => adjustHeadSize.y * (Get(raceName)?.adjustHeadSize.y ?? 1f); set => adjustHeadSize.y = value; }
        public float AdjustHeadSizeZ { get => adjustHeadSize.z * (Get(raceName)?.adjustHeadSize.z ?? 1f); set => adjustHeadSize.z = value; }

        public float AdjustBodyAddonSizeX { get => adjustBodyAddonSize.x * (Get(raceName)?.adjustBodyAddonSize.x ?? 1f); set => adjustBodyAddonSize.x = value; }
        public float AdjustBodyAddonSizeY { get => adjustBodyAddonSize.y * (Get(raceName)?.adjustBodyAddonSize.y ?? 1f); set => adjustBodyAddonSize.y = value; }
        public float AdjustBodyAddonSizeZ { get => adjustBodyAddonSize.z * (Get(raceName)?.adjustBodyAddonSize.z ?? 1f); set => adjustBodyAddonSize.z = value; }
        public float AdjustHeadAddonSizeX { get => adjustHeadAddonSize.x * (Get(raceName)?.adjustHeadAddonSize.x ?? 1f); set => adjustHeadAddonSize.x = value; }
        public float AdjustHeadAddonSizeY { get => adjustHeadAddonSize.y * (Get(raceName)?.adjustHeadAddonSize.y ?? 1f); set => adjustHeadAddonSize.y = value; }
        public float AdjustHeadAddonSizeZ { get => adjustHeadAddonSize.z * (Get(raceName)?.adjustHeadAddonSize.z ?? 1f); set => adjustHeadAddonSize.z = value; }

        public float AdjustHeadOffsetX { get => adjustHeadOffset.x + (Get(raceName)?.adjustHeadOffset.x ?? 0f); set => adjustHeadOffset.x = value; }
        public float AdjustHeadOffsetY { get => adjustHeadOffset.y + (Get(raceName)?.adjustHeadOffset.y ?? 0f); set => adjustHeadOffset.y = value; }
        public Vector2 AdjustHeadOffsetVerticalDiff { get => adjustHeadOffsetVerticalDiff + (Get(raceName)?.adjustHeadOffsetVerticalDiff ?? Vector2.zero); set => adjustHeadOffsetVerticalDiff = value; }
        public float AdjustHeadOffsetVerticalDiffSide { get => adjustHeadOffsetVerticalDiff.x + (Get(raceName)?.adjustHeadOffsetVerticalDiff.x ?? 0f); set => adjustHeadOffsetVerticalDiff.x = value; }
        public float AdjustHeadOffsetVerticalDiffBack { get => adjustHeadOffsetVerticalDiff.y + (Get(raceName)?.adjustHeadOffsetVerticalDiff.y ?? 0f); set => adjustHeadOffsetVerticalDiff.y = value; }

        public float AdjustBodyAddonOffsetX { get => adjustBodyAddonOffset.x + (Get(raceName)?.adjustBodyAddonOffset.x ?? 0f); set => adjustBodyAddonOffset.x = value; }
        public float AdjustBodyAddonOffsetY { get => adjustBodyAddonOffset.y + (Get(raceName)?.adjustBodyAddonOffset.y ?? 0f); set => adjustBodyAddonOffset.y = value; }
        public float AdjustHeadAddonOffsetX { get => adjustHeadAddonOffset.x + (Get(raceName)?.adjustHeadAddonOffset.x ?? 0f); set => adjustHeadAddonOffset.x = value; }
        public float AdjustHeadAddonOffsetY { get => adjustHeadAddonOffset.y + (Get(raceName)?.adjustHeadAddonOffset.y ?? 0f); set => adjustHeadAddonOffset.y = value; }
        public Vector2 AdjustDrawLoc { get => (adjustDrawLoc + (Get(raceName)?.adjustDrawLoc ?? Vector2.zero)) / 1000; set => adjustDrawLoc = value; }
        public Vector3 AdjustChangeBodyOffset { get => adjustChangeBodyOffset + (Get(raceName)?.adjustChangeBodyOffset ?? Vector3.zero); set => adjustChangeBodyOffset = value; }
        public Vector3 AdjustChangeBodySize { get => Vector3.Scale(adjustChangeBodySize, Get(raceName)?.adjustChangeBodySize ?? Vector3.one); }
        public Vector2 AdjustChangeBodyAngle { get => adjustChangeBodyAngle + (Get(raceName)?.adjustChangeBodyAngle ?? Vector2.zero); set => adjustChangeBodyAngle = value; }

        public float PortraitSize { get => adjustPortraitSize / 100 * (Get(raceName)?.adjustPortraitSize ?? 100f) / 100; set => adjustPortraitSize = value; }
        public float PortraitOffsetX { get => (adjustPortraitOffset.x + (Get(raceName)?.adjustPortraitOffset.x ?? 0f)) / 1000; set => adjustPortraitOffset.x = value; }
        public float PortraitOffsetY { get => (adjustPortraitOffset.y + (Get(raceName)?.adjustPortraitOffset.y ?? 0f)) / 1000; set => adjustPortraitOffset.y = value; }
        public Rot4 PortraitRotation { get => new Rot4(adjustPortraitRotation); set => adjustPortraitRotation = value.AsInt; }

        public float PortraitAngle { get => (adjustPortraitAngle + (Get(raceName)?.adjustPortraitAngle ?? 0f)) % 360; }
        public float PortraitHeadOffsetMult { get => adjustPortraitHeadOffsetMult / 100 * (Get(raceName)?.adjustPortraitHeadOffsetMult ?? 100) / 100; }

        public float BorderScaleMult { get => IsRace ? borderScaleMult : Get(raceName)?.borderScaleMult ?? borderScaleMult; }
        public int CacheSize { get => IsRace ? cacheSize : Get(raceName)?.cacheSize ?? cacheSize; }
        public int Resolution { get => IsRace ? resolution : Get(raceName)?.resolution ?? resolution; }

        public float LargestValue { get => raceBorderScale * (Get(raceName)?.largestValue ?? largestValue); }
        public float LargestHeadValue { get => raceBorderScale * (Get(raceName)?.largestHeadValue ?? largestHeadValue); }
        public float LargestBodyValue { get => raceBorderScale * (Get(raceName)?.largestBodyValue ?? largestBodyValue); }


        private StylishCalculator calc = null;
        public StylishCalculator Calc
        {
            get
            {
                if (calc == null) CalculateValue();
                return calc;
            }
        }
        public bool Calculated => calc != null;

        public Dictionary<string, ApparelDict> Apparels
        {
            get
            {
                if (apparels == null) new Dictionary<string, ApparelDict>();
                return apparels;
            }
            set { apparels = value; }
        }

        public Dictionary<string, List<StylishSet>> adjuster = new Dictionary<string, List<StylishSet>>();
        public Dictionary<string, int> adjustCount = new Dictionary<string, int>();
        public List<string> clearCaches = new List<string>();
        public float locY;
        public Vector3 ToBodyLocY(Vector3 v3)
        {
            v3.y = locY;
            return v3;
        }
        public void ApparelAdjusterInit(string thingId)
        {
            if (adjuster == null) adjuster = new Dictionary<string, List<StylishSet>>();
            if (adjustCount == null) adjustCount = new Dictionary<string, int>();
            if (!adjuster.ContainsKey(thingId))
            {
                adjuster.Add(thingId, new List<StylishSet>() { null });
            }
            else
            {
                adjuster[thingId] = new List<StylishSet>() { null };
            }
            if (!adjustCount.ContainsKey(thingId))
            {
                adjustCount.Add(thingId, 0);
            }
            else
            {
                adjustCount[thingId] = 0;
            }
        }
        public StylishSet GetAdjuster(string thingId)
        {
            if (!adjuster.ContainsKey(thingId)) return null;
            if (adjustCount[thingId] >= adjuster[thingId].Count) adjustCount[thingId] = 0;
            return adjuster[thingId][adjustCount[thingId]];
        }
        public void RemoveAdjuster(string thingId)
        {
            if (adjuster.ContainsKey(thingId)) adjuster.Remove(thingId);
            if (adjustCount.ContainsKey(thingId)) adjuster.Remove(thingId);
        }
        public int CountUp(string thingId)
        {
            if (!adjustCount.ContainsKey(thingId) || !adjuster.ContainsKey(thingId)) return -1;
            return adjustCount[thingId]++;
        }
        public void AddApparelAdjuster(string thingId, Apparel ap)
        {
            if (!adjuster.ContainsKey(thingId))
            {
                ApparelAdjusterInit(thingId);
            }
            adjuster[thingId].Add(GetApparel(ap));
        }
        public void RemoveAdjusterByUnfindedPawn()
        {
            bool removeAll(string thingId)
            {
                if (!PawnsFinder.AllMaps.Any(x => x.ThingID == thingId))
                {
                    adjustCount.Remove(thingId);
                    if (clearCaches.Contains(thingId)) clearCaches.Remove(thingId);
                    return true;
                }
                return false;
            }
            adjuster.RemoveAll(x => removeAll(x.Key));
        }

        public void RemoveApparelStyleSet(string modName, string layer)
        {
            if (Apparels == null || Apparels.Count == 0) return;
            if (!Apparels.ContainsKey(modName) || !Apparels[modName].ContainsKey(layer)) return;
            Apparels[modName].Remove(layer);
            if (Apparels[modName].Count == 0) Apparels.Remove(modName);
        }
        public ApparelDict GetApparels(string modName)
        {
            if (Calc.apparels.ContainsKey(modName)) return Calc.apparels[modName];
            return null;
        }
        public StylishSet GetApparel(Apparel app, bool ignoreCommon = false)
        {
            return GetApparel(app.def.modContentPack.Name, StylishSet.LayerIs(app).defName, ignoreCommon);
        }
        public StylishSet GetApparel(string modName, string layer, bool ignoreCommon)
        {
            ApparelDict ad = GetApparels(modName);
            if (ad == null) return null;
            if (ad.ContainsKey(layer)) return ad[layer];
            return ignoreCommon ? null : ad.GetCommon;
        }
        public Dictionary<string, StylishSet> Equipments
        {
            get
            {
                NewEquipments();
                return equipments;
            }
            set { equipments = value; }
        }
        public void NewEquipments()
        {
            if (equipments == null) equipments = new Dictionary<string, StylishSet>
            {
                { StylishRimSettings.COMMON, new StylishSet(StylishRimSettings.COMMON) },
                { StylishRimSettings.EXTRA1, new StylishSet(StylishRimSettings.EXTRA1) },
                { StylishRimSettings.EXTRA2, new StylishSet(StylishRimSettings.EXTRA2) }
            };
            if (!equipments.ContainsKey(StylishRimSettings.COMMON)) equipments.Add(StylishRimSettings.COMMON, new StylishSet(StylishRimSettings.COMMON));
            if (!equipments.ContainsKey(StylishRimSettings.EXTRA1)) equipments.Add(StylishRimSettings.EXTRA1, new StylishSet(StylishRimSettings.EXTRA1));
            if (!equipments.ContainsKey(StylishRimSettings.EXTRA2)) equipments.Add(StylishRimSettings.EXTRA2, new StylishSet(StylishRimSettings.EXTRA2));
        }
        public StylishSet GetEquipment(ThingWithComps eq)
        {
            return GetEquipment(eq?.def.defName);
        }
        public StylishSet GetEquipment(string defName = null)
        {
            if (defName == null || !Calc.equipments.ContainsKey(defName))
            {
                return GetEquipmentCommon;
            }
            return Calc.equipments[defName];
        }
        public StylishSet GetEquipmentCommon
        {
            get
            {
                if (Calc?.equipments?.ContainsKey(StylishRimSettings.COMMON) ?? false) return Calc.equipments[StylishRimSettings.COMMON];
                return new StylishSet(StylishRimSettings.COMMON);
            }
        }
        public StylishSet GetEquipmentExtra1
        {
            get
            {
                if (Calc?.equipments?.ContainsKey(StylishRimSettings.EXTRA1) ?? false) return Calc.equipments[StylishRimSettings.EXTRA1];
                return new StylishSet(StylishRimSettings.EXTRA1);
            }
        }
        public StylishSet GetEquipmentExtra2
        {
            get
            {
                if (Calc?.equipments?.ContainsKey(StylishRimSettings.EXTRA2) ?? false) return Calc.equipments[StylishRimSettings.EXTRA2];
                return new StylishSet(StylishRimSettings.EXTRA2);
            }
        }
        public Dictionary<string, StylishSet> Hairs
        {
            get
            {
                if (hairs == null) hairs = new Dictionary<string, StylishSet>();
                return hairs;
            }
            set { hairs = value; }
        }
        public Dictionary<string, StylishSet> Genes
        {
            get
            {
                if (genes == null) genes = new Dictionary<string, StylishSet>();
                return genes;
            }
            set { genes = value; }
        }
        public StylishSet GetHair(string modName)
        {
            if (modName == null || !Calc.hairs.ContainsKey(modName)) return null;
            return Calc.hairs[modName];
        }
        public StylishSet GetGene(string modName)
        {
            if (modName == null || !Calc.genes.ContainsKey(modName)) return null;
            return Calc.genes[modName];
        }
        public void SetChangeBodies()
        {
            if (AlienDef == null)
            {
                Def = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.defName == (raceName ?? key)).FirstOrDefault();
            }
            if (!changeBodyRaceName.NullOrEmpty())
            {
                if (changeBodyRaceName.Length > 7 && changeBodyRaceName.Substring(0, 7) == "Stylish")
                {
                    changer = new StylishBodyChanger(changeBodyRaceName.Substring(7));
                    if (IsPawn)
                    {
                        if (changeBodyTypeName.NullOrEmpty())
                        {
                            ResetBodyChanger();
                        }
                        else
                        {
                            changer.bodyType = StylishBodyChanger.ContainBodyTypes(changeBodyRaceName.Substring(7)).Where(x => x.defName == changeBodyTypeName).FirstOrDefault();
                            if (changer.bodyType == null) ResetBodyChanger();
                        }
                    }
                }
                else
                {
                    foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.defName == changeBodyRaceName))
                    {
                        changer = new StylishBodyChanger(def as ThingDef_AlienRace);
                        if (IsPawn && !changeBodyRaceName.NullOrEmpty())
                        {
                            if (changeBodyTypeName.NullOrEmpty())
                            {
                                ResetBodyChanger();
                            }
                            else
                            {
                                changer.bodyType = (def as ThingDef_AlienRace).alienRace.generalSettings.alienPartGenerator.bodyTypes.Where(x => x.defName == changeBodyTypeName).FirstOrDefault();
                            }
                        }
                    }
                    if (changer == null) return;
                }
                //changer.shaderType = StylishBodyChanger.Shaders.Where(x => x.defName == changeShaderName).FirstOrDefault();
                changer.changeColor = adjustChangeBodyColor;

            }
            else if (!ChangeBodyRaceName.NullOrEmpty())
            {
                Get(raceName)?.SetChangeBodies();
            }
            else
            {
                ResetBodyChanger();
            }
        }
        public void ResetBodyChanger(bool resetValue = false)
        {
            changeBodyRaceName = null;
            changeBodyTypeName = null;
            //changeShaderName = null;
            changer = null;
            if (resetValue)
            {
                adjustChangeBodySize = Vector3.one;
                //adjustChangeBodyOffset = Vector3.zero;
            }
        }
        public void CalculateValue(bool force = false)
        {
            if (!force && !loadedScale)
            {
                LoadScale();
            }
            if (loadedScale)
            {
                calc = new StylishCalculator(this);
                ClearApparelCache();
            }
        }

        public void ApplyValue()
        {
            ApplyLargestValue();
            CalculateValue();
            if (disableCache)
            {
                StylishAtlasManager.AddDisableCacheByID(key);
            }
            else
            {
                StylishAtlasManager.RemoveDisableCache(key);
            }
        }

        public void LoadScale()
        {
            foreach (ThingDef d in DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.defName == (raceName ?? key)))
            {
                LoadScale(d);
                return;
            }
        }

        public void LoadScale(ThingDef d)
        {
            AlienPartGenerator apg = (d as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator;
            raceBorderScale = apg?.borderScale ?? 1f;
            raceAtlasScale = apg?.atlasScale ?? 1;
            raceDrawSize = apg?.customDrawSize ?? Vector2.one;
            raceHeadDrawSize = apg?.customHeadDrawSize ?? Vector2.one;
            racePortraitDrawSize = StylishModSettings.commonizationPortrait ? raceDrawSize : (apg?.customPortraitDrawSize ?? Vector2.one);
            racePortraitHeadDrawSize = StylishModSettings.commonizationPortrait ? raceHeadDrawSize : (apg?.customPortraitHeadDrawSize ?? Vector2.one);
            if (Find.Maps != null)
            {
                loadedScale = true;
            }
        }

        public void ApplyLargestValue()
        {
            ApplyLargestBodyValue();
            ApplyLargestHeadValue();
            largestValue = Math.Max(largestBodyValue, largestHeadValue);
        }
        private void ApplyLargestBodyValue() => largestBodyValue = Math.Max(AdjustBodySizeX, AdjustBodySizeY);
        private void ApplyLargestHeadValue() => largestHeadValue = Math.Max(Math.Max(AdjustHeadSizeX, AdjustHeadSizeY), AdjustHeadSizeZ);

        public PawnStyleSet Copy()
        {
            PawnStyleSet temp = new PawnStyleSet
            {
                adjustBodySize = adjustBodySize,
                adjustHeadSize = adjustHeadSize,
                adjustHeadOffset = adjustHeadOffset,
                adjustHeadOffsetVerticalDiff = adjustHeadOffsetVerticalDiff,
                adjustPortraitRotation = adjustPortraitRotation,
                adjustPortraitSize = adjustPortraitSize,
                adjustPortraitOffset = adjustPortraitOffset,
                adjustPortraitDisableHeadGear = adjustPortraitDisableHeadGear,
                adjustPortraitDisableClothes = adjustPortraitDisableClothes,
                adjustPortraitAngle = adjustPortraitAngle,

                hairs = CopyHairs(),
                equipments = CopyEquipments(),
                apparels = CopyApparels(),
                genes = CopyGenes()
            };
            return temp;
        }
        public void PasteBy(PawnStyleSet cache)
        {
            if (cache == null) return;
            adjustBodySize = cache.adjustBodySize;
            adjustHeadSize = cache.adjustHeadSize;
            adjustHeadOffset = cache.adjustHeadOffset;
            adjustHeadOffsetVerticalDiff = cache.adjustHeadOffsetVerticalDiff;
            adjustPortraitRotation = cache.adjustPortraitRotation;
            adjustPortraitSize = cache.adjustPortraitSize;
            adjustPortraitOffset = cache.adjustPortraitOffset;
            adjustPortraitDisableHeadGear = cache.adjustPortraitDisableHeadGear;
            adjustPortraitDisableClothes = cache.adjustPortraitDisableClothes;
            adjustPortraitAngle = cache.adjustPortraitAngle;

            hairs = cache.CopyHairs();
            equipments = cache.CopyEquipments();
            apparels = cache.CopyApparels();
            genes = cache.CopyGenes();
        }
        public Dictionary<string, StylishSet> CopyHairs() => CopyDict(hairs);
        public Dictionary<string, StylishSet> CopyEquipments() => CopyDict(equipments);
        public Dictionary<string, ApparelDict> CopyApparels() => CopyApparels(apparels);
        public Dictionary<string, StylishSet> CopyGenes() => CopyDict(genes);
        private Dictionary<string, StylishSet> CopyDict(Dictionary<string, StylishSet> dict)
        {
            if (dict == null) return null;
            Dictionary<string, StylishSet> temp = new Dictionary<string, StylishSet>();
            foreach (KeyValuePair<string, StylishSet> pair in dict) { temp.Add(pair.Key, pair.Value.Clone); }
            return temp;
        }
        private Dictionary<string, ApparelDict> CopyApparels(Dictionary<string, ApparelDict> dict)
        {
            if (dict == null) return null;
            Dictionary<string, ApparelDict> temp = new Dictionary<string, ApparelDict>();
            foreach (KeyValuePair<string, ApparelDict> pair in dict)
            {
                if (pair.Value == null) continue;
                ApparelDict tempA = new ApparelDict();
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    tempA.Add(pair.Value.Keys[i], pair.Value.Values[i].Clone);
                }
                temp.Add(pair.Key, tempA);
            }
            return temp;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref key, "key", null);
            Scribe_Values.Look<string>(ref name, "name", null);
            Scribe_Values.Look<string>(ref raceName, "raceName", null);
            Scribe_Values.Look<string>(ref changeBodyRaceName, nameof(changeBodyRaceName), null);
            Scribe_Values.Look<string>(ref changeBodyTypeName, nameof(changeBodyTypeName), null);
            //Scribe_Values.Look<string>(ref changeShaderName, nameof(changeShaderName), null);
            Scribe_Values.Look<string>(ref defaultBodyTypeDefName, nameof(defaultBodyTypeDefName), string.Empty);
            if (StylishModSettings.ModVersion < 1.0f)
            {
                EarlierVer1_0();
            }
            else
            {
                Scribe_Values.Look<float>(ref adjustBodySize.x, "bodySizeX", 1f);
                Scribe_Values.Look<float>(ref adjustBodySize.y, "bodySizeY", 1f);
                Scribe_Values.Look<float>(ref adjustBodySize.z, "bodySizeZ", 1f);
                Scribe_Values.Look<float>(ref adjustHeadSize.x, "headSizeX", 1f);
                Scribe_Values.Look<float>(ref adjustHeadSize.y, "headSizeY", 1f);
                Scribe_Values.Look<float>(ref adjustHeadSize.z, "headSizeZ", 1f);
                Scribe_Values.Look<float>(ref adjustHeadOffset.x, "headOffsetX", 0f);
                Scribe_Values.Look<float>(ref adjustHeadOffset.y, "headOffsetY", 0f);
                Scribe_Values.Look<float>(ref adjustHeadOffsetVerticalDiff.x, "headOffsetVerticalDiffSide", 0f);
                Scribe_Values.Look<float>(ref adjustHeadOffsetVerticalDiff.y, "headOffsetVerticalDiffBack", 0f);
                Scribe_Values.Look<float>(ref adjustBodyAddonSize.x, "bodyAddonSizeX", 1f);
                Scribe_Values.Look<float>(ref adjustBodyAddonSize.y, "bodyAddonSizeY", 1f);
                Scribe_Values.Look<float>(ref adjustBodyAddonSize.z, "bodyAddonSizeZ", 1f);
                Scribe_Values.Look<float>(ref adjustHeadAddonSize.x, "headAddonSizeX", 1f);
                Scribe_Values.Look<float>(ref adjustHeadAddonSize.y, "headAddonSizeY", 1f);
                Scribe_Values.Look<float>(ref adjustHeadAddonSize.z, "headAddonSizeZ", 1f);
                Scribe_Values.Look<float>(ref adjustBodyAddonOffset.x, "bodyAddonOffsetX", 0f);
                Scribe_Values.Look<float>(ref adjustBodyAddonOffset.y, "bodyAddonOffsetY", 0f);
                Scribe_Values.Look<float>(ref adjustHeadAddonOffset.x, "headAddonOffsetX", 0f);
                Scribe_Values.Look<float>(ref adjustHeadAddonOffset.y, "headAddonOffsetY", 0f);
            }
            if (StylishModSettings.ModVersion < 4.1f)
            {
                Scribe_Values.Look<int>(ref resolution, "resolutionLimitMin", 2);
            }
            else
            {
                Scribe_Values.Look<int>(ref resolution, "resolution", 0);
            }
            Scribe_Values.Look<float>(ref adjustDrawLoc.x, "adjustDrawLocX", 0f);
            Scribe_Values.Look<float>(ref adjustDrawLoc.y, "adjustDrawLocY", 0f);
            Scribe_Values.Look<float>(ref adjustChangeBodyOffset.x, "adjustChangeBodyOffsetX", 0f);
            Scribe_Values.Look<float>(ref adjustChangeBodyOffset.y, "adjustChangeBodyOffsetY", 0f);
            Scribe_Values.Look<float>(ref adjustChangeBodyOffset.z, "adjustChangeBodyOffsetZ", 0f);
            Scribe_Values.Look<float>(ref adjustChangeBodySize.x, "adjustChangeBodySizeX", 1f);
            Scribe_Values.Look<float>(ref adjustChangeBodySize.y, "adjustChangeBodySizeY", 1f);
            Scribe_Values.Look<float>(ref adjustChangeBodySize.z, "adjustChangeBodySizeZ", 1f);
            Scribe_Values.Look<float>(ref adjustChangeBodyAngle.x, "adjustChangeBodyAngleX", 0f);
            Scribe_Values.Look<float>(ref adjustChangeBodyAngle.y, "adjustChangeBodyAngleY", 0f);
            Scribe_Values.Look<float>(ref adjustChangeBodyColor.x, "adjustChangeBodyHue", 0f);
            Scribe_Values.Look<float>(ref adjustChangeBodyColor.y, "adjustChangeBodySaturation", 0f);
            Scribe_Values.Look<float>(ref adjustChangeBodyColor.z, "adjustChangeBodyValue", 0f);
            Scribe_Values.Look<float>(ref adjustPortraitOffset.x, "portraitOffseX", 0f);
            Scribe_Values.Look<float>(ref adjustPortraitOffset.y, "portraitOffseY", 0f);
            Scribe_Values.Look<float>(ref adjustPortraitSize, "portraitSize", 100f);
            Scribe_Values.Look<int>(ref adjustPortraitRotation, "portraitRotation", 2);
            Scribe_Values.Look<float>(ref adjustPortraitAngle, "portraitAngle", 0f);
            Scribe_Values.Look<float>(ref adjustPortraitHeadOffsetMult, "adjustPortraitHeadOffsetMult", 100);
            Scribe_Values.Look<bool>(ref adjustPortraitDisableHeadGear, nameof(adjustPortraitDisableHeadGear), false);
            Scribe_Values.Look<bool>(ref adjustPortraitDisableClothes, nameof(adjustPortraitDisableClothes), false);
            Scribe_Values.Look<int>(ref cacheSize, nameof(cacheSize), 6);
            Scribe_Values.Look<float>(ref borderScaleMult, nameof(borderScaleMult), 1f);
            Scribe_Values.Look<int>(ref cacheWidth, nameof(cacheWidth), 16);
            Scribe_Values.Look<bool>(ref ignoreRaceRestriction, nameof(ignoreRaceRestriction), false);
            Scribe_Values.Look<bool>(ref leaveOneCache, nameof(leaveOneCache), false);
            Scribe_Values.Look<bool>(ref disableUtilityAdjust, nameof(disableUtilityAdjust), false);
            Scribe_Values.Look<bool>(ref disableCache, nameof(disableCache), false);
            Scribe_Values.Look<float>(ref largestValue, "largestValue", 1f);
            Scribe_Values.Look<float>(ref largestBodyValue, "largestBodyValue", 1f);
            Scribe_Values.Look<float>(ref largestHeadValue, "largestHeadValue", 1f);
            Scribe_Values.Look<bool>(ref showHair, "showHair", false);
            Scribe_Values.Look<Vector2>(ref raceDrawSize, "raceDrawSize", Vector2.one);
            Scribe_Values.Look<Vector2>(ref raceHeadDrawSize, "raceHeadDrawSize", Vector2.one);
            Scribe_Values.Look<Vector2>(ref racePortraitDrawSize, "racePortraitDrawSize", Vector2.one);
            Scribe_Values.Look<Vector2>(ref racePortraitHeadDrawSize, "racePortraitHeadDrawSize", Vector2.one);

            Scribe_Collections.Look(ref equipments, "equipments", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref hairs, "hairs", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref apparels, nameof(apparels), LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref genes, "genes", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref addons, nameof(addons), LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref corrections, nameof(corrections), LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref drawLocCorrections, nameof(drawLocCorrections), LookMode.Value, LookMode.Value);

        }

        private void EarlierVer1_0()
        {
            Scribe_Values.Look<Vector3>(ref adjustBodySize, "bodySize", Vector3.one);
            Scribe_Values.Look<Vector3>(ref adjustHeadSize, "headSize", Vector3.one);
            Scribe_Values.Look<Vector3>(ref adjustBodyAddonSize, "bodyAddonSize", Vector3.one);
            Scribe_Values.Look<Vector3>(ref adjustHeadAddonSize, "headAddonSize", Vector3.one);
            Scribe_Values.Look<Vector2>(ref adjustHeadOffset, "headOffset", Vector2.zero);
            Scribe_Values.Look<Vector2>(ref adjustBodyAddonOffset, "bodyAddonOffset", Vector2.zero);
            Scribe_Values.Look<Vector2>(ref adjustHeadAddonOffset, "headAddonOffset", Vector2.zero);
            Scribe_Values.Look<Vector2>(ref adjustPortraitOffset, "portraitOffset", Vector2.zero);
        }

    }
}
