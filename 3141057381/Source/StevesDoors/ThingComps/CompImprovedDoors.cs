using System.Collections.Generic;
using StevesDoors.Utils;
using UnityEngine;
using Verse;

namespace StevesDoors
{
    public class CompImprovedDoors : ThingComp
    {
        protected bool IsAccentGraphic;
        protected bool ShowAccentGraphics = true;
        protected float FadeMultiplier;
        protected float RotationAngle;
        protected Rot4 Rotation;
        protected Vector3 MoveDir;
        protected Vector3 DrawPos;
        protected Color DoorColor = Color.white;
        protected Color AccentColor = Color.white;
        protected ImprovedDoorExtension Ext;
        protected Matrix4x4 Matrix;
        protected Material FinalMat;
        protected readonly MaterialPropertyBlock Mpb = new();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Ext = parent.def.GetModExtension<ImprovedDoorExtension>();
            Rotation = parent.Rotation;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (Ext != null)
            {
                if (Ext.isLaserDoor)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Door Color",
                        defaultDesc = "Change the color of the door.",
                        icon = ContentFinder<Texture2D>.Get("StevesDoors/UI/Icons/LaserDoorColorPicker_GizmoIcon"),
                        action = () =>
                        {
                            Find.WindowStack.Add(new Dialogue_DoorColorPicker(DoorColor, newColor => DoorColor = newColor));
                        }
                    };
                }
                if (Ext.hasAccentColors)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Accent Color",
                        defaultDesc = "Change the color of the doors' accent pieces.",
                        icon = ContentFinder<Texture2D>.Get("StevesDoors/UI/Icons/DoorAccentColorPicker_GizmoIcon"),
                        action = () =>
                        {
                            Find.WindowStack.Add(new Dialogue_DoorAccentsColorPicker(AccentColor, newColor => AccentColor = newColor));
                        }
                    };
                    yield return new Command_Toggle
                    {
                        defaultLabel = "Show Accents",
                        defaultDesc = "Toggle whether or not accent pieces are rendered.",
                        icon = ContentFinder<Texture2D>.Get("StevesDoors/UI/Icons/DoorShowAccents_GizmoIcon"),
                        isActive = () => ShowAccentGraphics,
                        toggleAction = delegate
                        {
                            ShowAccentGraphics = !ShowAccentGraphics;
                        }
                    };
                }
            }
        }
    }
}
