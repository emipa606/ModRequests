﻿using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StatueOfColonist {
    public class StatueOfColonistRenderer {
        private const float YOffset_Behind = 0.00390625f;

        private const float YOffset_Body = 0.0078125f;

        private const float YOffset_Wounds = 0.01953125f;

        private const float YOffset_Shell = 0.0234375f;

        private const float YOffset_Head = 0.02734375f;

        private const float YOffset_OnHead = 0.03125f;

        public Building_StatueOfColonist parent;

        private RotDrawMode CurRotDrawMode {
            get {
                return RotDrawMode.Fresh;
            }
        }

        public StatueOfColonistRenderer(Building_StatueOfColonist parent) {
            this.parent = parent;
        }

        public void Render(StatueOfColonistGraphicSet graphics, Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump,float scale, Building_StatueOfColonist.HeadRenderMode headRenderMode, ThingDef raceDef, BodyTypeDef bodyTypeDef, LifeStageDef lifeStageDef) {
            Graphics_Internal_DrawMesh_Patch.scale = scale;

            if (graphics.ShouldResolve) {
                Log.Message("graphics.ShouldResolve" + graphics.parent.Position + "/" + !graphics.AllResolved + "/" + graphics.forceResolve + "/" + graphics.parent.ThingID);
                graphics.ResolveAllGraphics();
            }
            Mesh mesh = null;
            if (renderBody) {
                Vector3 loc = rootLoc;
                loc.y += YOffset_Body;
                mesh = GetBodyMesh(portrait, raceDef, bodyFacing);
                List<Material> list = graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                for (int i = 0; i < list.Count; i++) {
                    GenDraw.DrawMeshNowOrLater(mesh, loc, quat, list[i], portrait);
                    loc.y += YOffset_Behind;
                }
                if (bodyDrawType == RotDrawMode.Fresh) {
                    Vector3 drawLoc = rootLoc;
                    drawLoc.y += YOffset_Wounds;
                }
            }
            Vector3 vector = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North) {
                a.y += YOffset_Head;
                vector.y += YOffset_Shell;
            } else {
                a.y += YOffset_Shell;
                vector.y += YOffset_Head;
            }
            Vector3 headOffset = Vector3.zero;
            if (graphics.headGraphic != null) {
                Vector3 b = quat * this.BaseHeadOffsetAt(headFacing, graphics.data.bodyType, raceDef, lifeStageDef);
                headOffset = new Vector3(b.x, b.y, b.z);
                b = new Vector3(b.x * scale, b.y, b.z * scale);
                Material material = graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
                if (material != null) {
                    Mesh mesh2 = GetHeadMesh(portrait, raceDef, headFacing); 
                    GenDraw.DrawMeshNowOrLater(mesh2, a + b, quat, material, portrait);
                }
                Vector3 loc2 = rootLoc + b;
                loc2.y += YOffset_OnHead;
                bool flag = false;
                if (!portrait || !Prefs.HatsOnlyOnMap) {
                    Mesh mesh3 = GetHairMesh(graphics, portrait, raceDef, headFacing);
                    List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
                    for (int j = 0; j < apparelGraphics.Count; j++) {
                        if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead && headRenderMode != Building_StatueOfColonist.HeadRenderMode.HairOnly) {
                            if (headRenderMode == Building_StatueOfColonist.HeadRenderMode.Default) {
                                flag = true;
                            }
                            Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                            Vector3 locHat = loc2;
                            locHat.y += 0.001f * (j + 1) * scale;
                            GenDraw.DrawMeshNowOrLater(mesh3, locHat, quat, material2, portrait);
                        }
                    }
                }
                // Face tattoo
                PawnRenderFlags flags = PawnRenderFlags.None;
                if (ModsConfig.IdeologyActive && graphics.faceTattooGraphic != null && bodyDrawType != RotDrawMode.Dessicated && (bodyFacing != Rot4.North || this.parent.Data.faceTattooDef.visibleNorth)) {
                    Vector3 onHeadLoc = rootLoc + headOffset;
                    onHeadLoc.y += 0.0289575271f;
                    onHeadLoc.y -= 0.00144787633f;
                    Mesh mesh2 = GetHairMesh(graphics, portrait, raceDef, headFacing);
                    GenDraw.DrawMeshNowOrLater(mesh2, onHeadLoc, quat, graphics.faceTattooGraphic?.MatAt(headFacing, null), flags.FlagSet(PawnRenderFlags.DrawNow));
                }
                // Beard
                bool flag2 = bodyFacing == Rot4.North || this.parent.Data.beardDef == BeardDefOf.NoBeard;
                if (!flag2 && bodyDrawType != RotDrawMode.Dessicated && !flags.FlagSet(PawnRenderFlags.HeadStump) && this.parent.Data.beardDef != null) {
                    Vector3 loc4 = this.OffsetBeardLocationForCrownType(this.parent.Data.crownType, headFacing, loc2) + this.parent.Data.beardDef.GetOffset(this.parent.Data.crownType, headFacing);
                    Mesh mesh2 = GetHairMesh(graphics, portrait, raceDef, headFacing);
                    Material material3 = graphics.beardGraphic?.MatAt(headFacing, null);
                    if (material3 != null) {
                        GenDraw.DrawMeshNowOrLater(mesh2, loc4, quat, material3, flags.FlagSet(PawnRenderFlags.DrawNow));
                    }
                }
                // Hair
                if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump && graphics != null) {
                    Mesh mesh2 = GetHairMesh(graphics, portrait, raceDef, headFacing);
                    Material mat = graphics.HairMatAt(headFacing);
                    if (mesh2 != null && mat != null) {
                        GenDraw.DrawMeshNowOrLater(mesh2, loc2, quat, mat, portrait);
                    }
                }
            }
            if (renderBody) {
                for (int k = 0; k < graphics.apparelGraphics.Count; k++) {
                    ApparelGraphicRecord apparelGraphicRecord = graphics.apparelGraphics[k];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell) {
                        Material material3 = apparelGraphicRecord.graphic?.MatAt(bodyFacing, null);
                        GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material3, portrait);
                    }
                }
                PawnRenderFlags flags = PawnRenderFlags.None;
                if (ModsConfig.IdeologyActive && graphics.bodyTattooGraphic != null && bodyDrawType != RotDrawMode.Dessicated && (bodyFacing != Rot4.North || this.parent.Data.bodyTattooDef.visibleNorth)) {
                    Vector3 loc = vector;
                    loc.y += 0.00144787633f;
                    GenDraw.DrawMeshNowOrLater(this.GetBodyOverlayMeshSet()?.MeshAt(bodyFacing), loc, quat, graphics.bodyTattooGraphic?.MatAt(bodyFacing, null), flags.FlagSet(PawnRenderFlags.DrawNow));
                }
            }

            if (!this.parent.def.castEdgeShadows && this.parent.Data.raceDef?.race?.specialShadowData != null) {
                if (graphics.shadowGraphic == null) {
                    graphics.shadowGraphic = new Graphic_Shadow(this.parent.Data.raceDef.race.specialShadowData);
                }
                graphics.shadowGraphic.Draw(rootLoc, Rot4.North, this.parent, 0f);
            }

            DrawAddons(graphics, portrait, vector, headOffset, raceDef, quat, bodyFacing, bodyTypeDef, scale);



            Graphics_Internal_DrawMesh_Patch.Reset();
        }

        public virtual Mesh GetBodyMesh(bool portrait, ThingDef raceDef, Rot4 bodyFacing) {
            return MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
        }

        public virtual Mesh GetHeadMesh(bool portrait, ThingDef raceDef, Rot4 headFacing) {
            return MeshPool.humanlikeHeadSet.MeshAt(headFacing);
        }

        public virtual Mesh GetHairMesh(StatueOfColonistGraphicSet graphics, bool portrait, ThingDef raceDef, Rot4 headFacing) {
            return graphics.HairMeshSet.MeshAt(headFacing);
        }

        public virtual void DrawAddons(StatueOfColonistGraphicSet graphics, bool portrait, Vector3 vector, Vector3 headOffset, ThingDef raceDef, Quaternion quat, Rot4 rotation, BodyTypeDef bodyTypeDef, float scale) {
        }

        public Vector3 BaseHeadOffsetAt(Rot4 rotation, BodyTypeDef bodyType, ThingDef raceDef, LifeStageDef lifeStageDef) {
            Vector2 headOffset = GetHeadOffset(rotation, bodyType, raceDef, lifeStageDef);
            Vector3 result;
            switch (rotation.AsInt) {
            case 0:
                result = new Vector3(0f, 0f, headOffset.y);
                break;
            case 1:
                result = new Vector3(headOffset.x, 0f, headOffset.y);
                break;
            case 2:
                result = new Vector3(0f, 0f, headOffset.y);
                break;
            case 3:
                result = new Vector3(-headOffset.x, 0f, headOffset.y);
                break;
            default:
                result = Vector3.zero;
                break;
            }
            return result;
        }

        public virtual Vector2 GetHeadOffset(Rot4 rotation, BodyTypeDef bodyType, ThingDef raceDef, LifeStageDef lifeStageDef) {
            return bodyType.headOffset;
        }

        private Vector3 OffsetBeardLocationForCrownType(CrownType crownType, Rot4 headFacing, Vector3 beardLoc) {
            if (this.parent.Data.crownType == CrownType.Narrow) {
                if (headFacing == Rot4.East) {
                    beardLoc += Vector3.right * -0.05f;
                } else if (headFacing == Rot4.West) {
                    beardLoc += Vector3.right * 0.05f;
                }
                beardLoc += Vector3.forward * -0.05f;
            }
            return beardLoc;
        }

        private GraphicMeshSet GetBodyOverlayMeshSet() {
            if (!this.parent.Data.raceDef.race.Humanlike) {
                return MeshPool.humanlikeBodySet;
            }
            BodyTypeDef bodyType = this.parent.Data.bodyType;
            if (bodyType == BodyTypeDefOf.Male) {
                return MeshPool.humanlikeBodySet_Male;
            }
            if (bodyType == BodyTypeDefOf.Female) {
                return MeshPool.humanlikeBodySet_Female;
            }
            if (bodyType == BodyTypeDefOf.Thin) {
                return MeshPool.humanlikeBodySet_Thin;
            }
            if (bodyType == BodyTypeDefOf.Fat) {
                return MeshPool.humanlikeBodySet_Fat;
            }
            if (bodyType == BodyTypeDefOf.Hulk) {
                return MeshPool.humanlikeBodySet_Hulk;
            }
            return MeshPool.humanlikeBodySet;
        }
    }
}
