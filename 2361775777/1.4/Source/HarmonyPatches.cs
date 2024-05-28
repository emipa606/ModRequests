using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using static HarmonyLib.AccessTools;
using RimWorld;
using Verse;

namespace BMBIFVTE
{
	public class HarmonyPatches : Mod
	{
		public HarmonyPatches(ModContentPack content) : base(content)
		{
			var harmony = new Harmony("BMBIFVTE");

            //Patching Character Editor to use my icon when found
            //TypeByName("CharacterEditor.DefTool")?.AccessMethod("GetCreateMainButton")?.Patch(harmony, postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CharacterEditor)));

            //Patching Hospitality's settings to not disable my icon
            TypeByName("Hospitality.HospitalityModBase")?.AccessMethod("UpdateMainButtonIcon")?.Patch(harmony, transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Hospitality)));
		}

		public static void CharacterEditor(ref MainButtonDef __result)
		{
			__result.iconPath = "UI/Buttons/MainButtons/CharacterEditor";
		}

		public static IEnumerable<CodeInstruction> Hospitality(IEnumerable<CodeInstruction> instructions)
		{
			yield return new CodeInstruction(OpCodes.Ret);
		}
	}

	public static class AccessToolsExtensions
	{
		public static MethodInfo Patch(this MethodBase methodBase, Harmony harmony, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null) => harmony.Patch(methodBase, prefix, postfix, transpiler);
		public static MethodInfo AccessMethod(this Type type, string name) => Method(type, name);
	}
}