using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;
using AppearanceClothes;
using AlienRace;
using System.Collections.Generic;
using System.Linq;

namespace AppearanceClothes_AlienRacePatch {
    [StaticConstructorOnStartup]
    class HarmonyPatches {
        static HarmonyPatches() {
            var harmony = new Harmony("com.tammybee.apparentclothes.alienracepatch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
	}

	[HarmonyPatch(typeof(ITab_Pawn_AppearanceClothes))]
	[HarmonyPatch("GetBodyTypeDefs")]
	public static class ITab_Pawn_AppearanceClothes_GetBodyTypeDefs_Patch {
		static void Postfix(Pawn pawn, ref List<BodyTypeDef> __result) {
			ThingDef_AlienRace thingDef_AlienRace = pawn.def as ThingDef_AlienRace;
            if (thingDef_AlienRace?.alienRace?.generalSettings?.alienPartGenerator != null && !thingDef_AlienRace.alienRace.generalSettings.alienPartGenerator.alienbodytypes.NullOrEmpty()) {
				__result = thingDef_AlienRace.alienRace.generalSettings.alienPartGenerator.alienbodytypes;
			}
		}
	}

	[HarmonyPatch(typeof(ITab_Pawn_AppearanceClothes))]
    [HarmonyPatch("CanAdd")]
    public static class ITab_Pawn_AppearanceClothes_CanAdd_Patch {
        static void Postfix(ITab_Pawn_AppearanceClothes __instance,ThingDef def, ref bool __result) {
            if (__result) {
                Pawn pawn = Traverse.Create(__instance).Property("SelPawnForGear").GetValue<Pawn>();
                ThingDef_AlienRace thingDef_AlienRace = pawn.def as ThingDef_AlienRace;
                if (thingDef_AlienRace != null) {
                    __result = RaceRestrictionSettings.CanWear(def, pawn.def);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet_ResolveAllGraphics_Patch))]
    [HarmonyPatch("GetNakedGraphic")]
    public static class PawnGraphicSet_ResolveAllGraphics_Patch_GetNakedGraphic_Patch {
        static void Postfix(Pawn pawn, BodyTypeDef bodyType, ref Graphic __result) {
            ThingDef_AlienRace thingDef_AlienRace = pawn.def as ThingDef_AlienRace;
            if (thingDef_AlienRace != null && pawn.def.defName != "Human") {
                GraphicPaths currentGraphicPath = Traverse.Create(AccessTools.TypeByName("GraphicPathsExtension")).Method("GetCurrentGraphicPath").GetValue<GraphicPaths>(thingDef_AlienRace.alienRace.graphicPaths, pawn.ageTracker.CurLifeStage);
                if (!currentGraphicPath.body.NullOrEmpty()) {
                    AlienPartGenerator.AlienComp alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
                    string bodyMask = currentGraphicPath.bodyMasks.NullOrEmpty()
                                          ? string.Empty
                                          : currentGraphicPath.bodyMasks + ((alienComp.bodyMaskVariant >= 0
                                                                           ? alienComp.bodyMaskVariant
                                                                           : (alienComp.bodyMaskVariant =
                                                                                  Rand.Range(min: 0, currentGraphicPath.BodyMaskCount))) > 0
                                                                          ? alienComp.bodyMaskVariant.ToString()
                                                                          : string.Empty);
                    Shader shader = (ContentFinder<Texture2D>.Get(AlienPartGenerator.GetNakedPath(bodyType, currentGraphicPath.body, thingDef_AlienRace.alienRace.generalSettings.alienPartGenerator.useGenderedBodies ? pawn.gender.ToString() : "") + "_northm", false) == null) ? ShaderDatabase.Cutout : ShaderDatabase.CutoutComplex;
                    __result = thingDef_AlienRace.alienRace.generalSettings.alienPartGenerator.GetNakedGraphic(bodyType, shader, pawn.story.SkinColor, thingDef_AlienRace.alienRace.generalSettings.alienPartGenerator.SkinColor(pawn, false), currentGraphicPath.body, pawn.gender.ToString(), bodyMask);
                }
            }
        }
    }

    /*
    [HarmonyPatch(typeof(PawnGraphicSet_ResolveAllGraphics_Patch))]
    [HarmonyPatch("GetRottingGraphic")]
    public static class PawnGraphicSet_ResolveAllGraphics_Patch_GetRottingGraphic_Patch {
        static void Postfix(Pawn pawn, BodyTypeDef bodyType, ref Graphic __result) {
            ThingDef_AlienRace thingDef_AlienRace = pawn.def as ThingDef_AlienRace;
            if (thingDef_AlienRace != null) {
                GraphicPaths currentGraphicPath = Traverse.Create(AccessTools.TypeByName("GraphicPathsExtension")).Method("GetCurrentGraphicPath").GetValue<GraphicPaths>(thingDef_AlienRace.alienRace.graphicPaths, pawn.ageTracker.CurLifeStage);
                if (!currentGraphicPath.body.NullOrEmpty()) {
                    __result = thingDef_AlienRace.alienRace.generalSettings.alienPartGenerator.GetNakedGraphic(bodyType, ShaderDatabase.Cutout, PawnGraphicSet.RottingColor, PawnGraphicSet.RottingColor, currentGraphicPath.body, pawn.gender.ToString());
                }
            }
        }
    }
    */
}
