using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System.Collections.Generic;
using Verse.AI.Group;
using System.Linq;
using System.Reflection.Emit;
using HugsLib;


namespace rim3d
{


    public class HarmonyPatches : Mod
    {
        static public Harmony harmony;
        public HarmonyPatches(ModContentPack content) : base(content)
        {
            harmony = new Harmony("com.yayo.rim3d");


        }

    }

    public class patch : ModBase
    {
        public override string ModIdentifier => "rim3d";
        static patch()
        {
            HarmonyPatches.harmony.PatchAll();
        }
    }




    // 발사체
    [HarmonyPatch(typeof(Projectile), "Draw")]
    public class Prefix10
    {
        static Quaternion quat;
        [HarmonyPriority(0)]
        public static bool Prefix(Projectile __instance, Vector3 ___destination, Vector3 ___origin)
        {
            if (!core.mode3d) return true;
            float num = Traverse.Create(__instance).Property("ArcHeightFactor").GetValue<float>() * GenMath.InverseParabola(Traverse.Create(__instance).Property("DistanceCoveredFraction").GetValue<float>());
            Vector3 drawPos = __instance.DrawPos;
            Vector3 position = drawPos;
            position = new Vector3(position.x, num, position.z);
            if (__instance.def.projectile.shadowSize > 0f)
            {
                //DrawShadow(drawPos, num);
                AccessTools.Method(typeof(Projectile), "DrawShadow").Invoke(__instance, new object[] { drawPos, num });
            }
            //quat = __instance.ExactRotation;
            quat = Quaternion.LookRotation((___destination - ___origin).Yto0());
            //quat = Quaternion.LookRotation(-Current.Camera.transform.up, -Current.Camera.transform.forward);
            //quat.eulerAngles += new Vector3(-90f, 0f, 0f);

            //quat.w += 90f;
            Graphics.DrawMesh(MeshPool.GridPlane(__instance.def.graphicData.drawSize * 2f), position, quat, __instance.def.DrawMatSingle, 0);
            //Comps_PostDraw();
            AccessTools.Method(typeof(ThingWithComps), "Comps_PostDraw").Invoke(__instance as ThingWithComps, new object[] {});
            return false;
        }

    }

    //[HarmonyPatch(typeof(Projectile), "DrawShadow")]
    //public class Prefix11
    //{
    //    static Quaternion quat;
    //    [HarmonyPriority(0)]
    //    public static bool Prefix(Projectile __instance, Material ___shadowMaterial, Vector3 drawLoc, float height)
    //    {
    //        if (!core.mode3d) return true;
    //        if (!(___shadowMaterial == null))
    //        {
    //            float num = __instance.def.projectile.shadowSize * Mathf.Lerp(1f, 0.6f, height);
    //            Vector3 s = new Vector3(num, 1f, num);
    //            Vector3 vector = new Vector3(0f, -0.01f, 0f);
    //            Matrix4x4 matrix = default(Matrix4x4);
    //            matrix.SetTRS(drawLoc + vector, Quaternion.identity, s);
    //            Graphics.DrawMesh(MeshPool.plane10, matrix, ___shadowMaterial, 0);
    //        }
    //        return false;
    //    }

    //}





    // 그림자
    [HarmonyPatch(typeof(Graphic_Shadow), "DrawWorker")]
    public class Prefix11
    {
        [HarmonyPriority(0)]
        public static bool Prefix(Mesh ___shadowMesh, ShadowData ___shadowInfo, ref Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            if (!core.mode3d) return true;
            return false;
        }

    }









