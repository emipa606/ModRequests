using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using RimWorld.Planet;

namespace allowAdjacentSettle
{
	[StaticConstructorOnStartup]
	internal static class HarmonyInit
	{
		static HarmonyInit()
		{
			new Harmony("whatamidoing.allowAdjacentSettle").PatchAll();
		}
	}
	[HarmonyPatch(typeof(TileFinder), "IsValidTileForNewSettlement")]
	public static class SettlePatch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
		{
			var codes = new List<CodeInstruction>(instructions);
			var jumpIndex = -1; //the jump we need to alter
			var startIndex = -1; //the start of where we want to remove
			var endIndex = -1; //the instruction after where we want to remove
			bool foundPreceding = false; //to find the thing before the things
			Label label = il.DefineLabel(); //so we can make the jump work
			
			//first, find the brfalse jump before the jump, so we can set it to the one after
			for (var i = 0; i < codes.Count; i++)
            {
                if (!foundPreceding && codes[i].operand != null)
                {
					if (codes[i].operand.ToString() == "CannotLandImpassableMountains")
					{
						foundPreceding = true;
					}
				}
                else
                {
                    if (codes[i].opcode == OpCodes.Brfalse_S)
                    {
						jumpIndex = i;
						break;
                    }
                }
            }
			if(jumpIndex == -1)
            {
				Log.Error("[AdjacentSettlements] Unable to find jump instruction, returning unmodified instructions!");
				return instructions;
            }

			//then find the call that marks the start of the block we want to nuke
			foundPreceding = false;
			for (var j = jumpIndex + 1; j < codes.Count; j++)
            {
                if (!foundPreceding)
                {
                    if (codes[j].opcode == OpCodes.Ret)
                    {
						foundPreceding = true;
                    }
                }
                else
                {
                    if (codes[j].opcode == OpCodes.Call)
                    {
						startIndex = j;
						break;
                    }
                }
            }
			if (startIndex == -1)
			{
				Log.Error("[AdjacentSettlements] Unable to find start call instruction, returning unmodified instructions!");
				return instructions;
			}
			foundPreceding = false;

			//do something similar to find the call that marks the start of the block after the nuked one, and our new jump destination
			for (var k = startIndex + 1; k < codes.Count; k++)
			{
				if (!foundPreceding)
				{
					if (codes[k].opcode == OpCodes.Ldc_I4_0)
					{
						foundPreceding = true;
					}
				}
				else
				{
					if (codes[k].opcode == OpCodes.Call)
					{
						endIndex = k;
						break;
					}
				}
			}
			if (endIndex == -1)
			{
				Log.Error("[AdjacentSettlements] Unable to find end call instruction, returning unmodified instructions!");
				return instructions;
			}

			codes[endIndex].labels.Add(label);
			codes[jumpIndex].operand = label;
			codes.RemoveRange(startIndex, endIndex - startIndex);

			Log.Message("[AdjacentSettlements] Transpiler patch successful(?)");
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(SettlementProximityGoodwillUtility), "AppendProximityGoodwillOffsets")]
	internal class GoodwillPatch
	{
		public static bool Prefix()
		{
			return !LoadedModManager.GetMod<AllowAdjacentSettle>().GetSettings<Settings>().disableRelationsHit;
		}
	}
}
