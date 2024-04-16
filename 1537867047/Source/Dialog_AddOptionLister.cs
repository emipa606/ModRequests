using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AppearanceClothes {
    class Dialog_AddOptionLister : Window {
        protected List<AddMenuOption> options;

        protected const float ButSpacing = 0f;

        protected Vector2 scrollPosition;

        protected Listing_Standard listing;

        private BodyTypeDef bodyType;

        protected static readonly Vector2 ButSize = new Vector2(230f, 27f);

        protected readonly float ColumnSpacing = 20f;

        protected readonly float SectSpacing = 8f;

        private static int indexBackgroundColor = 0;

        private static Rot4 rot = Rot4.South;

        private static readonly Color[] BackgroundColors = new Color[] {
            Color.grey,
            Color.white,
            Color.black
        };

        public override Vector2 InitialSize {
            get {
                return new Vector2(640f, 556f);
            }
        }

        public override bool IsDebug {
            get {
                return true;
            }
        }

        public Dialog_AddOptionLister(IEnumerable<AddMenuOption> options,BodyTypeDef bodyType) {
            this.optionalTitle = "AppearanceClothes.DialogAddOptionListerTitle".Translate();
            this.doCloseX = true;
            this.onlyOneOfTypeAllowed = true;
            this.absorbInputAroundWindow = true;
            this.bodyType = bodyType;

            this.options = options.ToList<AddMenuOption>();
        }

        public override void DoWindowContents(Rect inRect) {
            {
                WidgetRow widgetRow = new WidgetRow(0f, 0f, UIDirection.RightThenUp, 99999f, 4f);
                if (widgetRow.ButtonIcon(MyTexButton.ChangeColor, "AppearanceClothes.TipChangeBackgroundColor".Translate(), new Color?(GenUI.SubtleMouseoverColor), null, null, true)) {
                    Dialog_AddOptionLister.indexBackgroundColor = (Dialog_AddOptionLister.indexBackgroundColor + 1) % (BackgroundColors.Length + 1);
                }
                if (widgetRow.ButtonIcon(MyTexButton.RotRight, "AppearanceClothes.TipRotateApparel".Translate(), new Color?(GenUI.SubtleMouseoverColor), null, null, true)) {
                    rot.Rotate(RotationDirection.Clockwise);
                }
            }

            Rect outRect = new Rect(inRect);
            outRect.y += 32f;
            outRect.height -= 32f;

            float iconSize = AppearanceClothesMod.Settings.clothIconSize;

            this.listing = new Listing_Standard();
            this.listing.ColumnWidth = iconSize;

            float num = (iconSize + listing.verticalSpacing) * 6 - listing.verticalSpacing;
            if (num < outRect.height - 16f) {
                num = outRect.height - 16f;
            }
            Rect rect = new Rect(0f, 0f, (iconSize + 17) * ((this.options.Count / 6) + 1), num);

            if (Dialog_AddOptionLister.indexBackgroundColor > 0) {
                GUI.color = BackgroundColors[Dialog_AddOptionLister.indexBackgroundColor - 1];
                GUI.DrawTexture(outRect, BaseContent.WhiteTex);
                GUI.color = Color.white;
            }
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect, true);
            this.listing.Begin(rect);
            this.DoListingItems();
            this.listing.End();
            Widgets.EndScrollView();
        }

        public override void PostClose() {
            base.PostClose();
            UI.UnfocusCurrentControl();
        }

        protected void DoListingItems() {
            foreach (AddMenuOption current in this.options) {
                AddAction(current.thingDef, current.method);
            }
        }

        private void AddAction(ThingDef thingDef, Action action) {
            float size = AppearanceClothesMod.Settings.clothIconSize;
            if (this.listing.ButtonThing(thingDef, size, size, Color.white, bodyType, Dialog_AddOptionLister.rot)) {
                this.Close(true);
                action();
            }
            GUI.color = Color.white;
        }
    }
}