    // 모든 오브젝트 실시간 생성
    [HarmonyPatch(typeof(DynamicDrawManager), "DrawDynamicThings")]
    public class Prefix3
    {
        [HarmonyPriority(0)]
        public static bool Prefix(Thing __instance, bool ___drawingNow, Map ___map, HashSet<Thing> ___drawThings)
        {
            if (!core.mode3d) return true;

            if (!DebugViewSettings.drawThingsDynamic)
            {
                return false;
            }
            ___drawingNow = true;
            try
            {
                bool[] fogGrid = ___map.fogGrid.fogGrid;
                CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
                currentViewRect.ClipInsideMap(___map);
                currentViewRect = currentViewRect.ExpandedBy(1);
                CellIndices cellIndices = ___map.cellIndices;
                //foreach (Thing drawThing in ___drawThings)
                //{
                //    IntVec3 position = drawThing.Position;
                //    if ((currentViewRect.Contains(position) || drawThing.def.drawOffscreen) && (!fogGrid[cellIndices.CellToIndex(position)] || drawThing.def.seeThroughFog) && (!(drawThing.def.hideAtSnowDepth < 1f) || !(___map.snowGrid.GetDepth(position) > drawThing.def.hideAtSnowDepth)))
                //    {
                //        try
                //        {
                //            drawThing.Draw();
                //        }
                //        catch (Exception ex)
                //        {
                //            Log.Error(string.Concat("Exception drawing ", drawThing, ": ", ex.ToString()));
                //        }
                //    }
                //}
                foreach (Thing drawThing in Current.Game.CurrentMap.listerThings.AllThings)
                {
                    //if(drawThing.def.plant?.IsTree ?? false) Log.Message($"{drawThing.Label}");
                    //if (drawThing.def.drawerType != DrawerType.MapMeshOnly) continue;
                    IntVec3 position = drawThing.Position;
                    if ((currentViewRect.Contains(position) || drawThing.def.drawOffscreen) && (!fogGrid[cellIndices.CellToIndex(position)] || drawThing.def.seeThroughFog) && (!(drawThing.def.hideAtSnowDepth < 1f) || !(___map.snowGrid.GetDepth(position) > drawThing.def.hideAtSnowDepth)))
                    {
                        try
                        {
                            drawThing.Draw();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(string.Concat("Exception drawing ", drawThing, ": ", ex.ToString()));
                        }
                    }
                }


            }
            catch (Exception ex2)
            {
                Log.Error("Exception drawing dynamic things: " + ex2);
            }
            ___drawingNow = false;
            return false;
        }
    }





    // 프린트 생성 방지
    [HarmonyPatch(typeof(Plant), "Print")]
    public class Prefix6
    {
        [HarmonyPriority(0)]
        public static bool Prefix(SectionLayer layer)
        {
            if (!core.mode3d) return true;
            return false;
        }

    }
    [HarmonyPatch(typeof(MapDrawer), "RegenerateEverythingNow")]
    public class Prefix7
    {
        [HarmonyPriority(0)]
        public static bool Prefix()
        {
            if (!core.mode3d) return true;
            return false;
        }
    }
    [HarmonyPatch(typeof(Thing), "Print")]
    public class Prefix8
    {
        [HarmonyPriority(0)]
        public static bool Prefix(SectionLayer layer)
        {
            if (!core.mode3d) return true;
            return false;
        }
    }
    [HarmonyPatch(typeof(ThingWithComps), "Print")]
    public class Prefix9
    {
        [HarmonyPriority(0)]
        public static bool Prefix(SectionLayer layer)
        {
            if (!core.mode3d) return true;
            return false;
        }
    }






    // 오브젝트 생성
    [HarmonyPatch(typeof(ThingWithComps), "Draw")]
    public class Prefix5
    {
        [HarmonyPriority(0)]
        public static bool Prefix(ThingWithComps __instance)
        {
            if (!core.mode3d) return true;
            if (__instance is Pawn) return true;
            Prefix4.Prefix(__instance);
            return true;
        }

    }

    // 식물 바람 효과
    [HarmonyPatch(typeof(WindManager), "WindManagerTick")]
    public class Prefix12
    {
        [HarmonyPriority(0)]
        public static bool Prefix(WindManager __instance)
        {
            if (!core.mode3d) return true;
            return false;
        }

    }



