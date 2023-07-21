using System;
using Verse;
using HarmonyLib;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse.AI;
using StatueOfColonist;
using AlienRace;
using System.Collections.Generic;
using System.Linq;
using static AlienRace.AlienPartGenerator;

namespace StatueOfColonistAlienRacePatch {
    [StaticConstructorOnStartup]
    class HarmonyPatches {
        static HarmonyPatches() {
            var harmony = new Harmony("com.tammybee.statueofcolonistalienracepatch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            var method_Org_Constructor = (MethodBase)(typeof(StatueOfColonistData).GetConstructors()[2]);
            var method_Patch_PostFix = typeof(StatueOfColonistData_StatueOfColonistData_Patch).GetMethod("Postfix", AccessTools.all);
            harmony.Patch(method_Org_Constructor, null, new HarmonyMethod(method_Patch_PostFix));
        }
    }

    //[HarmonyPatch(typeof(StatueOfColonistData))]
    //[HarmonyPatch(new Type[] { typeof(Pawn), typeof(Color), typeof(string) })]
    class StatueOfColonistData_StatueOfColonistData_Patch {
        static void Postfix(StatueOfColonistData __instance, Pawn pawn) {
            __instance.raceDef = pawn.def;
        }
    }

    [HarmonyPatch(typeof(Building_StatueOfColonist))]
    [HarmonyPatch("ResolveGraphics")]
    [HarmonyPatch(new Type[] { typeof(Pawn) })]
    class Building_StatueOfColonist_ResolveGraphics_Patch {
        static void Prefix(Building_StatueOfColonist __instance, Pawn pawn) {
            AlienComp alienComp = pawn.GetComp<AlienComp>();
            if (alienComp != null) {
                __instance.GetComp<AlienComp>().crownType = alienComp.crownType;
                __instance.GetComp<AlienComp>().addonVariants = alienComp.addonVariants;
            }
        }
    }

