using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StylishRim
{
    // 髪型、服装、武器などの調整値を一つのクラスで管理する脳筋クラス
    // 今になって思えばabstructとか継承とか、ほら。
    // 段階的に実装していったのと面倒くさがってキメラ化したのでもう手が付けられん
    // 脳筋キメラ
    public class StylishSet : IExposable
    {
        public static string BODY_COMMON = "BodyCommon";
        public static string OVERHEAD = "Overhead";
        public static string EYECOVER = "EyeCover";
        public static List<ApparelLayerDef> LayerList;
        private static void Init()
        {
            LayerList = new List<ApparelLayerDef>();
            foreach (ApparelLayerDef layer in DefDatabase<ApparelLayerDef>.AllDefs)
            {
                if (!LayerList.Contains(layer)) LayerList.Add(layer);
            }
            LayerList.SortBy(def => -def.drawOrder);
        }

        public static ApparelLayerDef LayerIs(Apparel app)
        {
            if (LayerList == null || LayerList[0] == null) Init();
            ApparelLayerDef layer = LayerIs(app.def.apparel.layers);
            if (layer == null) return app.def.apparel.LastLayer;
            return layer;
        }
        private static ApparelLayerDef LayerIs(List<ApparelLayerDef> layers)
        {
            foreach (ApparelLayerDef layer in LayerList)
            {
                if (layers.Contains(layer)) return layer;
            }
            return null;
        }


        public bool[] flipSet = new bool[4] { false, false, false, false };

        private static readonly float FIX_PRIORITY = 0.00144787634f;
        private static readonly float FIX_PRIORITY_HALF = 0.001f;
        public string key = string.Empty;
        public string layer = string.Empty;
        public string label = string.Empty;
        public Vector3 size = Vector3.one;
        public Vector4 offset = Vector4.zero;
        public Vector4 angle = Vector4.zero;
        public float priority = 0;
        public bool hide = false;
        public bool useCommonWhenAiming = false;
        public bool northInvert = false;

        public bool IsEmpty => key == string.Empty || key == string.Empty || layer == string.Empty;
        public bool IsBodyCommon => layer == BODY_COMMON;
        public bool IsHeadPart => layer == OVERHEAD || layer == EYECOVER;
        public bool FlipByRot(Rot4 rot)
        {
            return flipSet[rot.AsInt];
        }

        public float Priority = 0f;

        public void CalcDepth()
        {
            float num = 0;
            if (priority < 0)
            {
                num = -FIX_PRIORITY_HALF;
            }
            else if (priority > 0)
            {
                num = FIX_PRIORITY_HALF;
            }
            Priority = (num + FIX_PRIORITY * priority) / 2;
        }

        public Vector3 sizeNS = Vector3.one;
        public Vector3 sizeWE = Vector3.one;
        public Vector3 offsetNorth = Vector3.zero;
        public Vector3 offsetOffHandNorth = Vector3.zero;
        public Vector3 offsetNorthInvertPriority = Vector3.zero;
        public Vector3 offsetOffHandNorthInvertPriority = Vector3.zero;
        public Vector3 offsetEast = Vector3.zero;
        public Vector3 offsetEastInvertPriority = Vector3.zero;
        public Vector3 offsetSouth = Vector3.zero;
        public Vector3 offsetOffHandSouth = Vector3.zero;
        public Vector3 offsetSouthInvertPriority = Vector3.zero;
        public Vector3 offsetOffHandSouthInvertPriority = Vector3.zero;
        public Vector3 offsetWest = Vector3.zero;
        public Vector3 offsetWestInvertPriority = Vector3.zero;
        public float angleNorth = 0;
        public float angleOffHandNorth = 0;
        public float angleEast = 0;
        public float angleSouth = 0;
        public float angleOffHandSouth = 0;
        public float angleWest = 0;
        public float usageAngleNorth = 0;
        public float usageAngleOffHandNorth = 0;
        public float usageAngleEast = 0;
        public float usageAngleSouth = 0;
        public float usageAngleOffHandSouth = 0;
        public float usageAngleWest = 0;
        public void Calculate()
        {
            CalcDepth();
            sizeNS = new Vector3(size.x, 1f, size.y);
            sizeWE = new Vector3(size.z, 1f, size.y);
            offsetEast = new Vector3(offset.z, Priority, offset.w);
            offsetEastInvertPriority = new Vector3(offset.z, -Priority, offset.w);
            offsetSouth = new Vector3(offset.x, Priority, offset.y);
            offsetOffHandSouth = new Vector3(-offset.x, Priority, offset.y);
            offsetSouthInvertPriority = new Vector3(offset.x, -Priority, offset.y);
            offsetOffHandSouthInvertPriority = new Vector3(offset.x, -Priority, offset.y);
            offsetWest = new Vector3(-offset.z, Priority, offset.w);
            offsetWestInvertPriority = new Vector3(-offset.z, -Priority, offset.w);
            angleEast = angle.y;
            angleSouth = angle.x;
            angleOffHandSouth = 360 - angle.x;
            angleWest = 360 - angle.y;
            usageAngleEast = angle.w;
            usageAngleSouth = angle.z;
            usageAngleOffHandSouth = 360 - angle.z;
            usageAngleWest = 360 - angle.w;
            if (northInvert)
            {
                offsetNorth = new Vector3(-offset.x, Priority, offset.y);
                offsetNorthInvertPriority = new Vector3(-offset.x, -Priority, offset.y);
                angleNorth = 360 - angle.x;
                usageAngleNorth = 360 - angle.z;
                offsetOffHandNorth = new Vector3(offset.x, Priority, offset.y);
                offsetOffHandNorthInvertPriority = new Vector3(offset.x, -Priority, offset.y);
                angleOffHandNorth = angle.x;
                usageAngleOffHandNorth = angle.z;
            }
            else
            {
                offsetNorth = offsetSouth;
                offsetNorthInvertPriority = offsetSouthInvertPriority;
                angleNorth = angleSouth;
                usageAngleNorth = usageAngleSouth;
                offsetOffHandNorth = offsetOffHandSouth;
                offsetOffHandNorthInvertPriority = offsetOffHandSouthInvertPriority;
                angleOffHandNorth = angleOffHandSouth;
                usageAngleOffHandNorth = usageAngleOffHandSouth;
            }
        }

        public Vector3 SizeByRot(Rot4 rot)
        {
            return rot.IsHorizontal ? sizeWE : sizeNS;
        }
        public Vector3 OffsetByRot(Rot4 rot)
        {
            if (rot == Rot4.North) return offsetNorth;
            if (rot == Rot4.East) return offsetEast;
            if (rot == Rot4.South) return offsetSouth;
            return offsetWest;
        }
        public Vector3 OffsetByRotInvertPriority(Rot4 rot)
        {
            if (rot == Rot4.North) return offsetNorthInvertPriority;
            if (rot == Rot4.East) return offsetEast;
            if (rot == Rot4.South) return offsetSouth;
            return offsetWest;
        }
        public Vector3 OffsetByRotInvertPriorityEast(Rot4 rot)
        {
            if (rot == Rot4.North) return offsetNorthInvertPriority;
            if (rot == Rot4.East) return offsetEastInvertPriority;
            if (rot == Rot4.South) return offsetSouth;
            return offsetWest;
        }
        public Vector3 OffsetByRotInvertPriorityWest(Rot4 rot)
        {
            if (rot == Rot4.North) return offsetNorthInvertPriority;
            if (rot == Rot4.East) return offsetEast;
            if (rot == Rot4.South) return offsetSouth;
            return offsetWestInvertPriority;
        }
        public Vector3 OffsetByRotInvertPrioritySide(Rot4 rot)
        {
            if (rot == Rot4.North) return offsetNorthInvertPriority;
            if (rot == Rot4.East) return offsetEastInvertPriority;
            if (rot == Rot4.South) return offsetSouth;
            return offsetWestInvertPriority;
        }
        public Vector3 OffsetOffHandByRot(Rot4 rot)
        {
            if (rot == Rot4.North) return offsetOffHandNorthInvertPriority;
            if (rot == Rot4.East) return offsetEast;
            if (rot == Rot4.South) return offsetOffHandSouth;
            return offsetWest;
        }
        public float AngleByRot(Rot4 rot)
        {
            if (rot == Rot4.North) return angleNorth;
            if (rot == Rot4.East) return angleEast;
            if (rot == Rot4.South) return angleSouth;
            return angleWest;
        }
        public bool ChangePriority => priority != 0;
        public Quaternion AngleQuatByRot(Rot4 rot)
        {
            return Quaternion.AngleAxis(rot == Rot4.North ? angleNorth : rot == Rot4.East ? angleEast : rot == Rot4.South ? angleSouth : angleWest, Vector3.up);
        }
        public Quaternion AngleQuatOffHandByRot(Rot4 rot)
        {
            return Quaternion.AngleAxis(rot == Rot4.North ? angleOffHandNorth : rot == Rot4.East ? angleEast : rot == Rot4.South ? angleOffHandSouth : angleWest, Vector3.up);
        }
        public Quaternion UsageAngleQuatByRot(Rot4 rot)
        {
            return Quaternion.AngleAxis(rot == Rot4.North ? usageAngleNorth : rot == Rot4.East ? usageAngleEast : rot == Rot4.South ? usageAngleSouth : usageAngleWest, Vector3.up);
        }
        public Quaternion UsageAngleQuatOffHandByRot(Rot4 rot)
        {
            return Quaternion.AngleAxis(rot == Rot4.North ? usageAngleOffHandNorth : rot == Rot4.East ? usageAngleEast : rot == Rot4.South ? usageAngleOffHandSouth : usageAngleWest, Vector3.up);
        }

        public StylishSet() { flipSet = new bool[4] { false, false, false, false }; }
        public StylishSet(Apparel apparel)
        {
            Init(apparel.def.modContentPack.Name, LayerIs(apparel).defName, LayerIs(apparel).label.Translate());
            northInvert = true;
        }
        public StylishSet(HairDef hair)
        {
            Init(hair.modContentPack.Name, string.Empty, string.Empty);
        }
        public StylishSet(ThingWithComps eq)
        {
            Init(eq.def.defName, string.Empty, string.Empty);
        }
        public StylishSet(string key, string layer, string label)
        {
            Init(key, layer, label);
            northInvert = true;
        }
        public StylishSet(string key)
        {
            this.key = key;
        }
        public StylishSet(StylishSet ss)
        {
            Init(ss.key, ss.layer, ss.label, ss.size, ss.offset, ss.angle, ss.priority, ss.hide, ss.useCommonWhenAiming, ss.northInvert, ss.flipSet);
        }
        public StylishSet(string key, string layer, string label, Vector3 size, Vector4 offset, Vector4 angle, float depth, bool invisible, bool useCommonWhenAiming, bool northInvert, bool[] flips)
        {
            Init(key, layer, label, size, offset, angle, depth, invisible, useCommonWhenAiming, northInvert, flips);
        }
        public StylishSet(string key, string layer, string label, float sizeX, float sizeY, float sizeZ, float offsetX, float offsetY, float offsetZ, float offsetW, float angleX, float angleY, float angleZ, float angleW, float depth, bool invisible, bool useCommonWhenAiming, bool northInvert, bool[] flips)
        {
            Init(key, layer, label, sizeX, sizeY, sizeZ, offsetX, offsetY, offsetZ, offsetW, angleX, angleY, angleZ, angleW, depth, invisible, useCommonWhenAiming, northInvert, flips);
        }
        private void Init(string key, string layer, string label, Vector3 size, Vector4 offset, Vector4 angle, float depth, bool invisible, bool useCommonWhenAiming, bool northInvert, bool[] flips)
        {
            Init(key, layer, label, size.x, size.y, size.z, offset.x, offset.y, offset.z, offset.w, angle.x, angle.y, angle.z, angle.w, depth, invisible, useCommonWhenAiming, northInvert, flips);
        }
        private void Init(string key, string layer, string label, float sizeX = 1f, float sizeY = 1f, float sizeZ = 1f, float offsetX = 0f, float offsetY = 0f, float offsetZ = 0f, float offsetW = 0f, float angleX = 0f, float angleY = 0f, float angleZ = 0f, float angleW = 0f, float depth = 0f, bool invisible = false, bool useCommonWhenAiming = false, bool northInvert = false, bool[] flips = null)
        {
            this.key = key;
            this.layer = layer;
            this.label = label;
            size.x = sizeX;
            size.y = sizeY;
            size.z = sizeZ;
            offset.x = offsetX;
            offset.y = offsetY;
            offset.z = offsetZ;
            offset.w = offsetW;
            angle.x = angleX;
            angle.y = angleY;
            angle.z = angleZ;
            angle.w = angleW;
            priority = depth;
            hide = invisible;
            this.useCommonWhenAiming = useCommonWhenAiming;
            this.northInvert = northInvert;
            flipSet = new bool[4] { false, false, false, false };
            if (flips != null)
            {
                flipSet[0] = flips[0];
                flipSet[1] = flips[1];
                flipSet[2] = flips[2];
                flipSet[3] = flips[3];
            }
        }
        public StylishSet Clone => new StylishSet(this);

        public StylishSet CombineSelf(StylishSet ss)
        {
            if (ss == null) return this;
            size.Scale(ss.size);
            offset += ss.offset;
            angle.x = (angle.x + ss.angle.x) % 360f;
            angle.y = (angle.y + ss.angle.y) % 360f;
            angle.z = (angle.z + ss.angle.z) % 360f;
            angle.w = (angle.w + ss.angle.w) % 360f;
            priority += ss.priority;
            return this;
        }
        public StylishSet Combine(StylishSet ss)
        {
            return Clone.CombineSelf(ss);
        }
        public void ExposeData()
        {
            if (StylishUpdater.MOD_VERSION < 6.0)
            {
                Scribe_Values.Look<string>(ref key, "modName", string.Empty);
            }
            else
            {
                Scribe_Values.Look<string>(ref key, nameof(key), string.Empty);
            }
            Scribe_Values.Look<string>(ref layer, nameof(layer), string.Empty);
            Scribe_Values.Look<string>(ref label, nameof(label), string.Empty);
            Scribe_Values.Look<float>(ref size.x, "sizeX", 1f);
            Scribe_Values.Look<float>(ref size.y, "sizeY", 1f);
            Scribe_Values.Look<float>(ref size.z, "sizeZ", 1f);
            Scribe_Values.Look<float>(ref offset.x, "offsetX", 0f);
            Scribe_Values.Look<float>(ref offset.y, "offsetY", 0f);
            Scribe_Values.Look<float>(ref offset.z, "offsetZ", 0f);
            Scribe_Values.Look<float>(ref offset.w, "offsetW", 0f);
            Scribe_Values.Look<float>(ref angle.x, "angleX", 0f);
            Scribe_Values.Look<float>(ref angle.y, "angleY", 0f);
            Scribe_Values.Look<float>(ref angle.z, "angleZ", 0f);
            Scribe_Values.Look<float>(ref angle.w, "angleW", 0f);
            Scribe_Values.Look<float>(ref priority, "priority", 0f);
            Scribe_Values.Look<bool>(ref hide, nameof(hide), false);
            Scribe_Values.Look<bool>(ref useCommonWhenAiming, nameof(useCommonWhenAiming), false);
            Scribe_Values.Look<bool>(ref northInvert, nameof(northInvert), false);
            Scribe_Values.Look<bool>(ref flipSet[0], "flipN", false);
            Scribe_Values.Look<bool>(ref flipSet[1], "flipE", false);
            Scribe_Values.Look<bool>(ref flipSet[2], "flipS", false);
            Scribe_Values.Look<bool>(ref flipSet[3], "flipW", false);
        }
    }
}
