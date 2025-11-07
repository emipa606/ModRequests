using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace BillDoorsLootsAndShelves
{
    public class Building_Locker : Building_ShelfExpanded, ISuspendableThingHolder, IBeautyContainer
    {
        public override int MaxItemsInCell => useOpenTexture ? base.MaxItemsInCell : 0;

        protected Graphic GlassGraphic => def.building.mechGestatorCylinderGraphic?.Graphic;

        protected Graphic OpenGraphic => def.graphicData.GraphicColoredFor(this);
        protected MultiDisplayExtension multiDisplayExtension => def.GetModExtension<MultiDisplayExtension>();

        protected LockerExtension extension;

        protected CompPowerTrader compPower;

        protected CompRefuelable compRefuelable;

        protected CompFlickable compFlickable;

        public List<DisplayOffsetInfo> displayInfos;

        public DisplayOffsetInfo DisplayInfoForIndex(int index)
        {
            if (index - 1 > MaxItems)
            {
                index %= MaxItems;
            }
            if (index <= 0) return displayInfo;
            if (displayInfos == null) displayInfos = new List<DisplayOffsetInfo>();
            if (displayInfos.Count < index)
            {
                while (displayInfos.Count < index)
                {
                    displayInfos.Add(GetDisplayInfoFromExtension(displayInfos.Count));
                }
            }
            return displayInfos[index - 1];
        }

        public DisplayOffsetInfo displayInfo = new DisplayOffsetInfo();
        public bool isDisplay => extension != null && extension.isDisplay;

        public int MaxItems => def.building.maxItemsInCell;
        public bool IsContentsSuspended => extension != null && extension.stasis && (compPower?.PowerOn ?? true) && (compRefuelable?.HasFuel ?? true) && (compFlickable?.SwitchIsOn ?? true);

        public void MoveInfoIndex(ref int index, bool upwards)
        {
            if (displayInfos == null || displayInfos.Empty()) return;
            if (index > displayInfos.Count || index < 0) return;
            foreach (var i in displayInfos)
            {
                i.AssignShelfVars(displayInfo);
            }

            if (upwards)
            {
                if (index >= displayInfos.Count) return;
                if (index > 0)
                {
                    displayInfos.Swap(index - 1, index);
                }
                else
                {
                    (displayInfos[0], displayInfo) = (displayInfo, displayInfos[0]);
                }
                index++;
            }
            else
            {
                if (index == 0) return;
                if (index == 1)
                {
                    (displayInfos[0], displayInfo) = (displayInfo, displayInfos[0]);
                }
                else
                {
                    displayInfos.Swap(index - 1, index - 2);
                }
                index--;
            }
        }

        public void MoveThingIndex(ref int index, bool upwards)
        {
            if (tempStorage.NullOrEmpty()) return;
            if (index > tempStorage.Count || index < 0) return;

            if (upwards)
            {
                if (index + 1 >= tempStorage.Count) return;
                SwapThingIndex(index, index + 1);
                index++;
            }
            else
            {
                if (index == 0) return;
                SwapThingIndex(index, index - 1);
                index--;
            }
        }

        public void SwapThingIndex(int a, int b)
        {
            List<Thing> things = new List<Thing>(tempStorage.Count);
            foreach (Thing item in tempStorage)
            {
                things.Add(item);
            }
            try
            {
                tempStorage.Clear();
                for (int i = 0; i < things.Count; i++)
                {
                    if (i != a && i != b)
                    {
                        tempStorage.TryAddOrTransfer(things[i]);
                    }
                    if (i == a)
                    {
                        tempStorage.TryAddOrTransfer(things[b]);
                    }
                    if (i == b)
                    {
                        tempStorage.TryAddOrTransfer(things[a]);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                tempStorage.Clear();
                tempStorage.TryAddRangeOrTransfer(things);
            }
            (materials[a], materials[b]) = (materials[b], materials[a]);
            Notify_ColorChanged();
        }

        protected void InitDisplayInfos()
        {
            if (displayInfos == null) displayInfos = new List<DisplayOffsetInfo>();
            if (displayInfos.Count < MaxItems - 1)
            {
                while (displayInfos.Count < MaxItems - 1)
                {
                    displayInfos.Add(GetDisplayInfoFromExtension(displayInfos.Count + 1));
                }
            }
        }

        DisplayOffsetInfo GetDisplayInfoFromExtension(int index)
        {
            if (multiDisplayExtension != null && index < multiDisplayExtension.defaultDisplayInfos.Count)
            {
                return new DisplayOffsetInfo(multiDisplayExtension.defaultDisplayInfos[index]);
            }
            return new DisplayOffsetInfo(extension);
        }

        protected void TrimDisplayInfos()
        {
            if (displayInfos != null)
            {
                if (MaxItems == 1)
                {
                    displayInfos.Clear();
                }
                else if (displayInfos.Count > MaxItems - 1)
                {
                    while (displayInfos.Count > MaxItems - 1)
                    {
                        displayInfos.RemoveLast();
                    }
                }
            }
        }

        public override void PostPostMake()
        {
            base.PostPostMake();

            extension = def.GetModExtension<LockerExtension>();
            displayInfo = GetDisplayInfoFromExtension(0);
        }

        public override IEnumerable<IntVec3> AllSlotCells()
        {
            if (!base.Spawned)
            {
                yield break;
            }
            if (isDisplay)
            {
                yield return this.TrueCenter().ToIntVec3();
                yield break;
            }
            foreach (IntVec3 i in base.AllSlotCells())
            {
                yield return i;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            extension = def.GetModExtension<LockerExtension>();
            compPower = this.TryGetComp<CompPowerTrader>();
            compRefuelable = this.TryGetComp<CompRefuelable>();
            compFlickable = this.TryGetComp<CompFlickable>();
            InitDisplayInfos();
        }

        public override void Flick()
        {
            if (useOpenTexture)
            {
                int safety = this.OccupiedRect().Area * MaxItemsInCell;
                while (slotGroup.HeldThings.Any() && safety > 0)
                {
                    safety--;
                    var t = slotGroup.HeldThings.FirstOrDefault();
                    t.DeSpawn();
                    tempStorage.TryAdd(t);
                    if (t is IThingHolder)
                    {
                        displayInfo.showInnerContainer = true;
                    }
                }
                RefreshMaterial();
                useOpenTexture = false;
            }
            else
            {
                useOpenTexture = true;
                if (tempStorage.Any())
                {
                    tempStorage.TryDropAll(this.TrueCenter().ToIntVec3(), Map, ThingPlaceMode.Direct);
                }
            }
            Notify_ColorChanged();
            if (extension != null && extension.sound != null)
            {
                extension.sound.PlayOneShot(new TargetInfo(Position, Map));
            }
        }

        public Thing GetInnerThing(Thing thing)
        {
            if (displayInfo.showInnerContainer)
            {
                if (thing is IThingHolder holder)
                {
                    return holder.GetDirectlyHeldThings().First();
                }
                if (thing is ThingWithComps twc)
                {
                    foreach (ThingComp comp in twc.AllComps)
                    {
                        if (comp is IThingHolder holdercomp)
                        {
                            return holdercomp.GetDirectlyHeldThings().First();
                        }
                    }
                }
            }
            return thing;
        }

        List<Material> materials = new List<Material>();

        protected List<Material> Materials
        {
            get
            {
                if (materials.NullOrEmpty() && tempStorage.Any())
                {
                    RefreshMaterial();
                }
                return materials;
            }
        }

        public float BeautyOffset => ;

        public string BeautyOffsetExplanation => throw new NotImplementedException();

        public void RefreshMaterial()
        {
            materials.Clear();
            for (int i = 0; i < tempStorage.Count; i++)
            {
                materials.Add(GetMaterial(i));
            }
            Notify_ColorChanged();
        }

        public override void Print(SectionLayer layer)
        {
            if (isDisplay)
            {
                var altLayer = def.altitudeLayer;

                if (!useOpenTexture && tempStorage.Any())
                {
                    if (!displayInfo.hideTop)
                    {
                        int lidLayer = tempStorage.Count + 3;
                        if (LidGraphic != null) PrintOverlay(layer, this, altLayer.AltitudeFor(lidLayer), LidGraphic);
                        if (GlassGraphic != null) PrintOverlay(layer, this, altLayer.AltitudeFor(lidLayer - 1), GlassGraphic);
                    }
                    if (tempStorage.Any())
                    {
                        for (int i = 0; i < tempStorage.Count; i++)
                        {
                            DrawInnerThing(i, layer, altLayer);
                        }
                    }
                    if (BottomGraphic != null)
                    {
                        PrintOverlay(layer, this, altLayer.AltitudeFor(), BottomGraphic);
                    }
                    else
                    {
                        PrintOverlay(layer, this, altLayer.AltitudeFor(), OpenGraphic);
                    }
                }
                else
                {
                    PrintOverlay(layer, this, altLayer.AltitudeFor(), OpenGraphic);
                }
            }
            else
            {
                base.Print(layer);
            }
        }

        protected void DrawInnerThing(int index, SectionLayer layer, AltitudeLayer altLayer)
        {
            if (index < 0 || index > tempStorage.Count() - 1 || index > Materials.Count() - 1) return;
            var content = GetInnerThing(tempStorage[index]);
            var displayInfol = DisplayInfoForIndex(index);

            Vector2 drawsizeAltered = content.DefaultGraphic.drawSize * displayInfol.drawSizeMult;
            Vector3 center = this.TrueCenter() + displayInfol.drawOffset;

            center.y = altLayer.AltitudeFor(index + 1);

            if (content is Pawn)
            {
                Printer_Mesh.PrintMesh(layer, Matrix4x4.TRS(center, Quaternion.AngleAxis(displayInfol.drawRot, Vector3.up), new Vector3(drawsizeAltered.x, 0, drawsizeAltered.y)), mat: Materials[index], mesh: pawnMesh);
            }
            else
            {
                Printer_Plane.PrintPlane(layer, center, drawsizeAltered, Materials[index], AngleFromRot(Rotation) + content.def.equippedAngleOffset + displayInfol.drawRot, displayInfol.shouldFlip);
            }
        }

        Mesh pawnMesh;

        public Material GetMaterial(int index)
        {
            if (index < 0 || index > tempStorage.Count() - 1) return null;
            Thing thing = GetInnerThing(tempStorage[index]);
            Graphic graphic = thing.DefaultGraphic;
            var displayInfol = DisplayInfoForIndex(index);
            if (thing is Pawn pawn)
            {
                GlobalTextureAtlasManager.TryGetPawnFrameSet(pawn, out var frameSet, out var _);
                Rot4 rot = displayInfol.rotOverride.IsValid ? displayInfol.rotOverride : Rotation;
                int frameIndex = frameSet.GetIndex(rot, PawnDrawMode.BodyAndHead);
                if (frameSet.isDirty[frameIndex])
                {
                    Find.PawnCacheCamera.rect = frameSet.uvRects[frameIndex];
                    Find.PawnCacheRenderer.RenderPawn(pawn, frameSet.atlas, Vector3.zero, 1f, 0f, rot);
                    Find.PawnCacheCamera.rect = new Rect(0f, 0f, 1f, 1f);
                    frameSet.isDirty[frameIndex] = false;
                }
                pawnMesh = frameSet.meshes[frameIndex];
                return MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Transparent));
            }

            if (graphic is Graphic_StackCount graphic_StackCount)
            {
                return graphic_StackCount.SubGraphicForStackCount(displayInfol.useSingleText ? 1 : thing.stackCount, thing.def).MatSingleFor(thing);
            }
            return graphic.MatSingleFor(thing);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (tempStorage.Count > 0 && mode != DestroyMode.Vanish)
            {
                if (mode != DestroyMode.Deconstruct || mode != DestroyMode.Refund || mode != DestroyMode.WillReplace)
                {
                    List<Pawn> pawnList = new List<Pawn>();
                    foreach (Thing item2 in tempStorage)
                    {
                        if (item2 is Pawn item)
                        {
                            pawnList.Add(item);
                        }
                    }
                    foreach (Pawn item3 in pawnList)
                    {
                        HealthUtility.DamageUntilDowned(item3);
                    }
                }
                tempStorage.TryDropAll(Position, Map, ThingPlaceMode.Near);
            }
            tempStorage.ClearAndDestroyContents();
            base.Destroy(mode);
        }

        public override void TickRare()
        {
            base.TickRare();
            tempStorage.ThingOwnerTickRare();
        }

        public override void Tick()
        {
            base.Tick();
            tempStorage.ThingOwnerTick();
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!displayInfo.description.NullOrEmpty())
            {
                stringBuilder.AppendLine(displayInfo.description);
            }
            if (!base.GetInspectString().NullOrEmpty())
            {
                stringBuilder.AppendLine(base.GetInspectString());
            }
            string str = !useOpenTexture ? tempStorage.ContentsString : ((string)"BDshelvesEmpty".Translate());
            stringBuilder.Append("CasketContains".Translate() + ": " + str.CapitalizeFirst());
            return stringBuilder.ToString().TrimEnd();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            TrimDisplayInfos();
            Scribe_Deep.Look(ref displayInfo, "displayInfo");
            Scribe_Collections.Look(ref displayInfos, "displayInfos", LookMode.Deep);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo2 in base.GetGizmos())
            {
                yield return gizmo2;
            }
        }
    }

    public class LockerExtension : DefModExtension
    {
        public float drawSizeMult = 1;
        public Vector3 drawOffset = new Vector3(0, 0, 0.15f);
        public SoundDef sound;
        public bool isDisplay;
        public bool stasis;
    }

    public class MultiDisplayExtension : DefModExtension
    {
        public List<DisplayOffsetInfo> defaultDisplayInfos = new List<DisplayOffsetInfo>();
    }
}
