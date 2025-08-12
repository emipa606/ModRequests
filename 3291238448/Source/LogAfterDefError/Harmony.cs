using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;

namespace LogAfterDefError {

	internal static class HarmonyPatches {
		internal static Harmony Instance { get; private set; }

		static HarmonyPatches() {
			Instance = new Harmony("ordpus.logafterdeferror");
			Instance.PatchAll(Assembly.GetExecutingAssembly());
		}

	}


	[HarmonyPatch(typeof(DirectXmlLoader), nameof(DirectXmlLoader.DefFromNode))]
	internal static class Verse__DirectXmlLoader__DefFromNode {

		internal static void Prefix(ref LogMessage __state) {
			__state = Log.messageQueue.lastMessage;
		}

		internal static void Postfix(Def __result, LoadableXmlAsset loadingAsset, LogMessage __state) {
			if(__result == null || !LogAfterDefErrorModSettings.enabled || Log.messageQueue.lastMessage == __state) return;
			var mod = loadingAsset?.mod ?? null;
			if(Log.messageCount > 900) Log.ResetMessageCount();
			Log.Message("[Def Error] " + __result.defName + ":" + mod?.Name + loadingAsset?.FullFilePath?.Replace(mod?.RootDir, "") + "\n");
		}
	}

}
