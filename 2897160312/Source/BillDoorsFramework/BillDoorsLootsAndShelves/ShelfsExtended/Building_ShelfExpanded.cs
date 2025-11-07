using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace BillDoorsLootsAndShelves
{
    public class Building_ShelfExpanded : Building_Storage, IThingHolder
    {
        public ThingOwner tempStorage;

        protected bool useOpenTexture = true;

        public bool Opened => useOpenTexture;

        protected Graphic BottomGraphic => def.building.fullGraveGraphicData?.GraphicColoredFor(this);

        protected Graphic LidGraphic => def.building.mechGestatorTopGraphic?.GraphicColoredFor(this);

        public Building_ShelfExpanded()
        {
            tempStorage = new ThingOwner<Thing>(this, oneStackOnly: false);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (tempStorage.Any())
            {
                tempStorage.TryDropAll(this.Position, map, ThingPlaceMode.Near, nearPlaceValidator: (IntVec3 c) => { return c.IsInside(this); });
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo2 in base.GetGizmos())
            {
                yield return gizmo2;
            }

            if (GetComp<CompUsable>() == null)
            {
                yield return coverGizmo();
            }
        }

        protected virtual Gizmo coverGizmo()
        {
            Command_Action stop = new Command_Action();
            stop.defaultLabel = "BDshelves_Lid".Translate();
            stop.icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true);
            stop.action = delegate
            {
                Flick();
            };
            return stop;
        }

        protected override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal); Notify_ColorChanged();
            if (signal == "Unlock")
            {
                Flick();
            }
        }

        public virtual void Flick()
        {
            useOpenTexture = !useOpenTexture;
            Notify_ColorChanged();
        }

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
            if (useOpenTexture && LidGraphic != null)
            {
                PrintOverlay(layer, this, AltitudeLayer.ItemImportant.AltitudeFor(9), LidGraphic);
            }
            else
            {
                PrintOverlay(layer, this, AltitudeLayer.ItemImportant.AltitudeFor(9), Graphic);
            }
        }

        protected float AngleFromRot(Rot4 rot)
        {
            if (Graphic.ShouldDrawRotated)
            {
                float asAngle = rot.AsAngle;
                asAngle += Graphic.DrawRotatedExtraAngleOffset;
                if ((rot == Rot4.West && Graphic.WestFlipped) || (rot == Rot4.East && Graphic.EastFlipped))
                {
                    asAngle += 180f;
                }
                return asAngle;
            }
            return 0f;
        }

        public void PrintOverlay(SectionLayer layer, Thing thing, float offset, Graphic Graphic, Shader shaderOverride = null)
        {
            Vector2 size;
            bool flag;
            if (Graphic.ShouldDrawRotated)
            {
                size = Graphic.data.drawSize;
                flag = false;
            }
            else
            {
                size = (thing.Rotation.IsHorizontal ? Graphic.data.drawSize.Rotated() : Graphic.data.drawSize);
                flag = (thing.Rotation == Rot4.West && Graphic.WestFlipped) || (thing.Rotation == Rot4.East && Graphic.EastFlipped);
            }
            if (thing.MultipleItemsPerCellDrawn())
            {
                size *= 0.8f;
            }

            float num = AngleFromRot(thing.Rotation);
            if (flag && Graphic.data != null)
            {
                num += Graphic.data.flipExtraRotation;
            }
            Vector3 center = thing.TrueCenter() + Graphic.DrawOffset(thing.Rotation);
            center.y = offset;
            Material material = Graphic.MatAt(thing.Rotation, thing);
            Graphic.TryGetTextureAtlasReplacementInfo(material, thing.def.category.ToAtlasGroup(), flag, vertexColors: true, out material, out var uvs, out var vertexColor);
            if (shaderOverride != null)
            {
                material.shader = shaderOverride;
            }

            Printer_Plane.PrintPlane(layer, center, size, material, num, flag, uvs, new Color32[4] { vertexColor, vertexColor, vertexColor, vertexColor });
            if (Graphic.ShadowGraphic != null && thing != null)
            {
                Graphic.ShadowGraphic.Print(layer, thing, 0f);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref useOpenTexture, "useOpenTexture", false);
            Scribe_Deep.Look(ref tempStorage, "innerContainer", this);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return tempStorage;
        }
        public override Graphic Graphic => useOpenTexture && BottomGraphic != null ? BottomGraphic : base.Graphic;

    }

    public class ShelfExpandedExtension : DefModExtension
    {
    }
}
