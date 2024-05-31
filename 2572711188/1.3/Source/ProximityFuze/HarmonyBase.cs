using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
namespace ProximityFuze {
	[StaticConstructorOnStartup]
	internal static class HarmonyBase {

		internal static Harmony instance;

		static HarmonyBase() {
			instance = new Harmony("ordpus.proximityfuze");
			instance.PatchAll();
		}

		public static bool IsLdloc(this CodeInstruction code, int index) {
			if(index < 0) return false;
			switch(index) {
				case 0: if(code.opcode == OpCodes.Ldloc_0) return true; break;
				case 1: if(code.opcode == OpCodes.Ldloc_1) return true; break;
				case 2: if(code.opcode == OpCodes.Ldloc_2) return true; break;
				case 3: if(code.opcode == OpCodes.Ldloc_3) return true; break;
			}
			if(code.opcode == OpCodes.Ldloc || code.opcode == OpCodes.Ldloca || code.opcode == OpCodes.Ldloca_S || code.opcode == OpCodes.Ldloc_S)
				return (code.operand as LocalBuilder)?.LocalIndex == index;
			return false;
		}

		public static string LowerFirst(this string str) {
			return char.ToLower(str[0]) + str.Substring(1);
		}

	}
}