    [HarmonyPatch(typeof(StatueOfColonistRenderer))]
    [HarmonyPatch("GetBodyMesh")]
    class StatueOfColonistRenderer_GetBodyMesh_Patch {
        static bool Prefix(StatueOfColonistRenderer __instance,ref Mesh __result, bool portrait, ThingDef raceDef, Rot4 bodyFacing) {
            if (raceDef == null || raceDef == ThingDefOf.Human) return true;
            AlienPartGenerator.AlienComp alienComp = __instance.parent.GetComp<AlienPartGenerator.AlienComp>();
            //Log.Message(raceDef.label + ":" + (alienComp != null));
            if (alienComp != null) {
                if (portrait) {
                    __result = alienComp.alienPortraitGraphics.bodySet.MeshAt(bodyFacing);
                } else {
                    __result = alienComp.alienGraphics.bodySet.MeshAt(bodyFacing);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(StatueOfColonistRenderer))]
    [HarmonyPatch("GetHeadMesh")]
    class StatueOfColonistRenderer_GetHeadMesh_Patch {
        static bool Prefix(StatueOfColonistRenderer __instance, ref Mesh __result, bool portrait, ThingDef raceDef, Rot4 headFacing) {
            if (raceDef == null || raceDef == ThingDefOf.Human) return true;
            AlienPartGenerator.AlienComp alienComp = __instance.parent.GetComp<AlienPartGenerator.AlienComp>();
            if (alienComp != null) {
                if (portrait) {
                    __result = alienComp.alienPortraitGraphics.headSet.MeshAt(headFacing);
                } else {
                    __result = alienComp.alienGraphics.headSet.MeshAt(headFacing);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(StatueOfColonistRenderer))]
    [HarmonyPatch("GetHairMesh")]
    class StatueOfColonistRenderer_GetHairMesh_Patch {
        static bool Prefix(StatueOfColonistRenderer __instance, ref Mesh __result, StatueOfColonistGraphicSet graphics, bool portrait, ThingDef raceDef, Rot4 headFacing) {
            if (raceDef == null || raceDef == ThingDefOf.Human) return true;
            AlienPartGenerator.AlienComp alienComp = __instance.parent.GetComp<AlienPartGenerator.AlienComp>();
            if (alienComp != null) {
                if (portrait) {
                    __result = alienComp.alienPortraitGraphics.hairSetAverage.MeshAt(headFacing);
                } else {
                    __result = alienComp.alienGraphics.hairSetAverage.MeshAt(headFacing);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(StatueOfColonistRenderer))]
    [HarmonyPatch("DrawAddons")]
    class StatueOfColonistRenderer_DrawAddons_Patch {
        static void Postfix(StatueOfColonistRenderer __instance, StatueOfColonistGraphicSet graphics, bool portrait, Vector3 vector, ThingDef raceDef, Quaternion quat, Rot4 rotation, BodyTypeDef bodyTypeDef, float scale) {
            ThingDef_AlienRace alienProps = raceDef as ThingDef_AlienRace;
            if (alienProps == null) return;

            List<AlienPartGenerator.BodyAddon> addons = alienProps.alienRace?.generalSettings?.alienPartGenerator?.bodyAddons;
            if (addons == null) return;
            AlienPartGenerator.AlienComp alienComp = __instance.parent.GetComp<AlienPartGenerator.AlienComp>();
            if (alienComp == null) return;

            //alienComp.crownType = __instance.parent.Data.crownType.ToString();
            for (int i = 0; i < addons.Count; i++) {
                AlienPartGenerator.BodyAddon ba = addons[index: i];

                //if (!CanDrawAddon(ba,__instance)) continue;

                AlienPartGenerator.RotationOffset offset = rotation == Rot4.South ?
                                                               ba.offsets.south :
                                                               rotation == Rot4.North ?
                                                                   ba.offsets.north :
                                                                   rotation == Rot4.East ?
                                                                    ba.offsets.east :
                                                                    ba.offsets.west;

                Vector2 bodyOffset = (portrait ? offset?.portraitBodyTypes ?? offset?.bodyTypes : offset?.bodyTypes)?.FirstOrDefault(predicate: to => to.bodyType == bodyTypeDef)
                                   ?.offset ?? Vector2.zero;
                Vector2 crownOffset = (portrait ? offset?.portraitCrownTypes ?? offset?.crownTypes : offset?.crownTypes)?.FirstOrDefault(predicate: to => to.crownType == alienComp.crownType)
                                    ?.offset ?? Vector2.zero;

                float moffsetX = 0.42f;
                float moffsetZ = -0.22f;
                float moffsetY = ba.inFrontOfBody ? 0.3f + ba.layerOffset : -0.3f - ba.layerOffset;
                float num = ba.angle;

                if (rotation == Rot4.North) {
                    moffsetX = 0f;
                    moffsetY = -moffsetY;
                    moffsetZ = -0.55f;
                    num = 0;
                }

                moffsetY -= 0.02f;

                moffsetX += bodyOffset.x + crownOffset.x;
                moffsetZ += bodyOffset.y + crownOffset.y;

                if (rotation == Rot4.East) {
                    moffsetX = -moffsetX;
                    num = -num; //Angle
                }

                Vector3 offsetVector = new Vector3(x: moffsetX * scale, y: moffsetY, z: moffsetZ * scale);


                if (i < graphics.bodyAddonGraphics.Count && i >= 0) {
                    GenDraw.DrawMeshNowOrLater(mesh: graphics.bodyAddonGraphics[index: i].MeshAt(rot: rotation), loc: vector + offsetVector.RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: quat)) * 2f * 57.29578f),
                        quat: Quaternion.AngleAxis(angle: num, axis: Vector3.up) * quat, mat: graphics.bodyAddonGraphics[index: i].MatAt(rot: rotation), drawNow: portrait);
                }
            }
        }

        public static bool CanDrawAddon(BodyAddon ba, StatueOfColonistRenderer renderer) {
            StatueOfColonistData data = renderer.parent.Data;
            return (data.wornApparelDefs.NullOrEmpty() || ((ba.hiddenUnderApparelTag.NullOrEmpty() && ba.hiddenUnderApparelFor.NullOrEmpty()) ||
                !data.wornApparelDefs.Any(predicate: def => def.apparel.bodyPartGroups.Any(bpgd => ba.hiddenUnderApparelFor.Contains(item: bpgd)) ||
                    def.apparel.tags.Any(s => ba.hiddenUnderApparelTag.Contains(s))))) && 
                (data.gender == Gender.Female ? ba.drawForFemale : ba.drawForMale);
        }

    }

    [HarmonyPatch(typeof(StatueOfColonistRenderer))]
    [HarmonyPatch("BaseHeadOffsetAt")]
    static class StatueOfColonistRenderer_BaseHeadOffsetAt_Patch {
        static void Postfix(Rot4 rotation, BodyTypeDef bodyType, ThingDef raceDef, LifeStageDef lifeStageDef, ref Vector3 __result) {
            Vector2 offset = (raceDef as ThingDef_AlienRace)?.alienRace.graphicPaths.GetCurrentGraphicPath(lifeStageDef).headOffsetDirectional?.GetOffset(rotation) ?? Vector2.zero;
            __result.x += offset.x;
            __result.z += offset.y;
        }

        public static GraphicPaths GetCurrentGraphicPath(this List<GraphicPaths> list, LifeStageDef lifeStageDef) =>
            list.FirstOrDefault(predicate: gp => gp.lifeStageDefs?.Contains(lifeStageDef) ?? false) ?? list.First();
    }

    [HarmonyPatch(typeof(StatueOfColonistRenderer))]
    [HarmonyPatch("GetHeadOffset")]
    static class StatueOfColonistRenderer_GetHeadOffset_Patch {
        static void Postfix(Rot4 rotation, BodyTypeDef bodyType, ThingDef raceDef, LifeStageDef lifeStageDef, ref Vector2 __result) {
            __result += (raceDef as ThingDef_AlienRace)?.alienRace.graphicPaths.GetCurrentGraphicPath(lifeStageDef).headOffset ?? Vector2.zero;
        }
    }

    [HarmonyPatch(typeof(Building_StatueOfColonist))]
    [HarmonyPatch("CopyStatueOfColonistFromPreset")]
    class Building_StatueOfColonist_CopyStatueOfColonistFromPreset_Patch {
        static void Postfix(Building_StatueOfColonist __instance, StatueOfColonistPreset preset) {
            if (preset.raceDefName != "Human") {
                __instance.GetComp<AlienComp>().crownType = preset.alienCrownType;
                __instance.GetComp<AlienComp>().addonVariants = preset.addonVariants;
            }
        }
    }

    [HarmonyPatch(typeof(StatueOfColonistPreset))]
    [HarmonyPatch("SetStatueOfColonist")]
    class StatueOfColonistPreset_SetStatueOfColonist_Patch {
        static void Postfix(StatueOfColonistPreset __instance, Building_StatueOfColonist statue) {
            AlienComp alienComp = statue.GetComp<AlienComp>();
            if (alienComp != null) {
                __instance.alienCrownType = alienComp.crownType;
                __instance.addonVariants = alienComp.addonVariants;
            }
        }
    }

    [HarmonyPatch(typeof(Window_EditStatue))]
    [HarmonyPatch("InitRaceDefs")]
    static class Window_EditStatue_InitRaceDefs_Patch {
        static void Postfix(Window_EditStatue __instance) {
            foreach(ThingDef_AlienRace thingDef_AlienRace in DefDatabase<ThingDef_AlienRace>.AllDefsListForReading) {
                __instance.raceDefs.Add(thingDef_AlienRace);
            }
        }
    }

    [HarmonyPatch(typeof(Window_EditStatue))]
    [HarmonyPatch("GetHeadTypes")]
    static class Window_EditStatue_GetHeadTypes_Patch {
        static void Postfix(Window_EditStatue __instance, ref List<object> __result, ThingDef raceDef) {
            ThingDef_AlienRace alienRaceDef = raceDef as ThingDef_AlienRace;
            if (alienRaceDef == null || raceDef == ThingDefOf.Human) return;
            __result = new List<object>();
            foreach (string crownType in alienRaceDef.alienRace.generalSettings.alienPartGenerator.aliencrowntypes) {
                __result.Add(crownType);
            }
            //Log.Message(__result.Join());
        }
    }

    [HarmonyPatch(typeof(Window_EditStatue))]
    [HarmonyPatch("GetHeadTypeItem")]
    static class Window_EditStatue_GetHeadTypeItem_Patch {
        static void Postfix(Window_EditStatue __instance, ref FloatMenuOption __result, object src, ThingDef raceDef) {
            if (__result != null) return;

            ThingDef_AlienRace alienRaceDef = raceDef as ThingDef_AlienRace;
            if (alienRaceDef == null) return;

            string crownType = src as string;
            if (crownType == null) return;
            Action action = delegate {
                //__instance.Statue.Data.headGraphicPath = headType.GetHeadGraphicPath();
                //__instance.Statue.Data.crownType = headType.GetCrownType();
                Traverse.Create(__instance).Field("headName").SetValue(crownType);
                __instance.Statue.GetComp<AlienComp>().crownType = crownType;
                __instance.Statue.ResolveGraphics();
            };
            FloatMenuOption item = new FloatMenuOption(crownType, action, MenuOptionPriority.Default, null, null, 0f, null, null);
            __result = item;
        }
    }

    [HarmonyPatch(typeof(Window_EditStatue))]
    [HarmonyPatch("GetBodyTypes")]
    static class Window_EditStatue_GetBodyTypes_Patch {
        static void Postfix(Window_EditStatue __instance, ref List<BodyTypeDef> __result, ThingDef raceDef) {
            ThingDef_AlienRace alienRaceDef = raceDef as ThingDef_AlienRace;
            if (alienRaceDef == null || alienRaceDef.alienRace.generalSettings.alienPartGenerator.alienbodytypes.NullOrEmpty()) return;
            __result = alienRaceDef.alienRace.generalSettings.alienPartGenerator.alienbodytypes;
        }
    }

    [HarmonyPatch(typeof(Window_EditStatue))]
    [HarmonyPatch("IsAvailableGender")]
    static class Window_EditStatue_IsAvailableGender_Patch {
        static void Postfix(ref bool __result, Gender gender, ThingDef raceDef) {
            ThingDef_AlienRace alienRaceDef = raceDef as ThingDef_AlienRace;
            if (alienRaceDef == null) return;
            if (alienRaceDef.alienRace.generalSettings.maleGenderProbability == 1f) {
                __result = (gender == Gender.Male);
            } else if (alienRaceDef.alienRace.generalSettings.maleGenderProbability == 0f) {
                __result = (gender == Gender.Female);
            }
        }
    }

    [HarmonyPatch(typeof(Window_EditStatue))]
    [HarmonyPatch("IsAvailableHair")]
    static class Window_EditStatue_IsAvailableHair_Patch {
        static void Postfix(ref bool __result,HairDef hairDef, ThingDef raceDef) {
            ThingDef_AlienRace alienRaceDef = raceDef as ThingDef_AlienRace;
            if (alienRaceDef == null || raceDef == ThingDefOf.Human) return;
            if (!alienRaceDef.alienRace.hairSettings.hasHair) {
                __result = false;
                return;
            }
            List<string> alienHairTags = alienRaceDef.alienRace.hairSettings.hairTags;
            if (alienHairTags.NullOrEmpty()) {
                alienHairTags = alienRaceDef.race.AnyPawnKind.defaultFactionType.hairTags;
            }
            __result = alienHairTags.SharesElementWith(hairDef.hairTags);
        }
    }

    [HarmonyPatch(typeof(Window_EditStatue))]
    [HarmonyPatch("UpdateRace")]
    static class Window_EditStatue_UpdateRace_Patch {
        static void Prefix(Window_EditStatue __instance) {
            ThingDef_AlienRace alienRaceDef = __instance.Statue.Data.raceDef as ThingDef_AlienRace;
            if (alienRaceDef != null) {
                if (alienRaceDef.alienRace.generalSettings.maleGenderProbability == 1f) {
                    __instance.Statue.Data.gender = Gender.Male;
                } else if (alienRaceDef.alienRace.generalSettings.maleGenderProbability == 0f) {
                    __instance.Statue.Data.gender = Gender.Female;
                }

                if (!__instance.GetBodyTypes(__instance.Statue.Data.raceDef).Contains(__instance.Statue.Data.bodyType)) {
                    __instance.Statue.Data.bodyType = __instance.GetBodyTypes(__instance.Statue.Data.raceDef)[0];
                }

                if (!alienRaceDef.race.lifeStageAges.ConvertAll(lsa => lsa.def).Contains(__instance.Statue.Data.lifeStageDef)) {
                    __instance.Statue.Data.lifeStageDef = alienRaceDef.race.lifeStageAges.Last().def;
                }
                __instance.GetHeadTypeItem(__instance.GetHeadTypes(alienRaceDef)[0], alienRaceDef).action.Invoke();


                if (alienRaceDef.alienRace.hairSettings.hasHair) {
                    List<string> alienHairTags = alienRaceDef.alienRace.hairSettings.hairTags;
                    HairDef hairDef = DefDatabase<HairDef>.AllDefsListForReading.Find(h => h.texPath == __instance.Statue.Data.hairGraphicPath);
                    if (alienHairTags.NullOrEmpty()) {
                        alienHairTags = alienRaceDef.race.AnyPawnKind.defaultFactionType.hairTags;
                    }
                    if (hairDef == null || !alienHairTags.SharesElementWith(hairDef.hairTags)) {
                        HairDef newHairDef = DefDatabase<HairDef>.AllDefsListForReading.Where(h => h.hairTags.SharesElementWith(alienHairTags)).RandomElement();
                        __instance.Statue.Data.hairGraphicPath = newHairDef.texPath;
                        Traverse.Create(__instance.Statue).Field("hairName").SetValue(newHairDef.LabelCap.ToString());
                        Traverse.Create(__instance).Field("hairName").SetValue(newHairDef.LabelCap.ToString());
                    }
                } else {
                    __instance.Statue.Data.hairGraphicPath = "";
                    Traverse.Create(__instance.Statue).Field("hairName").SetValue("");
                    Traverse.Create(__instance).Field("hairName").SetValue("");
                }
                Log.Message(alienRaceDef.defName + __instance.Statue.Data.hairGraphicPath);

                __instance.Statue.GetComp<AlienComp>().addonVariants = new List<int>();
                __instance.Statue.Data.wornApparelDefs.RemoveAll(ap => !Util.CanWear(ap, __instance.Statue.Data.bodyType));
            } else {
                if (__instance.Statue.Data.gender == Gender.None) {
                    __instance.Statue.Data.gender = Gender.Male;
                }

                if (!__instance.GetBodyTypes(__instance.Statue.Data.raceDef).Contains(__instance.Statue.Data.bodyType)) {
                    __instance.Statue.Data.bodyType = __instance.GetBodyTypes(__instance.Statue.Data.raceDef)[0];
                }

                if (!__instance.Statue.Data.raceDef.race.lifeStageAges.ConvertAll(lsa => lsa.def).Contains(__instance.Statue.Data.lifeStageDef)) {
                    __instance.Statue.Data.lifeStageDef = alienRaceDef.race.lifeStageAges.Last().def;
                }
                __instance.GetHeadTypeItem(__instance.GetHeadTypes(__instance.Statue.Data.raceDef)[0], __instance.Statue.Data.raceDef).action.Invoke();
            }
        }
    }

    [HarmonyPatch(typeof(Window_EditStatue))]
    [HarmonyPatch("EditAddons")]
    static class Window_EditStatue_EditAddons_Patch {
        static void Postfix(Window_EditStatue __instance, ref float y) {
            Rect rectLabelAddonsOfStatue = new Rect(0f, y + 2, 100f, 28f);
            Widgets.Label(rectLabelAddonsOfStatue, "StatueOfColonist.AddonOfStatue".Translate());
            Rect rectButtonAddonsOfStatue = new Rect(100f, y, 200f, 28f);
            bool active = !(__instance.Statue.Data.raceDef == ThingDefOf.Human || (__instance.Statue.Data.raceDef as ThingDef_AlienRace).alienRace.generalSettings.alienPartGenerator.bodyAddons.NullOrEmpty());
            if (Widgets.ButtonText(rectButtonAddonsOfStatue, "StatueOfColonist.EditAddonOfStatue".Translate(), true, false, active)) {
                Find.WindowStack.Add(new Dialog_EditBodyAddons(__instance.Statue));
            }
            y += 28f;
        }
    }

    [HarmonyPatch(typeof(Window_EditStatue))]
    [HarmonyPatch("CopyFromColonistInternal")]
    static class Window_EditStatue_CopyFromColonistInternal_Patch {
        static void Postfix(Window_EditStatue __instance, Pawn p) {
            AlienComp alienComp = p.GetComp<AlienComp>();
            if (alienComp == null) return;
            __instance.Statue.GetComp<AlienComp>().crownType = alienComp.crownType;
            __instance.Statue.GetComp<AlienComp>().addonVariants = alienComp.addonVariants;
        }
    }

    [StaticConstructorOnStartup]
    class DefGenerator_GenerateImpliedDefs_PreResolve_Patch {
        static DefGenerator_GenerateImpliedDefs_PreResolve_Patch() {
            LongEventHandler.ExecuteWhenFinished(delegate {
                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs) {
                    if (thingDef != null && (thingDef.thingClass == typeof(Building_StatueOfColonist) || thingDef.defName == "TMB_MinifiedStatueOfColonistK" || thingDef.defName.Contains("TMB_StatueOfColonistK"))) {
                        //Log.Message("Add AlienComp:" + thingDef.defName);
                        thingDef.comps.Add(item: new CompProperties(compClass: typeof(AlienPartGenerator.AlienComp)));
                    }
                }
            });
        }
    }

    [HarmonyPatch(typeof(StatueOfColonistGraphicSet))]
    [HarmonyPatch("ResolveAllGraphics")]
    static class StatueOfColonistGraphicSet_ResolveAllGraphics_Patch {
        static bool Prefix(StatueOfColonistGraphicSet __instance,float scale) {
            if (__instance.data.raceDef is ThingDef_AlienRace alienProps && __instance.data.raceDef != ThingDefOf.Human) {
                AlienComp alienComp = __instance.parent.GetComp<AlienComp>();
                /*
                if (alienComp.fixGenderPostSpawn) {
                    __instance.pawn.gender = Rand.Value >= alienProps.alienRace.generalSettings.maleGenderProbability ? Gender.Female : Gender.Male;
                    __instance.pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn: __instance.pawn);


                    Traverse.Create(root: __instance.pawn.story).Field(name: "headGraphicPath").SetValue(
                        value: alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(lifeStageDef: alien.ageTracker.CurLifeStage).head.NullOrEmpty() ?
                                   "" :
                                   alienProps.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(
                                       userpath: alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(lifeStageDef: alien.ageTracker.CurLifeStage).head, pawn: __instance.pawn));

                    alienComp.fixGenderPostSpawn = false;
                }
                */

                GraphicPaths graphicPaths = alienProps.alienRace.graphicPaths.GetCurrentGraphicPath(__instance.data.lifeStageDef);

                alienComp.customDrawSize = graphicPaths.customDrawSize;
                alienComp.customPortraitDrawSize = graphicPaths.customPortraitDrawSize;

                
                Traverse.Create(alienComp).Method("AssignProperMeshs").GetValue();

                __instance.data.headGraphicPath = AlienPartGenerator.GetAlienHead(graphicPaths.head, alienProps.alienRace.generalSettings.alienPartGenerator.useGenderedHeads ?
                                                                                                                      __instance.data.gender.ToString() :
                                                                                                                      "", crowntype: alienComp.crownType);
                Shader shaderCutout = ShaderDatabase.LoadShader(__instance.data.shaderCutoutPath);

                __instance.nakedGraphic = alienProps.alienRace.generalSettings.alienPartGenerator.GetNakedGraphic(__instance.data.bodyType, ShaderDatabase.CutoutSkin, __instance.data.color, __instance.data.color, graphicPaths.body, __instance.data.gender.ToString());
                __instance.rottingGraphic = alienProps.alienRace.generalSettings.alienPartGenerator.GetNakedGraphic(__instance.data.bodyType, shader: ShaderDatabase.CutoutSkin, PawnGraphicSet.RottingColor,PawnGraphicSet.RottingColor,graphicPaths.body, __instance.data.gender.ToString());
                __instance.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(path: (graphicPaths.skeleton == GraphicPaths.VANILLA_SKELETON_PATH ? __instance.data.bodyType.bodyDessicatedGraphicPath : graphicPaths.skeleton), shader: ShaderDatabase.Cutout);
                __instance.headGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.data.headGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, __instance.data.color);
                __instance.desiccatedHeadGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.data.headGraphicPath, ShaderDatabase.Cutout, Vector2.one, __instance.data.color);
                __instance.skullGraphic = GraphicDatabase.Get<Graphic_Multi>(path: graphicPaths.skull, shader: ShaderDatabase.Cutout, drawSize: Vector2.one, __instance.data.color);
                if (!__instance.data.hairGraphicPath.NullOrEmpty()) {
                    __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.data.hairGraphicPath, shaderCutout, Vector2.one, __instance.data.color);
                }
                __instance.headStumpGraphic = GraphicDatabase.Get<Graphic_Multi>(graphicPaths.stump, ShaderDatabase.Cutout, Vector2.one,__instance.data.color);
                __instance.desiccatedHeadStumpGraphic = GraphicDatabase.Get<Graphic_Multi>(graphicPaths.stump, ShaderDatabase.Cutout, Vector2.one, PawnGraphicSet.RottingColor);

                AlienPartGenerator apg = alienProps.alienRace.generalSettings.alienPartGenerator;
                __instance.bodyAddonGraphics = new List<Graphic>();
                if (alienComp.addonVariants == null)
                    alienComp.addonVariants = new List<int>();
                int sharedIndex = 0;
                for (int i = 0; i < apg.bodyAddons.Count; i++) {
                    Graphic g = GetPath(apg.bodyAddons[index: i], __instance.data, ref sharedIndex, alienComp.addonVariants.Count > i ? (int?)alienComp.addonVariants[index: i] : null);
                    __instance.bodyAddonGraphics.Add(item: g);
                    if (alienComp.addonVariants.Count <= i)
                        alienComp.addonVariants.Add(item: sharedIndex);
                }

                __instance.ResolveApparelGraphics();

                //Log.Message("[ResolveAllGraphics]" + __instance.parent.ThingID + "/" + __instance.parent.Position + "/" + alienComp.addonVariants.Join() + "/" + __instance.bodyAddonGraphics.Join(g => g.path));
                return false;
            }
            return true;
        }

