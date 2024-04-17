using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StatueOfColonist {
    public class StatueOfColonistGraphicSet {
        public Building_StatueOfColonist parent;

        public StatueOfColonistData data;

        public Graphic nakedGraphic;

        public Graphic rottingGraphic;

        public Graphic dessicatedGraphic;

        public Graphic headGraphic;

        public Graphic desiccatedHeadGraphic;

        public Graphic skullGraphic;

        public Graphic headStumpGraphic;

        public Graphic desiccatedHeadStumpGraphic;

        public Graphic hairGraphic;

        public Graphic beardGraphic;

        public Graphic bodyTattooGraphic;

        public Graphic faceTattooGraphic;

        public Graphic_Shadow shadowGraphic;

        public List<Graphic> bodyAddonGraphics;

        public List<ApparelGraphicRecord> apparelGraphics = new List<ApparelGraphicRecord>();

        private List<Material> cachedMatsBodyBase = new List<Material>();

        private int cachedMatsBodyBaseHash = -1;

        public static readonly Color RottingColor = new Color(0.34f, 0.32f, 0.3f);

        public bool forceResolve = false;

        public bool AllResolved {
            get {
                return this.nakedGraphic != null;
            }
        }

        public bool ShouldResolve {
            get {
                return !AllResolved || forceResolve;
            }
        }

        public GraphicMeshSet HairMeshSet {
            get {
                if (this.data.crownType == CrownType.Average) {
                    return MeshPool.humanlikeHairSetAverage;
                }
                if (this.data.crownType == CrownType.Narrow) {
                    return MeshPool.humanlikeHairSetNarrow;
                }
                Log.Error("Unknown crown type: " + this.data.crownType);
                return MeshPool.humanlikeHairSetAverage;
            }
        }

        public StatueOfColonistGraphicSet(StatueOfColonistData data,Building_StatueOfColonist parent,bool forceResolve=false) {
            this.data = data;
            this.parent = parent;
            this.nakedGraphic = null;
            this.forceResolve = forceResolve;
        }

        public List<Material> MatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh) {
            int num = facing.AsInt + 1000 * (int)bodyCondition;
            if (num != this.cachedMatsBodyBaseHash) {
                this.cachedMatsBodyBase.Clear();
                this.cachedMatsBodyBaseHash = num;
                if (bodyCondition == RotDrawMode.Fresh) {
                    this.cachedMatsBodyBase.Add(this.nakedGraphic.MatAt(facing, null));
                } else if (bodyCondition == RotDrawMode.Rotting || this.dessicatedGraphic == null) {
                    this.cachedMatsBodyBase.Add(this.rottingGraphic.MatAt(facing, null));
                } else if (bodyCondition == RotDrawMode.Dessicated) {
                    this.cachedMatsBodyBase.Add(this.dessicatedGraphic.MatAt(facing, null));
                }
                for (int i = 0; i < this.apparelGraphics.Count; i++) {
                    if (this.apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Shell && this.apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead) {
                        this.cachedMatsBodyBase.Add(this.apparelGraphics[i].graphic.MatAt(facing, null));
                    }
                }
            }
            return this.cachedMatsBodyBase;
        }

        public Material HeadMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false) {
            Material material = null;
            if (bodyCondition == RotDrawMode.Fresh) {
                if (stump) {
                    material = this.headStumpGraphic.MatAt(facing, null);
                } else {
                    material = this.headGraphic.MatAt(facing, null);
                }
            } else if (bodyCondition == RotDrawMode.Rotting) {
                if (stump) {
                    material = this.desiccatedHeadStumpGraphic.MatAt(facing, null);
                } else {
                    material = this.desiccatedHeadGraphic.MatAt(facing, null);
                }
            } else if (bodyCondition == RotDrawMode.Dessicated && !stump) {
                material = this.skullGraphic.MatAt(facing, null);
            }
            return material;
        }

        public Material HairMatAt(Rot4 facing) {
            if (this.hairGraphic == null) {
                return null;
            }
            return this.hairGraphic.MatAt(facing, null);
        }

        public void ClearCache() {
            this.cachedMatsBodyBaseHash = -1;
        }

        public void ResolveAllGraphics(float scale = 1f) {
            Log.Message("ResolveAllGraphics");
            Shader shaderCutout = ShaderDatabase.LoadShader(this.data.shaderCutoutPath);

            this.ClearCache();
            this.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(this.data.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, this.data.color);
            this.rottingGraphic = GraphicDatabase.Get<Graphic_Multi>(this.data.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, this.data.color);
            this.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(this.data.bodyType.bodyDessicatedGraphicPath, ShaderDatabase.Cutout);
            this.headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(this.data.headGraphicPath, this.data.color,false);
            this.desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(this.data.headGraphicPath, PawnGraphicSet.RottingColorDefault, false);
            this.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
            this.headStumpGraphic = GraphicDatabaseHeadRecords.GetStump(this.data.color);
            this.desiccatedHeadStumpGraphic = GraphicDatabaseHeadRecords.GetStump(PawnGraphicSet.RottingColorDefault);
            this.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(this.data.hairGraphicPath, shaderCutout, Vector2.one, this.data.color);
            if (this.bodyAddonGraphics == null) {
                this.bodyAddonGraphics = new List<Graphic>();
            }

            this.ResolveTatooGraphic();
            this.ResolveBoardGraphic();

            this.ResolveApparelGraphics();

            this.forceResolve = false;
        }

        public void ResolveTatooGraphic() {
            if (ModsConfig.IdeologyActive) {
                Color skinColor = this.data.color;
                skinColor.a *= 0.8f;
                if (this.data.faceTattooDef != null && this.data.faceTattooDef != TattooDefOf.NoTattoo_Face) {
                    this.faceTattooGraphic = GraphicDatabase.Get<Graphic_Multi>(this.data.faceTattooDef.texPath, ShaderDatabase.CutoutSkinOverlay, Vector2.one, skinColor, Color.white, null, this.data.headGraphicPath);
                } else {
                    this.faceTattooGraphic = null;
                }
                if (this.data.bodyTattooDef != null && this.data.bodyTattooDef != TattooDefOf.NoTattoo_Body) {
                    this.bodyTattooGraphic = GraphicDatabase.Get<Graphic_Multi>(this.data.bodyTattooDef.texPath, ShaderDatabase.CutoutSkinOverlay, Vector2.one, skinColor, Color.white, null, this.data.bodyType.bodyNakedGraphicPath);
                } else {
                    this.bodyTattooGraphic = null;
                }
            }
        }

        public void ResolveBoardGraphic() {
            if (this.data.beardDef != null) {
                Color c = this.data.color;
                c.r *= this.data.beardBlightness;
                c.g *= this.data.beardBlightness;
                c.b *= this.data.beardBlightness;
                this.beardGraphic = GraphicDatabase.Get<Graphic_Multi>(this.data.beardDef.texPath, ShaderDatabase.Transparent, Vector2.one, c);
            }
        }

        public void ResolveApparelGraphics() {
            Shader shaderCutout = ShaderDatabase.LoadShader(this.data.shaderCutoutPath);

            this.ClearCache();
            this.apparelGraphics.Clear();
            foreach (ThingDef apparelDef in this.data.wornApparelDefs) {
                Apparel apparel = MakeApparel(apparelDef, this.data.color);
                ApparelGraphicRecord item;
                if (TryGetGraphicApparel(apparel, this.data.color, this.data.bodyType, shaderCutout, out item)) {
                    this.apparelGraphics.Add(item);
                }
            }
        }

        private static Apparel MakeApparel(ThingDef def, Color color) {
            Apparel apparel = (Apparel)ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def));
            apparel.SetColor(color, false);
            return apparel;
        }

        public static bool TryGetGraphicApparel(Apparel apparel, Color color, BodyTypeDef bodyType, Shader shader, out ApparelGraphicRecord rec) {
            if (bodyType == null) {
                Log.Error("Getting apparel graphic with undefined body type.");
                bodyType = BodyTypeDefOf.Male;
            }
            if (apparel.def.apparel.wornGraphicPath.NullOrEmpty()) {
                rec = new ApparelGraphicRecord(null, null);
                return false;
            }
            string path;
            if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead) {
                path = apparel.def.apparel.wornGraphicPath;
            } else {
                path = apparel.def.apparel.wornGraphicPath + "_" + bodyType.ToString();
            }
            Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, color);
            rec = new ApparelGraphicRecord(graphic, apparel);
            return true;
        }
    }
}
