﻿using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace StartupImpact.Patch
{
    [HarmonyPatch(typeof(ModContentPack), "LoadDefs")]
    class ModContentPackLoadDefs
    {
        static IEnumerable<LoadableXmlAsset> Postfix(IEnumerable<LoadableXmlAsset> __result, ModContentPack __instance)
        {
            var info = StartupImpact.modlist.Get(__instance);
            info.Start("defs-0-load");

            foreach (var v in __result)
            {
                yield return v;
            }

            info.Stop("defs-0-load");

            yield break;
        }
    }

    [HarmonyPatch(typeof(ModContentPack), "LoadPatches")]
    class ModContentPackLoadPatches
    {
        public static Dictionary<string, ModInfo> patchMods = new Dictionary<string, ModInfo>();

        static void Prefix(ModContentPack __instance)
        {
            StartupImpact.modlist.Get(__instance).Start("load-patches");
        }
        static void Postfix(ModContentPack __instance)
        {
            ModInfo info = StartupImpact.modlist.Get(__instance);
            info.Stop("load-patches");

            foreach(PatchOperation patch in __instance.Patches) {
                patchMods[patch.sourceFile] = info;
            }
        }
    }

    [HarmonyPatch]
    class ModContentPackReloadContentDelegate
    {
        static MethodBase TargetMethod()
        {
            MethodInfo methodReloadContent = AccessTools.DeclaredMethod(typeof(ModContentPack), "ReloadContentInt");
            if (methodReloadContent != null)
                return methodReloadContent;
            return AccessTools.GetDeclaredMethods(typeof(ModContentPack)).Where(x => x.Name.StartsWith("<ReloadContent>")).FirstOrDefault();
        }

        static bool Prefix(ModContentPack __instance)
        {
            DeepProfilerStart.mute = true;

            var inst = Traverse.Create(__instance);

            var info = StartupImpact.modlist.Get(__instance);

            info.Start("audioclips");
            inst.Field<ModContentHolder<AudioClip>>("audioClips").Value.ReloadAll();
            info.Stop("audioclips");

            info.Start("textures");
            inst.Field<ModContentHolder<Texture2D>>("textures").Value.ReloadAll();
            info.Stop("textures");

            info.Start("strings");
            inst.Field<ModContentHolder<string>>("strings").Value.ReloadAll();
            info.Stop("strings");

            info.Start("assetBundles");
            __instance.assetBundles.ReloadAll();
            inst.Field<List<List<string>>>("allAssetNamesInBundleCached").Value = null;
            info.Stop("assetBundles");

            DeepProfilerStart.mute = false;

            return false;
        }

    }

    
}
