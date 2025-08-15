using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace StylishRim
{
    // 本当はパッチを厳密に管理して～のパッチに失敗したので～の機能は無効になります
    // みたいなことをやるべきなんだろうけどもう手遅れだ
    // 手遅れだ！！！！！！
    [StaticConstructorOnStartup()]
    public static class StylishRim
    {
        internal static Harmony harmony;
        internal static bool showHair = false;
        internal static bool alienRace = false;

        internal static string NESTED_CLASS_DRAW_APPAREL_NAME = "DrawApparel";
        internal static string NESTED_CLASS_DRAW_EXTRA_EYE_NAME = "DrawExtraEye";
        internal static string NESTED_CLASS_DRAW_GENE_NAME = "DrawGene|";
        internal static string NESTED_CLASS_DRAW_GENE_EYES_NAME = "DrawGeneEyes";
        internal static string NESTED_CLASS_TRY_DRAW_GENES_NAME = "TryDraw";
        internal static string NESTED_CLASS_DRAW_ADDON = "g__DrawAddon";

        static StylishRim()
        {
            try
            {
                StylishRedefinition.Redefine();
            }
            catch { }
            harmony = new Harmony("tank.mod.stylishrim");
            Type targetType;
            Type patchType;

            //AlienRace
            targetType = AccessTools.TypeByName("AlienRace.HarmonyPatches");
            if (targetType != null)
            {
                // AlienRaceに寄生するのだ
                //AlienRace.HarmonyPatches
                patchType = typeof(HarmonyPatch_AlienRace);
                //harmony.Patch(AccessTools.Method(targetType, nameof(AlienRace.HarmonyPatches.GetPawnMesh)), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.GetPawnMesh_Transpiler)));
                //harmony.Patch(AccessTools.Method(targetType, nameof(AlienRace.HarmonyPatches.GetPawnHairMesh)), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.GetPawnHairMesh_Transpiler)));
                harmony.Patch(AccessTools.Method(targetType, nameof(AlienRace.HarmonyPatches.DrawAddons)), new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.DrawAddons_Prefix)), new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.DrawAddons_Postfix)), new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.DrawAddons_Transpiler)));
                foreach (MethodInfo mi in AccessTools.GetDeclaredMethods(typeof(AlienRace.HarmonyPatches)))
                {
                    if (mi.Name.Contains(NESTED_CLASS_DRAW_ADDON))
                    {
                        harmony.Patch(mi, null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.DrawAddonCompiled_Transpiler)));
                    }
                }
                harmony.Patch(AccessTools.Method(targetType, nameof(AlienRace.HarmonyPatches.GetBorderSizeForPawn)), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.GetBorderSizeForPawn_Transpiler)));
                harmony.Patch(AccessTools.Method(targetType, nameof(AlienRace.HarmonyPatches.GetAtlasSizeForPawn)), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.GetAtlasSizeForPawn_Transpiler)));
                harmony.Patch(AccessTools.Method(targetType, nameof(AlienRace.HarmonyPatches.ResolveAllGraphicsPrefix)), null, new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.ResolveAllGraphicsPrefix_PostFix)), new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.ResolveAllGraphicsPrefix_Transpiler)));
                //harmony.Patch(AccessTools.Method(targetType, nameof(AlienRace.HarmonyPatches.GlobalTextureAtlasGetFrameSetPrefix)), null, new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.GlobalTextureAtlasGetFrameSetPrefix_Postfix)));
                harmony.Patch(AccessTools.Method(targetType, nameof(AlienRace.HarmonyPatches.CacheRenderPawnPrefix)), null, new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.CacheRenderPawnPrefix_Postfix)));

                //================================ PREFIX !!!!!!!!!!!!!!=================================//
                harmony.Patch(AccessTools.Method(targetType, nameof(AlienRace.HarmonyPatches.TextureAtlasSameRace)), new HarmonyMethod(patchType, nameof(HarmonyPatch_AlienRace.TextureAtlasSameRace_Prefix)));
                //================================ PREFIX !!!!!!!!!!!!!!=================================//

                harmony.Patch(AccessTools.Method(targetType, nameof(AlienRace.HarmonyPatches.CanEquipPostfix)), null, null, new HarmonyMethod(typeof(HarmonyPatch_AlienRaceApparelRestrict), nameof(HarmonyPatch_AlienRaceApparelRestrict.CanEquipPostfix_Transpiler)));

                harmony.Patch(AccessTools.Method(typeof(AlienRace.ThingDef_AlienRace), nameof(AlienRace.ThingDef_AlienRace.ResolveReferences)), null, null, new HarmonyMethod(typeof(HarmonyPatch_AlienRace), nameof(HarmonyPatch_AlienRace.ResolveReferences_Transpiler)));
                /*
                harmony.Patch(AccessTools.Method(typeof(AlienRace.AlienPartGenerator.RotationOffset), nameof(AlienRace.AlienPartGenerator.RotationOffset.GetOffset)), new HarmonyMethod(typeof(HarmonyPatch_AlienRace), nameof(HarmonyPatch_AlienRace.GetOffset_Prefix)));
                */
                alienRace = true;

            }
            try
            {
                // VFEV.Facepaint.HarmonyPatches_Facepaint
                targetType = AccessTools.TypeByName("VFEV.Facepaint.HarmonyPatches_Facepaint");
                if (targetType != null)
                {
                    patchType = typeof(HarmonyPatch_HarmonyPatches_Facepaint);
                    harmony.Patch(AccessTools.Method(targetType, "RenderFacepaint"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_HarmonyPatches_Facepaint.RenderFacepaint_Transpiler)));
                    Log.Message("[Stylish Rim] Vanilla Factions Expanded - Vikings patched.");
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }
            try
            {
                //ShowHair
                targetType = AccessTools.TypeByName("ShowHair.Patch_PawnRenderer_DrawHeadHair");
                if (targetType != null)
                {
                    showHair = true;
                    //ShowHair.Patch_PawnRenderer_DrawHeadHair
                    //targetType = typeof(ShowHair.Patch_PawnRenderer_DrawHeadHair);
                    patchType = typeof(HarmonyPatch_ShowHair);
                    harmony.Patch(AccessTools.Method(targetType, "DrawMeshNowOrLaterPatch"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_ShowHair.DrawMeshNowOrLaterPatch_Transpiler)));
                    Log.Message("[Stylish Rim] [KV] Show Hair With Hats or Hide All Hats patched.");
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }
            try
            {
                //HatDisplaySelection
                targetType = AccessTools.TypeByName("HatDisplaySelection.Patch");
                if (targetType != null)
                {
                    showHair = true;
                    harmony.Patch(AccessTools.Method(targetType, "DrawHatWithHair_14"), new HarmonyMethod(typeof(HarmonyPatch_PawnRender), nameof(HarmonyPatch_PawnRender.DrawHeadHair_Prefix)), null, new HarmonyMethod(typeof(HarmonyPatch_HatDisplaySelection), nameof(HarmonyPatch_HatDisplaySelection.DrawHatWithHair_14_Transpiler)));
                    /*
                    harmony.Patch(AccessTools.Method(targetType, "DrawApparel2"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_HatDisplaySelection.DrawApparel2_Transpiler)));
                    harmony.Patch(AccessTools.Method(targetType, "DrawHatCEWithHair"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_HatDisplaySelection.DrawHatCEWithHair_Transpiler)));
                    harmony.Patch(AccessTools.Method(targetType, "DrawHatWithHair"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_HatDisplaySelection.DrawHatWithHair_Transpiler)));
                    harmony.Patch(AccessTools.Method(targetType, "DrawHatWithHair2"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_HatDisplaySelection.DrawHatWithHair2_Transpiler)));
                    */

                    patchType = typeof(HarmonyPatch_HatDisplaySelection);
                    foreach (Type t in typeof(PawnRenderer).GetNestedTypes(AccessTools.all))
                    {
                        if (AccessTools.GetDeclaredMethods(t).Any(x => x.Name.Contains(NESTED_CLASS_DRAW_APPAREL_NAME)))
                        {
                            HarmonyPatch_HatDisplaySelection.nestedType = t;
                            foreach (MethodInfo mi in AccessTools.GetDeclaredMethods(t))
                            {
                                if (mi.Name.Contains(NESTED_CLASS_DRAW_APPAREL_NAME))
                                {
                                    harmony.Patch(mi, null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_HatDisplaySelection.DrawHeadHairCompiled_Transpiler)));
                                }
                                else if (mi.Name.Contains(NESTED_CLASS_DRAW_EXTRA_EYE_NAME))
                                {
                                    harmony.Patch(mi, null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_HatDisplaySelection.DrawExtraEyesCompiled_Transpiler)));
                                }
                                else if (mi.Name.Contains(NESTED_CLASS_DRAW_GENE_NAME))
                                {
                                    harmony.Patch(mi, null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_HatDisplaySelection.DrawHeadGenesCompiled_Transpiler)));
                                }
                            }
                        }
                    }
                    Log.Message("[Stylish Rim] Hats Display Selection patched.");
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }
            try
            {
                //Dubs Apparel Tweaks
                targetType = AccessTools.TypeByName("QuickFast.bs+H_DrawHeadHair");
                if (targetType != null)
                {
                    harmony.Patch(AccessTools.Method(targetType, "Prefix"), null, null, new HarmonyMethod(typeof(HarmonyPatch_DubsApparelTweaks), nameof(HarmonyPatch_DubsApparelTweaks.Prefix_Transpiler)));
                    Log.Message("[Stylish Rim] Patching to draw hair in alternative mode in Dubs Apparel Tweaks.");
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }

            try
            {
                //yayoAni
                targetType = AccessTools.TypeByName("yayoAni.patch_DrawEquipmentAiming");
                if (targetType != null)
                {
                    //yayoAni.PawnRenderer_override
                    patchType = typeof(HarmonyPatch_yayoAni);
                    harmony.Patch(AccessTools.Method(targetType, "Prefix"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_yayoAni.DrawEquipmentAiming_Transpiler)));
                }
                //yayoAni(Continue)
                targetType = AccessTools.TypeByName("yayoAni.Compat.HumanoidAlienRaces+Prefix_AlienRace_HarmonyPatches_DrawAddons");
                if (targetType != null)
                {
                    //yayoAni.Compat.HumanoidAlienRaces+Prefix_AlienRace_HarmonyPatches_DrawAddons
                    patchType = typeof(HarmonyPatch_yayoAni);
                    harmony.Patch(AccessTools.Method(targetType, "Prefix"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_yayoAni.Prefix_AlienRace_HarmonyPatches_DrawAddons_Prefix_Transpiler)));
                    Log.Message("[Stylish Rim] Yayo's Animation patched.");
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }
            /*
            try
            {
                //RimThreaded
                targetType = AccessTools.TypeByName("RimThreaded.PortraitsCache_Patch");
                if (targetType != null)
                {
                    // RimThreadedに寄生するのだ
                    //RimThreaded.PortraitsCache_Patch
                    patchType = typeof(HarmonyPatch_RimThreaded);
                    harmony.Patch(AccessTools.Method(targetType, "Get"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_RimThreaded.Get_Transpiler)));

                    Log.Message("[Stylish Rim] RimThreaded patched.");
                }
                else
                {
                    */
            /*
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }
            */
            try
            {
                //harFix_yayoAni
                targetType = AccessTools.TypeByName("harFix_yayoAni.Prefix_AlienRace_HarmonyPatches_DrawAddons");
                if (targetType != null)
                {
                    // YayoAniFixに寄生するのだ
                    //harFix_yayoAni.Prefix_AlienRace_HarmonyPatches_DrawAddons
                    patchType = typeof(HarmonyPatch_HAR_Fix_for_Yayo_Animation);
                    harmony.Patch(AccessTools.Method(targetType, "Prefix"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_HAR_Fix_for_Yayo_Animation.Prefix_Transpiler)));

                    Log.Message("[Stylish Rim] HAR Fix for Yayo Animation patched.");
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }
            try
            {
                //FacialAnimation
                targetType = AccessTools.TypeByName("FacialAnimation.DrawFaceGraphicsComp");
                if (targetType != null)
                {
                    // Facial Animationに寄生す……こいつ寄生しづらいぞ
                    //FacialAnimation.DrawFaceGraphicsComp
                    StylishModSettings.includeFacialAnimation = true;
                    patchType = typeof(HarmonyPatch_FacialAnimation);
                    harmony.Patch(AccessTools.Method(targetType, "DrawGraphics"), new HarmonyMethod(patchType, "DrawGraphics_Prefix"), new HarmonyMethod(patchType, "DrawGraphics_Postfix"));
                    targetType = AccessTools.TypeByName("FacialAnimation.GraphicHelper");
                    harmony.Patch(AccessTools.Method(targetType, "DrawMeshNowOrLaterWithScale", new Type[]
                    {// Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, float scaleW, float scaleH
                        typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool), typeof(float), typeof(float)
                    }), new HarmonyMethod(patchType, "DrawMeshNowOrLaterWithScale_Prefix"));
                    harmony.Patch(AccessTools.Method("FacialAnimation.HarmonyPatches:DrawFace"), null, null, new HarmonyMethod(patchType, "DrawFace_Transpiler"));
                    /*

                    MethodInfo mi = AccessTools.GetDeclaredMethods(targetType).Find(x => x.Name.Contains("DrawBodyPart"));
                    harmony.Patch(mi.MakeGenericMethod(new Type[1] { AccessTools.TypeByName("FacialAnimation.BrowControllerComp") }), null, null, new HarmonyMethod(patchType, "DrawBodyPart_Transpiler"));
                    harmony.Patch(mi.MakeGenericMethod(new Type[1] { AccessTools.TypeByName("FacialAnimation.EmotionControllerComp") }), null, null, new HarmonyMethod(patchType, "DrawBodyPart_Transpiler"));
                    harmony.Patch(mi.MakeGenericMethod(new Type[1] { AccessTools.TypeByName("FacialAnimation.EyeballControllerComp") }), null, null, new HarmonyMethod(patchType, "DrawBodyPart_Transpiler"));
                    harmony.Patch(mi.MakeGenericMethod(new Type[1] { AccessTools.TypeByName("FacialAnimation.HeadControllerComp") }), null, null, new HarmonyMethod(patchType, "DrawBodyPart_Transpiler"));
                    harmony.Patch(mi.MakeGenericMethod(new Type[1] { AccessTools.TypeByName("FacialAnimation.LidOptionControllerComp") }), null, null, new HarmonyMethod(patchType, "DrawBodyPart_Transpiler"));
                    harmony.Patch(mi.MakeGenericMethod(new Type[1] { AccessTools.TypeByName("FacialAnimation.LidControllerComp") }), null, null, new HarmonyMethod(patchType, "DrawBodyPart_Transpiler"));
                    harmony.Patch(mi.MakeGenericMethod(new Type[1] { AccessTools.TypeByName("FacialAnimation.MouthControllerComp") }), null, null, new HarmonyMethod(patchType, "DrawBodyPart_Transpiler"));
                    harmony.Patch(mi.MakeGenericMethod(new Type[1] { AccessTools.TypeByName("FacialAnimation.SkinControllerComp") }), null, null, new HarmonyMethod(patchType, "DrawBodyPart_Transpiler"));
                    */

                    Log.Message("[Stylish Rim] [NL] Facial Animation - WIP patched.");
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }
/*
            try
            {
                //NewRatkin
                targetType = AccessTools.TypeByName("NewRatkin.Shield");
                if (targetType != null)
                {
                    patchType = typeof(HarmonyPatch_NewRatkin);
                    harmony.Patch(AccessTools.Method(targetType, "DrawWornExtras"), new HarmonyMethod(patchType, nameof(HarmonyPatch_NewRatkin.DrawWornExtras_Prefix)));
                    harmony.Patch(AccessTools.Method(targetType, "DrawShield"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_NewRatkin.DrawShield_Transpiler)));
                }
                targetType = AccessTools.TypeByName("SYS.DrawEquipment_WeaponBackPatch");
                if (targetType != null)
                {
                    patchType = typeof(HarmonyPatch_NewRatkin);
                    harmony.Patch(AccessTools.Method(targetType, "DrawEquipmentAiming"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_NewRatkin.DrawEquipmentAiming_Transpiler)));
                    harmony.Patch(AccessTools.Method(targetType, "DrawEquipmentPrefix"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_NewRatkin.DrawEquipmentPrefix_Transpiler)));
                    harmony.Patch(AccessTools.Method(targetType, "DrawSheath", new Type[]
                    {
                    typeof(Pawn), typeof(Thing), typeof(UnityEngine.Vector3), typeof(float), typeof(Graphic)
                    }), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_NewRatkin.DrawSheath_Transpiler)));

                    Log.Message("[Stylish Rim] NewRatkinPlus patched.");
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }
*/
/*
            try
            {
                //RatkinWeapons
                targetType = AccessTools.TypeByName("RatkinWeapons.Shield_2");
                if (targetType != null)
                {
                    patchType = typeof(HarmonyPatch_NewRatkin);
                    harmony.Patch(AccessTools.Method(targetType, "DrawWornExtras"), new HarmonyMethod(patchType, nameof(HarmonyPatch_NewRatkin.DrawWornExtras_Prefix)));
                    harmony.Patch(AccessTools.Method(targetType, "DrawShield"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_NewRatkin.DrawShield_Transpiler)));
                    Log.Message("[Stylish Rim] Ratkin Weapons+ patched.");
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }
*/
            try
            {
                //CompOversizedWeapon
                targetType = AccessTools.TypeByName("CompOversizedWeapon.HarmonyCompOversizedWeapon");
                if (targetType != null)
                {
                    patchType = typeof(HarmonyPatch_CompOversizedWeapon);
                    harmony.Patch(AccessTools.Method(targetType, "DrawEquipmentAimingPreFix"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_CompOversizedWeapon.DrawEquipmentAimingPreFix_Transpiler)));
                    Log.Message("[Stylish Rim] CompOversizedWeapon patched.");
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }
            try
            {
                //Dual Wield
                targetType = AccessTools.TypeByName("DualWield.Harmony.PawnRenderer_DrawEquipmentAiming");
                if (targetType != null)
                {
                    patchType = typeof(HarmonyPatch_DualWield);
                    harmony.Patch(AccessTools.Method(targetType, "Prefix"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_DualWield.Prefix_Transpiler)));
                    harmony.Patch(AccessTools.Method(targetType, "DrawEquipmentAimingOverride"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_DualWield.DrawEquipmentAimingOverride_Transpiler)));
                    Log.Message("[Stylish Rim] Dual Wield patched.");
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }

            try//FactionBlender.HarmonyPatches
            {
                targetType = AccessTools.TypeByName("FactionBlender.HarmonyPatches");
                if (targetType != null)
                {
                    StylishAutoSetting.factionBlender = true;
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }

            try//AnimalGear.AnimalGearController
            {
                targetType = AccessTools.TypeByName("AnimalGear.AnimalGearController");
                if (targetType != null)
                {
                    StylishAtlasManager.enableAnimalGear = true;
                }
            }
            catch (Exception ex) { Log.Error(ex.ToString()); }


            //RimWorld.WornGraphicData
            targetType = typeof(RimWorld.WornGraphicData);
            patchType = typeof(HarmonyPatch_WornGraphicData);
            harmony.Patch(AccessTools.Method(targetType, nameof(RimWorld.WornGraphicData.BeltOffsetAt)), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_WornGraphicData.BeltOffsetAt_Transpiler)));
            harmony.Patch(AccessTools.Method(targetType, nameof(RimWorld.WornGraphicData.BeltScaleAt)), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_WornGraphicData.BeltScaleAt_Transpiler)));

            //Verse.GlobalTextureAtlasManager
            targetType = typeof(GlobalTextureAtlasManager);
            patchType = typeof(HarmonyPatch_GlobalTextureAtlasManager);
            harmony.Patch(AccessTools.Method(targetType, nameof(GlobalTextureAtlasManager.GlobalTextureAtlasManagerUpdate)), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_GlobalTextureAtlasManager.GlobalTextureAtlasManagerUpdate_Transpiler)));
            harmony.Patch(AccessTools.Method(targetType, nameof(GlobalTextureAtlasManager.FreeAllRuntimeAtlases)), null, new HarmonyMethod(patchType, nameof(HarmonyPatch_GlobalTextureAtlasManager.FreeAllRuntimeAtlases_Postfix)));
            harmony.Patch(AccessTools.Method(targetType, nameof(GlobalTextureAtlasManager.TryGetPawnFrameSet)), new HarmonyMethod(patchType, nameof(HarmonyPatch_GlobalTextureAtlasManager.TryGetPawnFrameSet_Prefix)), null, new HarmonyMethod(patchType, nameof(HarmonyPatch_GlobalTextureAtlasManager.TryGetPawnFrameSet_Transpiler)));

            //Verse.PawnRenderer
            targetType = typeof(PawnRenderer);
            patchType = typeof(HarmonyPatch_PawnRender);
            harmony.Patch(AccessTools.Method(targetType, "DrawHeadHair"), new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawHeadHair_Prefix)), null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawHeadHair_Transpiler)));
            harmony.Patch(AccessTools.Method(targetType, nameof(PawnRenderer.RenderPawnAt)), new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.RenderPawnAt_Prefix)),null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.RenderPawnAt_Transpiler)));
            harmony.Patch(AccessTools.Method(targetType, "RenderPawnInternal"), null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.RenderPawnInternal_Postfix)), new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.RenderPawnInternal_Transpiler)));
            harmony.Patch(AccessTools.Method(targetType, "DrawPawnBody"), new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawPawnBody_Prefix)), new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawPawnBody_Postfix)), new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawPawnBody_Transpiler)));
            harmony.Patch(AccessTools.Method(targetType, "DrawBodyGenes"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawBodyGenes_Transpiler)));
            harmony.Patch(AccessTools.Method(targetType, "DrawPawnFur"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawPawnFur_Transpiler)));
            harmony.Patch(AccessTools.Method(targetType, "DrawBodyApparel"), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawBodyApparel_Transpiler)));
            harmony.Patch(AccessTools.Method(targetType, "DrawEquipment"), new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawEquipment_Prefix)));
            harmony.Patch(AccessTools.Method(targetType, nameof(PawnRenderer.DrawEquipmentAiming)), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawEquipmentAiming_Transpiler)));
            harmony.Patch(AccessTools.Method(targetType, "GetBlitMeshUpdatedFrame"), new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.GetBlitMeshUpdatedFrame_Prefix)), null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.GetBlitMeshUpdatedFrame_Transpiler)));

            foreach (Type t in typeof(PawnRenderer).GetNestedTypes(AccessTools.all))
            {
                if (AccessTools.GetDeclaredMethods(t).Any(x => x.Name.Contains(NESTED_CLASS_DRAW_APPAREL_NAME)))
                {
                    HarmonyPatch_PawnRender.nestedType = t;
                    foreach (MethodInfo mi in AccessTools.GetDeclaredMethods(t))
                    {
                        if (mi.Name.Contains(NESTED_CLASS_DRAW_APPAREL_NAME))
                        {
                            harmony.Patch(mi, null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawHeadHairCompiled_Transpiler)));
                        }
                        else if (mi.Name.Contains(NESTED_CLASS_DRAW_EXTRA_EYE_NAME))
                        {
                            harmony.Patch(mi, null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawExtraEyesCompiled_Transpiler)));
                        }
                        else if (mi.Name.Contains(NESTED_CLASS_DRAW_GENE_NAME))
                        {
                            harmony.Patch(mi, null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawHeadGenesCompiled_Transpiler)));
                        }
                    }
                }
            }
            //harmony.Patch(AccessTools.GetDeclaredMethods(typeof(PawnRenderer).GetNestedTypes()).First((MethodInfo info) => info.HasAttribute<CompilerGeneratedAttribute>() && info.Name.Contains("DrawHeadHair")), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnRender.DrawHeadHairCompiled_Transpiler)));

            //Verse.PawnRenderFlagsExtension
            harmony.Patch(AccessTools.Method(typeof(PawnRenderFlagsExtension), nameof(PawnRenderFlagsExtension.FlagSet)), new HarmonyMethod(typeof(HarmonyPatch_PawnRenderFlagsExtension), nameof(HarmonyPatch_PawnRenderFlagsExtension.FlagSet_Prefix)));

            // Verse.PawnGraphicSet
            targetType = typeof(PawnGraphicSet);
            patchType = typeof(HarmonyPatch_PawnGraphicSet);
            harmony.Patch(AccessTools.Method(targetType, nameof(PawnGraphicSet.MatsBodyBaseAt)), new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnGraphicSet.MatsBodyBaseAt_Prefix)), null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnGraphicSet.MatsBodyBaseAt_Transpiler)));
            harmony.Patch(AccessTools.Method(targetType, nameof(PawnGraphicSet.ResolveApparelGraphics)), null, null, new HarmonyMethod(patchType, nameof(HarmonyPatch_PawnGraphicSet.ResolveApparelGraphics_Transpiler)));

            // ApparelGraphicRecordGetter
            harmony.Patch(AccessTools.Method(typeof(RimWorld.ApparelGraphicRecordGetter), nameof(RimWorld.ApparelGraphicRecordGetter.TryGetGraphicApparel)), null, null, new HarmonyMethod(typeof(HarmonyPatch_ApparelGraphicRecordGetter), nameof(HarmonyPatch_ApparelGraphicRecordGetter.TryGetGraphicApparel_Transpiler)));

            // Verse.PawnTextureAtlas
            harmony.Patch(AccessTools.Constructor(typeof(PawnTextureAtlas)), null, null, new HarmonyMethod(typeof(HarmonyPatch_PawnTextureAtlas), nameof(HarmonyPatch_PawnTextureAtlas.Constructor_Transpiler)));

            //RimWorld.PortraitsCache
            harmony.Patch(AccessTools.Method(typeof(RimWorld.PortraitsCache), nameof(RimWorld.PortraitsCache.Get)), null, null, new HarmonyMethod(typeof(HarmonyPatch_PawnCacheRenderer), nameof(HarmonyPatch_PawnCacheRenderer.Get_Transpiler)));

            // RimWorld.PawnCacheRenderer
            harmony.Patch(AccessTools.Method(typeof(RimWorld.PawnCacheRenderer), nameof(RimWorld.PawnCacheRenderer.RenderPawn)), null, null, new HarmonyMethod(typeof(HarmonyPatch_PawnCacheRenderer), nameof(HarmonyPatch_PawnCacheRenderer.RenderPawn_Transpiler)));

            // RimWorld.Pawn_StyleTracker
            harmony.Patch(AccessTools.PropertyGetter(typeof(RimWorld.Pawn_StyleTracker), nameof(RimWorld.Pawn_StyleTracker.HasUnwantedHairStyle)), new HarmonyMethod(typeof(HarmonyPatch_Pawn_StyleTracker), nameof(HarmonyPatch_Pawn_StyleTracker.HasUnwantedHairStyle_Prefix)));

            // RimWorld.ColonistBar
            patchType = typeof(HarmonyPatch_ColonistBar);
            harmony.Patch(AccessTools.Method(typeof(RimWorld.ColonistBar), nameof(RimWorld.ColonistBar.ColonistBarOnGUI)), new HarmonyMethod(patchType, nameof(HarmonyPatch_ColonistBar.ColonistBarOnGUI_Prefix)), new HarmonyMethod(patchType, nameof(HarmonyPatch_ColonistBar.ColonistBarOnGUI_Postfix)));

            StylishRedefinition.RedefinePortraitHeadRatio();
        }
    }
}