        public static Graphic GetPath(BodyAddon bodyAddon, StatueOfColonistData data, ref int sharedIndex, int? savedIndex = new int?()) {
            string returnPath;
            int variantCounting;
            /*
            if (bodyAddon.backstoryGraphics?.FirstOrDefault(predicate: babgs => pawn.story.AllBackstories.Any(predicate: bs => bs.identifier == babgs.backstory)) is BodyAddonBackstoryGraphic babg) {
                returnPath = babg.path;
                variantCounting = babg.variantCount;
            } else if (bodyAddon.hediffGraphics?.FirstOrDefault(predicate: bahgs => pawn.health.hediffSet.hediffs.Any(predicate: h => h.def.defName == bahgs.hediff && (h.Part == null || bodyAddon.bodyPart.NullOrEmpty() || (h.Part.untranslatedCustomLabel == bodyAddon.bodyPart || h.Part.def.defName == bodyAddon.bodyPart)))) is BodyAddonHediffGraphic bahg) {
                returnPath = bahg.path;
                variantCounting = bahg.variantCount;
            }
            */
            returnPath = bodyAddon.path;
            variantCounting = bodyAddon.variantCount;
            int tv;
            return !returnPath.NullOrEmpty() ?
                        GraphicDatabase.Get<Graphic_Multi>(path: returnPath = (returnPath + ((tv = (savedIndex.HasValue ? (sharedIndex = savedIndex.Value) :
                                (bodyAddon.linkVariantIndexWithPrevious ?
                                    sharedIndex % variantCounting :
                                    (sharedIndex = Rand.Range(min: 0, max: variantCounting))))) == 0 ? "" : tv.ToString())),
                            ShaderDatabase.Transparent,
                                bodyAddon.drawSize * 1.5f,
                                    data.color) :
                        null;
        }
    }


    [HarmonyPatch(typeof(AlienComp))]
    [HarmonyPatch("PostSpawnSetup")]
    static class AlienComp_PostSpawnSetup_Patch {
        public static bool Prefix(AlienComp __instance,bool respawningAfterLoad) {
            if (__instance.parent.def is ThingDef_AlienRace) {
                return true;
            }
            return false;
        }
    }
}
