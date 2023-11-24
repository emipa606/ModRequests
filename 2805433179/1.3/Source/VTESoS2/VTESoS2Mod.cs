using HarmonyLib;
using RimWorld;
using SaveOurShip2;
using Verse;

namespace VTESoS
{
    public class VTESoS2Mod : Mod
    {
        public static Harmony Harm;

        public VTESoS2Mod(ModContentPack content) : base(content)
        {
            Harm = new Harmony("vanillaexpanded.vtexe.sos2");
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                Harm.Patch(AccessTools.Method(typeof(ShipInteriorMod2), nameof(ShipInteriorMod2.SceneLoaded)),
                    new HarmonyMethod(GetType(), nameof(SceneLoaded_Prefix)), new HarmonyMethod(GetType(), nameof(SceneLoaded_Postfix)));
            });
        }

        public static void SceneLoaded_Prefix(ref bool ___loadedGraphics, out bool __state)
        {
            __state = ___loadedGraphics;
            ___loadedGraphics = true;
        }

        public static void SceneLoaded_Postfix(bool __state, ref bool ___loadedGraphics)
        {
            if (__state) return;
            ___loadedGraphics = false;
            foreach (var thingToResolve in CompShuttleCosmetics.GraphicsToResolve.Keys)
            {
                var graphicsResolved = new Graphic[CompShuttleCosmetics.GraphicsToResolve[thingToResolve].graphics.Count];
                var graphicsHoverResolved = new Graphic[CompShuttleCosmetics.GraphicsToResolve[thingToResolve].graphicsHover.Count];
                for (var i = 0; i < CompShuttleCosmetics.GraphicsToResolve[thingToResolve].graphics.Count; i++)
                {
                    var graphicData = CompShuttleCosmetics.GraphicsToResolve[thingToResolve].graphics[i];
                    graphicsResolved[i] = GraphicDatabase.Get(graphicData.graphicClass, graphicData.texPath,
                        (graphicData.shaderType ?? ShaderTypeDefOf.Cutout).Shader, graphicData.drawSize, graphicData.color, graphicData.colorTwo, graphicData,
                        graphicData.shaderParameters);
                }

                for (var j = 0; j < CompShuttleCosmetics.GraphicsToResolve[thingToResolve].graphicsHover.Count; j++)
                {
                    var graphicData = CompShuttleCosmetics.GraphicsToResolve[thingToResolve].graphicsHover[j];
                    graphicsHoverResolved[j] = GraphicDatabase.Get(graphicData.graphicClass, graphicData.texPath,
                        (graphicData.shaderType ?? ShaderTypeDefOf.Cutout).Shader, graphicData.drawSize, graphicData.color, graphicData.colorTwo, graphicData,
                        graphicData.shaderParameters);
                }

                CompShuttleCosmetics.graphics.Add(thingToResolve.defName, graphicsResolved);
                CompShuttleCosmetics.graphicsHover.Add(thingToResolve.defName, graphicsHoverResolved);
            }

            ___loadedGraphics = true;
        }
    }
}