using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;
using System;
using System.Management.Instrumentation;
using UnityEngine;
using static UnityEngine.Random;
using System.Net;
using System.Reflection.Emit;
using System.Reflection;
using Verse.AI;
using System.Linq;
using RimWorld.Planet;
using System.Runtime.Remoting.Messaging;

/*
todo: modded xenotypes (not the usual customxenotype class but the xenotype class for people creating their own mods with this)

todo: after creating xenogerm with genders the xenotype creation screen gets a little iffy (displays the genders of the genes from the xenogerm)

 */

namespace GenderedGenes
{
    [StaticConstructorOnStartup]
    public class MyPatcher
    {
        static MyPatcher()
        {
            var harmony = new Harmony("GarryFlowers.GenderedGenes");

            harmony.PatchAll();

            harmony.Patch(CreateXenotypePatches.getDelegate1(), postfix: new HarmonyMethod(typeof(CreateXenotypePatches).GetMethod("loadXenotypePatch1")));
            harmony.Patch(CreateXenotypePatches.getDelegate2(), postfix: new HarmonyMethod(typeof(CreateXenotypePatches).GetMethod("loadXenotypePatch2")));
            harmony.Patch(CreateXenogermPatches.getDelegate(), postfix: new HarmonyMethod(typeof(CreateXenogermPatches).GetMethod("loadXenogermPatch")));
        }
    }

    [HarmonyPatch(typeof(Gene))]
    class OverridePatch
    {
        [HarmonyPatch("OverrideBy")]
        [HarmonyPrefix]
        public static bool overridePatch(Gene __instance, Gene overriddenBy)
        {

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();

            if (storage.trackerGeneGenders.TryGetValue(__instance, out Gender gender) && __instance.pawn.gender != gender && gender != Gender.None)
            {
                List<Gene> dependers = GeneTrackerPatches.overrideGene(__instance.pawn, __instance, __instance);

                /*while (dependers.Any())
                {
                    if (!dependers.Last().Overridden)
                    {
                        dependers.AddRange(GeneTrackerPatches.overrideGene(__instance.pawn, dependers.Last(), __instance));

                    }

                    dependers.RemoveLast();
                }*/
                return false;

            }
            else if (overriddenBy != null && storage.trackerGeneGenders.TryGetValue(overriddenBy, out gender) && __instance.pawn.gender != gender && gender != Gender.None)
            {
                return false;
            }
            else
            {
                GeneTrackerPatches.overrideGene(__instance.pawn, __instance, overriddenBy);
            }
            return false;
        }

        /*[HarmonyPatch("OverrideBy")]
        [HarmonyPrefix]
        public static bool overridePatch(Gene __instance, Gene overriddenBy)
        {

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();

            if (storage.trackerGeneGenders.TryGetValue(__instance, out Gender gender) && __instance.pawn.gender != gender && gender != Gender.None)
            {}
        }*/
    }

    [HarmonyPatch(typeof(Pawn_GeneTracker))]
    class GeneTrackerPatches
    {

        [HarmonyPrefix]
        [HarmonyPatch("AddGene", new Type[] { typeof(Gene), typeof(bool) })]
        public static bool AddGenePatch(Pawn_GeneTracker __instance, Gene gene, bool addAsXenogene, ref Gene __result, List<Gene> ___endogenes, List<Gene> ___xenogenes)
        {
            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            if(!storage.trackerTrackerGenders.TryGetValue(__instance, out Dictionary<GeneDef, Gender> genders))
            {
                return true;
            }
            if (genders.TryGetValue(gene.def, out Gender gender))
            {
                if (__instance.pawn.gender != gender && gender != Gender.None)
                {
                    if (addAsXenogene)
                    {
                        ___xenogenes.Add(gene);
                        gene.overriddenByGene = gene;
                        storage.trackerGeneGenders[gene] = gender;
                    }
                    else
                    {
                        ___endogenes.Add(gene);
                        gene.overriddenByGene = gene;
                        storage.trackerGeneGenders[gene] = gender;
                    }

                    __result = gene;
                    //notifyGenesChangedPatch(gene.def, __instance);
                    return false;
                }
            }

            return true;
        }

