using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
    internal static class DesignatorExtension
    {
        public static GizmoResult DoCustomGuizmoOnGUI(this Designator instance, Rect butRect, GizmoRenderParms parms,
            bool simplified = false)
        {
            var DrawIconMethod = AccessTools.Method(typeof(Designator), "DrawIcon");

            Text.Font = GameFont.Tiny;
            var color = Color.white;
            var flag1 = false;
            if (Mouse.IsOver(butRect))
            {
                flag1 = true;
                if (!instance.Disabled)
                    color = GenUI.MouseoverColor;
            }

            MouseoverSounds.DoRegion(butRect, SoundDefOf.Mouseover_Command);
            if (parms.highLight)
                Widgets.DrawStrongHighlight(butRect.ExpandedBy(12f));
            var material = instance.Disabled || parms.lowLight ? TexUI.GrayscaleGUI : null;
            GUI.color = parms.lowLight ? Command.LowLightBgColor : color;
            GenUI.DrawTextureWithMaterial(butRect,
                parms.shrunk ? instance.BGTextureShrunk : (Texture)instance.BGTexture, material);
            GUI.color = color;
            DrawIconMethod.Invoke(instance, new object[] { butRect, material, parms });
            var flag2 = false;
            GUI.color = Color.white;
            if (parms.lowLight)
                GUI.color = Command.LowLightLabelColor;
            var keyA = instance.hotKey == null ? KeyCode.None : instance.hotKey.defaultKeyCodeA;
            var keyB = instance.hotKey == null ? KeyCode.None : instance.hotKey.defaultKeyCodeB;
            var k = keyA == KeyCode.None || CustomGizmoGridDrawer.drawnHotKeys.ContainsKey(keyA) ? keyB : keyA;
            if (k != KeyCode.None && !CustomGizmoGridDrawer.drawnHotKeys.ContainsKey(k))
            {
                var vector2 = parms.shrunk ? new Vector2(3f, 0.0f) : new Vector2(5f, 3f);
                Widgets.Label(new Rect(butRect.x + vector2.x, butRect.y + vector2.y, butRect.width - 10f, 18f),
                    k.ToStringReadable());
                CustomGizmoGridDrawer.drawnHotKeys.Add(k, instance);
                if (instance.hotKey.KeyDownEvent)
                {
                    flag2 = true;
                    Event.current.Use();
                }
            }

            if (CustomGizmoGridDrawer.customActivator != null && CustomGizmoGridDrawer.customActivator(instance))
                flag2 = true;
            if (Widgets.ButtonInvisible(butRect))
                flag2 = true;
            if (!parms.shrunk)
            {
                var topRightLabel = instance.TopRightLabel;
                if (!topRightLabel.NullOrEmpty())
                {
                    var vector2 = Text.CalcSize(topRightLabel);
                    Rect rect;
                    var position = rect = new Rect((float)(butRect.xMax - (double)vector2.x - 2.0), butRect.y + 3f,
                        vector2.x, vector2.y);
                    position.x -= 2f;
                    position.width += 3f;
                    Text.Anchor = TextAnchor.UpperRight;
                    GUI.DrawTexture(position, TexUI.GrayTextBG);
                    var label = topRightLabel;
                    Widgets.Label(rect, label);
                    Text.Anchor = TextAnchor.UpperLeft;
                }

                var labelCap = instance.LabelCap;
                if (!labelCap.NullOrEmpty() && !simplified)
                {
                    var height = Text.CalcHeight(labelCap, butRect.width);
                    var rect = new Rect(butRect.x, (float)(butRect.yMax - (double)height + 12.0), butRect.width,
                        height);
                    GUI.DrawTexture(rect, TexUI.GrayTextBG);
                    Text.Anchor = TextAnchor.UpperCenter;
                    Widgets.Label(rect, labelCap);
                    Text.Anchor = TextAnchor.UpperLeft;
                }

                GUI.color = Color.white;
            }

            if (!instance.HighlightTag.NullOrEmpty() && (Find.WindowStack.FloatMenu == null ||
                                                         !Find.WindowStack.FloatMenu.windowRect.Overlaps(butRect)))
                UIHighlighter.HighlightOpportunity(butRect, instance.HighlightTag);
            Text.Font = GameFont.Small;
            if (flag2)
            {
                if (instance.Disabled)
                {
                    if (!instance.disabledReason.NullOrEmpty())
                        Messages.Message(instance.disabledReason, MessageTypeDefOf.RejectInput, false);
                    return new GizmoResult(GizmoState.Mouseover, null);
                }

                GizmoResult gizmoResult;
                if (Event.current.button == 1)
                {
                    gizmoResult = new GizmoResult(GizmoState.OpenedFloatMenu, Event.current);
                }
                else
                {
                    if (!TutorSystem.AllowAction((EventPack)instance.TutorTagSelect))
                        return new GizmoResult(GizmoState.Mouseover, null);
                    gizmoResult = new GizmoResult(GizmoState.Interacted, Event.current);
                    TutorSystem.Notify_Event((EventPack)instance.TutorTagSelect);
                }

                return gizmoResult;
            }

            return flag1 ? new GizmoResult(GizmoState.Mouseover, null) : new GizmoResult(GizmoState.Clear, null);
        }
    }
}