using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace StatueOfAnimal {
    public class StatueOfAnimalData : IExposable {
        public PawnKindDef pawnKindDef;
        public int lifeStageIndex;
        public Gender gender;
        public Vector2 offset;
        public float scale;
        public bool packed;
        public Color color;

        public PawnKindLifeStage CurKindLifeStage {
            get {
                return pawnKindDef.lifeStages[this.lifeStageIndex];
            }
        }

        public StatueOfAnimalData() {
            this.pawnKindDef = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(x => x.RaceProps.Animal).RandomElement();
            this.lifeStageIndex = this.pawnKindDef.lifeStages.Count - 1;
            this.gender = (Gender)Enum.ToObject(typeof(Gender), UnityEngine.Random.Range(0, 3));
            this.offset = Vector2.zero;
            this.scale = 1f;
            this.packed = false;
            this.color = Color.white;
        }


        public StatueOfAnimalData(Color color) {
            this.pawnKindDef = DefDatabase<PawnKindDef>.AllDefsListForReading.FindAll(x => x.RaceProps.Animal).RandomElement();
            this.lifeStageIndex = this.pawnKindDef.lifeStages.Count - 1;
            this.gender = (Gender)Enum.ToObject(typeof(Gender), UnityEngine.Random.Range(0,3));
            this.offset = Vector2.zero;
            this.scale = 1f;
            this.packed = false;
            this.color = color;
        }

        public StatueOfAnimalData(Pawn pawn,Color color) {
            this.pawnKindDef = pawn.kindDef;
            this.lifeStageIndex = pawn.ageTracker.CurLifeStageIndex;
            this.gender = pawn.gender;
            this.offset = Vector2.zero;
            this.scale = 1f;
            this.packed = false;
            this.color = color;
        }

        public StatueOfAnimalData(StatueOfAnimalData source,Color color) {
            this.pawnKindDef = source.pawnKindDef;
            this.lifeStageIndex = source.lifeStageIndex;
            this.gender = source.gender;
            this.offset = source.offset;
            this.scale = source.scale;
            this.packed = source.packed;
            this.color = color;
        }

        public bool IsSameTexture(int otherLifeStageIndex) {
            PawnKindLifeStage otherLifeStage = pawnKindDef.lifeStages[otherLifeStageIndex];
            if (gender == Gender.Female && CurKindLifeStage.femaleGraphicData != null) {
                return CurKindLifeStage.femaleGraphicData.texPath == otherLifeStage.femaleGraphicData.texPath;
            }
            return CurKindLifeStage.bodyGraphicData.texPath == otherLifeStage.bodyGraphicData.texPath;
        }

        public void CopyFromPreset(StatueOfAnimalPreset preset) {
            this.pawnKindDef = preset.pawnKindDef;
            this.lifeStageIndex = preset.lifeStageIndex;
            this.gender = preset.gender;
            this.offset = preset.offset;
            this.scale = preset.scale;
            this.packed = preset.packed;
        }

        public void ExposeData() {
            Scribe_Defs.Look(ref pawnKindDef, "pawnKindDef");
            Scribe_Values.Look(ref lifeStageIndex, "lifeStageIndex");
            Scribe_Values.Look(ref gender, "gender");
            Scribe_Values.Look(ref offset, "offset");
            Scribe_Values.Look(ref scale, "scale");
            Scribe_Values.Look(ref color, "color");
            Scribe_Values.Look(ref packed, "packed");
        }
    }
}
