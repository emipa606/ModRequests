using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CM_Callouts
{
	// Token: 0x0200000D RID: 13
	[StaticConstructorOnStartup]
	public static class MoteMaker_Patches
	{
		// Token: 0x02000033 RID: 51
		[HarmonyPatch(typeof(MoteMaker))]
		[HarmonyPatch("ThrowText", new Type[]
		{
			typeof(Vector3),
			typeof(Map),
			typeof(string),
			typeof(Color),
			typeof(float)
		})]
		public static class MoteMaker_ThrowText
		{
			// Token: 0x0600006D RID: 109 RVA: 0x00004671 File Offset: 0x00002871
			[HarmonyTranspiler]
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				MethodInfo moteSpawner = AccessTools.Method(typeof(GenSpawn), "Spawn", new Type[]
				{
					typeof(Thing),
					typeof(IntVec3),
					typeof(Map),
					typeof(WipeMode)
				}, null);
				MethodInfo newMoteSpawner = AccessTools.Method(typeof(CalloutTracker), "ThrowText", new Type[]
				{
					typeof(Thing),
					typeof(IntVec3),
					typeof(Map),
					typeof(WipeMode)
				}, null);
				bool replaceCall = true;
				foreach (CodeInstruction instruction in instructions)
				{
					bool flag = replaceCall && instruction.Calls(moteSpawner);
					if (flag)
					{
						replaceCall = false;
						instruction.operand = newMoteSpawner;
					}
					yield return instruction;
				}
				yield break;
			}
		}
	}
}