    // 문
    [HarmonyPatch(typeof(Building_Door), "Draw")]
    public class Prefix13
    {
        [HarmonyPriority(0)]
        public static bool Prefix(Building_Door __instance)
        {
            if (!core.mode3d) return true;

            __instance.Rotation = Building_Door.DoorRotationAt(__instance.Position, __instance.Map);
            float num = 0f + 0.45f * Traverse.Create(__instance).Property("OpenPct").GetValue<float>();
            for (int i = 0; i < 2; i++)
            {
                Vector3 vector = default(Vector3);
                Mesh mesh;
                if (i == 0)
                {
                    vector = new Vector3(0f, 0f, -1f);
                    //mesh = MeshPool.plane10;
                    mesh = MeshPool.GridPlane(new Vector2(1f, __instance.def.holdsRoof ? core.val_wallHeight : 1f));
                }
                else
                {
                    vector = new Vector3(0f, 0f, 1f);
                    //mesh = MeshPool.plane10Flip;
                    mesh = MeshPool.GridPlaneFlip(new Vector2(1f, __instance.def.holdsRoof ? core.val_wallHeight : 1f));
                }
                Rot4 rotation = __instance.Rotation;
                rotation.Rotate(RotationDirection.Clockwise);
                vector = rotation.AsQuat * vector;
                Vector3 drawPos = __instance.DrawPos;
                drawPos.y = AltitudeLayer.DoorMoveable.AltitudeFor();
                drawPos += vector * num;

                
                //Quaternion quat = __instance.Rotation.AsQuat;
                Quaternion quat;
                
                if (__instance.def.holdsRoof)
                {
                    // 일반 문
                    float gap = __instance.def.holdsRoof ? 0.4f : 0f;
                    drawPos = new Vector3(drawPos.x, core.val_wallHeight * 0.35f, drawPos.z);
                    if (rotation == Rot4.South)
                    {
                        // 동, 서
                        Graphics.DrawMesh(mesh, drawPos + new Vector3(-gap, 0f, 0f), Quaternion.Euler(90f, 0f, 90f), __instance.Graphic.MatAt(__instance.Rotation), 0);
                        Graphics.DrawMesh(mesh, drawPos + new Vector3(gap, 0f, 0f), Quaternion.Euler(90f, 0f, -90f), __instance.Graphic.MatAt(__instance.Rotation), 0);

                        //Graphics.DrawMesh(mesh, drawPos + new Vector3(-gap - 0.01f, core.val_wallHeight * 0.5f, 0f), Quaternion.Euler(90f, 0f, 90f), __instance.Graphic.MatAt(__instance.Rotation), 0);
                        //Graphics.DrawMesh(mesh, drawPos + new Vector3(gap + 0.01f, core.val_wallHeight * 0.5f, 0f), Quaternion.Euler(90f, 0f, -90f), __instance.Graphic.MatAt(__instance.Rotation), 0);
                    }
                    else
                    {
                        // 남, 북
                        quat = Quaternion.Euler(-90f, 0f, 0f);
                        Graphics.DrawMesh(mesh, drawPos + new Vector3(0f, 0f, -gap), quat, __instance.Graphic.MatAt(__instance.Rotation), 0);
                        //Graphics.DrawMesh(mesh, drawPos + new Vector3(0f, core.val_wallHeight * 0.5f, -gap -0.01f), quat, __instance.Graphic.MatAt(__instance.Rotation), 0);
                    }
                }
                else
                {
                    // 울타리
                    drawPos = new Vector3(drawPos.x, 0.5f, drawPos.z);
                    if (rotation == Rot4.South)
                    {
                        // 동, 서
                        Graphics.DrawMesh(mesh, drawPos, Quaternion.Euler(-90f, 0f, 90f), __instance.Graphic.MatAt(Rot4.South), 0);
                        Graphics.DrawMesh(mesh, drawPos, Quaternion.Euler(-90f, 0f, -90f), __instance.Graphic.MatAt(Rot4.South), 0);
                    }
                    else
                    {
                        // 남, 북
                        quat = Quaternion.Euler(-90f, 0f, 0f);
                        Graphics.DrawMesh(mesh, drawPos, quat, __instance.Graphic.MatAt(Rot4.South), 0);
                    }
                }
                
                //else
                //{
                //    quat = Quaternion.Euler(-90f, 0f, 0f);
                //    Graphics.DrawMesh(mesh, drawPos, quat, __instance.Graphic.MatAt(__instance.Rotation), 0);
                //}
                    

                
                __instance.Graphic.ShadowGraphic?.DrawWorker(drawPos, __instance.Rotation, __instance.def, __instance, 0f);
            }
            AccessTools.Method(typeof(ThingWithComps), "Comps_PostDraw").Invoke(__instance, new object[] { });
            return false;
        }

    }