        public static List<Gene> overrideGene(Pawn pawn, Gene gene, Gene source)
        {
            gene.overriddenByGene=source;
            if (source != null)
            {
                if (pawn.abilities != null && !gene.def.abilities.NullOrEmpty())
                {
                    for (int i = 0; i < gene.def.abilities.Count; i++)
                    {
                        pawn.abilities.RemoveAbility(gene.def.abilities[i]);
                    }
                }
                /*if (!gene.def.forcedTraits.NullOrEmpty() || !gene.def.suppressedTraits.NullOrEmpty())
                {
                    if (pawn.story != null)
                    {
                        TraitSet traits = pawn.story.traits;
                        for (int num = traits.allTraits.Count - 1; num >= 0; num--)
                        {
                            if (traits.allTraits[num].sourceGene == gene)
                            {
                                traits.allTraits[num].sourceGene = null;
                                traits.RemoveTrait(traits.allTraits[num], true);
                            }
                        }
                        traits.RecalculateSuppression();
                    }
                }*/
                if (gene.def.passionMod != null)
                {
                    SkillRecord skill = pawn.skills.GetSkill(gene.def.passionMod.skill);
                    skill.passion = gene.NewPassionForOnRemoval(skill);
                }
                gene.PostRemove();
                pawn.skills?.DirtyAptitudes();
                pawn.Notify_DisabledWorkTypesChanged();
            }
            List<Gene> ret = new List<Gene>();
            foreach (Gene depender in pawn.genes.GenesListForReading)
            {
                if (depender.def.prerequisite == gene.def)
                {
                    ret.Add(depender);
                }
            }
            return ret;
        }

        
        [HarmonyPostfix]
        [HarmonyPatch("CheckForOverrides")]
        public static void overridePatch(ref Pawn_GeneTracker __instance)
        {
            /*GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            List<Gene> genesListForReading = __instance.GenesListForReading;

            foreach (Gene gene in genesListForReading)
            {
                if (gene.Overridden)
                {
                    continue;
                }
                if (storage.trackerGeneGenders.TryGetValue(gene, out Gender gender))
                {
                    if (__instance.pawn.gender != gender && gender!=Gender.None)
                    {

                        Gene source = gene;
                        List<Gene> dependents = overrideGene(__instance.pawn, gene, source);

                        while (dependents.Any())
                        {
                            if (!dependents.Last().Overridden)
                            {
                                dependents.AddRange(overrideGene(__instance.pawn, dependents.Last(), source));
                            }
                            dependents.RemoveLast();
                        }
                    }
                }
            }*/
            __instance.pawn.Drawer.renderer.SetAllGraphicsDirty();//.graphics.SetGeneGraphicsDirty();
        }

        [HarmonyReversePatch]
        [HarmonyPatch("CheckForOverrides")]
        public static void CheckForOverrides(object instance)
        {
            throw new NotImplementedException("It's a stub");
        }

        /*
        [HarmonyReversePatch]
        [HarmonyPatch("Notify_GenesChanged")]
        public static void NotifyGenesChanged(object instance, GeneDef addedOrRemovedGene)
        {
            throw new NotImplementedException("It's a stub");
        }*/

        /*
        [HarmonyPatch("SelectGene")]
        [HarmonyPrefix]
        public static bool selectGenePatch(Gene __instance, Predicate<Gene> validator, bool __result)
        {
            validator = (Gene g) => (validator(g) && false);
            return true;
        }*/

        
        [HarmonyPatch("Notify_GenesChanged")]
        [HarmonyPostfix]
        public static void notifyGenesChangedPatch(GeneDef addedOrRemovedGene, Pawn_GeneTracker __instance)
        {
            if (!addedOrRemovedGene.RandomChosen)
            {
                return;
            }

            List<Gene> genes = __instance.GenesListForReading;

            //Predicate<Gene> validator = ((Gene g) => g.Active && g.def.ConflictsWith(addedOrRemovedGene));

            List<Gene> tmpGenes = new List<Gene>();

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            
            for (int i = 0; i < genes.Count; i++)
            {
                //if ((genes[i].Active || genes[i].Overridden) && validator(genes[i]))
                
                if(genes[i].def.ConflictsWith(addedOrRemovedGene)){

                    if (storage.trackerGeneGenders.TryGetValue(genes[i], out Gender gender) && gender!=Gender.None && gender != __instance.pawn.gender)
                    {
                        genes[i].OverrideBy(genes[i]);
                    }
                    else
                    {
                        genes[i].OverrideBy(null);
                        tmpGenes.Add(genes[i]);
                    }
                }
            }

            if(!tmpGenes.Where((Gene g) => __instance.Xenogenes.Contains(g)).TryRandomElement(out Gene chosen))
            {
                tmpGenes.TryRandomElement(out chosen);
            }
            if (chosen != null)
            {
                //chosen.OverrideBy(chosen);
                foreach (Gene item in genes)
                {
                    if (item != chosen && item.def.ConflictsWith(chosen.def))
                    {
                        item.OverrideBy(chosen);
                    }
                }
            }
        }

    }

