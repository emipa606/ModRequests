using RimWorld;
using System;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatueOfAnimal {
    public class Building_StatueOfAnimal : Building_Art {
        public string name;

        private StatueOfAnimalRenderer renderer;
        private StatueOfAnimalData dataStatue;
        private StatueOfAnimalGraphicSet graphicsStatue;
        private StatueOfAnimalGraphicSet graphicsBlueprint;
        private StatueOfAnimalGraphicSet graphicsCanDesignateGhost;
        private StatueOfAnimalGraphicSet graphicsCanNotDesignateGhost;

        private Graphic_Shadow shadowGraphic;
        private Graphic_Shadow specialShadowGraphic;

        private Graphic baseGraphic;
        private Graphic baseGraphicBlueprint;
        private Graphic baseGraphicCanDesignateGhost;
        private Graphic baseGraphicCanNotDesignateGhost;
        private BaseGraphicType baseGraphicType;

        [TweakValue("Graphics", -5f, 5f)]
        private static float StatueYOffset = 0.0385f;

        private static readonly Color ColorBlueprint = new Color(0.5f, 0.5f, 1f, 0.35f);
        private static readonly Color ColorCanDesignateGhost = new Color(0.5f, 1f, 0.6f, 0.4f);
        private static readonly Color ColorCanNotDesignateGhost = new Color(1f, 0f, 0f, 0.4f);

        public override string LabelNoCount {
            get {
                string label = GenLabel.ThingLabel(this.def, this.Stuff, 1);
                QualityCategory quality;
                string qualityLabel = "";
                if (this.TryGetQuality(out quality)) {
                    qualityLabel = quality.GetLabel();
                }
                if (!name.NullOrEmpty()) {
                    return "StatueOfAnimal.StatueNameFormat".Translate( label, name, qualityLabel );
                } else {
                    return "StatueOfAnimal.StatueNameFormatIfNoStatueName".Translate( label, name, qualityLabel );
                }
            }
        }

        public enum RenderMode {
            Normal,
            Blueprint,
            CanDesignateGhost,
            CanNotDesignateGhost
        }

        public bool ShouldBeNamed {
            get {
                return name.NullOrEmpty() && StatueOfAnimalMod.Settings.autoName;
            }
        }

        public bool IsValid {
            get {
                return (dataStatue != null) && (dataStatue.pawnKindDef != null) && dataStatue.pawnKindDef.RaceProps.Animal;
            }
        }

        public StatueOfAnimalDef Def {
            get {
                return def as StatueOfAnimalDef;
            }
        }

        public StatueOfAnimalData Data {
            get {
                return dataStatue;
            }
            set {
                dataStatue = value;
            }
        }

        public bool HasPackedGraphic {
            get {
                return graphicsStatue.packGraphic != null;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!map.dynamicDrawManager.DrawThingsForReading.Contains(this)) {
                map.dynamicDrawManager.RegisterDrawable(this);
            }

            if (!IsValid) {
                InitStatue();
            }
            //ResolveGraphics();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish) {
            if (this.Map.dynamicDrawManager.DrawThingsForReading.Contains(this)) {
                this.Map.dynamicDrawManager.DeRegisterDrawable(this);
            }
            base.DeSpawn(mode);
        }

        public void InitStatue() {
            bool isDecided = false;
            
            float value = UnityEngine.Random.value;
            if (value < StatueOfAnimalMod.Settings.possibilityOfStatueFromPreset / 100f) {
                List<StatueOfAnimalPreset> candicates = StatueOfAnimalPref.presets.FindAll(p => p.IsValid);
                if (!candicates.NullOrEmpty()) {
                    StatueOfAnimalPreset preset = candicates.RandomElementByWeight(p => p.weight);
                    CopyStatueOfAnimalFromPreset(preset);
                    if (ShouldBeNamed) {
                        this.name = preset.name;
                    } else {
                        this.name = "";
                    }
                    isDecided = true;
                }
            }

            if(!isDecided) {
                this.dataStatue = new StatueOfAnimalData(this.DrawColor);
                if (ShouldBeNamed) {
                    this.name = Data.pawnKindDef.LabelCap;
                } else {
                    this.name = "";
                }
            }
        }

        public void CopyStatueOfAnimalFromPreset(StatueOfAnimalPreset preset) {
            if (dataStatue == null) {
                dataStatue = new StatueOfAnimalData(this.DrawColor);
            }
            dataStatue.CopyFromPreset(preset);
            ResolveGraphics();
        }

        public override void ExposeData() {
            base.ExposeData();

            Scribe_Values.Look(ref this.name, "name","");
            Scribe_Deep.Look(ref this.dataStatue, "dataStatue");
            Scribe_Values.Look(ref this.baseGraphicType, "baseGraphicType", BaseGraphicType.None);
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false) {
            base.DrawAt(drawLoc, flip);
            DrawBase(drawLoc, RenderMode.Normal);

            Vector3 pos = drawLoc + new Vector3(Def.offsetX, StatueYOffset, Def.offsetZ);
            Render(pos, this.Rotation, false, Def.scale, Data.packed);
        }

        public override void Draw() {
            base.Draw();
            DrawBase(this.DrawPos, RenderMode.Normal);

            Vector3 pos = DrawPos + new Vector3(Def.offsetX, StatueYOffset, Def.offsetZ);
            Render(pos, this.Rotation, false, Def.scale, Data.packed);
        }

        public void DrawBase(Vector3 pos,RenderMode mode) {
            if (this.baseGraphic == null) {
                ResolveBaseGraphic();
            }
            if (mode == RenderMode.Normal) {
                this.baseGraphic.Draw(pos, Rot4.North, this, 0f);
            } else if (mode == RenderMode.Blueprint) {
                this.baseGraphicBlueprint.Draw(pos, Rot4.North, this, 0f);
            } else if (mode == RenderMode.CanDesignateGhost) {
                this.baseGraphicCanDesignateGhost.Draw(pos, Rot4.North, this, 0f);
            } else if (mode == RenderMode.CanNotDesignateGhost) {
                this.baseGraphicCanNotDesignateGhost.Draw(pos, Rot4.North, this, 0f);
            }
        }

        public void ResolveBaseGraphic() {
            this.baseGraphic = GetBaseGraphic(Data.color);
            this.baseGraphicBlueprint = GetBaseGraphic(ColorBlueprint);
            this.baseGraphicCanDesignateGhost = GetBaseGraphic(ColorCanDesignateGhost);
            this.baseGraphicCanNotDesignateGhost = GetBaseGraphic(ColorCanNotDesignateGhost);
        }

        public Graphic GetBaseGraphic(Color color) {
            int size = def.defName == "TMB_StatueOfAnimal1x" ? 1 : 2;
            string filePath = "Things/Building/StatueOfAnimal";
            if (size == 1) {
                filePath += "X1/";
            } else if (size == 2) {
                filePath += "X2/";
            } else {
                return null;
            }
            if (baseGraphicType == BaseGraphicType.None) {
                filePath += "None";
            } else if (baseGraphicType == BaseGraphicType.Circle) {
                filePath += "Circle";
            } else if (baseGraphicType == BaseGraphicType.Rect) {
                filePath += "Rect";
            } else {
                return null;
            }
            return GraphicDatabase.Get<Graphic_Single>(filePath, ShaderDatabase.Transparent, def.graphicData.drawSize, color, Color.white);
        }

        public void ResolveGraphics(Pawn pawn) {
            this.dataStatue = new StatueOfAnimalData(pawn, this.DrawColor);
            ResolveGraphics();
        }

        public void ResolveGraphics(RenderMode mode = RenderMode.Normal) {
            this.renderer = new StatueOfAnimalRenderer();
            if (mode == RenderMode.Normal) {
                this.graphicsStatue = new StatueOfAnimalGraphicSet(this.dataStatue);
                GraphicData graphicData = Data.CurKindLifeStage.GetGraphicData(this.Data.gender);
                if (graphicData != null && graphicData.shadowData != null) {
                    this.shadowGraphic = new Graphic_Shadow(graphicData.shadowData);
                } else {
                    this.shadowGraphic = null;
                }
                if (Data.pawnKindDef.RaceProps.specialShadowData != null) {
                    this.specialShadowGraphic = new Graphic_Shadow(Data.pawnKindDef.RaceProps.specialShadowData);
                } else {
                    this.specialShadowGraphic = null;
                }
                this.graphicsBlueprint = null;
                this.graphicsCanDesignateGhost = null;
                this.graphicsCanNotDesignateGhost = null;
            } else if (mode == RenderMode.Blueprint) {
                this.graphicsBlueprint = new StatueOfAnimalGraphicSet(new StatueOfAnimalData(this.dataStatue, ColorBlueprint));
            } else if (mode == RenderMode.CanDesignateGhost) {
                this.graphicsCanDesignateGhost = new StatueOfAnimalGraphicSet(new StatueOfAnimalData(this.dataStatue, ColorCanDesignateGhost));
            } else if (mode == RenderMode.CanNotDesignateGhost) {
                this.graphicsCanNotDesignateGhost = new StatueOfAnimalGraphicSet(new StatueOfAnimalData(this.dataStatue, ColorCanNotDesignateGhost));
            }

            //Resources.UnloadUnusedAssets();
            //System.GC.Collect();
        }

        public void Render(Vector3 rootLoc, Rot4 bodyFacing, bool portrait, float scale, bool packed, RenderMode mode = RenderMode.Normal) {
            if (GetGraphicSet(mode) == null || !GetGraphicSet(mode).AllResolved) {
                ResolveGraphics(mode);
            }

            StatueOfAnimalGraphicSet graphics = GetGraphicSet(mode);

            Vector3 loc = rootLoc + new Vector3(Data.offset.x, 0f,Data.offset.y);
            Graphic_Shadow sg = null;
            Graphic_Shadow ssg = null;
            if (mode == RenderMode.Normal) {
                sg = this.shadowGraphic;
                ssg = this.specialShadowGraphic;
            }
            renderer.Render(graphics, loc, 0f, bodyFacing, portrait, scale * Data.scale,packed, sg, ssg, this);
        }

        private StatueOfAnimalGraphicSet GetGraphicSet(RenderMode mode) {
            StatueOfAnimalGraphicSet graphics = this.graphicsStatue;
            if (mode == RenderMode.Blueprint) {
                graphics = this.graphicsBlueprint;
            } else if (mode == RenderMode.CanDesignateGhost) {
                graphics = this.graphicsCanDesignateGhost;
            } else if (mode == RenderMode.CanNotDesignateGhost) {
                graphics = this.graphicsCanNotDesignateGhost;
            }
            return graphics;
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos() {
            foreach (Gizmo c in base.GetGizmos()) {
                yield return c;
            }

            foreach (Gizmo gizmo in GetStatueGizmos()) {
                yield return gizmo;
            }
        }

        public IEnumerable<Gizmo> GetStatueGizmos() {
            Command_Action gizmoBaseGraphicType = new Command_Action {
                icon = GetGizmoIcon(),
                defaultLabel = GetGizmoLabel()
            };

            gizmoBaseGraphicType.action = delegate {
                int index = (int)this.baseGraphicType;
                index = (index + 1) % Enum.GetNames(typeof(BaseGraphicType)).Length;
                this.baseGraphicType = (BaseGraphicType)Enum.ToObject(typeof(BaseGraphicType), index);
                gizmoBaseGraphicType.icon = GetGizmoIcon();
                gizmoBaseGraphicType.defaultLabel = GetGizmoLabel();
                ResolveBaseGraphic();
            };

            yield return gizmoBaseGraphicType;

            Command_Action gizmoRename = new Command_Action {
                defaultDesc = "StatueOfAnimal.CommandRenameDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/Rename", true),
                defaultLabel = "StatueOfAnimal.CommandRenameLabel".Translate()
            };

            gizmoRename.action = delegate {
                Find.WindowStack.Add(new Dialog_NameStatue(this));
            };

            yield return gizmoRename;

            Command_Action gizmoEditStatue = new Command_Action {
                defaultDesc = "StatueOfAnimal.CommandEditStatueDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport", true),
                defaultLabel = "StatueOfAnimal.CommandEditStatueLabel".Translate()
            };

            gizmoEditStatue.action = delegate {
                Find.WindowStack.Add(new Window_EditStatue(this));
            };

            yield return gizmoEditStatue;
        }

        private string GetGizmoLabel() {
            string type = "StatueOfAnimal.CommandBaseGraphicNoneLabel".Translate();
            if (baseGraphicType == BaseGraphicType.Circle) {
                type = "StatueOfAnimal.CommandBaseGraphicCircleLabel".Translate();
            } else if (baseGraphicType == BaseGraphicType.Rect) {
                type = "StatueOfAnimal.CommandBaseGraphicRectLabel".Translate();
            }
            return "StatueOfAnimal.CommandBaseGraphicLabel".Translate(type);
        }

        private Texture2D GetGizmoIcon() {
            if (baseGraphicType == BaseGraphicType.None) {
                return ContentFinder<Texture2D>.Get("UI/Commands/CommandBaseGraphicNone", true);
            } else if (baseGraphicType == BaseGraphicType.Circle) {
                return ContentFinder<Texture2D>.Get("UI/Commands/CommandBaseGraphicCircle", true);
            }

            return ContentFinder<Texture2D>.Get("UI/Commands/CommandBaseGraphicRect", true);
        }
    }
}