    // 오브젝트 배치
    [HarmonyPatch(typeof(Thing), "Draw")]
    public class Prefix4
    {
        [HarmonyPriority(0)]
        public static bool Prefix(Thing __instance)
        {
            if (!core.mode3d) return true;
            if (__instance is Pawn) return true;
            if(__instance is Corpse) return true;

            

            try
            {
                float y = 0f;
                float z = 0f;
                Material mat;
                bool north = false;
                bool south = false;
                bool west = false;
                bool east = false;

                if (__instance.def.plant != null && __instance.def.plant.IsTree)
                {
                    y = 0.5f;
                    __instance.Graphic.drawSize = Vector2.one * 3f;
                }
                Mesh mesh;
                Quaternion quat;

                if(__instance.def == ThingDefOf.MinifiedThing)
                {
                    Graphics.DrawMesh(__instance.Graphic.MeshAt(__instance.Rotation), new Vector3(__instance.DrawPos.x, 0.5f, __instance.DrawPos.z), Quaternion.Euler(-90f, 0f, 0f), __instance.Graphic.MatAt(__instance.Rotation, __instance), 0);
                }
                else if (__instance.def.graphicData.linkType == LinkDrawerType.CornerFiller)
                {
                    // 벽, 산
                    Graphic_Linked gl = (Graphic_Linked)__instance.Graphic;
                    int num = 0;
                    int num2 = 1;

                    for (int i = 0; i < 4; i++)
                    {
                        IntVec3 c = __instance.Position + GenAdj.CardinalDirections[i];

                        if (gl.ShouldLinkWith(c, __instance))
                        {
                            if (i == 0) north = true; //1
                            if (i == 1) east = true; // 2
                            if (i == 2) south = true; // 4
                            if (i == 3) west = true; // 8
                            num += num2;
                        }
                        num2 *= 2;
                    }
                    mat = MaterialAtlasPool.SubMaterialFromAtlas(gl.MatSingleFor(__instance), (LinkDirections)num);
                    Material matSide = MaterialAtlasPool.SubMaterialFromAtlas(gl.MatSingleFor(__instance), (LinkDirections)5);

                    float height = core.val_wallHeight;

                    // top
                    //mesh = new Mesh
                    Graphics.DrawMesh(MeshPool.GridPlane(Vector2.one), new Vector3(__instance.DrawPos.x, height, __instance.DrawPos.z), Quaternion.Euler(0f, 0f, 0f), mat, 0);

                    Mesh mesh2 = MeshPool.GridPlane(new Vector2(1f, height));

                    // side
                    if (!south) Graphics.DrawMesh(mesh2, new Vector3(__instance.DrawPos.x, height * 0.5f, __instance.DrawPos.z - 0.5f), Quaternion.Euler(-90f, 0f, 0f), matSide, 0);
                    if (!west) Graphics.DrawMesh(mesh2, new Vector3(__instance.DrawPos.x - 0.5f, height * 0.5f, __instance.DrawPos.z), Quaternion.Euler(-90f, 0f, 90f), matSide, 0);
                    if (!east) Graphics.DrawMesh(mesh2, new Vector3(__instance.DrawPos.x + 0.5f, height * 0.5f, __instance.DrawPos.z), Quaternion.Euler(-90f, 0f, -90f), matSide, 0);


                }
                else if (__instance.def.graphicData.linkType == LinkDrawerType.Asymmetric)
                {
                    // 울타리
                    Graphic_Linked gl = (Graphic_Linked)__instance.Graphic;
                    mat = MaterialAtlasPool.SubMaterialFromAtlas(gl.MatSingleFor(__instance), (LinkDirections)10);
                    Mesh mesh2 = MeshPool.GridPlane(new Vector2(1f, 1f));

                    int num = 0;
                    int num2 = 1;
                    for (int i = 0; i < 4; i++)
                    {
                        IntVec3 c = __instance.Position + GenAdj.CardinalDirections[i];

                        if (gl.ShouldLinkWith(c, __instance))
                        {
                            if (i == 0) north = true; //1
                            if (i == 1) east = true; // 2
                            if (i == 2) south = true; // 4
                            if (i == 3) west = true; // 8
                            num += num2;
                        }
                        num2 *= 2;
                    }
                    if (num == 5)
                    {
                        Graphics.DrawMesh(mesh2, new Vector3(__instance.DrawPos.x, 0.5f, __instance.DrawPos.z), Quaternion.Euler(-90f, 0f, 90f), mat, 0);
                        Graphics.DrawMesh(mesh2, new Vector3(__instance.DrawPos.x, 0.5f, __instance.DrawPos.z), Quaternion.Euler(-90f, 0f, -90f), mat, 0);
                    }
                    else
                    {
                        Graphics.DrawMesh(mesh2, new Vector3(__instance.DrawPos.x, 0.5f, __instance.DrawPos.z), Quaternion.Euler(-90f, 0f, 0f), mat, 0);
                    }

                }
                else
                {

                    mesh = __instance.Graphic.MeshAt(__instance.Rotation);


                    if (__instance.def.designationCategory == DesignationCategoryDefOf.Structure)
                    {
                        // 구조물
                        mat = __instance.Graphic.MatAt(__instance.Rotation, __instance);
                        Graphics.DrawMesh(mesh, new Vector3(__instance.DrawPos.x, __instance.def.size.z * 0.5f, __instance.DrawPos.z + z), Quaternion.Euler(-90f, 0f, 0f), mat, 0);

                    }
                    else if (__instance.def.building != null)
                    {
                        // 그외 일반 가구

                        if (__instance.Graphic != null && __instance.Graphic.data != null && __instance.def.size != null)
                        {
                            if (__instance.Graphic.data.drawSize.y > __instance.def.size.z * core.val_verticalBuildingFactor)
                            {
                                // 세워진 물체
                                mat = __instance.Graphic.MatAt(Rot4.South, __instance);
                                if (__instance.Rotation == Rot4.South || __instance.Rotation == Rot4.North)
                                {

                                    Graphics.DrawMesh(mesh, new Vector3(__instance.DrawPos.x, __instance.def.size.z * 0.5f, __instance.DrawPos.z + z), Quaternion.Euler(-90f, 0f, 0f), mat, 0);
                                }
                                else
                                {
                                    Graphics.DrawMesh(mesh, new Vector3(__instance.DrawPos.x, __instance.def.size.z * 0.5f, __instance.DrawPos.z + z), Quaternion.Euler(-90f, 0f, 90f), mat, 0);
                                    Graphics.DrawMesh(mesh, new Vector3(__instance.DrawPos.x, __instance.def.size.z * 0.5f, __instance.DrawPos.z + z), Quaternion.Euler(-90f, 0f, -90f), mat, 0);
                                }

                                return false;
                            }
                        }
                        // 눞혀진 물체
                        mat = __instance.Graphic.MatAt(__instance.Rotation, __instance);
                        quat = Quaternion.Euler(0f, 0f, 0f);
                        Graphics.DrawMesh(mesh, new Vector3(__instance.DrawPos.x, 0.01f, __instance.DrawPos.z + z), quat, mat, 0);


                    }
                    else
                    {
                        // 아이템들?
                        //mat = __instance.Graphic.MatAt(__instance.Rotation, __instance);
                        //quat = Quaternion.Euler(-90f, 0f, 0f);
                        //Graphics.DrawMesh(mesh, new Vector3(__instance.DrawPos.x, 0.5f, __instance.DrawPos.z + z), quat, mat, 0);
                        Graphics.DrawMesh(__instance.Graphic.MeshAt(__instance.Rotation), new Vector3(__instance.DrawPos.x, 0.5f, __instance.DrawPos.z), Quaternion.Euler(-90f, 0f, 0f), __instance.Graphic.MatAt(__instance.Rotation, __instance), 0);
                    }


                }

            }
            catch
            {
                if (core.val_debug) Log.Message($"Prefix4 : error with {__instance.Label}({__instance.def.defName})");
                Graphics.DrawMesh(__instance.Graphic.MeshAt(__instance.Rotation), new Vector3(__instance.DrawPos.x, 0.5f, __instance.DrawPos.z), Quaternion.Euler(-90f, 0f, 0f), __instance.Graphic.MatAt(__instance.Rotation, __instance), 0);
            }
            
            return false;
        }

    }

