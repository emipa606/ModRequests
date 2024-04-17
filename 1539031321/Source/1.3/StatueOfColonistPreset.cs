using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace StatueOfColonist {
    public class StatueOfColonistPreset : IExposable {
        public string name;
        public BodyTypeDef bodyType;
        public string headGraphicPath;
        public string hairDefName;
        public CrownType crownType;
        public List<StatueOfColonistApparelData> apparels = new List<StatueOfColonistApparelData>();
        public float weight = 1.0f;
        public string raceDefName;
        public string lifeStageDefName;
        public Gender gender;

        public HeadType headType;
        public HairDef hairDef;
        public ThingDef raceDef;
        public LifeStageDef lifeStageDef;

        public BeardDef beardDef;
        public float beardBlightness;
        public TattooDef faceTattooDef;
        public TattooDef bodyTattooDef;

        public string alienCrownType;
        public List<int> addonVariants;

        public string bufferWeight = "1.0";

        public bool IsValid {
            get {
                return apparels.All(ap => ap.IsValid) && hairDef != null;
            }
        }

        public StatueOfColonistPreset() {
            this.apparels = new List<StatueOfColonistApparelData>();
        }

        public StatueOfColonistPreset(string name,Building_StatueOfColonist statue) {
            this.name = name;
            SetStatueOfColonist(statue);
        }

        public void SetStatueOfColonist(Building_StatueOfColonist statue) {
            this.bodyType = statue.Data.bodyType;
            this.headGraphicPath = statue.Data.headGraphicPath;
            this.hairDefName = Util.GetHairDefFromGraphicPath(statue.Data.hairGraphicPath).defName;
            this.crownType = statue.Data.crownType;
            this.gender = statue.Data.gender;
            this.apparels = new List<StatueOfColonistApparelData>();
            foreach (ThingDef apparel in statue.Data.wornApparelDefs) {
                apparels.Add(new StatueOfColonistApparelData(apparel));
            }

            this.beardDef = statue.Data.beardDef;
            this.beardBlightness = statue.Data.beardBlightness;
            this.faceTattooDef = statue.Data.faceTattooDef;
            this.bodyTattooDef = statue.Data.bodyTattooDef;

            headType = Util.GetHeadTypeFromGraphicPath(headGraphicPath);
            hairDef = DefDatabase<HairDef>.GetNamed(hairDefName,false);
            raceDef = statue.Data.raceDef;
            raceDefName = raceDef.defName;
            lifeStageDef = statue.Data.lifeStageDef;
            lifeStageDefName = lifeStageDef.defName;

        }

        public void ExposeData() {
            Scribe_Values.Look<string>(ref this.name, "name");
            Scribe_Defs.Look<BodyTypeDef>(ref bodyType, "bodyType");
            Scribe_Values.Look<string>(ref this.headGraphicPath, "headGraphicPath");
            Scribe_Values.Look<string>(ref this.hairDefName, "hairDefName");
            Scribe_Values.Look<CrownType>(ref this.crownType, "crownType");
            Scribe_Values.Look<float>(ref this.weight, "weight",1f);
            Scribe_Values.Look<string>(ref this.raceDefName, "raceDefName","Human");
            Scribe_Values.Look<string>(ref this.lifeStageDefName, "lifeStageDefName", "HumanlikeAdult");
            Scribe_Values.Look<Gender>(ref this.gender, "gender", Gender.Male);
            Scribe_Collections.Look<StatueOfColonistApparelData>(ref this.apparels, "appearanceClothes", LookMode.Deep);

            Scribe_Defs.Look(ref beardDef, "beardDef");
            Scribe_Values.Look<float>(ref this.beardBlightness, "beardBlightness", 1f);
            Scribe_Defs.Look(ref faceTattooDef, "faceTattooDef");
            Scribe_Defs.Look(ref bodyTattooDef, "bodyTattooDef");

            Scribe_Values.Look(ref this.alienCrownType, "alienCrownType");
            Scribe_Collections.Look(ref this.addonVariants, "addonVariants");

            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                if (this.raceDefName == "Human") {
                    headType = Util.GetHeadTypeFromGraphicPath(headGraphicPath);
                }
                hairDef = DefDatabase<HairDef>.GetNamed(hairDefName,false);
                raceDef = DefDatabase<ThingDef>.GetNamed(raceDefName, false);
                lifeStageDef = DefDatabase<LifeStageDef>.GetNamed(lifeStageDefName, false);
                bufferWeight = weight.ToString();
            }
        }

        public string GetToolTip() {
            StringBuilder sb = new StringBuilder();

            if (raceDef != ThingDefOf.Human) {
                if (raceDef != null) {
                    sb.AppendLine("StatueOfColonist.RaceData".Translate(raceDef.LabelCap));
                } else {
                    sb.AppendLine("StatueOfColonist.RaceIsNotFound".Translate(raceDefName));
                }
                sb.AppendLine("StatueOfColonist.GenderData".Translate(gender.GetLabel(false)));
            }
            if (lifeStageDef != null) {
                sb.AppendLine("StatueOfColonist.LifeStageData".Translate(lifeStageDef.LabelCap));
            } else {
                sb.AppendLine("StatueOfColonist.LifeStageIsNotFound".Translate(lifeStageDefName));
            }
            if (hairDef != null) {
                sb.AppendLine("StatueOfColonist.HairData".Translate( hairDef.LabelCap ));
            } else {
                sb.AppendLine("StatueOfColonist.HairIsNotFound".Translate( hairDefName ));
            }
            if (raceDef == ThingDefOf.Human) {
                sb.AppendLine("StatueOfColonist.HeadData".Translate(headType.GetName()));
            } else {
                sb.AppendLine("StatueOfColonist.HeadData".Translate(alienCrownType));
            }
            sb.AppendLine("StatueOfColonist.BodyData".Translate( bodyType.GetName() ));


            if (ModsConfig.IdeologyActive) {
                if (faceTattooDef != null) {
                    sb.AppendLine("StatueOfColonist.FaceTatooData".Translate(faceTattooDef.LabelCap));
                }
                if (bodyTattooDef != null) {
                    sb.AppendLine("StatueOfColonist.BodyTatooData".Translate(bodyTattooDef.LabelCap));
                }
            }
            if (beardDef != null) {
                sb.AppendLine("StatueOfColonist.BeardData".Translate(beardDef.LabelCap));
                sb.AppendLine("StatueOfColonist.BeardBlightnessData".Translate(beardBlightness));
            }

            foreach (StatueOfColonistApparelData ap in apparels) {
                if (ap.IsValid) {
                    sb.AppendLine("StatueOfColonist.ApparelData".Translate( ap.apparelDef.LabelCap ));
                }
            }

            if (apparels.Any(ap => !ap.IsValid)) {
                if (sb.Length > 0) {
                    sb.AppendLine();
                }
                foreach (StatueOfColonistApparelData ap in apparels) {
                    if (!ap.IsValid) {
                        sb.AppendLine("StatueOfColonist.ApparelIsNotFound".Translate( ap.apparelDefName ));
                    }
                }
            }

            return sb.ToString();
        }
    }
}
