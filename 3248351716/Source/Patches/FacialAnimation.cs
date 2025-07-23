using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

// FacialAnimation basically has its own dedicated code for rendering faces.
// So we have to patch that code if the mod is loaded. 
// Not sure how often the vanilla ShaderUtility.GetSkinShader runs, but it works best on a fresh pawn / save to work properly.

// FacialAnimation Code looks like:

///////////////////////////////////////////////////////////////////////////////////////////////////
// namespace FacialAnimation;

// public static class GraphicHelper
// {
//     private static Dictionary<Vector2, GraphicMeshSet> headGraphicMeshSetList;

//     private static Dictionary<string, FaceAdjustmentDef> faceAdjustmentDict;

//     public static string GetSkinShader(Pawn pawn, FaceTypeDef def)
//     {
//         ...
//         return def.shader;
//     }
    
//     ...
    
//     static GraphicHelper()
//     {
//         headGraphicMeshSetList = new Dictionary<Vector2, GraphicMeshSet>();
//         faceAdjustmentDict = new Dictionary<string, FaceAdjustmentDef>();
//         foreach (ThingDef allRace in FAHelper.GetAllRaces())
//         {
//             FaceAdjustmentDef defaultFaceSizeAndPositionDef = FaceAdjustmentDefOf.DefaultFaceSizeAndPositionDef;
//             faceAdjustmentDict.Add(((Def)allRace).defName, defaultFaceSizeAndPositionDef);
//         }
//         foreach (FaceAdjustmentDef allDef in DefDatabase<FaceAdjustmentDef>.AllDefs)
//         {
//             faceAdjustmentDict[allDef.RaceName] = allDef;
//         }
//     }
// }
///////////////////////////////////////////////////////////////////////////////////////////////////

namespace SkinTones.Patches
{
    [HarmonyPatch]
    [HarmonyPatch(typeof(ShaderUtility), nameof(ShaderUtility.GetSkinShader))]

    public class FacialAnimationPatch
    {
        public static bool hasPatched = false;

        public static bool Prepare()
        {
            // If FacialAnimation assembly is not found, do not add patch to do the late patch
            var assembly = AppDomain.CurrentDomain.GetAssemblies().
                              SingleOrDefault(assembly => assembly.
                                              GetName().Name == "FacialAnimation");
            return assembly != null;
        }

        // FacialAnimation.GraphicHelper is a static class, so we need to late patch
        // to prevent initializing the static constructor before supposed to.
        // See https://harmony.pardeike.net/articles/patching-edgecases.html#static-constructors
        public static void Postfix()
        {
            if (!hasPatched)
            {
                hasPatched = true;
                try
                {
                    var harmony = new Harmony("h2forge.InclusiveSkinTones");
                    // This GetSkinShader is a different method with the same name as the vanilla Rimworld one.
                    var target = AccessTools.Method(AccessTools.TypeByName("FacialAnimation.GraphicHelper"), "GetSkinShader");
                    if (target == null)
                    {
                        Log.Error("h2forge.InclusiveSkinTones :: Failed to find target method while attempting to patch FacialAnimation.");
                    }
                    var post = new HarmonyMethod(PostGraphicHelperGetSkinShader)
                    {
                        after = ["rimworld.Nals.FacialAnimation"]
                    };
                    harmony.Patch(target, postfix: post);
                    Log.Message("2forge.InclusiveSkinTones :: Patched for FacialAnimation");
                }
                catch (Exception e)
                {
                    Log.Error("2forge.InclusiveSkinTones :: Failed to patch FacialAnimation.GraphicHelperGetSkinShader");
                    Log.Error(e.Message);
                }
            }
        }

        public static void PostGraphicHelperGetSkinShader(Pawn pawn, object def, ref string __result)
        {
            if (!SkinTonesSettings.ApplySkinShader || PawnSkinColors.IsDarkSkin(pawn.story.SkinColor))
            {
                // FacialAnimation returns shader path as a string, vanilla returns the Shader object
                __result = "Map/Cutout";
            }
        }
    }
}