    //[HarmonyPatch(typeof(GenDraw), "DrawMeshNowOrLater")]
    //[HarmonyPatch(new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) })]
    //public class Prefix0
    //{
    //    [HarmonyPriority(0)]
    //    public static bool Prefix(Mesh mesh, ref Vector3 loc, ref Quaternion quat, Material mat, bool drawNow)
    //    {

    //        loc = new Vector3(loc.x, 0f, loc.z);
    //        quat.eulerAngles = quat.eulerAngles + new Vector3(-90f, 0f, 0f);
    //        return true;
    //    }


    //}


    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnAt")]
    public class Patch_PawnRenderer_RenderPawnAt
    {
        static Pawn pawn;

        [HarmonyPriority(0)]
        public static bool Prefix(PawnRenderer __instance, Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
        {
            if (!core.mode3d) return true;
            pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (!__instance.graphics.AllResolved)
            {
                __instance.graphics.ResolveAllGraphics();
            }

            Rot4 rot = rotOverride ?? pawn.Rotation;
            PawnRenderFlags pawnRenderFlags = (PawnRenderFlags)AccessTools.Method(typeof(PawnRenderer), "GetDefaultRenderFlags").Invoke(__instance, new object[] { pawn });
            if (neverAimWeapon)
            {
                pawnRenderFlags |= PawnRenderFlags.NeverAimWeapon;
            }

            RotDrawMode curRotDrawMode = Traverse.Create(__instance).Property("CurRotDrawMode").GetValue<RotDrawMode>();
            bool flag = pawn.RaceProps.Humanlike && curRotDrawMode != RotDrawMode.Dessicated && !pawn.IsInvisible();
            PawnTextureAtlasFrameSet frameSet = null;
            if (flag && !GlobalTextureAtlasManager.TryGetPawnFrameSet(pawn, out frameSet, out var _))
            {
                flag = false;
            }

            int IdTick = pawn.thingIDNumber * 100;
            Vector3 drawLoc3D = new Vector3(drawLoc.x, 0f, drawLoc.z);
            Quaternion quat3D = Quaternion.Euler(-90f, 0f, 0f);
            if (pawn.GetPosture() == PawnPosture.Standing)
            {

                if (flag)
                {
                    Material original = MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout));
                    original = (Material)AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded").Invoke(__instance, new object[] { original, pawn, false });

                    GenDraw.DrawMeshNowOrLater((Mesh)AccessTools.Method(typeof(PawnRenderer), "GetBlitMeshUpdatedFrame").Invoke(__instance, new object[] { frameSet, rot, PawnDrawMode.BodyAndHead }), drawLoc3D, quat3D, original, drawNow: false);

                    // need fix
                    AccessTools.Method(typeof(PawnRenderer), "DrawDynamicParts").Invoke(__instance, new object[] { drawLoc3D, 0f, rot, pawnRenderFlags });
                }
                else
                {
                    // need fix
                    AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal").Invoke(__instance, new object[] { drawLoc3D, 0f, true, rot, curRotDrawMode, pawnRenderFlags });
                }
                AccessTools.Method(typeof(PawnRenderer), "DrawCarriedThing").Invoke(__instance, new object[] { drawLoc3D });
                if (!pawnRenderFlags.FlagSet(PawnRenderFlags.Invisible))
                {
                    AccessTools.Method(typeof(PawnRenderer), "DrawInvisibleShadow").Invoke(__instance, new object[] { drawLoc3D + new Vector3(0f, 0.1f, 0f) });
                }
            }
            else
            {
                bool showBody = true;
                Vector3 bodyPos = (Vector3)AccessTools.Method(typeof(PawnRenderer), "GetBodyPos").Invoke(__instance, new object[] { drawLoc3D, showBody });
                float angle = __instance.BodyAngle();
                Rot4 rot2 = __instance.LayingFacing();

                if (flag)
                {
                    Material original2 = MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout));
                    original2 = (Material)AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded").Invoke(__instance, new object[] { original2, pawn, false });

