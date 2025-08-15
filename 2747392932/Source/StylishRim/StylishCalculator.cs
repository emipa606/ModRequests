using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace StylishRim
{
    public class StylishCalculator
    {

        public Dictionary<string, StylishSet> hairs = new Dictionary<string, StylishSet>();
        public Dictionary<string, StylishSet> equipments = new Dictionary<string, StylishSet>();
        public Dictionary<string, ApparelDict> apparels = new Dictionary<string, ApparelDict>();
        public Dictionary<string, StylishSet> genes = new Dictionary<string, StylishSet>();
        public Dictionary<string, Dictionary<int, StylishAddon>> addons = new Dictionary<string, Dictionary<int, StylishAddon>>();
        public bool HasAddons(string bodyType)
        {
            return addons != null && addons.ContainsKey(bodyType);
        }
        public Dictionary<int, StylishAddon> GetAddons(string bodyType)
        {
            if (addons == null || addons.Count == 0) return null;
            if (addons.ContainsKey(bodyType))
            {
                return addons[bodyType];
            }
            return addons.FirstOrDefault().Value;
        }

        public PawnStyleSet pss;


        public float largestValue = 1f;
        public float largestHeadValue = 1f;
        public float largestBodyValue = 1f;

        public float maxHeadSize = 1f;
        public float maxBodySize = 1f;
        public float minHeadSize = 1f;
        public float minBodySize = 1f;

        public int atlasScale = 1;
        public float borderScale = 1f;
        public float drawScale = 1f;
        public float calcScale = 1f;
        public int cacheX = 8;
        public int cacheY = 8;

        public float headOffsetDiff;
        public float portraitHeadOffsetDiff;

        public Vector3 multHeadSize = Vector3.one;
        public Vector3 multBodySize = Vector3.one;
        public Vector3 multHeadAddonSize = Vector3.one;
        public Vector3 multBodyAddonSize = Vector3.one;

        public Vector3 multHeadOffset = Vector3.one;
        public Vector3 multBodyOffset = Vector3.one;
        public Vector3 multHeadAddonOffset = Vector3.one;
        public Vector3 multBodyAddonOffset = Vector3.one;
        public Vector3 multPortraitHeadOffset = Vector3.one;
        public Vector3 multPortraitBodyOffset = Vector3.one;
        public Vector3 multPortraitHeadAddonOffset = Vector3.one;
        public Vector3 multPortraitBodyAddonOffset = Vector3.one;


        public Vector3 addHeadOffset = Vector3.zero;
        public Vector3 addBodyOffset = Vector3.zero;
        public Vector3 addHeadAddonOffset = Vector3.zero;
        public Vector3 addBodyAddonOffset = Vector3.zero;
        public Vector3 addPortraitHeadOffset = Vector3.zero;
        public Vector3 addPortraitBodyOffset = Vector3.zero;
        public Vector3 addPortraitHeadAddonOffset = Vector3.zero;
        public Vector3 addPortraitBodyAddonOffset = Vector3.zero;

        public float multPortraitSize = 1f;
        public Vector3 multPortraitOffset = Vector3.one;
        public Vector3 addPortraitOffset = Vector3.zero;

        public Vector3 multUtilitySize = Vector3.one;
        public Vector3 multUtilityOffset = Vector3.one;

        public Vector3 changeBodySize = Vector3.one;
        public Vector3 changeBodyOffset = Vector3.zero;
        public Vector2 changeBodyAngle = Vector2.zero;

        public Dictionary<string, BodyTypeCorrection> bodyTypeCorrections;

        public Vector3 AddHeadOffset;
        public Vector3 AddHeadOffsetNorth = Vector3.zero;
        public Vector3 AddHeadOffsetEast = Vector3.zero;
        public Vector3 AddHeadOffsetSouth = Vector3.zero;
        public Vector3 AddHeadOffsetWest = Vector3.zero;

        public Vector3 AddPortraitHeadOffset;
        public Vector3 AddPortraitHeadOffsetNorth = Vector3.zero;
        public Vector3 AddPortraitHeadOffsetEast = Vector3.zero;
        public Vector3 AddPortraitHeadOffsetSouth = Vector3.zero;
        public Vector3 AddPortraitHeadOffsetWest = Vector3.zero;
        private Vector3 SizeVertical(Vector3 v3) => new Vector3(v3.x, 1f, v3.y);
        private Vector3 SizeHorizontal(Vector3 v3) => new Vector3(v3.z, 1f, v3.y);

        private Vector3 HeadSizeVertical = Vector3.one;
        private Vector3 HeadSizeHorizontal = Vector3.one;
        private Vector3 BodySizeVertical = Vector3.one;
        private Vector3 BodySizeHorizontal = Vector3.one;
        private Vector3 HeadAddonSizeVertical = Vector3.one;
        private Vector3 HeadAddonSizeHorisontal = Vector3.one;
        private Vector3 BodyAddonSizeVertical = Vector3.one;
        private Vector3 BodyAddonSizeHorisontal = Vector3.one;
        private Vector3 ChangeBodySizeVertical = Vector3.one;
        private Vector3 ChangeBodySizeHorisontal = Vector3.one;
        private Vector3 ChangeBodyOffsetVertical = Vector3.zero;
        private Vector3 ChangeBodyOffsetEast = Vector3.zero;
        private Vector3 ChangeBodyOffsetWest = Vector3.zero;
        private float ChangeBodyAngleNorth;
        private float ChangeBodyAngleEast;
        private float ChangeBodyAngleSouth;
        private float ChangeBodyAngleWest;
        public Vector3 AddHeadOffsetByRot(Rot4 rot, bool isPortrait, string bodyType = null)
        {
            if (CorrectionHasBodyType(bodyType))
            {
                BodyTypeCorrection btc = GetBodyTypeCorrection(bodyType);
                switch (rot.AsInt)
                {
                    case 0: return isPortrait ? btc.AddPortraitHeadOffsetNorth : btc.AddHeadOffsetNorth;
                    case 1: return isPortrait ? btc.AddPortraitHeadOffsetEast : btc.AddHeadOffsetEast;
                    case 2: return isPortrait ? btc.AddPortraitHeadOffsetSouth : btc.AddHeadOffsetSouth;
                    default: return isPortrait ? btc.AddPortraitHeadOffsetWest : btc.AddHeadOffsetWest;
                }
            }
            else
            {
                switch (rot.AsInt)
                {
                    case 0: return isPortrait ? AddPortraitHeadOffsetNorth : AddHeadOffsetNorth;
                    case 1: return isPortrait ? AddPortraitHeadOffsetEast : AddHeadOffsetEast;
                    case 2: return isPortrait ? AddPortraitHeadOffsetSouth : AddHeadOffsetSouth;
                    default: return isPortrait ? AddPortraitHeadOffsetWest : AddHeadOffsetWest;
                }
            }
        }

        public bool CorrectionHasBodyType(string bodyType)
        {
            return bodyTypeCorrections != null && bodyTypeCorrections.ContainsKey(bodyType);
        }
        public BodyTypeCorrection GetBodyTypeCorrection(string bodyType)
        {
            return bodyTypeCorrections[bodyType];
        }
        public struct BodyTypeCorrection
        {
            public BodyTypeCorrection(StylishCalculator c, Vector3 correction, Vector3 drawLocCorrection)
            {
                AddHeadOffset = c.AddHeadOffset;
                AddHeadOffsetNorth = c.AddHeadOffsetNorth;
                AddHeadOffsetEast = c.AddHeadOffsetEast;
                AddHeadOffsetSouth = c.AddHeadOffsetSouth;
                AddHeadOffsetWest = c.AddHeadOffsetWest;

                AddPortraitHeadOffset = c.AddPortraitHeadOffset;
                AddPortraitHeadOffsetNorth = c.AddPortraitHeadOffsetNorth;
                AddPortraitHeadOffsetEast = c.AddPortraitHeadOffsetEast;
                AddPortraitHeadOffsetSouth = c.AddPortraitHeadOffsetSouth;
                AddPortraitHeadOffsetWest = c.AddPortraitHeadOffsetWest;

                correction /= 1000;
                AddHeadOffset.z += correction.y;
                AddHeadOffsetNorth.z += correction.y;
                AddHeadOffsetEast.z += correction.y;
                AddHeadOffsetSouth.z += correction.y;
                AddHeadOffsetWest.z += correction.y;
                AddHeadOffsetEast.x += correction.z;
                AddHeadOffsetWest.x += -correction.z;

                AddPortraitHeadOffset.z += correction.y;
                AddPortraitHeadOffsetNorth.z += correction.y;
                AddPortraitHeadOffsetEast.z += correction.y;
                AddPortraitHeadOffsetSouth.z += correction.y;
                AddPortraitHeadOffsetWest.z += correction.y;
                AddPortraitHeadOffsetEast.x += correction.z;
                AddPortraitHeadOffsetWest.x += -correction.z;

                drawLocCorrection /= 1000;
                Vector3 bodyOffset = c.addBodyOffset;
                bodyOffset.z += drawLocCorrection.y;
                AddBodyOffsetNorth = AddBodyOffsetEast = AddBodyOffsetSouth = AddBodyOffsetWest = bodyOffset;
                AddBodyOffsetEast.x = drawLocCorrection.z;
                AddBodyOffsetWest.x = -drawLocCorrection.z;
            }
            public Vector3 AddHeadOffset;
            public Vector3 AddPortraitHeadOffset;
            public Vector3 AddHeadOffsetNorth;
            public Vector3 AddHeadOffsetEast;
            public Vector3 AddHeadOffsetSouth;
            public Vector3 AddHeadOffsetWest;
            public Vector3 AddPortraitHeadOffsetNorth;
            public Vector3 AddPortraitHeadOffsetEast;
            public Vector3 AddPortraitHeadOffsetSouth;
            public Vector3 AddPortraitHeadOffsetWest;

            public Vector3 AddBodyOffsetNorth;
            public Vector3 AddBodyOffsetEast;
            public Vector3 AddBodyOffsetSouth;
            public Vector3 AddBodyOffsetWest;

            public Vector3 BodyOffsetByRot(Rot4 rot)
            {
                switch (rot.AsInt)
                {
                    case 0:
                        return AddBodyOffsetNorth;
                    case 1:
                        return AddBodyOffsetEast;
                    case 2:
                        return AddBodyOffsetSouth;
                    default:
                        return AddBodyOffsetWest;
                }
            }
            public void AddOffset(Vector2 drawLoc)
            {
                drawLoc /= 1000;
                AddBodyOffsetNorth.z += drawLoc.y;
                AddBodyOffsetSouth.z += drawLoc.y;
                AddBodyOffsetEast.z += drawLoc.y;
                AddBodyOffsetEast.x += drawLoc.x;
                AddBodyOffsetWest.z += drawLoc.y;
                AddBodyOffsetWest.x += -drawLoc.x;
            }
        }
        public Vector3 HeadSizeByRot(Rot4 rot)
        {
            return !rot.IsHorizontal ? HeadSizeVertical : HeadSizeHorizontal;
        }
        public Vector3 BodySizeByRot(Rot4 rot)
        {
            return !rot.IsHorizontal ? BodySizeVertical : BodySizeHorizontal;
        }
        public Vector3 HeadAddonSizeByRot(Rot4 rot)
        {
            return !rot.IsHorizontal ? HeadAddonSizeVertical : HeadAddonSizeHorisontal;
        }
        public Vector3 BodyAddonSizeByRot(Rot4 rot)
        {
            return !rot.IsHorizontal ? BodyAddonSizeVertical : BodyAddonSizeHorisontal;
        }
        public Vector3 ChangeBodySizeByRot(Rot4 rot)
        {
            return !rot.IsHorizontal ? ChangeBodySizeVertical : ChangeBodySizeHorisontal;
        }
        public Vector3 ChangeBodyOffsetByRot(Rot4 rot)
        {
            switch (rot.AsInt)
            {
                case 1: return ChangeBodyOffsetEast;
                case 3: return ChangeBodyOffsetWest;
                default: return ChangeBodyOffsetVertical;
            }
        }
        public Quaternion ChangeBodyAngleQuatByRot(Rot4 rot)
        {
            return Quaternion.AngleAxis(rot == Rot4.North ? ChangeBodyAngleNorth : rot == Rot4.East ? ChangeBodyAngleEast : rot == Rot4.South ? ChangeBodyAngleSouth : ChangeBodyAngleWest, Vector3.up);
        }

        // 設定適用時にスタイリッシュにあらかじめ計算しておくやつ
        // 描画ごとに計算は動かすだけならいいけどさすがに頭がおかしい
        public StylishCalculator(PawnStyleSet pss)
        {
            if (pss == null) return;
            this.pss = pss;
            if (pss.IsPawn) pss.racePss = PawnStyleSet.Get(pss.raceName);
            if (StylishModSettings.includeFacialAnimation)
            {
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.defName == (pss.raceName ?? pss.key)))
                {
                    foreach (CompProperties comp in def.comps.Where(x => x.compClass.Name == "DrawFaceGraphicsComp"))
                    {
                        pss.useFaceAnim = true;
                    }
                }
            }

            //pss.SetChangeBodies();

            Calculate();
            CalculateHairStyleSet();
            CalculateApparelStyleSet();
            CalculateEquipmentStyleSet();
            CalculateGeneStyleSet();
            CalculateAddons();

            StylishAdjuster.AddAtlasScaleRecord(pss.raceName ?? pss.key, atlasScale);
            StylishAdjuster.AddBorderScaleRecord(pss.raceName ?? pss.key, borderScale);
        }
        public virtual void Calculate()
        {
            atlasScale = pss.Resolution == 0 ? (int)Math.Min(Math.Round(pss.LargestValue * pss.raceAtlasScale, MidpointRounding.AwayFromZero), 8) : pss.Resolution;


            multHeadSize = pss.AdjustHeadSize;
            multBodySize = pss.AdjustBodySize;
            multHeadAddonSize = pss.AdjustHeadAddonSize;
            multBodyAddonSize = pss.AdjustBodyAddonSize;

            largestValue = pss.largestValue;
            largestHeadValue = pss.largestHeadValue;
            largestBodyValue = pss.largestBodyValue;
            borderScale = (pss.LargestHeadValue + pss.LargestBodyValue * 2) / 3 * pss.BorderScaleMult;
            drawScale = largestValue / borderScale;
            calcScale = borderScale * drawScale;


            multHeadOffset.x = multHeadAddonOffset.x = multPortraitHeadOffset.x = multPortraitHeadAddonOffset.x = multHeadSize.x;
            multHeadOffset.z = multHeadAddonOffset.z = multPortraitHeadOffset.z = multPortraitHeadAddonOffset.z = multHeadSize.z;

            multHeadOffset.y = multHeadAddonOffset.y = multBodyAddonOffset.y = multPortraitBodyAddonOffset.y = multBodySize.y;
            multPortraitHeadOffset.y = multPortraitHeadAddonOffset.y = multBodySize.y * pss.racePortraitDrawSize.y / pss.raceDrawSize.y * pss.PortraitHeadOffsetMult;

            multBodyAddonOffset.x = multBodyAddonOffset.z = multPortraitBodyAddonOffset.x = multPortraitBodyAddonOffset.z = multBodySize.x;

            multPortraitSize = 1 / pss.largestHeadValue;

            multPortraitOffset.y = 1 / multPortraitSize * pss.largestValue / largestValue;

            headOffsetDiff = (multHeadSize.y - multBodySize.y) / 4;
            portraitHeadOffsetDiff = (multHeadSize.y - multBodySize.y) / 4 * pss.racePortraitDrawSize.y / pss.raceDrawSize.y;

            addHeadOffset.x = addHeadOffset.z = pss.AdjustHeadOffsetX * pss.raceHeadDrawSize.x;
            addHeadOffset.y = pss.AdjustHeadOffsetY * pss.raceHeadDrawSize.y;

            addBodyOffset.z = (pss.AdjustBodySizeY - 1f) / 2f;

            addHeadAddonOffset.x = addHeadAddonOffset.z = pss.AdjustHeadAddonOffsetX * pss.raceHeadDrawSize.x;
            addHeadAddonOffset.y = pss.AdjustHeadAddonOffsetY * pss.raceHeadDrawSize.y / pss.raceBorderScale;

            addBodyAddonOffset.x = addBodyAddonOffset.z = pss.AdjustBodyAddonOffsetX * pss.raceDrawSize.x;
            addBodyAddonOffset.y = pss.AdjustBodyAddonOffsetY * pss.raceDrawSize.y;

            addPortraitHeadOffset.x = addPortraitHeadOffset.z = pss.AdjustHeadOffsetX * pss.racePortraitHeadDrawSize.x;
            addPortraitHeadOffset.y = pss.AdjustHeadOffsetY * pss.racePortraitHeadDrawSize.y;

            addPortraitHeadAddonOffset.x = addPortraitHeadAddonOffset.z = pss.AdjustHeadAddonOffsetX * pss.racePortraitHeadDrawSize.x;
            addPortraitHeadAddonOffset.y = pss.AdjustHeadAddonOffsetY * pss.racePortraitHeadDrawSize.y / pss.raceBorderScale;

            addPortraitBodyAddonOffset.x = addPortraitBodyAddonOffset.z = pss.AdjustBodyAddonOffsetX * pss.racePortraitDrawSize.x;
            addPortraitBodyAddonOffset.y = pss.AdjustBodyAddonOffsetY * pss.racePortraitDrawSize.y;

            addPortraitOffset.y = AddHeadOffset.y * multHeadOffset.y / pss.raceBorderScale / pss.raceDrawSize.y;

            multUtilitySize = new Vector3(pss.AdjustBodySize.y / pss.raceDrawSize.y, 0, pss.AdjustBodySize.x / pss.raceDrawSize.x) / pss.largestBodyValue;
            multUtilityOffset = new Vector3(pss.AdjustBodySizeX, 0, pss.AdjustBodySizeY);

            multHeadAddonSize.Scale(multHeadSize);
            multBodyAddonSize.Scale(multBodySize);

            AddHeadOffset = new Vector3(addHeadOffset.x, addHeadOffset.y + headOffsetDiff, addHeadOffset.z);
            AddHeadOffsetNorth = new Vector3(0, 0, addHeadOffset.y + headOffsetDiff + pss.AdjustHeadOffsetVerticalDiffBack * pss.raceHeadDrawSize.y);
            AddHeadOffsetEast = new Vector3(addHeadOffset.x, 0, addHeadOffset.y + headOffsetDiff + pss.AdjustHeadOffsetVerticalDiffSide * pss.raceHeadDrawSize.y);
            AddHeadOffsetSouth = new Vector3(0, 0, addHeadOffset.y + headOffsetDiff);
            AddHeadOffsetWest = new Vector3(-addHeadOffset.x, 0, addHeadOffset.y + headOffsetDiff + pss.AdjustHeadOffsetVerticalDiffSide * pss.raceHeadDrawSize.y);

            AddPortraitHeadOffset = new Vector3(addPortraitHeadOffset.x, addPortraitHeadOffset.y + portraitHeadOffsetDiff, addPortraitHeadOffset.z);
            AddPortraitHeadOffsetNorth = new Vector3(0, 0, addPortraitHeadOffset.y + portraitHeadOffsetDiff + pss.AdjustHeadOffsetVerticalDiffBack * pss.racePortraitHeadDrawSize.y);
            AddPortraitHeadOffsetEast = new Vector3(addPortraitHeadOffset.x, 0, addPortraitHeadOffset.y + portraitHeadOffsetDiff + pss.AdjustHeadOffsetVerticalDiffSide * pss.racePortraitHeadDrawSize.y);
            AddPortraitHeadOffsetSouth = new Vector3(0, 0, addPortraitHeadOffset.y + portraitHeadOffsetDiff);
            AddPortraitHeadOffsetWest = new Vector3(-addPortraitHeadOffset.x, 0, addPortraitHeadOffset.y + portraitHeadOffsetDiff + pss.AdjustHeadOffsetVerticalDiffSide * pss.racePortraitHeadDrawSize.y);

            bodyTypeCorrections = new Dictionary<string, BodyTypeCorrection>();
            List<BodyTypeDef> bd = pss.AlienDef.alienRace.generalSettings.alienPartGenerator.bodyTypes;
            if (bd.Count == 0) bd = PawnStyleSet.HumanDef.alienRace.generalSettings.alienPartGenerator.bodyTypes;
            foreach (BodyTypeDef bodyType in bd)
            {
                if (!bodyTypeCorrections.ContainsKey(bodyType.defName))
                {
                    BodyTypeCorrection btc = new BodyTypeCorrection(this, pss.GetCorrection(bodyType.defName), pss.GetDrawLocCorrection(bodyType.defName));
                    if (pss.IsPawn) btc.AddOffset(pss.adjustDrawLoc);
                    bodyTypeCorrections.Add(bodyType.defName, btc);
                }
            }

            changeBodySize = multBodySize;

            if (pss.Changer != null)
            {
                changeBodySize.Scale(pss.AdjustChangeBodySize);
                changeBodyOffset = pss.AdjustChangeBodyOffset;
                changeBodyAngle = pss.AdjustChangeBodyAngle;
                changeBodyAngle.x %= 360;
                changeBodyAngle.y %= 360;
                ChangeBodyAngleNorth = 360 - changeBodyAngle.x;
                ChangeBodyAngleEast = changeBodyAngle.y;
                ChangeBodyAngleSouth = changeBodyAngle.x;
                ChangeBodyAngleWest = 360 - changeBodyAngle.y;
            }

            HeadSizeVertical = SizeVertical(multHeadSize);
            HeadSizeHorizontal = SizeHorizontal(multHeadSize);
            BodySizeVertical = SizeVertical(multBodySize);
            BodySizeHorizontal = SizeHorizontal(multBodySize);
            HeadAddonSizeVertical = SizeVertical(multHeadAddonSize);
            HeadAddonSizeHorisontal = SizeHorizontal(multHeadAddonSize);
            BodyAddonSizeVertical = SizeVertical(multBodyAddonSize);
            BodyAddonSizeHorisontal = SizeHorizontal(multBodyAddonSize);
            ChangeBodySizeVertical = SizeVertical(changeBodySize);
            ChangeBodySizeHorisontal = SizeHorizontal(changeBodySize);
            ChangeBodyOffsetVertical = new Vector3(changeBodyOffset.x, 0, changeBodyOffset.y);
            ChangeBodyOffsetEast = new Vector3(changeBodyOffset.z, 0, changeBodyOffset.y);
            ChangeBodyOffsetWest = new Vector3(-changeBodyOffset.z, 0, changeBodyOffset.y);
        }

        public void CalculateHairStyleSet()
        {
            PawnStyleSet rpss = PawnStyleSet.Get(pss.raceName);
            List<string> modList = new List<string>();
            if (rpss == null)
            {
                if (pss.Hairs == null) return;
                foreach (string key in pss.Hairs.Select(x => x.Key))
                {
                    if (!modList.Contains(key)) modList.Add(key);
                }
                foreach (string key in modList)
                {
                    hairs.Add(key, pss.Hairs[key].Clone);
                }
            }
            else
            {
                if (pss.Hairs == null)
                {
                    if (rpss.Hairs == null) return;
                    foreach (string key in rpss.Hairs.Select(x => x.Key))
                    {
                        if (!modList.Contains(key)) modList.Add(key);
                    }
                }
                else
                {
                    foreach (string key in pss.Hairs.Concat(rpss.Hairs).Select(x => x.Key))
                    {
                        if (!modList.Contains(key)) modList.Add(key);
                    }
                }
                foreach (string key in modList)
                {
                    if (pss.Hairs.ContainsKey(key))
                    {
                        if (rpss.Hairs.ContainsKey(key))
                        {
                            hairs.Add(key, pss.Hairs[key].Combine(rpss.Hairs[key]));
                        }
                        else
                        {
                            hairs.Add(key, pss.Hairs[key].Clone);
                        }
                    }
                    else
                    {
                        hairs.Add(key, rpss.Hairs[key].Clone);
                    }
                }
            }
            foreach (StylishSet ss in hairs.Values)
            {
                ss.size.Scale(multHeadSize);
                ss.Calculate();
            }
        }
        public void CalculateGeneStyleSet()
        {
            PawnStyleSet rpss = PawnStyleSet.Get(pss.raceName);
            List<string> modList = new List<string>();
            if (rpss == null)
            {
                if (pss.Genes == null) return;
                foreach (string key in pss.Genes.Select(x => x.Key))
                {
                    if (!modList.Contains(key)) modList.Add(key);
                }
                foreach (string key in modList)
                {
                    genes.Add(key, pss.Genes[key].Clone);
                }
            }
            else
            {
                if (pss.Genes == null)
                {
                    if (rpss.Genes == null) return;
                    foreach (string key in rpss.Genes.Select(x => x.Key))
                    {
                        if (!modList.Contains(key)) modList.Add(key);
                    }
                }
                else
                {
                    foreach (string key in pss.Genes.Concat(rpss.Genes).Select(x => x.Key))
                    {
                        if (!modList.Contains(key)) modList.Add(key);
                    }
                }
                foreach (string key in modList)
                {
                    if (pss.Genes.ContainsKey(key))
                    {
                        if (rpss.Genes.ContainsKey(key))
                        {
                            genes.Add(key, pss.Genes[key].Combine(rpss.Genes[key]));
                        }
                        else
                        {
                            genes.Add(key, pss.Genes[key].Clone);
                        }
                    }
                    else
                    {
                        genes.Add(key, rpss.Genes[key].Clone);
                    }
                }
            }
            foreach (StylishSet ss in genes.Values)
            {
                ss.Calculate();
            }
        }
        public void CalculateEquipmentStyleSet()
        {
            PawnStyleSet rpss = PawnStyleSet.Get(pss.raceName);
            List<string> keys = new List<string>();
            StylishSet common = new StylishSet(StylishRimSettings.COMMON);
            StylishSet extraPart1 = new StylishSet(StylishRimSettings.EXTRA1);
            StylishSet extraPart2 = new StylishSet(StylishRimSettings.EXTRA2);
            if (rpss == null)
            {
                if (pss.Equipments == null) return;
                if (pss.Equipments.ContainsKey(StylishRimSettings.COMMON)) common = pss.Equipments[StylishRimSettings.COMMON];
                if (pss.Equipments.ContainsKey(StylishRimSettings.EXTRA1)) extraPart1 = pss.Equipments[StylishRimSettings.EXTRA1];
                if (pss.Equipments.ContainsKey(StylishRimSettings.EXTRA2)) extraPart2 = pss.Equipments[StylishRimSettings.EXTRA2];
                foreach (string key in pss.Equipments.Select(x => x.Key))
                {
                    if (key != StylishRimSettings.COMMON && key != StylishRimSettings.EXTRA1 && key != StylishRimSettings.EXTRA2)
                    {
                        if (!keys.Contains(key)) keys.Add(key);
                    }
                }
                equipments.Add(StylishRimSettings.COMMON, common);
                equipments.Add(StylishRimSettings.EXTRA1, extraPart1);
                equipments.Add(StylishRimSettings.EXTRA2, extraPart2);
                foreach (string key in keys)
                {
                    equipments.Add(key, pss.Equipments[key].Combine(common));
                }
            }
            else
            {
                if (pss.Equipments == null)
                {
                    if (rpss.Equipments == null) return;
                    if (rpss.Equipments.ContainsKey(StylishRimSettings.COMMON)) common = rpss.Equipments[StylishRimSettings.COMMON];
                    if (rpss.Equipments.ContainsKey(StylishRimSettings.EXTRA1)) extraPart1 = rpss.Equipments[StylishRimSettings.EXTRA1];
                    if (rpss.Equipments.ContainsKey(StylishRimSettings.EXTRA2)) extraPart2 = rpss.Equipments[StylishRimSettings.EXTRA2];
                    foreach (string key in rpss.Equipments.Select(x => x.Key))
                    {
                        if (key != StylishRimSettings.COMMON && key != StylishRimSettings.EXTRA1 && key != StylishRimSettings.EXTRA2)
                        {
                            if (!keys.Contains(key)) keys.Add(key);
                        }
                    }
                }
                else
                {
                    if (pss.Equipments.ContainsKey(StylishRimSettings.COMMON)) common = pss.Equipments[StylishRimSettings.COMMON];
                    if (rpss.Equipments.ContainsKey(StylishRimSettings.COMMON)) common = common.Combine(rpss.Equipments[StylishRimSettings.COMMON]);
                    if (pss.Equipments.ContainsKey(StylishRimSettings.EXTRA1)) extraPart1 = pss.Equipments[StylishRimSettings.EXTRA1];
                    if (rpss.Equipments.ContainsKey(StylishRimSettings.EXTRA1)) extraPart1 = extraPart1.Combine(rpss.Equipments[StylishRimSettings.EXTRA1]);
                    if (pss.Equipments.ContainsKey(StylishRimSettings.EXTRA2)) extraPart2 = pss.Equipments[StylishRimSettings.EXTRA2];
                    if (rpss.Equipments.ContainsKey(StylishRimSettings.EXTRA2)) extraPart2 = extraPart2.Combine(rpss.Equipments[StylishRimSettings.EXTRA2]);
                    foreach (string key in pss.Equipments.Concat(rpss.Equipments).Select(x => x.Key))
                    {
                        if (key != StylishRimSettings.COMMON && key != StylishRimSettings.EXTRA1 && key != StylishRimSettings.EXTRA2)
                        {
                            if (!keys.Contains(key)) keys.Add(key);
                        }
                    }
                }
                equipments.Add(StylishRimSettings.COMMON, common);
                equipments.Add(StylishRimSettings.EXTRA1, extraPart1);
                equipments.Add(StylishRimSettings.EXTRA2, extraPart2);
                foreach (string key in keys)
                {
                    if (pss.Equipments.ContainsKey(key))
                    {
                        if (rpss.Equipments.ContainsKey(key))
                        {
                            equipments.Add(key, pss.Equipments[key].Combine(rpss.Equipments[key]).Combine(common));
                        }
                        else
                        {
                            equipments.Add(key, pss.Equipments[key].Combine(common));
                        }
                    }
                    else
                    {
                        equipments.Add(key, rpss.Equipments[key].Combine(common));
                    }
                }
            }
            foreach (StylishSet ss in equipments.Values)
            {
                ss.Calculate();
            }
        }

        public void CalculateApparelStyleSet()
        {
            if (apparels == null) apparels = new Dictionary<string, ApparelDict>();
            PawnStyleSet rpss = PawnStyleSet.Get(pss.raceName);
            List<string> modList = new List<string>();
            if (rpss == null)
            {
                if (pss.Apparels == null) return;
                foreach (string key in pss.Apparels.Select(x => x.Key))
                {
                    if (!modList.Contains(key)) modList.Add(key);
                }

                foreach (string key in modList)
                {
                    apparels.Add(key, pss.Apparels[key].Clone);
                }
            }
            else
            {
                if (pss.Apparels == null)
                {
                    if (rpss.Apparels == null) return;
                    foreach (string key in rpss.Apparels.Select(x => x.Key))
                    {
                        if (!modList.Contains(key)) modList.Add(key);
                    }
                }
                else
                {
                    if (rpss.Apparels == null)
                    {
                        foreach (string key in pss.Apparels.Select(x => x.Key))
                        {
                            if (!modList.Contains(key)) modList.Add(key);
                        }
                    }
                    else
                    {
                        foreach (string key in pss.Apparels.Keys.Concat(rpss.Apparels.Keys))
                        {
                            if (!modList.Contains(key)) modList.Add(key);
                        }
                    }
                }

                foreach (string key in modList)
                {
                    if (pss.Apparels == null)
                    {
                        apparels.Add(key, rpss.Apparels[key].Clone);
                    }
                    else
                    {
                        if (rpss.Apparels == null)
                        {
                            apparels.Add(key, pss.Apparels[key].Clone);
                        }
                        else
                        {
                            if (!pss.Apparels.ContainsKey(key))
                            {
                                apparels.Add(key, rpss.Apparels[key].Clone);
                            }
                            else
                            {
                                if (!rpss.Apparels.ContainsKey(key))
                                {
                                    apparels.Add(key, pss.Apparels[key].Clone);
                                }
                                else
                                {
                                    foreach (KeyValuePair<string, StylishSet> p in pss.Apparels[key].Dict)
                                    {
                                        if (!rpss.Apparels[key].ContainsKey(p.Key))
                                        {
                                            if (!apparels.ContainsKey(key))
                                            {
                                                apparels.Add(key, new ApparelDict(p.Key, p.Value.Clone));
                                            }
                                            else
                                            {
                                                apparels[key].Add(p.Key, p.Value.Clone);
                                            }
                                        }
                                        else
                                        {
                                            if (!apparels.ContainsKey(key))
                                            {
                                                apparels.Add(key, new ApparelDict(p.Key, p.Value.Combine(rpss.Apparels[key][p.Key])));
                                            }
                                            else
                                            {
                                                apparels[key].Add(p.Key, p.Value.Combine(rpss.Apparels[key][p.Key]));
                                            }
                                        }
                                    }
                                    if (!apparels.ContainsKey(key))
                                    {
                                        apparels.Add(key, rpss.Apparels[key].Clone);
                                    }
                                    else
                                    {
                                        foreach (KeyValuePair<string, StylishSet> p in rpss.Apparels[key].Dict)
                                        {
                                            if (!apparels[key].ContainsKey(p.Key))
                                            {
                                                apparels[key].Add(p.Key, p.Value.Clone);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (ApparelDict ad in apparels.Values)
            {
                bool hasCommon = false;
                StylishSet com = ad.GetCommon;
                com.size.Scale(multBodySize);
                for (int i = 0; i < ad.Count; i++)
                {
                    if (!ad.Values[i].IsBodyCommon)
                    {
                        if (!ad.Values[i].IsHeadPart)
                        {
                            ad.Values[i] = ad.Values[i].Combine(com);
                        }
                        else
                        {
                            ad.Values[i].size.Scale(multHeadSize);
                        }
                    }
                    else
                    {
                        hasCommon = true;
                    }
                    ad.Values[i].Calculate();
                }
                if (!hasCommon)
                {
                    com.Calculate();
                    ad.Add(StylishSet.BODY_COMMON, com);
                }
            }
        }

        private void CalculateAddons()
        {
            addons = new Dictionary<string, Dictionary<int, StylishAddon>>();

            List<BodyTypeDef> bd = pss.AlienDef.alienRace.generalSettings.alienPartGenerator.bodyTypes;
            if (bd.Count == 0) bd = PawnStyleSet.HumanDef.alienRace.generalSettings.alienPartGenerator.bodyTypes;
            foreach (BodyTypeDef bodyType in bd)
            {
                addons.Add(bodyType.defName, new Dictionary<int, StylishAddon>());
                if (pss.addons == null)
                {
                    if (pss.racePss == null || pss.racePss.addons == null)
                    {
                        pss.SetUpAddon();
                        foreach (KeyValuePair<int, StylishAddon> p in pss.addons)
                        {
                            addons[bodyType.defName].Add(p.Key, p.Value.Clone);
                        }
                    }
                    else
                    {
                        pss.racePss.SetUpAddon();
                        foreach (KeyValuePair<int, StylishAddon> p in pss.racePss.addons)
                        {
                            addons[bodyType.defName].Add(p.Key, p.Value.Clone);
                        }
                    }
                }
                else
                {
                    pss.SetUpAddon();
                    if (pss.racePss == null || pss.racePss.addons == null)
                    {
                        foreach (KeyValuePair<int, StylishAddon> p in pss.addons)
                        {
                            addons[bodyType.defName].Add(p.Key, p.Value.Clone);
                        }
                    }
                    else
                    {
                        pss.racePss.SetUpAddon();
                        foreach (KeyValuePair<int, StylishAddon> p in pss.addons)
                        {
                            if (pss.racePss.addons.ContainsKey(p.Key))
                            {
                                addons[bodyType.defName].Add(p.Key, p.Value.Combine(pss.racePss.addons[p.Key]));
                            }
                            else
                            {
                                addons[bodyType.defName].Add(p.Key, p.Value.Clone);
                            }
                        }
                        foreach (KeyValuePair<int, StylishAddon> p in pss.racePss.addons)
                        {
                            if (!addons[bodyType.defName].ContainsKey(p.Key))
                            {
                                addons[bodyType.defName].Add(p.Key, p.Value.Clone);
                            }
                        }
                    }
                }
                if (addons[bodyType.defName] != null)
                {
                    BodyTypeCorrection btc = bodyTypeCorrections[bodyType.defName];
                    foreach (StylishAddon addon in addons[bodyType.defName].Values)
                    {
                        if (addon.isHead)
                        {
                            addon.Calculate(multHeadAddonSize, addHeadAddonOffset + (addon.alignWithHead ? Vector3.zero : btc.AddHeadOffset), addPortraitHeadAddonOffset + (addon.alignWithHead ? Vector3.zero : btc.AddPortraitHeadOffset), pss.raceHeadDrawSize, pss.racePortraitHeadDrawSize, addon.alignWithHead ? Vector2.zero : pss.AdjustHeadOffsetVerticalDiff);
                        }
                        else
                        {
                            addon.Calculate(multBodyAddonSize, addBodyAddonOffset, addPortraitBodyAddonOffset, pss.raceDrawSize, pss.racePortraitDrawSize, Vector2.zero);
                        }
                    }
                }
            }
        }
    }
}
