using System;
using UnityEngine;
using Verse;

namespace UINotIncluded
{
    public static class CustomButtons
    {
        public static Widgets.DraggableResult DraggableButton(Rect space, string label,
            CustomButtonState state = CustomButtonState.enabled, bool ConfigActionIcon = false)
        {
            Texture2D texture;
            bool doMousoverSound;

            switch (state)
            {
                case CustomButtonState.enabled:
                    texture = Mouse.IsOver(space) ? ModTextures.buttonDraggableMouseover : ModTextures.buttonDraggable;
                    doMousoverSound = true;
                    break;

                case CustomButtonState.disabled:
                    texture = ModTextures.buttonDraggable;
                    doMousoverSound = false;
                    break;

                default:
                    throw new NotImplementedException();
            }

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.DrawAtlas(space, texture);
            Widgets.Label(space, label);
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.BeginGroup(space);
            if (ConfigActionIcon)
            {
                var configSizef = space.height - 8f;
                var configSpace = new Rect(space.width - configSizef - 4f, 4f, configSizef, configSizef);
                GUI.DrawTexture(configSpace, ModTextures.iconCog);
            }

            GUI.EndGroup();

            return Widgets.ButtonInvisibleDraggable(space, doMousoverSound);
        }

        public static void DraggableButtonGhost(Rect space, string label)
        {
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            var texture = ModTextures.buttonDraggable;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.DrawAtlas(space, texture);
            Widgets.Label(space, label);
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }

    public enum CustomButtonState
    {
        enabled,
        disabled,
        ghost
    }
}