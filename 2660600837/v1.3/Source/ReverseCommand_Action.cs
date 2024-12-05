using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FilterableDesignator
{
	public class ReverseCommand_Action : Command_Action
	{
		public bool isReverse = true;

		[HarmonyPatch(typeof(InspectGizmoGrid), nameof(InspectGizmoGrid.DrawInspectGizmoGridFor))]
		public class DrawInspectGizmoGridFor_Patch
		{
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var done = false;
				CodeInstruction prevInstruction = null;
				foreach (var instruction in instructions)
				{
					if (!done && prevInstruction?.opcode == OpCodes.Newobj && AccessTools.Constructor(typeof(Command_Action)).Equals(prevInstruction.operand))
					{
						yield return instruction;
						done = true;
					}
					else
					{
						yield return instruction;
					}
					prevInstruction = instruction;
				}
			}
		}
	}
}
