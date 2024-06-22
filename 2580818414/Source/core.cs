using UnityEngine;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;
using System.Linq;
using RimWorld.Planet;
using System.Collections.Generic;
using HarmonyLib;
using System;
using System.Reflection;
using Verse.Noise;
using System.Collections;


namespace rim3d
{

    public class core : ModBase
    {
        public override string ModIdentifier => "rim3d";

        private SettingHandle<float> val_camSpeed_s;
        public static float val_camSpeed;

        private SettingHandle<float> val_wallHeight_s;
        public static float val_wallHeight;

        private SettingHandle<float> val_verticalBuildingFactor_s;
        public static float val_verticalBuildingFactor;


        private SettingHandle<float> val_angle_s;
        public static float val_angle;

        private SettingHandle<float> val_z_s;
        public static float val_z;

        private SettingHandle<float> val_fov_s;
        public static float val_fov;

        private SettingHandle<float> val_far_s;
        public static float val_far;


        private SettingHandle<bool> val_debug_s;
        public static bool val_debug;



        static public bool mode3d = false;
        static public List<ThingDef> ar_td = new List<ThingDef>();
        static public List<ThingDef> ar_td0 = new List<ThingDef>();
        static public List<ThingDef> ar_td2 = new List<ThingDef>();
        static public List<ThingDef> ar_td3 = new List<ThingDef>();


        public override void DefsLoaded()
        {
            val_camSpeed_s = Settings.GetHandle<float>("val_camSpeed", "cam move speed", "", 2f);
            val_wallHeight_s = Settings.GetHandle<float>("val_wallHeight", "wall height", "", 1.4f);
            val_verticalBuildingFactor_s = Settings.GetHandle<float>("val_verticalBuildingFactor", "vertical building factor", "", 2f);

            val_angle_s = Settings.GetHandle<float>("val_angle", "angle", "", 25f);
            val_fov_s = Settings.GetHandle<float>("val_fov", "fov", "", 45f);
            val_z_s = Settings.GetHandle<float>("val_z", "height", "", 6f);
            val_far_s = Settings.GetHandle<float>("val_far", "far cliping", "", 80f);

            val_debug_s = Settings.GetHandle<bool>("val_debug", "debug mode", "", false);

            SettingsChanged();


            foreach (ThingDef t in from t2 in DefDatabase<ThingDef>.AllDefs where t2.drawerType == DrawerType.None select t2)
            {
                ar_td.Add(t);
                ar_td0.Add(t);
            }
            foreach (ThingDef t in from t2 in DefDatabase<ThingDef>.AllDefs where t2.drawerType == DrawerType.MapMeshOnly select t2)
            {
                ar_td.Add(t);
                ar_td0.Add(t);
            }
            foreach (ThingDef t in from t2 in DefDatabase<ThingDef>.AllDefs where t2.drawerType == DrawerType.MapMeshAndRealTime select t2)
            {
                ar_td.Add(t);
                ar_td0.Add(t);
            }
        }
        

        public override void SettingsChanged()
        {
            val_camSpeed = val_camSpeed_s.Value;
            val_wallHeight = val_wallHeight_s.Value;
            val_verticalBuildingFactor = val_verticalBuildingFactor_s.Value;

            val_angle = val_angle_s.Value;
            val_fov = val_fov_s.Value;
            val_z = val_z_s.Value;
            val_far = val_far_s.Value;

            val_debug = val_debug_s.Value;
        }




        bool setuped = false;
        public override void WorldLoaded()
        {
            
        }

        float d_speed;
        float d_far;
        Vector3 d_angle;
        float d_fov;
        Vector3 d_pos;
        float d_orth;

        float zGap => 35f - 0.8f * val_angle_s;

        int t = 0;
        public override void Tick(int currentTick)
        {
            if(t == 0)
            {
                t++;
            }
            else if(t == 1)
            {
                d_speed = Current.CameraDriver.config.moveSpeedScale;
                d_far = Current.Camera.farClipPlane;
                d_angle = Current.Camera.transform.localEulerAngles;
                d_fov = Current.Camera.fieldOfView;
                d_pos = Current.Camera.transform.localPosition;
                
                setuped = true;
                t++;
            }

        }


        Vector3 pos => Traverse.Create(Current.CameraDriver).Field("rootPos").GetValue<Vector3>();
        float size => Traverse.Create(Current.CameraDriver).Field("rootSize").GetValue<float>();
        const float zoomSize = 60f;

        public override void Update()
        {
            
            if (!setuped) return;
            if (Current.ProgramState != ProgramState.Playing) return;
            if (mode3d)
            {
                Current.CameraDriver.config.moveSpeedScale = 2f / val_fov * val_camSpeed;
                Current.Camera.farClipPlane = val_far;
                Current.Camera.transform.localEulerAngles = new Vector3(val_angle, 0f, 0f);
                Current.Camera.fieldOfView = val_fov;
                
                Current.CameraDriver.SetRootPosAndSize(pos, zoomSize);
                Current.Camera.transform.localPosition = new Vector3(Current.Camera.transform.localPosition.x, val_z, Current.Camera.transform.localPosition.z);
                //Current.CameraDriver.config.zoomPreserveFactor = 130f;
            }
            else
            {
                d_pos = Current.Camera.transform.localPosition;
                d_orth = size;
            }
                


        }


