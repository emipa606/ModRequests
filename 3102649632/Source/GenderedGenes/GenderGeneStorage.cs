

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

namespace GenderedGenes
{
    public class GenderGeneStorage : GameComponent
    {

        public Dictionary<Gene, Gender> trackerGeneGenders = new Dictionary<Gene, Gender>();

        public Dictionary<CustomXenogerm, Dictionary<Genepack, Gender>> trackerCustomXenogermGenders = new Dictionary<CustomXenogerm, Dictionary<Genepack, Gender>>();

        public Dictionary<List<GeneDef>, Dictionary<GeneDef, Gender>> trackerGeneSetGenders = new Dictionary<List<GeneDef>, Dictionary<GeneDef, Gender>>();

        public Dictionary<Building_GeneAssembler, Dictionary<Genepack, Gender>> trackerAssemblingGenders = new Dictionary<Building_GeneAssembler, Dictionary<Genepack, Gender>>();

        public Dictionary<Xenogerm, Dictionary<GeneDef, Gender>> trackerXenogermGenders = new Dictionary<Xenogerm, Dictionary<GeneDef, Gender>>();

        public Dictionary<Pawn_GeneTracker, Dictionary<GeneDef, Gender>> trackerTrackerGenders = new Dictionary<Pawn_GeneTracker, Dictionary<GeneDef, Gender>>();

        public static Dictionary<CustomXenotype, Dictionary<GeneDef, Gender>> trackerXenotypeGenders = new Dictionary<CustomXenotype, Dictionary<GeneDef, Gender>>();
        public static Dictionary<GeneDef, Gender> trackerCurrentXenotypeGenders = new Dictionary<GeneDef, Gender>();
        public static Dictionary<Genepack, Gender> trackerCurrentXenogermGenders = new Dictionary<Genepack, Gender>();
        public static Dictionary<GeneDef, Gender> trackerInspectedGeneSet = new Dictionary<GeneDef, Gender>();
        
        public GenderGeneStorage(Game game) { }

        public override void GameComponentUpdate()
        {
        }

        public override void GameComponentTick()
        {
        }

        public override void GameComponentOnGUI()
        {
        }

        public override void ExposeData()
        {
        }

        public override void FinalizeInit()
        {
        }

        public override void StartedNewGame()
        {
        }

        public override void LoadedGame()
        {
        }
    }

