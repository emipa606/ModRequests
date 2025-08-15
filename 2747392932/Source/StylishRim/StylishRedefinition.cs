using AlienRace;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Verse;

namespace StylishRim
{
    [StaticConstructorOnStartup()]
    public static class StylishRedefinition
    {
        private static readonly string TAG_DEF_NAME = "defName";
        private static readonly string PATCH_DIR = "Patches/";
        public static Dictionary<Apparel, string> dic = new Dictionary<Apparel, string>();
        public static void Redefine()
        {
            RedefineThingDefModContentPack();
        }
        private static void RedefineThingDefModContentPack()
        {
            if (DefDatabase<ThingDef>.DefCount == 0) DefDatabase<ThingDef>.AddAllInMods();
            List<ThingDef> defs = DefDatabase<ThingDef>.AllDefs.Where(x => x.modContentPack == null).ToList();
            Log.Message($"[Stylish Rim] Stylish Redefinition, Undefined ThingDef count: {defs.Count}");
            if (defs.Count == 0) return;
            Dictionary<ModContentPack, int> result = new Dictionary<ModContentPack, int>();
            XmlNodeList xmls;
            ThingDef def;
            Tuple<string, string, string> word = new Tuple<string, string, string>("[Stylish Rim] Define [", "] ThingDef.modContentPack to [", "]");
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
            {
                if (!result.ContainsKey(mod)) result.Add(mod, 0);
                try
                {
                    foreach (LoadableXmlAsset xml in DirectXmlLoader.XmlAssetsInModFolder(mod, PATCH_DIR))
                    {
                        if (xml != null)
                        {
                            xmls = xml.xmlDoc.GetElementsByTagName(TAG_DEF_NAME);
                            for (int i = 0; i < xmls.Count; i++)
                            {
                                if (defs.Any(x => x.defName == xmls[i].InnerText))
                                {
                                    def = defs.Where(x => x.defName == xmls[i].InnerText).FirstOrDefault();
                                    if (def != null)
                                    {
                                        def.modContentPack = mod;
                                        defs.Remove(def);
                                        result[mod]++;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Log.Warning($"[Stylish Rim] Unreadable xml error: [{mod.Name}].");
                }
            }
            result.RemoveAll(x => x.Value == 0);
            if (result.Count > 0)
            {
                foreach (KeyValuePair<ModContentPack, int> p in result)
                {
                    Log.Message($"{word.Item1}{p.Value}{word.Item2}{p.Key.Name}{word.Item3}.");
                }
            }
            else
            {
                Log.Message($"[Stylish Rim] No redefinable ThingDef was found.");
            }
        }
        public static void RedefinePortraitHeadRatio()
        {/*
            if (DefDatabase<ThingDef_AlienRace>.DefCount == 0) DefDatabase<ThingDef_AlienRace>.AddAllInMods();
            List<ThingDef_AlienRace> defs = DefDatabase<ThingDef_AlienRace>.AllDefs.ToList();
            foreach (ThingDef_AlienRace def in defs)
            {
                if ((def.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize / def.alienRace.generalSettings.alienPartGenerator.customDrawSize) != (def.alienRace.generalSettings.alienPartGenerator.customPortraitHeadDrawSize / def.alienRace.generalSettings.alienPartGenerator.customPortraitDrawSize))
                {
                    Traverse tra = Traverse.Create(def.alienRace.generalSettings.alienPartGenerator);
                    Dictionary<Vector2, AlienPartGenerator.AlienGraphicMeshSet> dic = tra.Field<Dictionary<Vector2, AlienPartGenerator.AlienGraphicMeshSet>>("meshPools").Value;
                    dic.Remove(def.alienRace.generalSettings.alienPartGenerator.customPortraitHeadDrawSize);
                    def.alienRace.graphicPaths.ForEach(delegate (GraphicPaths gp)
                    {
                        dic.Remove(gp.customPortraitHeadDrawSize);
                    });
                    def.ResolveReferences();
                    def.alienRace.graphicPaths.ForEach(delegate (GraphicPaths gp)
                    {
                        gp.customPortraitHeadDrawSize = def.alienRace.generalSettings.alienPartGenerator.customPortraitHeadDrawSize;
                    });
                    def.alienRace.generalSettings.alienPartGenerator.GenerateMeshsAndMeshPools();
                }
            }*/
        }
        public static void RedifinePortraitDrawSize(ThingDef_AlienRace def)
        {
            string result;
            if (def.alienRace.generalSettings.alienPartGenerator.customDrawSize == Vector2.one)
            {
                result = RedifinePortraitHeadSize(def);
            }
            else if (def.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize == Vector2.one)
            {
                result = RedifinePortraitBodySize(def);
            }
            else if (def.alienRace.generalSettings.alienPartGenerator.customDrawSize.x != def.alienRace.generalSettings.alienPartGenerator.customDrawSize.y)
            {
                result = RedifinePortraitBodySize(def);
            }
            else if (def.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize.x != def.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize.y)
            {
                result = RedifinePortraitHeadSize(def);
            }
            else
            {
                result = RedifinePortraitHeadSize(def);
            }
            Log.Message($"[Stylish Rim] {def.defName} Head-to-body ratio is different between normal and portrait, redefine {result}");
        }
        private static string RedifinePortraitHeadSize(ThingDef_AlienRace def)
        {
            Vector2 newHeadSize = def.alienRace.generalSettings.alienPartGenerator.customPortraitDrawSize * (def.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize / def.alienRace.generalSettings.alienPartGenerator.customDrawSize);
            string result = $"customPortraitHeadDrawSize {def.alienRace.generalSettings.alienPartGenerator.customPortraitHeadDrawSize } to({ newHeadSize.x}, { newHeadSize.y}).";
            def.alienRace.generalSettings.alienPartGenerator.customPortraitHeadDrawSize = newHeadSize;
            return result;
        }
        private static string RedifinePortraitBodySize(ThingDef_AlienRace def)
        {
            Vector2 newBodySize = def.alienRace.generalSettings.alienPartGenerator.customPortraitHeadDrawSize * (def.alienRace.generalSettings.alienPartGenerator.customDrawSize / def.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize);
            string result = $"customPortraitDrawSize {def.alienRace.generalSettings.alienPartGenerator.customPortraitDrawSize} to ({newBodySize.x}, {newBodySize.y}).";
            def.alienRace.generalSettings.alienPartGenerator.customPortraitDrawSize = newBodySize;
            return result;
        }
    }
}
