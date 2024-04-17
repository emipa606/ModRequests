using RimWorld;
using System;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using System.Linq;

namespace StatueOfColonist {
    public class Building_StatueOfColonist : Building_Art {
        public string name;

        private StatueOfColonistRenderer renderer;
        private StatueOfColonistData dataStatue;
        private StatueOfColonistGraphicSet graphicsStatue;
        private StatueOfColonistGraphicSet graphicsBlueprint;
        private StatueOfColonistGraphicSet graphicsCanDesignateGhost;
        private StatueOfColonistGraphicSet graphicsCanNotDesignateGhost;

        private HeadRenderMode headRenderMode;

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
                    return "StatueOfColonist.StatueNameFormat".Translate( label, name, qualityLabel );
                } else {
                    return "StatueOfColonist.StatueNameFormatIfNoStatueName".Translate( label, name, qualityLabel );
                }
            }
        }

        public enum RenderMode {
            Normal,
            Blueprint,
            CanDesignateGhost,
            CanNotDesignateGhost
        }

        public enum HeadRenderMode {
            Default,
            HairOnly,
            HatAndHair
        }

        public bool ShouldBeNamed {
            get {
                return name.NullOrEmpty() && StatueOfColonistMod.Settings.autoName;
            }
        }

        public bool IsValid {
            get {
                return (dataStatue != null);
            }
        }

        public StatueOfColonistDef Def {
            get {
                return def as StatueOfColonistDef;
            }
        }

        public StatueOfColonistData Data {
            get {
                return dataStatue;
            }
            set {
                dataStatue = value;
            }
        }

        public int Size {
            get {
                int size = 0;
                if (def.defName == "TMB_StatueOfColonistKLarge") {
                    size = 1;
                }else if (def.defName == "TMB_StatueOfColonistKGrand") {
                    size = 2;
                }

                return size;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!IsValid) {
                InitializeStatue();
            }
        }

        public void InitializeStatue() {
            bool isDecided = false;
            float value = UnityEngine.Random.value;
            if (value < StatueOfColonistMod.Settings.possibilityOfStatueFromPreset / 100f) {
                List<StatueOfColonistPreset> candicates = StatueOfColonistPref.presets.FindAll(p => p.IsValid);
                if (!candicates.NullOrEmpty()) {
                    StatueOfColonistPreset preset = candicates.RandomElementByWeight(p => p.weight);
                    CopyStatueOfColonistFromPreset(preset);
                    if (ShouldBeNamed) {
                        this.name = preset.name;
                    } else {
                        this.name = "";
                    }
                    isDecided = true;
                }
            }

            if (!isDecided) {
                Pawn p = this.Map?.mapPawns?.FreeColonists?.RandomElement();
                if (p != null) {
                    ResolveGraphics(p);
                    ResolveArt(p);
                    if (ShouldBeNamed) {
                        this.name = p.LabelShort;
                    } else {
                        this.name = "";
                    }
                }
            }
        }

        public void CopyStatueOfColonistFromPreset(StatueOfColonistPreset preset) {
            if (dataStatue == null) {
                dataStatue = new StatueOfColonistData(this.DrawColor, "Map/Cutout");
            }
            dataStatue.CopyFromPreset(preset);
            ResolveGraphics();
        }

        public override void ExposeData() {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving && this.name == null) {
                this.name = "";
            }
            Scribe_Values.Look(ref this.name, "name","");
            Scribe_Deep.Look(ref this.dataStatue, "dataStatue");
            Scribe_Values.Look(ref this.headRenderMode, "headRenderMode");
            if (Scribe.mode == LoadSaveMode.PostLoadInit) {
                ResolveGraphics();
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false) {
            base.DrawAt(drawLoc, flip);

            Vector3 pos = drawLoc + new Vector3(Def.offsetX, StatueYOffset, Def.offsetZ);
            Render(pos, Quaternion.identity, true, this.Rotation, this.Rotation, RotDrawMode.Fresh, false, false, Def.scale);
        }

        public override void Draw() {
            base.Draw();

            Vector3 pos = DrawPos + new Vector3(Def.offsetX, StatueYOffset, Def.offsetZ);
            Render(pos, Quaternion.identity, true, this.Rotation, this.Rotation, RotDrawMode.Fresh, false, false, Def.scale);
        }

        public void ResolveGraphics(Pawn pawn) {
            this.dataStatue = new StatueOfColonistData(pawn, this.DrawColor, "Map/Cutout");
            ResolveGraphics();
        }

        public void ResolveGraphics() {
            this.renderer = new StatueOfColonistRenderer(this);
            this.graphicsStatue = new StatueOfColonistGraphicSet(this.dataStatue,this);
            this.graphicsBlueprint = new StatueOfColonistGraphicSet(new StatueOfColonistData(this.dataStatue, ColorBlueprint, "Map/CutoutSkin"), this);
            this.graphicsCanDesignateGhost = new StatueOfColonistGraphicSet(new StatueOfColonistData(this.dataStatue, ColorCanDesignateGhost, "Map/CutoutSkin"), this);
            this.graphicsCanNotDesignateGhost = new StatueOfColonistGraphicSet(new StatueOfColonistData(this.dataStatue, ColorCanNotDesignateGhost, "Map/CutoutSkin"), this);
        }

        public void Render(Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump, float scale, RenderMode mode = RenderMode.Normal) {
            StatueOfColonistGraphicSet graphics = this.graphicsStatue;
            if (mode == RenderMode.Blueprint) {
                graphics = this.graphicsBlueprint;
            } else if (mode == RenderMode.CanDesignateGhost) {
                graphics = this.graphicsCanDesignateGhost;
            } else if (mode == RenderMode.CanNotDesignateGhost) {
                graphics = this.graphicsCanNotDesignateGhost;
            }
            Vector3 loc = rootLoc + new Vector3(0f,0f,StatueOfColonistMod.Settings.GetOffsetStatueBody(Data.bodyType,Size)) + new Vector3(Data.offset.x, 0f, Data.offset.y);
            renderer.Render(graphics, loc, quat, renderBody, bodyFacing, headFacing, bodyDrawType, portrait, headStump, scale, headRenderMode, this.Data.raceDef, this.Data.bodyType, this.Data.lifeStageDef);
        }

        private Apparel MakeApparel(ThingDef def, Color color) {
            Apparel apparel = (Apparel)ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def));
            apparel.DrawColor = color;
            return apparel;
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
            Command_Action gizmoHeadRenderMode = new Command_Action {
                defaultDesc = GetGizmoDesc(),
                icon = GetGizmoIcon(),
                defaultLabel = GetGizmoLabel()
            };

            gizmoHeadRenderMode.action = delegate {
                int index = (int)this.headRenderMode;
                index = (index + 1) % Enum.GetNames(typeof(HeadRenderMode)).Length;
                this.headRenderMode = (HeadRenderMode)Enum.ToObject(typeof(HeadRenderMode), index);
                gizmoHeadRenderMode.defaultDesc = GetGizmoDesc();
                gizmoHeadRenderMode.icon = GetGizmoIcon();
                gizmoHeadRenderMode.defaultLabel = GetGizmoLabel();
            };

            yield return gizmoHeadRenderMode;

            Command_Action gizmoRename = new Command_Action {
                defaultDesc = "StatueOfColonist.CommandRenameDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/Rename", true),
                defaultLabel = "StatueOfColonist.CommandRenameLabel".Translate()
            };

            gizmoRename.action = delegate {
                Find.WindowStack.Add(new Dialog_NameStatue(this));
            };

            yield return gizmoRename;

            Command_Action gizmoEditStatue = new Command_Action {
                defaultDesc = "StatueOfColonist.CommandEditStatueDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport", true),
                defaultLabel = "StatueOfColonist.CommandEditStatueLabel".Translate()
            };

            gizmoEditStatue.action = delegate {
                Find.WindowStack.Add(new Window_EditStatue(this));
            };

            yield return gizmoEditStatue;
        }

        private string GetGizmoLabel() {
            if (headRenderMode == HeadRenderMode.Default) {
                return "StatueOfColonist.CommandDefaultLabel".Translate();
            } else if (headRenderMode == HeadRenderMode.HairOnly) {
                return "StatueOfColonist.CommandHairOnlyLabel".Translate();
            }

            return "StatueOfColonist.CommandHatAndHairLabel".Translate();
        }

        private string GetGizmoDesc() {
            if (headRenderMode == HeadRenderMode.Default) {
                return "StatueOfColonist.CommandDefaultDesc".Translate();
            } else if (headRenderMode == HeadRenderMode.HairOnly) {
                return "StatueOfColonist.CommandHairOnlyDesc".Translate();
            }

            return "StatueOfColonist.CommandHatAndHairDesc".Translate();
        }

        private Texture2D GetGizmoIcon() {
            if (headRenderMode == HeadRenderMode.Default) {
                return ContentFinder<Texture2D>.Get("UI/Commands/CommandToggleHeadRenderModeDefault", true);
            } else if (headRenderMode == HeadRenderMode.HairOnly) {
                return ContentFinder<Texture2D>.Get("UI/Commands/CommandToggleHeadRenderModeHairOnly", true);
            }

            return ContentFinder<Texture2D>.Get("UI/Commands/CommandToggleHeadRenderModeHatAndHair", true);
        }

        private static bool TryGetGraphicApparel(Apparel apparel, BodyTypeDef bodyType, Shader shader, out ApparelGraphicRecord rec) {
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
            Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
            rec = new ApparelGraphicRecord(graphic, apparel);
            return true;
        }

        public void AddApparel(ThingDef apparel) {
            this.Data.wornApparelDefs.Add(apparel);
            ResolveGraphics();
        }

        public void RemoveApparel(ThingDef apparel) {
            this.Data.wornApparelDefs.Remove(apparel);
            ResolveGraphics();
        }

        public bool ResolveArt(Pawn p) {
            bool succeed = false;
            CompArt comp = base.GetComp<CompArt>();
            if (comp != null) {
                int iterateCount = 0;
                while (iterateCount < 100) {
                    comp.InitializeArt(p);
                    if (comp.GenerateImageDescription().ToString().Contains(p.LabelShort)) {
                        succeed = true;
                        break;
                    }
                    iterateCount++;
                }

                if (!succeed) {
                    List<Thing> sarcophaguses = Find.CurrentMap.listerThings.ThingsOfDef(ThingDef.Named("Sarcophagus"));
                    foreach (Thing s in sarcophaguses) {
                        Building_Sarcophagus sarcophagus = s as Building_Sarcophagus;
                        if (sarcophagus != null && sarcophagus.Corpse.InnerPawn == p) {
                            CompArt comp2 = sarcophagus.GetComp<CompArt>();
                            if (comp2 != null && comp2.GenerateImageDescription().ToString().Contains(p.LabelShort)) {
                                InitializeArt(comp,comp2.TaleRef);
                                succeed = true;
                                break;
                            }
                        }
                    }
                }
            }
            return succeed;
        }

        private void InitializeArt(CompArt comp, TaleReference taleRef) {
            Traverse trvTaleRef = Traverse.Create(comp).Field("taleRef");
            Traverse trvTitleInt = Traverse.Create(comp).Field("titleInt");
            Traverse trvGenerateTitle = Traverse.Create(comp).Method("GenerateTitle");
            if (comp.TaleRef != null) {
                comp.TaleRef.ReferenceDestroyed();
                trvTaleRef.SetValue(null);
            } else {
                Traverse trvTale = Traverse.Create(taleRef).Field("tale");
                trvTaleRef.SetValue(new TaleReference(trvTale.GetValue<Tale>()));
                trvTitleInt.SetValue(trvGenerateTitle.GetValue());
            }
        }
    }
}