    [HarmonyPatch(typeof(Gene))]
    class SaveGenderedGenesPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("ExposeData")]
        public static void geneSavePatch(ref Gene __instance)
        {
            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();

            Gender gender;
            if (storage.trackerGeneGenders.TryGetValue(__instance, out gender) && gender!=Gender.None) {
                Scribe_Values.Look(ref gender, "geneGender");
            }

            if (Scribe.mode != LoadSaveMode.Saving)
            {
                Scribe_Values.Look(ref gender, "geneGender");
                if (gender != Gender.None)
                {
                    storage.trackerGeneGenders[__instance] = gender;
                }
            }

        }
    }

    [HarmonyPatch(typeof(CustomXenotype))]
    class SaveGenderedXenotypesPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("ExposeData")]
        public static void xenotypeSavePatch(ref CustomXenotype __instance)
        {
            //GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();

            Dictionary<GeneDef, Gender> genders = null;
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                genders = GenderGeneStorage.trackerCurrentXenotypeGenders;
                Scribe_Collections.Look(ref genders, "xenotypeGenders", LookMode.Def, LookMode.Value);
            }

            if (Scribe.mode != LoadSaveMode.Saving)
            {
                Scribe_Collections.Look(ref genders, "xenotypeGenders", LookMode.Def, LookMode.Value);
                if (genders != null)
                {
                    GenderGeneStorage.trackerXenotypeGenders[__instance] = genders;
                }
            }
        }
    }


    [HarmonyPatch(typeof(GeneSet))]
    class SaveGenderedGeneSetPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("ExposeData")]
        public static void GeneSetSavePatch(ref GeneSet __instance)
        {
            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();

            Dictionary<GeneDef, Gender> genders = null;
            if (Scribe.mode == LoadSaveMode.Saving && storage.trackerGeneSetGenders.TryGetValue(__instance.GenesListForReading, out genders))
            {
                Scribe_Collections.Look(ref genders, "geneSetGenders", LookMode.Def, LookMode.Value);
            }

            if (Scribe.mode != LoadSaveMode.Saving)
            {
                Scribe_Collections.Look(ref genders, "geneSetGenders", LookMode.Def, LookMode.Value);
                if (genders != null)
                {
                    storage.trackerGeneSetGenders[__instance.GenesListForReading] = genders;
                }
            }

        }
    }


    [HarmonyPatch(typeof(CustomXenogerm))]
    class SaveGenderedXenogermPatch
    {

        private static Dictionary<CustomXenogerm, List<Genepack>> geneLists = new Dictionary<CustomXenogerm, List<Genepack>>();
        private static Dictionary<CustomXenogerm, List<Gender>> genderLists = new Dictionary<CustomXenogerm, List<Gender>>();

        [HarmonyPostfix]
        [HarmonyPatch("ExposeData")]
        public static void xenogermSavePatch(ref CustomXenogerm __instance)
        {
            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();

            Dictionary<Genepack, Gender> genders = null;
            List<Genepack> list1 = null;
            List<Gender> list2 = null;

            if (Scribe.mode == LoadSaveMode.Saving && storage.trackerCustomXenogermGenders.TryGetValue(__instance, out genders))
            {

                list1=new List<Genepack>();
                list2=new List<Gender>();

                foreach (KeyValuePair<Genepack, Gender> gender in genders)
                {
                    list1.Add(gender.Key);
                    list2.Add(gender.Value);
                }

                Scribe_Collections.Look(ref list1, "customXenogermGenes", LookMode.Reference);
                Scribe_Collections.Look(ref list2, "customXenogermGenders", LookMode.Value);
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Collections.Look(ref list1, "customXenogermGenes", LookMode.Reference);
                Scribe_Collections.Look(ref list2, "customXenogermGenders", LookMode.Value);
                geneLists[__instance] = list1;
                genderLists[__instance] = list2;
            }

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                list1 = geneLists[__instance];
                Scribe_Collections.Look(ref list1, "customXenogermGenes", LookMode.Reference);
                
                list2 = genderLists[__instance];
                genders = new Dictionary<Genepack, Gender>();

                for (int i=0; i<list1.Count; i++)
                {
                    genders[list1[i]] = list2[i];
                }

                storage.trackerCustomXenogermGenders[__instance] = genders;
            }
        }
    }

    [HarmonyPatch(typeof(Building_GeneAssembler))]
    class SaveAssemblingXenogermPatch
    {

        private static Dictionary<Building_GeneAssembler, List<Genepack>> geneLists = new Dictionary<Building_GeneAssembler, List<Genepack>>();
        private static Dictionary<Building_GeneAssembler, List<Gender>> genderLists = new Dictionary<Building_GeneAssembler, List<Gender>>();

        [HarmonyPatch("ExposeData")]
        [HarmonyPostfix]
        public static void assemblerSavePatch(Building_GeneAssembler __instance)
        {
            /*if (___genepacksToRecombine.NullOrEmpty())
            {
                return;
            }*/

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();

            Dictionary<Genepack, Gender> genders = null;
            List<Genepack> list1 = null;
            List<Gender> list2 = null;

            if (Scribe.mode == LoadSaveMode.Saving && storage.trackerAssemblingGenders.TryGetValue(__instance, out genders))//what if multiple assemblers assemble identical xenogerms, but with different genders?
            {
                list1 = new List<Genepack>();
                list2 = new List<Gender>();

                foreach (KeyValuePair<Genepack, Gender> gender in genders)
                {
                    list1.Add(gender.Key);
                    list2.Add(gender.Value);
                }

                Scribe_Collections.Look(ref list1, "assemblingGenepacks", LookMode.Reference);
                Scribe_Collections.Look(ref list2, "assenmblingGenders", LookMode.Value);
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Scribe_Collections.Look(ref list1, "assemblingGenepacks", LookMode.Reference);
                Scribe_Collections.Look(ref list2, "assenmblingGenders", LookMode.Value);
                geneLists[__instance] = list1;
                genderLists[__instance] = list2;
            }

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                list1 = geneLists[__instance];
                Scribe_Collections.Look(ref list1, "assemblingGenepacks", LookMode.Reference);
                
                list2 = genderLists[__instance];
                genders = new Dictionary<Genepack, Gender>();

                for (int i = 0; i < list1.Count; i++)
                {
                    genders[list1[i]] = list2[i];
                }

                storage.trackerAssemblingGenders[__instance] = genders;
            }




        /*
        GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
        Dictionary<Genepack, Gender> genders=null;
        if (Scribe.mode == LoadSaveMode.Saving && storage.trackerAssemblingGenders.TryGetValue(___genepacksToRecombine, out genders))
        {
            List<Genepack> genesList = new List<Genepack>();
            List<Gender> gendersList = new List<Gender>();
            Scribe_Collections.Look(ref genders, "assemblerGenders", LookMode.Reference, LookMode.Value, ref genesList, ref gendersList);
        }

        if (Scribe.mode != LoadSaveMode.Saving)
        {
            List<Genepack> genesList = new List<Genepack>();
            List<Gender> gendersList = new List<Gender>();
            Scribe_Collections.Look(ref genders, "assemblerGenders", LookMode.Reference, LookMode.Value, ref genesList, ref gendersList);
            if (genders != null)
            {
                storage.trackerAssemblingGenders[___genepacksToRecombine] = genders;
            }
        }*/
        }


        [HarmonyPostfix]
        [HarmonyPatch("Reset")]
        public static void clearWIPXenogermGenders(Building_GeneAssembler __instance)
        {
            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            if (storage.trackerAssemblingGenders.ContainsKey(__instance)){
                storage.trackerAssemblingGenders.Remove(__instance);
            }

        }
    }

    [HarmonyPatch(typeof(Xenogerm))]
    class SaveXenogermPatch
    {

        [HarmonyPatch("ExposeData")]
        [HarmonyPostfix]
        public static void xenogermSavePatch(Xenogerm __instance)
        {

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            Dictionary<GeneDef, Gender> genders = null;
            if (Scribe.mode == LoadSaveMode.Saving && storage.trackerXenogermGenders.TryGetValue(__instance, out genders))
            {
                Scribe_Collections.Look(ref genders, "xenogermGenders", LookMode.Def, LookMode.Value);
            }

            if (Scribe.mode != LoadSaveMode.Saving)
            {
                Scribe_Collections.Look(ref genders, "xenogermGenders", LookMode.Def, LookMode.Value);
                if (genders != null)
                {
                    storage.trackerXenogermGenders[__instance] = genders;
                }
            }
        }
    }

    /*
    [HarmonyPatch(typeof(Pawn_GeneTracker))]
    class SaveGeneTrackerPatch
    {

        [HarmonyPatch("ExposeData")]
        [HarmonyPostfix]
        public static void geneTrackerSavePatch(Pawn_GeneTracker __instance)
        {

            GenderGeneStorage storage = Current.Game.GetComponent<GenderGeneStorage>();
            Dictionary<GeneDef, Gender> genders = null;
            if (Scribe.mode == LoadSaveMode.Saving && storage.trackerTrackerGenders.TryGetValue(__instance, out genders))
            {
                Scribe_Collections.Look(ref genders, "trackerGenders", LookMode.Def, LookMode.Value);
            }

            if (Scribe.mode != LoadSaveMode.Saving)
            {
                Scribe_Collections.Look(ref genders, "trackerGenders", LookMode.Def, LookMode.Value);
                if (genders != null)
                {
                    storage.trackerTrackerGenders[__instance] = genders;
                }
            }
        }
    }*/

}


