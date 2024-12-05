using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FilterableDesignator
{
	public static class HarmonyPatches
	{
		[HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
		public class LoadGame_Patch
		{
			[HarmonyPostfix]
			static void Postfix()
			{
				Utility.ClearAllMode();
			}
		}

		[HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
		public class InitNewGame_Patch
		{
			[HarmonyPostfix]
			static void Postfix()
			{
				Utility.ClearAllMode();
			}
		}
	}
}
