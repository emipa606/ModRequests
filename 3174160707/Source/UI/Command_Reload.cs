using Reload.Components;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Reload.UI
{
    public class Command_Reload : Command_Action
    {
        public CompReload comp;

        public Command_Reload(CompReload comp)
        {
            this.defaultLabel = "ReloadCap".Translate();
            this.groupable = false;
            this.comp = comp;
            this.action = () => comp.Reload();
        }
        public override float GetWidth(float maxWidth)
        {
            return 75f;
        }
        public override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
        {
            rect.position += new Vector2(iconOffset.x * rect.size.x, iconOffset.y * rect.size.y);
            if (!disabled || parms.lowLight)
            {
                GUI.color = IconDrawColor;
            }
            else
            {
                GUI.color = IconDrawColor.SaturationChanged(0f);
            }
            if (parms.lowLight)
            {
                GUI.color = GUI.color.ToTransparent(0.6f);
            }
            try
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Color color = Color.white;

                Rect rect_inner = GenUI.ContractedBy(rect, 6f);
                Rect rect_top = rect_inner.TopHalf();
                Rect rect_bottom = rect_inner.BottomHalf();
                Rect rect_weaponTexture = rect_top.LeftHalf();
                Rect rect_ammoTexture = rect_top.RightHalf();
                Rect rect_fill = rect_bottom.TopHalf();
                rect_fill.height *= 0.7f;
                Rect rect_remaining = rect_bottom.BottomHalf();
                Texture weaponTexture = Widgets.GetIconFor(comp.parent.def);
                Widgets.DrawTextureFitted(rect_weaponTexture, weaponTexture, 0.95f);
                if (Setting.EnableAmmo)
                    Widgets.DrawTextureFitted(rect_ammoTexture, Widgets.GetIconFor(comp.Props.ammoDef), 0.95f);
                Widgets.FillableBar(rect_fill, (float)comp.remainingAmmo / (float)comp.MagazineCapacity);
                Widgets.Label(rect_remaining, comp.LabelRemaining);
            }
            catch
            {
                Main.LogMessage("Couldn't draw Command_Reload Gizmo");
            }
            finally
            {
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }
    }
}
