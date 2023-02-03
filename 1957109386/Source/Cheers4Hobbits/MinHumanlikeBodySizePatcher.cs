using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Verse;

namespace Cheers4Hobbits
{
	class MinHumanlikeBodySizePatcher
	{
		static readonly MethodInfo methodof_Pawn_get_BodySize = typeof(Pawn).GetProperty(nameof(Pawn.BodySize)).GetGetMethod();
		static readonly MethodInfo methodof_MinHumanlikeBodySize =
			typeof(MinHumanlikeBodySizePatcher).GetMethod(nameof(MinHumanlikeBodySize), AccessTools.all);

		public static IEnumerable<CodeInstruction> MinHumanlikeBodySizeTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Callvirt && instruction.operand == methodof_Pawn_get_BodySize)
					yield return new CodeInstruction(OpCodes.Call, methodof_MinHumanlikeBodySize);
				else
					yield return instruction;
			}
		}

		static float MinHumanlikeBodySize(Pawn pawn)
		{
			var bodySize = pawn.BodySize;
			if (bodySize < 1.0f && pawn.RaceProps.Humanlike)
				return 1.0f;
			else
				return bodySize;
		}
	}
}
