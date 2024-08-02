using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TogglableStorageItemVisibility
{
    [HarmonyPatch(typeof(Thing), "DrawGUIOverlay")]
    internal static class Thing_DrawGUIOverlay_Patch
    {
        private static bool Prefix(Thing __instance)
        {
            if (__instance.StoringThing() is Building_Storage storage && __instance != storage
                && storage.HasToggleStorage(out var comp))
            {
                if (HoveringOn(storage))
                {
                    var heldThings = storage.slotGroup.HeldThings.ToList();
                    Vector2 mousePosition = Event.current.mousePosition;
                    var rectWidth = 400;
                    Rect mouseRect = new Rect(mousePosition.x + 8f, mousePosition.y + 8f, rectWidth, heldThings.Count * 32f);
                    Find.WindowStack.ImmediateWindow(84906312, mouseRect, WindowLayer.Super, delegate
                    {
                        var font = Text.Font;
                        var anchor = Text.Anchor;
                        Text.Font = GameFont.Small;
                        Text.Anchor = TextAnchor.MiddleLeft;
                        var pos = mouseRect.AtZero();
                        foreach (var heldThing in heldThings)
                        {
                            var iconRect = new Rect(pos.x, pos.y, 32, 32);
                            GUI.color = heldThing.DrawColor;
                            GUI.DrawTexture(iconRect, heldThing.def.uiIcon);
                            GUI.color = Color.white;
                            var labelRect = new Rect(iconRect.xMax + 5, iconRect.y, rectWidth - iconRect.width - 5, 32);
                            Widgets.Label(labelRect, heldThing.LabelCap);
                            pos.y += 32;
                        }
                        Text.Font = font;
                        Text.Anchor = anchor;
                    });
                }

                if (comp.itemVisibility is false)
                {
                    return false;
                }
            }
            return true;
        }


        public static bool HoveringOn(Thing thing)
        {
            if (Event.current.type != EventType.Repaint || Find.WindowStack.FloatMenu != null || (Find.Targeter.IsTargeting && Find.Targeter.targetingSource != null && Find.Targeter.targetingSource.HidePawnTooltips))
            {
                return false;
            }
            float cellSizePixels = Startup.GetCellSizePixels();
            Vector2 vector = new Vector2(cellSizePixels, cellSizePixels);
            var size = thing.RotatedSize;
            vector.x *= size.x;
            vector.y *= size.z;
            Vector2 vector2 = thing.DrawPos.MapToUIPosition();
            Rect rect = new Rect(0f, 0f, vector.x, vector.y);
            rect.x = vector2.x - vector.x / 2f;
            rect.y = vector2.y - vector.y / 2f;
            if (rect.Contains(Event.current.mousePosition))
            {
                return true;
            }
            return false;
        }

    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.Print))]
    public static class Thing_Print_Patch
    {
        public static bool Prefix(Thing __instance)
        {
            if (ShouldNotDraw(__instance))
            {
                return false;
            }
            return true;
        }

        public static bool ShouldNotDraw(this Thing __instance)
        {
            return __instance.StoringThing() is Thing thing && __instance != thing
                            && thing.HasToggleStorage(out var comp) && comp.itemVisibility is false;
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]
    public static class PawnRenderer_RenderPawnInternal_Patch
    {
        [HarmonyBefore(new string[] { "rimworld.Nals.FacialAnimation" })]
        private static bool Prefix(PawnRenderer __instance, ref Vector3 rootLoc, PawnRenderFlags flags)
        {
            if (!flags.FlagSet(PawnRenderFlags.Portrait))
            {
                Pawn ___pawn = __instance.graphics.pawn;
                if (___pawn.Dead && (___pawn.Corpse?.Spawned ?? false) &&
                    ___pawn.Corpse.ShouldNotDraw())
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class CompToggleStorageItemVisibility : ThingComp
    {
        public bool itemVisibility = true;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Toggle
            {
                defaultLabel = "Storage item visibility",
                icon = ContentFinder<Texture2D>.Get("UI/TogglableStorageItemVisibility"),
                isActive = () => itemVisibility,
                toggleAction = () => 
                { 
                    itemVisibility = !itemVisibility;
                    parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things);
                }
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref itemVisibility, "itemVisibility", true);
        }
    }
}