        public override void OnGUI()
        {

            if (Current.ProgramState == ProgramState.Playing && Find.CurrentMap != null && !WorldRendererUtility.WorldRenderedNow)
            {
                if ((int)Event.current.type == 4 && (int)Event.current.keyCode != 0 && rim3dKeyBingings.Toggle3D.JustPressed && !WorldRendererUtility.WorldRenderedNow)
                {
                    // 3d on off toggle
                    mode3d = !mode3d;

                    if (mode3d)
                    {
                        on();
                    }
                    else
                    {
                        off();
                    }
                }

            }
        }




        Section[,] sections => Traverse.Create(Current.Game.CurrentMap.mapDrawer).Field("sections").GetValue<Section[,]>();
        private IntVec2 SectionCount
        {
            get
            {
                IntVec2 result = default(IntVec2);
                result.x = Mathf.CeilToInt((float)Current.Game.CurrentMap.Size.x / 17f);
                result.z = Mathf.CeilToInt((float)Current.Game.CurrentMap.Size.z / 17f);
                return result;
            }
        }

        void on()
        {
            Current.Camera.orthographic = false;
            DebugViewSettings.drawThingsDynamic = true;
            Current.CameraDriver.config.moveSpeedScale = 2f / val_fov * val_camSpeed;
            Current.Camera.farClipPlane = val_far;
            Current.Camera.transform.localEulerAngles = new Vector3(val_angle, 0f, 0f);
            Current.Camera.fieldOfView = val_fov;
            //Current.Camera.transform.localPosition = new Vector3(Current.Camera.transform.localPosition.x, val_z, Current.Camera.transform.localPosition.z);
            Current.CameraDriver.SetRootPosAndSize(new Vector3(pos.x, val_z, pos.z - zGap), zoomSize);
            //foreach(ThingDef t in ar_td0) t.drawerType = DrawerType.RealtimeOnly;
            //foreach (ThingDef t in ar_td2) t.drawerType = DrawerType.RealtimeOnly;
            //foreach (ThingDef t in ar_td3) t.drawerType = DrawerType.RealtimeOnly;
            //Current.Game.CurrentMap.dynamicDrawManager.DrawDynamicThings();
            if (sections == null)
            {
                Traverse.Create(Current.Game.CurrentMap.mapDrawer).Field("sections").SetValue(new Section[SectionCount.x, SectionCount.z]);
            }
            for (int i = 0; i < SectionCount.x; i++)
            {
                for (int j = 0; j < SectionCount.z; j++)
                {
                    if (sections[i, j] == null)
                    {
                        sections[i, j] = new Section(new IntVec3(i, 0, j), Current.Game.CurrentMap);
                    }
                    List<SectionLayer> layers = Traverse.Create(sections[i, j]).Field("layers").GetValue<List<SectionLayer>>();
                    if (layers == null) continue;
                    foreach (SectionLayer l in layers)
                    {
                        if ((l.relevantChangeTypes == MapMeshFlag.Terrain)) continue;
                        AccessTools.Method(typeof(SectionLayer), "ClearSubMeshes").Invoke(l, new object[] { MeshParts.All });
                        AccessTools.Method(typeof(SectionLayer), "FinalizeMesh").Invoke(l, new object[] { MeshParts.All });
                    }
                }
            }
            
            
        }
        void off()
        {
            Current.Camera.orthographic = true;
            DebugViewSettings.drawThingsDynamic = true;
            Current.CameraDriver.config.moveSpeedScale = d_speed;
            Current.Camera.farClipPlane = d_far;
            Current.Camera.transform.localEulerAngles = d_angle;
            Current.Camera.fieldOfView = d_fov;
            Current.CameraDriver.SetRootPosAndSize(pos + new Vector3(0f, 0f, zGap), d_orth);
            //foreach (ThingDef t in ar_td0) t.drawerType = DrawerType.None;
            //foreach (ThingDef t in ar_td2) t.drawerType = DrawerType.MapMeshOnly;
            //foreach (ThingDef t in ar_td3) t.drawerType = DrawerType.MapMeshAndRealTime;
            Current.Game.CurrentMap.mapDrawer.RegenerateEverythingNow();
            //if (sections == null)
            //{
            //    Traverse.Create(Current.Game.CurrentMap.mapDrawer).Field("sections").SetValue(new Section[SectionCount.x, SectionCount.z]);
            //}
            //for (int i = 0; i < SectionCount.x; i++)
            //{
            //    for (int j = 0; j < SectionCount.z; j++)
            //    {
            //        if (sections[i, j] == null)
            //        {
            //            sections[i, j] = new Section(new IntVec3(i, 0, j), Current.Game.CurrentMap);
            //        }
            //        List<SectionLayer> layers = Traverse.Create(sections[i, j]).Field("layers").GetValue<List<SectionLayer>>();
            //        if (layers == null) continue;
            //        foreach (SectionLayer l in layers)
            //        {
            //            if (!(l is SectionLayer_Things)) continue;
            //            AccessTools.Method(typeof(SectionLayer), "ClearSubMeshes").Invoke(l, new object[] { MeshParts.All });
            //            AccessTools.Method(typeof(SectionLayer), "FinalizeMesh").Invoke(l, new object[] { MeshParts.All });
            //        }
            //    }
            //}
        }

        



    }










}