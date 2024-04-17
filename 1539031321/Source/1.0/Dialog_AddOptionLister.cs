using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace StatueOfColonist {
    class Dialog_AddOptionLister : Window {
        protected List<AddMenuOption> options;

        protected const float ButSpacing = 0f;

        protected Vector2 scrollPosition;

        protected Listing_Standard listing;

        private BodyTypeDef bodyType;

        protected static readonly Vector2 ButSize = new Vector2(230f, 27f);

        protected readonly float ColumnSpacing = 20f;

        protected readonly float SectSpacing = 8f;

        public override Vector2 InitialSize {
            get {
                return new Vector2(640f, 480f);
            }
        }

        public override bool IsDebug {
            get {
                return true;
            }
        }

        public Dialog_AddOptionLister(IEnumerable<AddMenuOption> options,BodyTypeDef bodyType) {
            this.optionalTitle = "StatueOfColonist.DialogAddOptionListerTitle".Translate();
            this.doCloseX = true;
            this.onlyOneOfTypeAllowed = true;
            this.absorbInputAroundWindow = true;
            this.bodyType = bodyType;

            this.options = options.ToList<AddMenuOption>();
        }

        public override void DoWindowContents(Rect inRect) {
            float iconSize = StatueOfColonistMod.Settings.clothIconSize;
            Rect outRect = new Rect(inRect);

            this.listing = new Listing_Standard();
            this.listing.ColumnWidth = iconSize;

            float num = (iconSize + listing.verticalSpacing) * 6;
            if (num < outRect.height) {
                num = outRect.height;
            }
            Rect rect = new Rect(0f, 0f, (iconSize + 17) * ((this.options.Count / 6) + 1), num);

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
            float size = StatueOfColonistMod.Settings.clothIconSize;
            if (this.listing.ButtonThing(thingDef, size, size, Color.white, bodyType)) {
                this.Close(true);
                action();
            }
            GUI.color = Color.white;
        }
    }
}
