using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace StatueOfColonist {
    public class StatueOfColonistData : IExposable {
        public BodyTypeDef bodyType;
        public string headGraphicPath;
        public string hairGraphicPath;
        public CrownType crownType;
        public Color color;
        public string shaderCutoutPath;
        public List<ThingDef> wornApparelDefs;
        public ThingDef raceDef;
        public LifeStageDef lifeStageDef;
        public Gender gender;
        public Vector2 offset;

        public StatueOfColonistData() { }

        public StatueOfColonistData(Color color, string shaderCutoutPath, ThingDef raceDef = null) {
            this.color = color;
            this.shaderCutoutPath = shaderCutoutPath;
            this.lifeStageDef = DefDatabase<LifeStageDef>.GetNamed("HumanlikeAdult");

            this.raceDef = raceDef;
            if (this.raceDef == null) {
                this.raceDef = ThingDefOf.Human;
            }
        }

        public StatueOfColonistData(Pawn pawn,Color color,string shaderCutoutPath) {
            this.bodyType = GetBodyTypeDef(pawn);
            this.headGraphicPath = pawn.story.HeadGraphicPath;
            this.hairGraphicPath = pawn.story.hairDef.texPath;
            this.crownType = pawn.story.crownType;
            this.raceDef = ThingDefOf.Human;
            this.lifeStageDef = pawn.ageTracker.CurLifeStage;
            this.gender = pawn.gender;
            this.offset = Vector2.zero;

            this.color = color;
            this.shaderCutoutPath = shaderCutoutPath;
            this.wornApparelDefs = GetApparelDefs(pawn);
        }

        public StatueOfColonistData(StatueOfColonistData source,Color color,string shaderCutoutPath) {
            if (source != null) {
                this.bodyType = source.bodyType;
                this.headGraphicPath = source.headGraphicPath;
                this.hairGraphicPath = source.hairGraphicPath;
                this.crownType = source.crownType;
                this.raceDef = source.raceDef;
                this.lifeStageDef = source.lifeStageDef;
                this.gender = source.gender;
                this.offset = source.offset;
                if (source.wornApparelDefs.NullOrEmpty()) {
                    this.wornApparelDefs = new List<ThingDef>();
                } else {
                    this.wornApparelDefs = new List<ThingDef>(source.wornApparelDefs);
                }
            }

            this.color = color;
            this.shaderCutoutPath = shaderCutoutPath;
        }

        private BodyTypeDef GetBodyTypeDef(Pawn p) {
            return p.story.bodyType;
        }

        private List<ThingDef> GetApparelDefs(Pawn p) {
            return p.apparel.WornApparel.ConvertAll<ThingDef>(ap => ap.def);
        }

        public void CopyFromPreset(StatueOfColonistPreset preset) {
            this.bodyType = preset.bodyType;
            this.headGraphicPath = preset.headGraphicPath;
            this.hairGraphicPath = preset.hairDef.texPath;
            this.crownType = preset.crownType;
            this.raceDef = preset.raceDef;
            this.lifeStageDef = preset.lifeStageDef;
            this.gender = preset.gender;

            this.wornApparelDefs = preset.apparels.ConvertAll(data => data.apparelDef);
        }

        public void ExposeData() {
            Scribe_Defs.Look(ref bodyType, "bodyType");
            Scribe_Values.Look(ref headGraphicPath, "headGraphicPath");
            Scribe_Values.Look(ref hairGraphicPath, "hairGraphicPath");
            Scribe_Values.Look(ref crownType, "crownType");
            Scribe_Values.Look(ref color, "color");
            Scribe_Values.Look(ref shaderCutoutPath, "shaderCutoutPath");
            Scribe_Defs.Look(ref raceDef, "raceDef");
            Scribe_Defs.Look(ref lifeStageDef, "lifeStageDef");
            Scribe_Values.Look(ref gender, "gender", Gender.Male);
            Scribe_Collections.Look(ref wornApparelDefs, "wornApparelDefs", LookMode.Def);
            Scribe_Values.Look(ref offset, "offset");
            if (Scribe.mode == LoadSaveMode.LoadingVars && raceDef == null) {
                raceDef = ThingDefOf.Human;
                lifeStageDef = DefDatabase<LifeStageDef>.GetNamed("HumanlikeAdult");
            }
        }

        public bool CanWearWithoutDroppingAnything(ThingDef apDef) {
            for (int i = 0; i < wornApparelDefs.Count; i++) {
                if (!ApparelUtility.CanWearTogether(apDef, wornApparelDefs[i], ThingDefOf.Human.race.body)) {
                    return false;
                }
            }
            return true;
        }
    }
}