                    // need fix
                    GenDraw.DrawMeshNowOrLater((Mesh)AccessTools.Method(typeof(PawnRenderer), "GetBlitMeshUpdatedFrame").Invoke(__instance, new object[] { frameSet, rot2, (!showBody) ? PawnDrawMode.HeadOnly : PawnDrawMode.BodyAndHead }), bodyPos, Quaternion.AngleAxis(angle, Vector3.up), original2, drawNow: false);
                    // need fix
                    AccessTools.Method(typeof(PawnRenderer), "DrawDynamicParts").Invoke(__instance, new object[] { bodyPos, angle, rot, pawnRenderFlags });
                }
                else
                {
                    // need fix
                    AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal").Invoke(__instance, new object[] { bodyPos, angle, showBody, rot2, curRotDrawMode, pawnRenderFlags });
                }
            }
            if (pawn.Spawned && !pawn.Dead)
            {
                pawn.stances.StanceTrackerDraw();
                pawn.pather.PatherDraw();
                pawn.roping.RopingDraw();
            }

            AccessTools.Method(typeof(PawnRenderer), "DrawDebug").Invoke(__instance, new object[] { });
            return false;
        }


        //[HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]
        //public class Patch_PawnRenderer_RenderPawnInternal
        //{
        //    [HarmonyPriority(0)]
        //    public static bool Prefix(PawnRenderer __instance, PawnWoundDrawer ___woundOverlays, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, Pawn ___pawn)
        //    {

        //        return true;
        //    }
        //}

        [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
        public class Patch2
        {
            [HarmonyPriority(0)]
            public static bool Prefix(PawnRenderer __instance, Pawn ___pawn, Thing eq, Vector3 drawLoc, float aimAngle)
            {
                if (!core.mode3d) return true;
                Mesh mesh = null;
                float num = aimAngle - 90f;
                if (aimAngle > 20f && aimAngle < 160f)
                {
                    mesh = MeshPool.plane10;
                    num += eq.def.equippedAngleOffset;
                }
                else if (aimAngle > 200f && aimAngle < 340f)
                {
                    mesh = MeshPool.plane10Flip;
                    num -= 180f;
                    num -= eq.def.equippedAngleOffset;
                }
                else
                {
                    mesh = MeshPool.plane10;
                    num += eq.def.equippedAngleOffset;
                }
                num %= 360f;
                CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();

                

                if (compEquippable != null)
                {
                    EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
                    drawLoc += new Vector3(drawOffset.x, drawOffset.z, drawOffset.y) + new Vector3(0f, 0f, -0.1f);
                    num += angleOffset;
                }
                Material material = null;
                Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;

                Vector3 angle3 = Vector3.zero;
                Quaternion quat = Quaternion.AngleAxis(num, Vector3.back);
                quat.SetLookRotation(new Vector3(0f, 1f, 0f));
                //angle3 = Quaternion.AngleAxis(num, Vector3.back).eulerAngles;
                //angle3 += new Vector3(angle3.z, 90f, 0f);

                //if (num > 180f)
                //{
                //    Quaternion quat = Quaternion.Euler(angle3);
                //    quat.w += 180f;
                //    angle3 = quat.eulerAngles;
                //}
                Graphics.DrawMesh(material: (graphic_StackCount == null) ? eq.Graphic.MatSingleFor(eq) : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq), mesh: mesh, position: drawLoc, rotation: quat, layer: 0);
                return false;
            }
        }




        [HarmonyPatch(typeof(PawnRenderer), "DrawPawnBody")]
        public class Patch1
        {
            [HarmonyPriority(0)]
            public static bool Prefix(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc, float angle, Rot4 facing, RotDrawMode bodyDrawType, PawnRenderFlags flags, ref Mesh bodyMesh)
            {
                if (!core.mode3d) return true;
                Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
                if (!___pawn.RaceProps.Humanlike)
                {
                    if(facing == Rot4.North)
                    {
                        facing = Rot4.East;
                    }
                    else if(facing == Rot4.South)
                    {
                        facing = Rot4.West;
                    }
                    if(!___pawn.Dead) quat.eulerAngles += new Vector3(-90f, 0f, 0f);
                }
                Vector3 vector = rootLoc;
                vector.y += 0.008687258f;
                Vector3 loc = vector;
                loc.y += 0.00144787633f;
                bodyMesh = null;
                if (bodyDrawType == RotDrawMode.Dessicated && !pawn.RaceProps.Humanlike && __instance.graphics.dessicatedGraphic != null && !flags.FlagSet(PawnRenderFlags.Portrait))
                {
                    __instance.graphics.dessicatedGraphic.Draw(vector, facing, pawn, angle);
                    return false;
                }
                if (pawn.RaceProps.Humanlike)
                {
                    bodyMesh = MeshPool.humanlikeBodySet.MeshAt(facing);
                }
                else
                {
                    bodyMesh = __instance.graphics.nakedGraphic.MeshAt(facing);
                }
                List<Material> list = __instance.graphics.MatsBodyBaseAt(facing, bodyDrawType, flags.FlagSet(PawnRenderFlags.Clothes));
                for (int i = 0; i < list.Count; i++)
                {
                    //Material mat = (flags.FlagSet(PawnRenderFlags.Cache) ? list[i] : OverrideMaterialIfNeeded(list[i], pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
                    Material mat = (flags.FlagSet(PawnRenderFlags.Cache) ? list[i] : (Material)AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded").Invoke(__instance, new object[] { list[i], pawn, flags.FlagSet(PawnRenderFlags.Portrait) }));
                    
                    GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                    vector.y += 0.00289575267f;
                }
                if (ModsConfig.IdeologyActive && __instance.graphics.bodyTattooGraphic != null && bodyDrawType != RotDrawMode.Dessicated && (facing != Rot4.North || pawn.style.FaceTattoo.visibleNorth))
                {
                    GenDraw.DrawMeshNowOrLater(__instance.GetBodyOverlayMeshSet().MeshAt(facing), loc, quat, __instance.graphics.bodyTattooGraphic.MatAt(facing), flags.FlagSet(PawnRenderFlags.DrawNow));
                }
                return false;
            }

        }


        //[HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]
        //public class Patch_PawnRenderer_RenderPawnInternal
        //{
        //    [HarmonyPriority(0)]
        //    public static bool Prefix(PawnRenderer __instance, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, Pawn ___pawn)
        //    {
        //        if (!__instance.graphics.AllResolved)
        //        {
        //            __instance.graphics.ResolveAllGraphics();
        //        }
        //        Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
        //        //quaternion.eulerAngles = quaternion.eulerAngles + new Vector3(90f, 0f, 0f);

        //        Vector3 vector = rootLoc;
        //        Vector3 vector2 = rootLoc;
        //        if (bodyFacing != Rot4.North)
        //        {
        //            vector2.y += 0.0231660213f;
        //            vector.y += 3f / 148f;
        //        }
        //        else
        //        {
        //            vector2.y += 3f / 148f;
        //            vector.y += 0.0231660213f;
        //        }
        //        Vector3 utilityLoc = rootLoc;
        //        utilityLoc.y += ((bodyFacing == Rot4.South) ? 0.00579150533f : 0.0289575271f);
        //        Mesh bodyMesh = null;
        //        Vector3 drawLoc;
        //        if (renderBody)
        //        {
        //            //DrawPawnBody(rootLoc, angle, bodyFacing, bodyDrawType, flags, out bodyMesh);
        //            Log.Message("A");
        //            AccessTools.Method(typeof(PawnRenderer), "DrawPawnBody").Invoke(__instance, new object[] { rootLoc, angle, bodyFacing, bodyDrawType, flags, bodyMesh });
        //            drawLoc = rootLoc;
        //            drawLoc.y += 0.009687258f;
        //            Log.Message("B");
        //            if (bodyDrawType == RotDrawMode.Fresh)
        //            {
        //                Log.Message("C");
        //                //woundOverlays.RenderOverBody(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), BodyTypeDef.WoundLayer.Body, bodyFacing, false);
        //                __instance.WoundOverlays.RenderOverBody(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), BodyTypeDef.WoundLayer.Body, bodyFacing, false);
        //            }
        //            Log.Message("D");
        //            if (renderBody && flags.FlagSet(PawnRenderFlags.Clothes))
        //            {
        //                Log.Message("E");
        //                //DrawBodyApparel(vector, utilityLoc, bodyMesh, angle, bodyFacing, flags);
        //                AccessTools.Method(typeof(PawnRenderer), "DrawBodyApparel").Invoke(__instance, new object[] { vector, utilityLoc, bodyMesh, angle, bodyFacing, flags });
        //            }
        //            Log.Message("F");
        //            drawLoc = rootLoc;
        //            drawLoc.y += 0.0221660212f;
        //            Log.Message("G");
        //            if (bodyDrawType == RotDrawMode.Fresh)
        //            {
        //                Log.Message("H");
        //                __instance.WoundOverlays.RenderOverBody(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), BodyTypeDef.WoundLayer.Body, bodyFacing, true);
        //            }
        //            Log.Message("I");
        //        }
        //        Vector3 vector3 = Vector3.zero;
        //        drawLoc = rootLoc;
        //        drawLoc.y += 0.0289575271f;
        //        if (__instance.graphics.headGraphic != null)
        //        {
        //            vector3 = quaternion * __instance.BaseHeadOffsetAt(bodyFacing);
        //            Material material = __instance.graphics.HeadMatAt(bodyFacing, bodyDrawType, flags.FlagSet(PawnRenderFlags.HeadStump), flags.FlagSet(PawnRenderFlags.Portrait), !flags.FlagSet(PawnRenderFlags.Cache));
        //            if (material != null)
        //            {
        //                GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(bodyFacing), vector2 + vector3, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
        //            }
        //        }
        //        if (bodyDrawType == RotDrawMode.Fresh)
        //        {
        //            __instance.WoundOverlays.RenderOverBody(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), BodyTypeDef.WoundLayer.Head, bodyFacing);
        //        }
        //        if (__instance.graphics.headGraphic != null)
        //        {
        //            //DrawHeadHair(rootLoc, vector3, angle, bodyFacing, bodyFacing, bodyDrawType, flags);
        //            AccessTools.Method(typeof(PawnRenderer), "DrawHeadHair").Invoke(__instance, new object[] { rootLoc, vector3, angle, bodyFacing, bodyFacing, bodyDrawType, flags });
        //        }
        //        if (!flags.FlagSet(PawnRenderFlags.Portrait) && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0 && __instance.graphics.packGraphic != null)
        //        {
        //            GenDraw.DrawMeshNowOrLater(bodyMesh, Matrix4x4.TRS(vector, quaternion, Vector3.one), __instance.graphics.packGraphic.MatAt(bodyFacing), flags.FlagSet(PawnRenderFlags.DrawNow));
        //        }
        //        if (!flags.FlagSet(PawnRenderFlags.Portrait) && !flags.FlagSet(PawnRenderFlags.Cache))
        //        {
        //            //DrawDynamicParts(rootLoc, angle, bodyFacing, flags);
        //            AccessTools.Method(typeof(PawnRenderer), "DrawDynamicParts").Invoke(__instance, new object[] { rootLoc, angle, bodyFacing, flags });
        //        }
        //        return false;
        //    }
        //}















}





}
