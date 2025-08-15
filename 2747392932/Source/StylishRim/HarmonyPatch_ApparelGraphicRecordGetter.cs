using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace StylishRim
{
    public static class HarmonyPatch_ApparelGraphicRecordGetter
    {
        internal static IEnumerable<CodeInstruction> TryGetGraphicApparel_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo getMethod = AccessTools.Method(typeof(GraphicDatabase), nameof(GraphicDatabase.Get),
                new System.Type[4]
                {// string path, Shader shader, Vector2 drawSize, Color color
                    typeof(string), typeof(Shader), typeof(Vector2), typeof(Color)
                },
                new System.Type[1] { typeof(Graphic_Multi) });
            foreach (CodeInstruction code in instructions)
            {
                if (code.OperandIs(getMethod))
                {
                    yield return CodeInstruction.Call(typeof(HarmonyPatch_ApparelGraphicRecordGetter), nameof(HarmonyPatch_ApparelGraphicRecordGetter.TryGetValueUnfindedApparelBodytype));
                    continue;
                }
                yield return code;
            }
        }
        public static Graphic TryGetValueUnfindedApparelBodytype(string path, Shader shader, Vector2 drawSize, Color color)
        {
            if (!StylishModSettings.resolveUnfindedApparelBodytype) return GraphicDatabase.Get<Graphic_Multi>(path, shader, drawSize, color);
            GraphicRequest req = new GraphicRequest(typeof(Graphic_Multi), path, shader, drawSize, color, Color.white, null, 0, null, null);
            req.color = (Color32)req.color;
            req.colorTwo = (Color32)req.colorTwo;
            req.renderQueue = (req.renderQueue == 0 && req.graphicData != null) ? req.graphicData.renderQueue : req.renderQueue;
            Dictionary<GraphicRequest, Graphic> dic = AccessTools.StaticFieldRefAccess<Dictionary<GraphicRequest, Graphic>>(typeof(GraphicDatabase), "allGraphics");
            if (!dic.TryGetValue(req, out Graphic graphic))
            {
                graphic = (Graphic)(object)new Graphic_Multi();
                graphic.Init(req);
                if (graphic.MatNorth == null)
                {
                    req.path = GetApparelPathForUnfindedPath(req.path);
                    graphic.Init(req);
                    req.path = path;
                }
                dic.Add(req, graphic);
            }
            return (Graphic_Multi)graphic;
        }
        private static string GetApparelPathForUnfindedPath(string path)
        {
            int sepLen = path.LastIndexOf('/');
            string wornGraphicPath = path.Substring(0, sepLen);
            foreach (Texture2D tex in ContentFinder<Texture2D>.GetAllInFolder(wornGraphicPath))
            {
                if (Regex.IsMatch(tex.name, $"{path.Substring(sepLen + 1, path.LastIndexOf('_') - sepLen - 1)}.*?_north"))
                {
                    return $"{wornGraphicPath}/{tex.name.Substring(0, tex.name.LastIndexOf('_'))}";
                }
            }
            return path;
        }
    }
}
