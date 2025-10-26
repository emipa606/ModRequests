using RimWorld;
using System;
//using CommunityCoreLibrary.ColorPicker;
using UnityEngine;
using Verse;
namespace ApparelColorChange
{
    [StaticConstructorOnStartup]
    public class Dialog_ColorChanger : Window
    {
        public enum GraphicSlotGroup
        {
            Invalid = -1,
            Body,
            OnSkinOnLegs,
            OnSkin,
            Middle,
            Shell,
            Head,
            Hair,
            Overhead
        }
        public class GraphicSlot
        {
            public Thing thing = null;
            public Graphic graphic = null;
            public Texture2D icon = null;
            public Color originalColor = Color.white;
            public Color newColor = Color.white;
            public CCL.ColorPicker.ColorWrapper colourWrapper;
            public bool Selected = false;
            private float yOffset = 0f;
            public bool Valid
            {
                get
                {
                    return thing != null;
                }
            }
            public string ThingLabel
            {
                get
                {
                    string result;
                    if (!Valid)
                    {
                        result = "ClutterColorChangerLabel".Translate();
                    }
                    else
                    {
                        result = thing.Label;
                    }
                    return result;
                }
            }
            public bool Changed
            {
                get
                {
                    return Valid && newColor != originalColor;
                }
            }
            public GraphicSlot()
            {
                thing = null;
                graphic = null;
                icon = null;
                originalColor = Color.white;
                newColor = Color.white;
                Selected = false;
                yOffset = 0f;
            }
            public GraphicSlot(Thing newThing, Graphic newGraphic, Texture2D newIcon, Color color, float newYOffset)
            {
                thing = newThing;
                graphic = newGraphic;
                icon = newIcon;
                originalColor = color;
                newColor = color;
                colourWrapper = new CCL.ColorPicker.ColorWrapper(color);
                Selected = false;
                yOffset = newYOffset;
            }
            public void ConfirmColor()
            {
                if (Valid)
                {
                    Apparel apparel = thing as Apparel;
                    if (apparel != null)
                    {
                        apparel.DrawColor = newColor;
                    }
                }
            }
            public void Draw(Rect rect)
            {
                if (Valid && graphic != null)
                {
                    Rect position = new Rect(rect.x, rect.y + yOffset, rect.width, rect.height);
                    GUI.color = newColor;
                    GUI.DrawTexture(position, graphic.MatFront.mainTexture);
                }
            }
            public void DrawIcon(Rect rect)
            {
                if (Valid && icon != null)
                {
                    GUI.color = newColor;
                    GUI.DrawTexture(rect, icon);
                }
            }
        }
        private static string WindowTitle;
        private static Vector2 WindowTitleSize;
        private static Rect WindowTitleRect;
        private static Pawn pawn;
        private static GraphicSlot[] GraphicSlots;
        private static GraphicSlotGroup SelectedSlot;
        private static float _previewSize;
        private static float _iconSize;
        private static Texture2D _icon;
        private static float _margin;
        private static float _listWidth;
        private static float _entryHeight;
        private static Texture2D _nameBackground;
        //private Vector2 _initialSize;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2( _previewSize + _margin + _listWidth + 36f, 40f + _previewSize * 2f + _margin * 3f + 38f + 36f );
            }
        }
        
        static Dialog_ColorChanger()
        {
            WindowTitleSize = Vector2.zero;
            _previewSize = 100f;
            _iconSize = 24f;
            _icon = ContentFinder<Texture2D>.Get("ClothIcon");
            _margin = 6f;
            _listWidth = 200f;
            _entryHeight = 30f;
            _nameBackground = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 0.3f));
            WindowTitle = "ClutterColorChangerTitle".Translate();
        }
        public Dialog_ColorChanger(Pawn p)
        {
            absorbInputAroundWindow = false;
            forcePause = true;
            closeOnClickedOutside = false;
            pawn = p;
            GraphicSlots = new GraphicSlot[8];
            SelectedSlot = GraphicSlotGroup.Invalid;
            for (int i = 0; i < 8; i++)
            {
                GraphicSlots[i] = new GraphicSlot();
            }
            SetGraphicSlot(GraphicSlotGroup.Body, pawn, GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.bodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor), pawn.def.uiIcon, pawn.story.SkinColor);
            SetGraphicSlot(GraphicSlotGroup.Head, pawn, GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor), pawn.def.uiIcon, pawn.story.SkinColor);
            SetGraphicSlot(GraphicSlotGroup.Hair, pawn, HairGraphic(pawn.story.hairDef), pawn.def.uiIcon, pawn.story.hairColor);
            foreach (Apparel current in pawn.apparel.WornApparel)
            {
                GraphicSlotGroup slotForApparel = GetSlotForApparel(current);
                if (slotForApparel != GraphicSlotGroup.Invalid)
                {
                    SetGraphicSlot(slotForApparel, current, ApparelGraphic(current.def, pawn.story.bodyType), current.def.uiIcon, current.DrawColor);
                }
            }
        }
        public override void PostOpen()
        {
            windowRect.x = windowRect.x - (windowRect.width - _margin) / 2f;
        }
        public override void PreClose()
        {
            while (Find.WindowStack.TryRemove(typeof(CCL.ColorPicker.Dialog_ColorPicker), false))
            {
            }
        }
        private Graphic HairGraphic(HairDef def)
        {
            Graphic result;
            if (def.texPath != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi>(def.texPath, ShaderDatabase.Cutout, new Vector2(38f, 38f), Color.white, Color.white);
            }
            else
            {
                result = null;
            }
            return result;
        }
        private Graphic ApparelGraphic(ThingDef def, BodyType bodyType)
        {
            Graphic result;
            if (def.apparel == null || def.apparel.wornGraphicPath.NullOrEmpty())
            {
                result = null;
            }
            else
            {
                result = GraphicDatabase.Get<Graphic_Multi>((def.apparel.LastLayer != ApparelLayer.Overhead) ? (def.apparel.wornGraphicPath + "_" + bodyType.ToString()) : def.apparel.wornGraphicPath, ShaderDatabase.Cutout, new Vector2(38f, 38f), Color.white, Color.white);
            }
            return result;
        }
        private void SetGraphicSlot(GraphicSlotGroup slotIndex, Thing newThing, Graphic newGraphic, Texture2D newIcon, Color newColor)
        {
            GraphicSlots[(int)slotIndex] = new GraphicSlot(newThing, newGraphic, newIcon, newColor, (slotIndex < GraphicSlotGroup.Head) ? 0f : -16f);
        }
        private GraphicSlotGroup GetSlotForApparel(Thing apparel)
        {
            ApparelProperties apparel2 = apparel.def.apparel;
            ApparelLayer lastLayer = apparel2.LastLayer;
            GraphicSlotGroup result;
            switch (lastLayer)
            {
                case ApparelLayer.OnSkin:
                    if (apparel2.bodyPartGroups.Count == 1 && apparel2.bodyPartGroups[0].Equals(BodyPartGroupDefOf.Legs))
                    {
                        result = GraphicSlotGroup.OnSkinOnLegs;
                        return result;
                    }
                    result = GraphicSlotGroup.OnSkin;
                    return result;
                case ApparelLayer.Middle:
                    result = GraphicSlotGroup.Middle;
                    return result;
                case ApparelLayer.Shell:
                    result = GraphicSlotGroup.Shell;
                    return result;
                case ApparelLayer.Overhead:
                    result = GraphicSlotGroup.Overhead;
                    return result;
            }
            Log.Warning("Could not resolve 'LastLayer' " + lastLayer.ToString());
            result = GraphicSlotGroup.Invalid;
            return result;
        }
        private void DrawSlotPicker(Rect parentRect)
        {
            GUI.BeginGroup(parentRect);
            string nameStringShort = pawn.NameStringShort;
            Vector2 vector = Text.CalcSize(nameStringShort);
            Rect rect = new Rect(0f, 0f, _previewSize, _previewSize);
            Rect rect2 = new Rect(0f, rect.yMax - vector.y, vector.x, vector.y);
            Rect rect3 = new Rect(0f, rect.yMax + _margin, _previewSize, _previewSize);
            Rect rect4 = new Rect(_previewSize + _margin, 0f, _listWidth, _entryHeight * 5f);
            rect2 = rect2.CenteredOnXIn(rect);
            rect4.y += parentRect.height / 2f - rect4.height / 2f;
            for (int i = 0; i < 6; i++)
            {
                GraphicSlots[i].Draw(rect);
            }
            if (GraphicSlots[7].Valid)
            {
                GraphicSlots[7].Draw(rect);
            }
            else
            {
                if (GraphicSlots[6].Valid)
                {
                    GraphicSlots[6].Draw(rect);
                }
            }
            if (SelectedSlot != GraphicSlotGroup.Invalid)
            {
                GraphicSlots[(int)SelectedSlot].DrawIcon(rect3);
            }
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(rect2.xMin - 3f, rect2.yMin, rect2.width + 6f, rect2.height), _nameBackground);
            Widgets.Label(rect2, nameStringShort);
            Widgets.DrawMenuSection(rect4);
            GUI.BeginGroup(rect4);
            SlotPicker(new Rect(0f, _entryHeight * 0f, rect4.width, _entryHeight), GraphicSlotGroup.OnSkinOnLegs);
            SlotPicker(new Rect(0f, _entryHeight * 1f, rect4.width, _entryHeight), GraphicSlotGroup.OnSkin);
            SlotPicker(new Rect(0f, _entryHeight * 2f, rect4.width, _entryHeight), GraphicSlotGroup.Middle);
            SlotPicker(new Rect(0f, _entryHeight * 3f, rect4.width, _entryHeight), GraphicSlotGroup.Shell);
            SlotPicker(new Rect(0f, _entryHeight * 4f, rect4.width, _entryHeight), GraphicSlotGroup.Overhead);
            GUI.EndGroup();
            GUI.EndGroup();
        }
        private void SlotPicker(Rect rect, GraphicSlotGroup slotIndex)
        {
            Rect rect2 = rect;
            rect2.xMin += _margin;
            string thingLabel = GraphicSlots[(int)slotIndex].ThingLabel;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (GraphicSlots[(int)slotIndex].Valid)
            {
                GUI.color = Color.white;
                Widgets.Label(rect2, thingLabel);
                if (Widgets.ButtonInvisible(rect))
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (i != (int)slotIndex)
                        {
                            GraphicSlots[i].Selected = false;
                        }
                    }
                    GraphicSlots[(int)slotIndex].Selected = true;
                    Find.WindowStack.TryRemove(typeof(CCL.ColorPicker.Dialog_ColorPicker));
                    GraphicSlot slot = GraphicSlots[(int)slotIndex];
                    Find.WindowStack.Add(new CCL.ColorPicker.Dialog_ColorPicker(slot.colourWrapper, delegate
                    {
                        slot.newColor = slot.colourWrapper.Color;
                    }, false, true)
                    {
                        initialPosition = new Vector2(windowRect.xMax + _margin, windowRect.yMin),
                        pickerSize = (int)(windowRect.height - 36f)
                    });
                    SelectedSlot = slotIndex;
                }
                Widgets.DrawHighlightIfMouseover(rect);
                if (GraphicSlots[(int)slotIndex].Selected)
                {
                    Widgets.DrawHighlightSelected(rect);
                }
            }
            else
            {
                GUI.color = Color.gray;
                Widgets.Label(rect2, thingLabel);
            }
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Color color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, GUI.color.a);
            Text.Font = GameFont.Medium;
            if (WindowTitleSize == Vector2.zero)
            {
                WindowTitleSize = Text.CalcSize(WindowTitle);
                WindowTitleRect = new Rect((InitialSize.x - 18f - 18f - WindowTitleSize.x) / 2f, 0f, WindowTitleSize.x, WindowTitleSize.y);
                Log.Message("Title Size: " + WindowTitleSize.ToString());
            }
            Widgets.Label(WindowTitleRect, WindowTitle);
            Rect position = new Rect(WindowTitleRect.xMin - _iconSize - _margin, 0f, _iconSize, _iconSize).CenteredOnYIn(WindowTitleRect);
            GUI.DrawTexture(position, _icon);
            Text.Font = GameFont.Small;
            DrawSlotPicker(new Rect(0f, WindowTitleSize.y + _margin, inRect.width, inRect.height - WindowTitleSize.y - 38f - _margin));
            GUI.color = color;


            DialogUtility.DoNextBackButtons( inRect, "ClutterColorChangerButtonAccept".Translate(), delegate
             {
                 // update gear graphics
                 GraphicSlot[] graphicSlots = GraphicSlots;
                 for ( int i = 0; i < graphicSlots.Length; i++ )
                 {
                     GraphicSlot graphicSlot = graphicSlots[i];
                     graphicSlot.ConfirmColor();
                 }
                 pawn.Drawer.renderer.graphics.ResolveApparelGraphics();

                 // update colonistbar
                 PortraitsCache.SetDirty(pawn);
                 Close();
             }, delegate
             {
                 Close();
             } );
        }
    }
}
