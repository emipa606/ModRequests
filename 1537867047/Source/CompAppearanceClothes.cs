using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AppearanceClothes {
    public class CompAppearanceClothes : ThingComp {
        private bool showAppearanceClothes = false;
        private List<Thing> appearanceClothes = new List<Thing>();
        public BodyTypeDef appearanceBodyTypeDef;
        private AppearanceClothesPreset tempAppearanceClothes = null;

        public BodyTypeDef AppearanceBodyTypeDef {
            get {
                return appearanceBodyTypeDef;
            }
        }

        public bool ShowAppearanceClothes {
            get {
                return this.showAppearanceClothes;
            }
        }

        public List<Thing> AppearanceClothes {
            get {
                if (this.appearanceClothes == null) {
                    this.appearanceClothes = new List<Thing>();
                }
                return this.appearanceClothes;
            }
        }

        private Pawn ParentPawn {
            get {
                return this.parent as Pawn;
            }
        }

        public void ToggleShowAppearanceClothes() {
            showAppearanceClothes = !showAppearanceClothes;
            UpdateAppearanceClothes();
        }

        public void AddAppearanceClothes(Thing apparel) {
            AppearanceClothes.Add(apparel);
            UpdateAppearanceClothes();
        }

        public void CopyAppearanceClothesFromApparel() {
            AppearanceClothes.Clear();
            foreach (Apparel ap in ParentPawn.apparel.WornApparel) {
                Apparel apparel = (Apparel)ThingMaker.MakeThing(ap.def, ap.Stuff);
                apparel.SetColor(ap.DrawColor,false);
                if (apparel.def.CanBeStyled()) {
                    apparel.SetStyleDef(ap.StyleDef);
                }
                AppearanceClothes.Add(apparel);
            }
            UpdateAppearanceClothes();
        }

        public void CopyAppearanceClothesFromPreset(AppearanceClothesPreset preset,bool update = true) {
            if (preset == null) {
                return;
            }
            AppearanceClothes.Clear();
            foreach (AppearanceClothData data in preset.appearanceClothes) {
                Apparel apparel = data.MakeApparel();
                if (apparel != null) {
                    AppearanceClothes.Add(apparel);
                }
            }
            if (update) {
                UpdateAppearanceClothes();
            }
        }

        public void RemoveAppearanceClothes(Thing apparel) {
            AppearanceClothes.Remove(apparel);
            UpdateAppearanceClothes();
        }

        public void UpdateAppearanceClothes() {
            Pawn pawn = ParentPawn;
            if (pawn != null && pawn.story != null && appearanceBodyTypeDef == null) {
                appearanceBodyTypeDef = pawn.story.bodyType;
            }
            RemoveAllUnavailableApparels();
            if (pawn.Drawer != null && pawn.Drawer.renderer != null && pawn.Drawer.renderer.graphics != null) {
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                PortraitsCache.SetDirty(pawn);
                GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
            }
        }

        public void RemoveAllUnavailableApparels() {
            Predicate<Thing> validator = delegate (Thing t) {
                return !t.def.IsAvailableForBody(appearanceBodyTypeDef);
            };
            appearanceClothes.RemoveAll(validator);
        }

        public bool CanWearWithoutDroppingAnything(ThingDef apDef) {
            if (apDef == null || this.ParentPawn?.RaceProps?.body == null || AppearanceClothes == null) {
                return false;
            }
            for (int i = 0; i < AppearanceClothes.Count; i++) {
                if (AppearanceClothes[i] != null && AppearanceClothes[i].def != null) {
                    if (!ApparelUtility.CanWearTogether(apDef, AppearanceClothes[i].def, this.ParentPawn.RaceProps.body)) {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void PostExposeData() {
            base.PostExposeData();
            Pawn pawn = parent as Pawn;
            if (pawn == null || pawn.RaceProps == null || !pawn.RaceProps.Humanlike) {
                return;
            }

            Scribe_Values.Look(ref this.showAppearanceClothes, "showAppearanceClothes",false);
            
            if (Scribe.mode == LoadSaveMode.Saving) {
                tempAppearanceClothes = new AppearanceClothesPreset("", this);
            }
            Scribe_Deep.Look(ref tempAppearanceClothes, "appearanceClothes");
            if (Scribe.mode == LoadSaveMode.PostLoadInit) {
                if (tempAppearanceClothes != null) {
                    CopyAppearanceClothesFromPreset(tempAppearanceClothes,false);
                }
            }

            Scribe_Defs.Look(ref this.appearanceBodyTypeDef, "appearanceBodyType");

            if (this.appearanceBodyTypeDef == null) {
                if (this.ParentPawn.story != null) {
                    this.appearanceBodyTypeDef = this.ParentPawn.story.bodyType;
                }
            }
        }
    }
}
