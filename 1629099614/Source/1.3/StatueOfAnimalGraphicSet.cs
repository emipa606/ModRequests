using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StatueOfAnimal {
    class StatueOfAnimalGraphicSet {
        public StatueOfAnimalData data;

        public Graphic nakedGraphic;

        public Graphic packGraphic;

        private List<Material> cachedMatsBodyBase = new List<Material>();

        private int cachedMatsBodyBaseHash = -1;

        public bool AllResolved {
            get {
                return this.nakedGraphic != null;
            }
        }

        public StatueOfAnimalGraphicSet(StatueOfAnimalData data) {
            this.data = data;
            ResolveAllGraphics();
        }

        public List<Material> MatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh) {
            int num = facing.AsInt + 1000 * (int)bodyCondition;
            if (num != this.cachedMatsBodyBaseHash) {
                this.cachedMatsBodyBase.Clear();
                this.cachedMatsBodyBaseHash = num;
                if (bodyCondition == RotDrawMode.Fresh) {
                    this.cachedMatsBodyBase.Add(this.nakedGraphic.MatAt(facing, null));
                }
            }
            return this.cachedMatsBodyBase;
        }

        public void ClearCache() {
            this.cachedMatsBodyBaseHash = -1;
        }

        public void ResolveAllGraphics() {
            Log.Message("ResolveAllGraphics");
            this.ClearCache();
            Shader shader = null;
            ShaderTypeDef shaderType;
            if (this.data.color.a == 1f) {
                shaderType = ShaderTypeDefOf.Cutout;
                shader = ShaderDatabase.Cutout;
            } else {
                shaderType = ShaderTypeDefOf.Transparent;
                shader = ShaderDatabase.Transparent;
            }

            PawnKindLifeStage curKindLifeStage = this.data.CurKindLifeStage;
            if (this.data.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null) {
                this.nakedGraphic = GetNakedGraphic(curKindLifeStage.bodyGraphicData.Graphic,shader, this.data.color);
            } else {
                this.nakedGraphic = GetNakedGraphic(curKindLifeStage.femaleGraphicData.Graphic, shader, this.data.color);
            }

            if (this.data.pawnKindDef.RaceProps.packAnimal) {
                this.packGraphic = GraphicDatabase.Get<Graphic_GrayscaleMulti>(this.nakedGraphic.path + "Pack", shader, this.nakedGraphic.drawSize, this.data.color);
            }
        }

        public Graphic GetNakedGraphic(Graphic graphic,Shader newShader, Color newColor) {
            return GraphicDatabase.Get<Graphic_GrayscaleMulti>(graphic.path, newShader, graphic.drawSize, newColor, Color.white);
        }
    }
}
