using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DireWolves
{
    public class AnimationDef : ThingDef
    {
        public new GraphicDataAnimation graphicData;

        public int maxLoopCount = 1;

        public int fpsRate = 1;
        public override void PostLoad()
        {
            base.PostLoad();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                this.graphicData.InitMainTextures();
            });
        }
    }
    public class Graphic_Animation : Graphic
    {

        public Graphic[] subGraphics;
        public override void Init(GraphicRequest req)
        {
            data = req.graphicData;
            if (req.path.NullOrEmpty())
            {
                throw new ArgumentNullException("folderPath");
            }
            if (req.shader == null)
            {
                throw new ArgumentNullException("shader");
            }
            base.path = req.path;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;
            List<Texture2D> list = (from x in ContentFinder<Texture2D>.GetAllInFolder(req.path)
                                    where !x.name.EndsWith(Graphic_Single.MaskSuffix)
                                    orderby x.name
                                    select x).ToList();
            if (list.NullOrEmpty())
            {
                Log.Error("Collection cannot init: No textures found at path " + req.path);
                subGraphics = new Graphic[1]
                {
                    BaseContent.BadGraphic
                };
                return;
            }
            subGraphics = new Graphic[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                string path = req.path + "/" + list[i].name;
                subGraphics[i] = GraphicDatabase.Get(typeof(Graphic_Single), path, req.shader, drawSize, color, colorTwo, null, req.shaderParameters);
            }
        }
    }
    public class Mote_Animation : Mote
    {
        protected bool destroy;
        public int curInd;

        protected int curLoopInd;
        public virtual void OnCycle_Completion()
        {
            curLoopInd++;
        }
        public override void Draw()
        {
            DrawCustom(def.altitudeLayer.AltitudeFor());
        }

        private Graphic graphicInt;
        public new AnimationDef def => base.def as AnimationDef;
        public new Graphic DefaultGraphic
        {
            get
            {
                if (graphicInt == null)
                {
                    if (def.graphicData == null)
                    {
                        return BaseContent.BadGraphic;
                    }
                    graphicInt = def.graphicData.GraphicColoredFor(this);
                }
                return graphicInt;
            }
        }
        public override Graphic Graphic => DefaultGraphic;
        public Graphic_Animation Graphic_Animation => Graphic as Graphic_Animation;

        protected int prevTick;
        public virtual void DrawCustom(float altitude)
        {
            exactPosition.y = altitude;

            var loc = DrawPos + this.def.graphicData.drawOffset;
            Vector3 s = new Vector3(this.def.graphicData.drawSize.x, 1f, this.def.graphicData.drawSize.y);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(loc, Quaternion.identity, s);
            var ind = this.curInd <= Graphic_Animation.subGraphics.Length - 1 ? this.curInd : Graphic_Animation.subGraphics.Length - 1;
            Graphics.DrawMesh(MeshPool.plane10, matrix, Graphic_Animation.subGraphics[ind].MatSingle, 0);

            if (prevTick != Find.TickManager.TicksGame && Find.TickManager.TicksGame % this.def.fpsRate == 0)
            {
                this.curInd++;
                if (this.curInd >= Graphic_Animation.subGraphics.Length - 1)
                {
                    this.OnCycle_Completion();
                    this.curInd = 0;
                }
            }
            prevTick = Find.TickManager.TicksGame;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref curInd, "curInd");
            Scribe_Values.Look(ref curLoopInd, "curLoopInd");
        }
    }
    public class GraphicDataAnimation : GraphicData
    {
        public float animationSpeedRate = 1f;

        private Material[] cachedMaterials;

        private static Dictionary<string, Material[]> loadedMaterials = new Dictionary<string, Material[]>();
        public Material[] Materials
        {
            get
            {
                if (cachedMaterials == null)
                {
                    InitMainTextures();
                }
                return cachedMaterials;
            }
        }
        public void InitMainTextures()
        {
            if (!loadedMaterials.TryGetValue(texPath, out var materials))
            {
                var mainTextures = LoadAllFiles(texPath).OrderBy(x => x).ToList();
                if (mainTextures.Count > 0)
                {
                    cachedMaterials = new Material[mainTextures.Count];
                    for (var i = 0; i < mainTextures.Count; i++)
                    {
                        var shader = this.shaderType != null ? this.shaderType.Shader : ShaderDatabase.DefaultShader;
                        cachedMaterials[i] = MaterialPool.MatFrom(mainTextures[i], shader, color);
                    }
                }
                else
                {
                    Log.Error("Error loading materials by this path: " + texPath);
                }
                loadedMaterials[texPath] = cachedMaterials;
            }
            else
            {
                cachedMaterials = materials;
            }
        }
        public List<string> LoadAllFiles(string folderPath)
        {
            var list = new List<string>();
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
            {
                foreach (var f in ModContentPack.GetAllFilesForMod(mod, "Textures/" + folderPath))
                {
                    var path = f.Value.FullName;
                    if (path.EndsWith(".png"))
                    {
                        path = path.Replace("\\", "/");
                        path = path.Substring(path.IndexOf("/Textures/") + 10);
                        path = path.Replace(".png", "");
                        list.Add(path);
                    }
                }
            }
            return list;
        }
    }
}
