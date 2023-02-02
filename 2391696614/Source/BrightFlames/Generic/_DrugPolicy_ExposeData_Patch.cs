using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BrightFlames
{
	/// <summary>
	/// Spawn a glower when a fire spawns, and remove it accordingly.
	/// </summary>
	internal static class _Fire_Patch
	{
		private static ThingDef fireGlowDef = ThingDef.Named("FireGlow");
		//private static Dictionary<Fire, CompGlower> glowers;
		//private static CompProperties_Glower properties = new CompProperties_Glower {compClass = typeof(CompGlower), glowColor = new ColorInt(252, 187, 113, 0) * 1.45f, glowRadius = 12};

		[HarmonyPatch(typeof(Fire), nameof(Fire.SpawnSetup))]
		public class SpawnSetup
		{
			[HarmonyPostfix]
			internal static void Postfix(Fire __instance, Map map)
			{
				if (__instance.parent == null)
				{
					var fireGlow = GenSpawn.Spawn(fireGlowDef, __instance.Position, map) as FireGlow;
					fireGlow.SetFire(__instance);

					//glowers ??= new Dictionary<Fire, CompGlower>();
					//
					//var glower = new CompGlower {props = properties};
					//glowers.Add(__instance, glower);
					//
					//map.mapDrawer.MapMeshDirty(__instance.Position, MapMeshFlag.Things);
					//map.glowGrid.RegisterGlower(glower);
				}
			}
		}

		//[HarmonyPatch(typeof(Fire), nameof(Fire.DeSpawn))]
		//public class DeSpawn
		//{
		//	[HarmonyPostfix]
		//	internal static void Postfix(Fire __instance, DestroyMode mode)
		//	{
		//		glowers ??= new Dictionary<Fire, CompGlower>();
		//		if (glowers.TryGetValue(__instance, out var glower))
		//		{
		//			__instance.Map.mapDrawer.MapMeshDirty(__instance.Position, MapMeshFlag.Things);
		//			__instance.Map.glowGrid.DeRegisterGlower(glower);
		//			glowers.Remove(__instance);
		//		}
		//	}
		//}
	}
}