        /*[HarmonyPatch(MethodBase.GetMethodFromHandle())]
        [HarmonyPrefix]
        public static bool selectGenePatch(Pawn_GeneTracker __instance, Predicate<Gene> validator, bool __result)
        {
            /*GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            if (storage.trackerGeneGenders.TryGetValue(__instance, out Dictionary<GeneDef, Gender> genders))
            {
                if (genders.TryGetValue(addedOrRemovedGene, out Gender gender) && gender != Gender.None && gender != __instance.pawn.gender)
                {

                }
            }
            validator = (Gene g) => (validator(g) && false);
            return true;
        }*/
    
    /*
    [HarmonyPatch]
    class MyPatch
    {
        public static MethodBase TargetMethod()
        {
            
            return typeof(Pawn_GeneTracker).GetNestedTypes(AccessTools.all).SelectMany(AccessTools.GetDeclaredMethods).First(mi => mi.Name.Contains("SelectGene"));

        }

        class PredWrapper<T>
        {
            public Predicate<T> pred;
            public PredWrapper(Predicate<T> pred){
                this.pred=pred;
            }
        }

        
        public static void Prefix(Pawn_GeneTracker __instance, ref Predicate<Gene> validator)
        {
            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();



            Predicate<Gene> p = (Gene g) =>
            {


                if (storage.trackerGeneGenders.TryGetValue(g, out Gender gender))
                {
                    return gender == Gender.None || __instance.pawn.gender == gender;
                }

                return true;

            };

            PredWrapper<Gene> wrap = new PredWrapper<Gene>(validator);

            //validator.Method
            validator = (Gene g) => (wrap.pred(g) && p(g));
        }


    }*/


