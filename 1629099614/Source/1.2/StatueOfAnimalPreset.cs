using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace StatueOfAnimal {
    public class StatueOfAnimalPreset : IExposable {
        public string name;

        public PawnKindDef pawnKindDef;
        public int lifeStageIndex;
        public Gender gender;
        public float scale;
        public Vector2 offset;
        public bool packed;

        public float weight = 1.0f;
        
        public string pawnKindDefName;
        public string bufferWeight = "1.0";

        public bool IsValid {
            get {
                return pawnKindDef != null;
            }
        }

        public StatueOfAnimalPreset() {
        }

        public StatueOfAnimalPreset(string name,Building_StatueOfAnimal statue) {
            this.name = name;
            SetStatueOfAnimal(statue);
        }

        public void SetStatueOfAnimal(Building_StatueOfAnimal statue) {
            this.pawnKindDef = statue.Data.pawnKindDef;
            this.lifeStageIndex = statue.Data.lifeStageIndex;
            this.gender = statue.Data.gender;
            this.scale = statue.Data.scale;
            this.offset = statue.Data.offset;
            this.packed = statue.Data.packed;
        }

        public void ExposeData() {
            if (Scribe.mode == LoadSaveMode.Saving) {
                if (this.pawnKindDef != null) {
                    this.pawnKindDefName = this.pawnKindDef.defName;
                }
            }
            
            Scribe_Values.Look(ref this.name, "name");
            Scribe_Values.Look(ref this.pawnKindDefName, "pawnKindDefName");
            Scribe_Values.Look(ref this.lifeStageIndex, "lifeStageIndex");
            Scribe_Values.Look(ref this.gender, "gender");
            Scribe_Values.Look(ref this.scale, "scale");
            Scribe_Values.Look(ref this.offset, "offset");
            Scribe_Values.Look(ref this.packed, "packed");
            Scribe_Values.Look(ref this.weight, "weight",1f);

            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                bufferWeight = weight.ToString();
                this.pawnKindDef = DefDatabase<PawnKindDef>.GetNamed(this.pawnKindDefName, false);
                if (this.pawnKindDef == null) {
                    Log.Warning("pawnKindDef \"" + pawnKindDefName + "\" is not found.\nThis message isn't error.");
                }
            }
        }

        public string GetToolTip() {
            StringBuilder sb = new StringBuilder();

            if (pawnKindDef != null) {
                sb.AppendLine("StatueOfAnimal.PawnKindData".Translate(pawnKindDef.LabelCap ));
                sb.AppendLine("StatueOfAnimal.GenderData".Translate(gender.GetLabel(true)));
                string lifeStageLabel = pawnKindDef.lifeStages[lifeStageIndex].label;
                sb.AppendLine("StatueOfAnimal.LifeStageData".Translate(lifeStageLabel));
                sb.AppendLine("StatueOfAnimal.PackedData".Translate(packed.ToString()));
                sb.AppendLine("StatueOfAnimal.OffsetData".Translate(offset.ToString()));
                sb.AppendLine("StatueOfAnimal.ScaleData".Translate(scale.ToStringPercent()));
            } else {
                sb.AppendLine("StatueOfAnimal.PawnKindIsNotFound".Translate(pawnKindDefName));
            }

            return sb.ToString();
        }
    }
}
