using System;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using Verse.AI;
using Harmony;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Copyprint
{
    [StaticConstructorOnStartup]
    public static class CopyprintPatch
    {
        private static readonly Type patchType = typeof(CopyprintPatch);
        static CopyprintPatch()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("com.Copyprint.rimworld.mod");
            harmonyInstance.Patch(AccessTools.Method(typeof(GhostUtility), name: "GhostGraphicFor"), new HarmonyMethod(patchType, "GhostGraphicForPrefix", null));
        }

        public static bool GhostGraphicForPrefix(ref Graphic __result, Dictionary<int, Graphic> ___ghostGraphics, Graphic baseGraphic, ThingDef thingDef, Color ghostCol)
        {
            if (baseGraphic is Graphic_RandomRotated)
            {
                int num = 0;
                num = Gen.HashCombine(num, thingDef.graphicData.Graphic);
                num = Gen.HashCombine(num, thingDef);
                num = Gen.HashCombineStruct(num, ghostCol);
                if (!___ghostGraphics.TryGetValue(num, out Graphic graphic))
                {
                    GraphicData graphicData = null;
                    if (baseGraphic.data != null)
                    {
                        graphicData = new GraphicData();
                        graphicData.CopyFrom(baseGraphic.data);
                        graphicData.shadowData = null;
                    }
                    graphic = GraphicDatabase.Get(typeof(Graphic_Single), thingDef.graphicData.texPath, ShaderTypeDefOf.EdgeDetect.Shader, baseGraphic.drawSize, ghostCol, Color.white, graphicData, null);

                    ___ghostGraphics.Add(num, graphic);
                }
                __result = graphic;
                return false;
            }
            return true;

        }
    }
}