    [HarmonyPatch(typeof(ITab_GenesPregnancy))]
    class ViewPregnantGeneDefsPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("UnbornBabyHediffGeneset")]
        public static void ThingWithGenesPostfix(GeneSet __result)
        {
            if (__result != null)
            {
                GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
                storage.trackerGeneSetGenders.TryGetValue(__result.GenesListForReading, out Dictionary<GeneDef, Gender> genes);
                GenderGeneStorage.trackerInspectedGeneSet = genes;
            }
        }
    }

    [HarmonyPatch(typeof(ITab_Genes))]
    class ViewGeneDefsPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch("ThingWithGenes")]
        public static void ThingWithGenesPostfix(Thing __result)
        {
            
            if (__result == null) return;
            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();

            if(__result is Pawn pawn)
            {
                return;
            } else if (__result is Xenogerm xenogerm)
            {
                if(storage.trackerXenogermGenders.TryGetValue(xenogerm, out Dictionary<GeneDef, Gender> genders))
                {
                    GenderGeneStorage.trackerInspectedGeneSet = genders;
                } else
                {
                    GenderGeneStorage.trackerInspectedGeneSet = new Dictionary<GeneDef, Gender>();
                }

            } else if(__result is GeneSetHolderBase set)
            {
                if (storage.trackerGeneSetGenders.TryGetValue(set.GeneSet.GenesListForReading, out Dictionary<GeneDef, Gender> genders))
                {
                    GenderGeneStorage.trackerInspectedGeneSet = genders;
                }
                else
                {
                    GenderGeneStorage.trackerInspectedGeneSet = new Dictionary<GeneDef, Gender>();
                }
            } else if(__result is Building_GrowthVat vat)
            {
                if(storage.trackerGeneSetGenders.TryGetValue(vat.selectedEmbryo.GeneSet.GenesListForReading, out Dictionary<GeneDef, Gender> genders))
                {
                    GenderGeneStorage.trackerInspectedGeneSet = genders;
                }
                else
                {
                    GenderGeneStorage.trackerInspectedGeneSet = new Dictionary<GeneDef, Gender>();
                }
            }
        }
    }


    [HarmonyPatch(typeof(GeneUIUtility))]
    class GeneUIPatches
    {

        [HarmonyPostfix]
        [HarmonyPatch("DrawGene")]
        public static void drawGenePatch(Gene gene, Rect geneRect)
        {
            Gender gender;
            if (!Current.Game.GetComponent<GenderGeneStorage>().trackerGeneGenders.TryGetValue(gene, out gender))
            {
                gender = Gender.None;
            }

            if (Gender.None == gender)
            {
                return;
            }

            GUI.BeginGroup(geneRect);

            /*
            float step = GeneCreationDialogBase.GeneSize.x / 4f;
            drawSymbol(new Rect(step - 6, 5, 16, 16), "/", Gender.None == gender);
            drawSymbol(new Rect(step * 2 - 6, 5, 16, 16), "m", Gender.Male == gender);
            drawSymbol(new Rect(step * 3 - 6, 5, 16, 16), "f", Gender.Female == gender);
            */
            drawSymbol(new Rect(6, 6, 16, 16), gender);
            GUI.EndGroup();
        }

        public static void drawSymbol(Rect rect, Gender gender)
        {
            
            Widgets.DrawBoxSolid(rect, new Color(0.212f, 0.212f, 0.212f));
            Widgets.DrawHighlight(rect);
            GUI.color = new Color(0.376f, 0.376f, 0.376f);
            Widgets.DrawBox(rect);


            GUI.color = Color.white;

            Text.Anchor = TextAnchor.MiddleCenter;

            Widgets.Label(rect, Gender.None == gender?"/":Gender.Male==gender?"M":"F");
            Text.Anchor = TextAnchor.UpperLeft;
        }

        [HarmonyPostfix]
        [HarmonyPatch("DrawGeneDef")]
        public static void drawGeneDefPatch(GeneDef gene, Rect geneRect)
        {
            Gender gender;
            if (GenderGeneStorage.trackerInspectedGeneSet==null || !GenderGeneStorage.trackerInspectedGeneSet.TryGetValue(gene, out gender) || Gender.None==gender)
            {
                return;
            }

            GUI.BeginGroup(geneRect);

            float step = GeneCreationDialogBase.GeneSize.x / 4f;
            /*
            drawSymbol(new Rect(step - 6, 5, 16, 16), "/", Gender.None == gender);
            drawSymbol(new Rect(step * 2 - 6, 5, 16, 16), "m", Gender.Male == gender);
            drawSymbol(new Rect(step * 3 - 6, 5, 16, 16), "f", Gender.Female == gender);
            */
            drawSymbol(new Rect(6, 6, 16, 16), gender);

            GUI.EndGroup();
        }
    }

    [HarmonyPatch(typeof(Dialog_CreateXenogerm))]
    class CreateXenogermPatches
    {


        public static MethodInfo getDelegate()
        {

            return typeof(Dialog_CreateXenogerm)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(method => method.Name.Contains("<DrawSearchRect>b__22_0"));
        }

        public static void loadXenogermPatch(CustomXenogerm xenogerm)
        {

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            Dictionary<Genepack, Gender> genders;
            GenderGeneStorage.trackerCurrentXenogermGenders = new Dictionary<Genepack, Gender>();
            if (storage.trackerCustomXenogermGenders.TryGetValue(xenogerm, out genders))
            {
                foreach (Genepack pack in genders.Keys)
                {
                    GenderGeneStorage.trackerCurrentXenogermGenders[pack] = genders[pack];
                }
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch("PostOpen")]
        public static void postOpenPatch()
        {
            GenderGeneStorage.trackerCurrentXenogermGenders = new Dictionary<Genepack, Gender>();
        }

        [HarmonyPrefix]
        [HarmonyPatch("DrawGenepack")]
        public static void changeXPatch(ref float curX)
        {
             curX += 24f;
        }

        [HarmonyPostfix]
        [HarmonyPatch("DrawGenepack")]
        public static void drawGenePackPatch(Genepack genepack, float curX, float curY, float packWidth)
        {
            /*
            bool randomChosen = true;
            foreach(GeneDef def in genepack.GeneSet.GenesListForReading)
            {
                if (!def.randomChosen)
                {
                    randomChosen = false;
                }
            }
            if (randomChosen)
            {
                return;
            }*/

            float x = curX - (packWidth + 3f);
            GUI.BeginGroup(new Rect(x - 26, curY + 8, 24, 64));

            Gender gender;
            if (!GenderGeneStorage.trackerCurrentXenogermGenders.TryGetValue(genepack, out gender))
            {
                gender = Gender.None;
            }

            drawSymbol(new Rect(0, 0, 16, 16), "/", Gender.None == gender, genepack, Gender.None);

            drawSymbol(new Rect(0, 24, 16, 16), "M", Gender.Male == gender, genepack, Gender.Male);

            drawSymbol(new Rect(0, 48, 16, 16), "F", Gender.Female == gender, genepack, Gender.Female);


            GUI.EndGroup();

        }

        public static void drawSymbol(Rect rect, string symbol, bool active, Genepack genepack, Gender gender)
        {

            Widgets.DrawBoxSolid(rect, new Color(0.212f, 0.212f, 0.212f));
            //Widgets.DrawHighlight(rect);
            GUI.color = active ? new Color(0.2f, 0.5f, 0.2f) : new Color(0.376f, 0.376f, 0.376f);
            Widgets.DrawBox(rect);
            GUI.color = Color.white;
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }

            GUI.color = Color.white;

            Text.Anchor = TextAnchor.MiddleCenter;

            Widgets.Label(rect, symbol);

            Text.Anchor = TextAnchor.UpperLeft;

            if (GUI.Button(rect, "", GUIStyle.none))
            {
                GenderGeneStorage.trackerCurrentXenogermGenders[genepack] = gender;
            }
        }

    }

    [HarmonyPatch(typeof(Dialog_CreateXenotype))]
    class CreateXenotypePatches
    {


        public static void loadXenotypePatch1(CustomXenotype xenotype)
        {

            GenderGeneStorage.trackerCurrentXenotypeGenders = GenderGeneStorage.trackerXenotypeGenders[xenotype];

        }

        public static void loadXenotypePatch2()
        {

            GenderGeneStorage.trackerCurrentXenotypeGenders = new Dictionary<GeneDef, Gender>();

        }

        public static MethodInfo getDelegate1()
        {

            return typeof(Dialog_CreateXenotype)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(method => method.Name.Contains("<DrawSearchRect>b__28_0"));
        }

        public static MethodInfo getDelegate2()
        {
            return typeof(Dialog_CreateXenotype)
                .GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance)
                .SelectMany(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                .Single(method => method.Name.Contains("<DrawSearchRect>b__3"));
        }

        [HarmonyPatch("AcceptInner")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {

            foreach (CodeInstruction instruction in instructions)
            {

                yield return instruction;

                if (instruction.opcode==OpCodes.Stfld && instruction.operand!=null && instruction.operand.Equals(AccessTools.Field(typeof(CustomXenotype), "iconDef")))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);

                    yield return new CodeInstruction(OpCodes.Call, typeof(CreateXenotypePatches).GetMethod("loadSavedXenotypeDataPatch"));
                }

            }
        }

        public static void loadSavedXenotypeDataPatch(object wrapper)
        {

            FieldInfo f = AccessTools.DeclaredField(wrapper.GetType(), "customXenotype");
            CustomXenotype xenotype = f.GetValue(wrapper) as CustomXenotype;

            GenderGeneStorage.trackerXenotypeGenders[xenotype] = new Dictionary<GeneDef, Gender>();
            foreach (GeneDef gene in GenderGeneStorage.trackerCurrentXenotypeGenders.Keys)
            {
                GenderGeneStorage.trackerXenotypeGenders[xenotype][gene] = GenderGeneStorage.trackerCurrentXenotypeGenders[gene];
            }
        }


            [HarmonyPrefix]
        [HarmonyPatch("PostOpen")]
        public static void postOpenPatch()
        {
            GenderGeneStorage.trackerCurrentXenotypeGenders = new Dictionary<GeneDef, Gender>();
        }

        [HarmonyPrefix]
        [HarmonyPatch("DrawGene")]
        public static void changeXPatch(ref float curX, bool selectedSection)
        {
            if (selectedSection)
            {
                curX += 24f;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("DrawGene")]
        public static void drawGenePatch(GeneDef geneDef, bool selectedSection, float curX, float curY, float packWidth)
        {
            if (!selectedSection/* || geneDef.randomChosen*/)
            {
                return;
            }
            float x = curX - (packWidth + 3f);
            GUI.BeginGroup(new Rect(x - 16, curY + 8, 24, 64));

            Gender gender;
            if (!GenderGeneStorage.trackerCurrentXenotypeGenders.TryGetValue(geneDef, out gender))
            {
                gender = Gender.None;
            }

            drawSymbol(new Rect(0, 0, 16, 16), "/", Gender.None == gender, geneDef, Gender.None);

            drawSymbol(new Rect(0, 24, 16, 16), "M", Gender.Male == gender, geneDef, Gender.Male);

            drawSymbol(new Rect(0, 48, 16, 16), "F", Gender.Female == gender, geneDef, Gender.Female);


            GUI.EndGroup();
        }

        public static void drawSymbol(Rect rect, string symbol, bool active, GeneDef geneDef, Gender gender)
        {

            Widgets.DrawBoxSolid(rect, new Color(0.212f, 0.212f, 0.212f));
            //Widgets.DrawHighlight(rect);
            GUI.color = active ? new Color(0.2f, 0.5f, 0.2f) : new Color(0.376f, 0.376f, 0.376f);
            Widgets.DrawBox(rect);
            GUI.color = Color.white;
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }

            GUI.color = Color.white;

            Text.Anchor = TextAnchor.MiddleCenter;

            Widgets.Label(rect, symbol);

            Text.Anchor = TextAnchor.UpperLeft;

            if (GUI.Button(rect, "", GUIStyle.none))
            {
                GenderGeneStorage.trackerCurrentXenotypeGenders[geneDef] = gender;
            }
        }
    }


    [HarmonyPatch(typeof(PawnGenerator))]
    class PawnGeneratorGenderPatch
    {

        [HarmonyPrefix]
        [HarmonyPatch("GenerateGenes")]
        public static void acquireGeneratorXenotypeGenderInfo(Pawn pawn, PawnGenerationRequest request)
        {
            if (pawn.genes == null)
            {
                return;
            }

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            Dictionary<GeneDef, Gender> genders = new Dictionary<GeneDef, Gender>();

            if (request.ForcedCustomXenotype != null)
            {
                if (GenderGeneStorage.trackerXenotypeGenders.TryGetValue(request.ForcedCustomXenotype, out Dictionary<GeneDef, Gender> forcedCustomXenotypeGenders))
                {
                    foreach (KeyValuePair<GeneDef, Gender> kvp in forcedCustomXenotypeGenders)
                    {
                        genders[kvp.Key] = kvp.Value;
                    }
                }
            }
            /*if (request.ForcedXenotype != null)
            {
                if (GenderGeneStorage.trackerXenotypeGenders.TryGetValue(request.ForcedXenotype, out Dictionary<GeneDef, Gender> current))
                {
                    foreach (KeyValuePair<GeneDef, Gender> kvp in current)
                    {
                        genders[kvp.Key] = kvp.Value;
                    }
                }
            }*/
            if (request.ForcedEndogenes!=null && storage.trackerGeneSetGenders.TryGetValue(request.ForcedEndogenes, out Dictionary<GeneDef, Gender> inheritedGeneGenders))
            {
                foreach(KeyValuePair<GeneDef, Gender> kvp in inheritedGeneGenders)
                {
                    genders[kvp.Key] = kvp.Value;
                }
            }

            if(request.ForcedXenogenes != null && storage.trackerGeneSetGenders.TryGetValue(request.ForcedXenogenes, out Dictionary<GeneDef, Gender> inheritedXenoGeneGenders))
            {
                foreach (KeyValuePair<GeneDef, Gender> kvp in inheritedXenoGeneGenders)
                {
                    genders[kvp.Key] = kvp.Value;
                }
            }

            storage.trackerTrackerGenders[pawn.genes] = genders;
        }

        [HarmonyPostfix]
        [HarmonyPatch("GenerateGenes")]
        public static void genderGenesPatch(Pawn pawn, PawnGenerationRequest request)
        {

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            if (request.ForcedCustomXenotype != null)
            {

                if (GenderGeneStorage.trackerXenotypeGenders.TryGetValue(request.ForcedCustomXenotype, out Dictionary<GeneDef, Gender> genders))
                {

                    foreach (Gene gene in pawn.genes.GenesListForReading)
                    {
                        if (genders.TryGetValue(gene.def, out Gender gender))
                        {

                            storage.trackerGeneGenders[gene] = gender;
                        }
                    }
                    GeneTrackerPatches.CheckForOverrides(pawn.genes);
                    //GeneTrackerPatches.overridePatch(ref pawn.genes);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PregnancyUtility))]
    class InheritGenderingPatch
    {
        /*
        [HarmonyPrefix]
        [HarmonyPatch("ApplyBirthOutcome")]
        public static void acquireBirthGeneGenderInfo(List<GeneDef> genes, Thing __result)
        {

            Pawn pawn = null;
            Corpse corpse = __result as Corpse;
            if (corpse != null)
            {
                pawn = corpse.InnerPawn;
            }
            else
            {
                pawn = __result as Pawn;
            }
            if (pawn != null)
            {
                GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();


                if (genes != null && storage.trackerGeneSetGenders.TryGetValue(genes, out Dictionary<GeneDef, Gender> genders))
                {
                    foreach (Gene gene in pawn.genes.GenesListForReading)
                    {
                        if (genders.TryGetValue(gene.def, out Gender gender))
                        {

                            storage.trackerGeneGenders[gene] = gender;
                        }
                    }

                    GeneTrackerPatches.CheckForOverrides(pawn.genes);
                }
            }
        }*/


        [HarmonyPostfix]
        [HarmonyPatch("ApplyBirthOutcome_NewTemp")]
        public static void addGenderingToBabbyPatch(List<GeneDef> genes, Thing __result)
        {
            Pawn pawn = null;
            Corpse corpse = __result as Corpse;
            if (corpse!=null)
            {
                pawn = corpse.InnerPawn;
            } else
            {
                pawn = __result as Pawn;
            }


            if (pawn != null)
            {
                GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();

                if (genes != null && storage.trackerGeneSetGenders.TryGetValue(genes, out Dictionary<GeneDef, Gender> genders))
                {
                    foreach (Gene gene in pawn.genes.GenesListForReading)
                    {
                        if (genders.TryGetValue(gene.def, out Gender gender))
                        {

                            storage.trackerGeneGenders[gene] = gender;
                        }
                    }

                    foreach(Gene gene in pawn.genes.GenesListForReading)
                    {
                        if (gene.def.RandomChosen)
                        {
                            GeneTrackerPatches.notifyGenesChangedPatch(gene.def, pawn.genes);
                            //GeneTrackerPatches.NotifyGenesChanged(pawn.genes, gene.def);
                        }
                    }
                    GeneTrackerPatches.CheckForOverrides(pawn.genes);
                    //GeneTrackerPatches.overridePatch(ref pawn.genes);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("GetInheritedGeneSet", new Type[]{typeof(Pawn), typeof(Pawn), typeof(bool)}, new ArgumentType[] {ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out})]
        public static void addGenderingsToGeneSetPatch(Pawn father, Pawn mother, bool success, GeneSet __result){

            if(father==null || mother == null)
            {
                return;
            }

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();

            Dictionary<GeneDef, Gender> genders = new Dictionary<GeneDef, Gender>();
            foreach(GeneDef gene in __result.GenesListForReading)
            {

                bool male=false;
                bool female = false;

                foreach (Gene endogene in father.genes.Endogenes)
                {
                    if (gene != endogene.def)
                    {
                        continue;
                    }
                    if (storage.trackerGeneGenders.TryGetValue(endogene, out Gender gender))
                    {
                        if (Gender.Female == gender)
                        {
                            female = true;
                        } else if(Gender.Male == gender) {
                            male = true;
                        }
                    }
                }

                foreach (Gene endogene in mother.genes.Endogenes)
                {
                    if (gene != endogene.def)
                    {
                        continue;
                    }
                    if (storage.trackerGeneGenders.TryGetValue(endogene, out Gender gender))
                    {
                        if (Gender.Female == gender)
                        {
                            female = true;
                        }
                        else if (Gender.Male == gender)
                        {
                            male = true;
                        }
                    }
                }

                if(male)
                {
                    if (female)
                    {
                        genders[gene]=Rand.Bool?Gender.Male:Gender.Female;
                    } else
                    {
                        genders[gene] = Gender.Male;
                    }
                } else if (female)
                {
                    genders[gene] = Gender.Female;
                }

            }
            if(genders.Keys.Count > 0)
            {
                storage.trackerGeneSetGenders[__result.GenesListForReading] = genders;
            }
        }
    }

    [HarmonyPatch(typeof(Building_GeneAssembler))]
    class StoreWIPXenogermPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void storeWIPXenogermGenders(Building_GeneAssembler __instance)
        {
            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            storage.trackerAssemblingGenders[__instance] = new Dictionary<Genepack, Gender>();
            foreach (Genepack genepack in GenderGeneStorage.trackerCurrentXenogermGenders.Keys)
            {
                storage.trackerAssemblingGenders[__instance][genepack] = GenderGeneStorage.trackerCurrentXenogermGenders[genepack];

            }

        }

        public static Building_GeneAssembler tmpAssembler;
        [HarmonyPrefix]
        [HarmonyPatch("Finish")]
        public static void tmpStoreAssembler(Building_GeneAssembler __instance)
        {
            tmpAssembler = __instance;
        }
    }

    [HarmonyPatch(typeof(CustomXenogermDatabase))]
    class SaveCustomXenogermPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Add")]
        public static void saveXenogermPatch(CustomXenogerm customXenogerm)
        {
            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();

            storage.trackerCustomXenogermGenders[customXenogerm] = new Dictionary<Genepack, Gender>();
            
            foreach(Genepack pack in GenderGeneStorage.trackerCurrentXenogermGenders.Keys)
            {
                storage.trackerCustomXenogermGenders[customXenogerm][pack] = GenderGeneStorage.trackerCurrentXenogermGenders[pack];
            }

        }
    }

    [HarmonyPatch(typeof(Xenogerm))]
    class XenogermItemPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("Initialize")]
        public static void addXenogermGendersPatch(Xenogerm __instance, List<Genepack> genepacks)
        {

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            Dictionary<GeneDef, Gender> gendersToStore = new Dictionary<GeneDef, Gender>();

            if (StoreWIPXenogermPatch.tmpAssembler == null)
            {
                return;
            }

            if(storage.trackerAssemblingGenders.TryGetValue(StoreWIPXenogermPatch.tmpAssembler, out Dictionary<Genepack, Gender> genders))
            {

                foreach(GeneDef def in __instance.GeneSet.GenesListForReading)
                {
                    bool male = false;
                    bool female = false;

                    foreach(Genepack pack in genepacks)
                    {
                        if (pack.GeneSet.GenesListForReading.Contains(def) && genders.TryGetValue(pack, out Gender gender))
                        {
                            if (Gender.Male == gender)
                            {
                                male = true;
                            } else if(Gender.Female == gender)
                            {
                                female = true;
                            }
                        }

                    }

                    if (male)
                    {
                        if (female)
                        {
                            gendersToStore[def] = Rand.Bool?Gender.Male:Gender.Female;
                        } else
                        {
                            gendersToStore[def] = Gender.Male;
                        }
                    } else if (female)
                    {
                        gendersToStore[def] = Gender.Female;
                    }

                }

                if(gendersToStore.Count > 0)
                {
                    storage.trackerXenogermGenders[__instance] = gendersToStore;
                }
            }
            StoreWIPXenogermPatch.tmpAssembler = null;

        }
    }

    [HarmonyPatch(typeof(GeneUtility))]
    class ImplantXenogermPatches
    {

        [HarmonyPrefix]
        [HarmonyPatch("ImplantXenogermItem")]
        public static void acquireXenogermItemGenderInfo(Pawn pawn, Xenogerm xenogerm)
        {

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            Dictionary<GeneDef, Gender> genders = new Dictionary<GeneDef, Gender>();

            if(storage.trackerXenogermGenders.TryGetValue(xenogerm, out Dictionary<GeneDef, Gender> current))
            {
                foreach(KeyValuePair<GeneDef, Gender> kvp in current)
                {
                    genders[kvp.Key]= kvp.Value;
                }
            }

            storage.trackerTrackerGenders[pawn.genes] = genders;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ImplantXenogermItem")]
        public static void applyXenogermGenders(Pawn pawn, Xenogerm xenogerm)
        {
            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            if(pawn.genes!=null && storage.trackerXenogermGenders.TryGetValue(xenogerm, out Dictionary<GeneDef, Gender> genders))
            {
                foreach(Gene gene in pawn.genes.Xenogenes)
                {
                    if(genders.TryGetValue(gene.def, out Gender gender))
                    {
                        storage.trackerGeneGenders[gene] = gender;
                    }
                }

                GeneTrackerPatches.CheckForOverrides(pawn.genes);
                //GeneTrackerPatches.overridePatch(ref pawn.genes);
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch("ReimplantXenogerm")]
        public static void copyGendersOnReimplantPrefix(Pawn caster, Pawn recipient, ref Dictionary<GeneDef, Gender> __state)
        {

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            Dictionary<GeneDef, Gender> genders = new Dictionary<GeneDef, Gender>();
            __state = new Dictionary<GeneDef, Gender>();

            foreach(Gene gene in caster.genes.Xenogenes)
            {

                if(storage.trackerGeneGenders.TryGetValue(gene, out Gender gender))
                {

                    __state[gene.def] = gender;
                    genders[gene.def] = gender;

                }

            }

            storage.trackerTrackerGenders[recipient.genes] = genders;
        }

        /*[HarmonyPrefix]
        [HarmonyPatch("ReimplantXenogerm")]
        public static void acquireXenogermGenderInfo(Pawn caster, Pawn recipient)
        {

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            Dictionary<Pawn_GeneTracker, Dictionary<GeneDef, Gender>> genders = new Dictionary<Pawn_GeneTracker, Dictionary<GeneDef, Gender>>();

            foreach (Gene gene in caster.genes.Xenogenes)
            {

                if (storage.trackerGeneGenders.TryGetValue(gene, out Gender gender))
                {

                    genders[recipient.genes][gene.def]= gender;

                }

            }
            storage.trackerTrackerGenders = genders;
        }*/

        [HarmonyPostfix]
        [HarmonyPatch("ReimplantXenogerm")]
        public static void copyGendersOnReimplantPostfix(Pawn caster, Pawn recipient, ref Dictionary<GeneDef, Gender> __state)
        {

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();

            foreach (KeyValuePair<GeneDef, Gender> genders in __state)
            {

                Gene gene = recipient.genes.Xenogenes.Find(recipientGene => recipientGene.def == genders.Key);
                storage.trackerGeneGenders[gene] = genders.Value;

            }
            GeneTrackerPatches.CheckForOverrides(recipient.genes);
            //GeneTrackerPatches.overridePatch(ref recipient.genes);

        }
    }
}
