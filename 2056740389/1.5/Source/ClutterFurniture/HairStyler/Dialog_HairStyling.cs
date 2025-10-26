using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace HairStyling
{
    [StaticConstructorOnStartup]
    public class Dialog_HairStyling : Window
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
        public class Apparel
        {
            public Thing thing = null;
            public Graphic graphic = null;
            public Texture2D icon = null;
            public Color color = Color.white;
            private float yOffset = 0f;
            public bool Valid
            {
                get
                {
                    return thing != null;
                }
            }
            public Apparel()
            {
                thing = null;
                graphic = null;
                icon = null;
                yOffset = 0f;
            }
            public Apparel(Thing thing, Graphic graphic, Texture2D icon, Color color, float yOffset)
            {
                this.thing = thing;
                this.graphic = graphic;
                this.icon = icon;
                this.color = color;
                this.yOffset = yOffset;
            }
            public void Draw(Rect rect)
            {
                if (Valid && graphic != null)
                {
                    Rect position = new Rect(rect.x, rect.y + yOffset, rect.width, rect.height);
                    GUI.color = color;
                    GUI.DrawTexture(position, graphic.MatFront.mainTexture);
                    GUI.color = Color.white;
                }
            }
            public void DrawIcon(Rect rect)
            {
                if (Valid && icon != null)
                {
                    GUI.color = color;
                    GUI.DrawTexture(rect, icon);
                    GUI.color = Color.white;
                }
            }
        }
        private static string _title;
        private static float _titleHeight;
        private static float _previewSize;
        private static float _iconSize;
        private static Texture2D _icon;
        private static float _margin;
        private static float _listWidth;
        private static int _columns;
        private static float _entrySize;
        private static Texture2D _nameBackground;
        private static List<HairDef> _hairDefs;
        private static Pawn pawn;
        private static Apparel[] Apparels;
        private static HairDef _newHair;
        private static HairDef originalHair;
        private static Color _newColour;
        private static Color originalColour;
        private static CCL.ColorPicker.ColorWrapper colourWrapper;
        private Vector2 _scrollPosition = Vector2.zero;
        public HairDef newHair
        {
            get
            {
                return _newHair;
            }
            set
            {
                _newHair = value;
                SetGraphicSlot(GraphicSlotGroup.Hair, pawn, HairGraphic(value), pawn.def.uiIcon, newColour);
            }
        }
        public Color newColour
        {
            get
            {
                return _newColour;
            }
            set
            {
                _newColour = value;
                Apparels[6].color = value;
            }
        }
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(_previewSize + _margin + _listWidth + 36f, 40f + _previewSize * 2f + _margin * 3f + 38f + 36f);
            }
        }
        static Dialog_HairStyling()
        {
            _title = "ClutterHairStylerTitle".Translate();
            _titleHeight = 40f;
            _previewSize = 100f;
            _iconSize = 24f;
            _icon = ContentFinder<Texture2D>.Get("ClothIcon");
            _margin = 6f;
            _listWidth = 200f;
            _columns = 4;
            _entrySize = _listWidth / (float)_columns;
            _nameBackground = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 0.3f));
            _hairDefs = DefDatabase<HairDef>.AllDefsListForReading;
        }
        public Dialog_HairStyling(Pawn p)
        {
            absorbInputAroundWindow = false;
            forcePause = true;
            closeOnClickedOutside = false;
            pawn = p;
            originalColour = (_newColour = pawn.story.hairColor);
            colourWrapper = new CCL.ColorPicker.ColorWrapper(newColour);
            _newHair = (originalHair = pawn.story.hairDef);
            Apparels = new Apparel[8];
            for (int i = 0; i < 8; i++)
            {
                Apparels[i] = new Apparel();
            }
            SetGraphicSlot(GraphicSlotGroup.Body, pawn, GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(pawn.story.bodyType, ShaderDatabase.CutoutSkin, pawn.story.SkinColor), pawn.def.uiIcon, pawn.story.SkinColor);
            SetGraphicSlot(GraphicSlotGroup.Head, pawn, GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor), pawn.def.uiIcon, pawn.story.SkinColor);
            SetGraphicSlot(GraphicSlotGroup.Hair, pawn, HairGraphic(pawn.story.hairDef), pawn.def.uiIcon, pawn.story.hairColor);
            foreach (RimWorld.Apparel current in pawn.apparel.WornApparel)
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
            Apparels[(int)slotIndex] = new Apparel(newThing, newGraphic, newIcon, newColor, (slotIndex < GraphicSlotGroup.Head) ? 0f : -16f);
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
        private void DrawUI(Rect parentRect)
        {
            GUI.BeginGroup(parentRect);
            string nameStringShort = pawn.NameStringShort;
            Vector2 vector = Text.CalcSize(nameStringShort);
            Rect rect = new Rect(0f, 0f, _previewSize, _previewSize);
            Rect rect2 = new Rect(0f, rect.yMax - vector.y, vector.x, vector.y);
            Rect rect3 = new Rect(0f, rect.yMax + _margin, _previewSize, _previewSize);
            Rect rect4 = new Rect(_previewSize + _margin, 0f, _listWidth, parentRect.height - _margin);
            rect2 = rect2.CenteredOnXIn(rect);
            for (int i = 0; i < 6; i++)
            {
                Apparels[i].Draw(rect);
            }
            if (Apparels[7].Valid)
            {
                Apparels[7].Draw(rect);
            }
            else
            {
                if (Apparels[6].Valid)
                {
                    Apparels[6].Draw(rect);
                }
            }
            Apparels[6].Draw(rect3);
            GUI.DrawTexture(new Rect(rect2.xMin - 3f, rect2.yMin, rect2.width + 6f, rect2.height), _nameBackground);
            Widgets.Label(rect2, nameStringShort);
            Widgets.DrawMenuSection(rect4);
            DrawHairPicker(rect4);
            GUI.EndGroup();
        }
        private void DrawHairPicker(Rect rect)
        {
            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt((float)_hairDefs.Count / (float)_columns);
            rect3.height = (float)num * _entrySize;
            Vector2 vector = new Vector2(_entrySize, _entrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / (float)_columns;
                vector.y -= 16f / (float)_columns;
                rect3.width -= 16f;
                rect3.height = vector.y * (float)num;
            }
            Widgets.BeginScrollView(rect2, ref _scrollPosition, rect3);
            GUI.BeginGroup(rect3);
            for (int i = 0; i < _hairDefs.Count; i++)
            {
                int num2 = i / _columns;
                int num3 = i % _columns;
                Rect rect4 = new Rect((float)num3 * vector.x, (float)num2 * vector.y, vector.x, vector.y);
                DrawHairPickerCell(_hairDefs[i], rect4);
            }
            GUI.EndGroup();
            Widgets.EndScrollView();
        }
        private void DrawHairPickerCell(HairDef hair, Rect rect)
        {
            GUI.DrawTexture(rect, HairGraphic(hair).MatFront.mainTexture);
            string text = hair.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (hair == newHair)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (hair == originalHair)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }
            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                newHair = hair;
                while (Find.WindowStack.TryRemove(typeof(CCL.ColorPicker.Dialog_ColorPicker)))
                {
                }
                Find.WindowStack.Add(new CCL.ColorPicker.Dialog_ColorPicker(colourWrapper, delegate
                {
                    newColour = colourWrapper.Color;
                }, false, true)
                {
                    initialPosition = new Vector2(windowRect.xMax + _margin, windowRect.yMin),
                    pickerSize = (int)(windowRect.height - 36f)
                });
            }
        }
        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(_iconSize + _margin, 0f, inRect.width - _iconSize - _margin, _titleHeight);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, _title);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            Rect position = new Rect(0f, 0f, _iconSize, _iconSize).CenteredOnYIn(rect);
            GUI.DrawTexture(position, _icon);
            DrawUI(new Rect(0f, _titleHeight + _margin, inRect.width, inRect.height - _titleHeight - 38f - _margin));
            DialogUtility.DoNextBackButtons( inRect, "ClutterColorChangerButtonAccept".Translate(), delegate
             {
                 // update render for graphics
                 pawn.Drawer.renderer.graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>( newHair.texPath, ShaderDatabase.Cutout, Vector2.one, newColour );

                 // uppdate story to persist across save/load
                 pawn.story.hairColor = newColour;
                 pawn.story.hairDef = newHair;

                 // force colonist bar to update
                 PortraitsCache.SetDirty( pawn );
                 Close();
             }, delegate
             {
                 Close();
             } );
        }
    }
}
