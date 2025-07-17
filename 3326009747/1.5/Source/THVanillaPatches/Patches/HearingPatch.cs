using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace THVanillaPatches.Patches
{
	public class HearingPatch(THPatchDef def) : ToggleablePatchParent(def)
	{
		protected override List<PatchInfo> Patches => new List<PatchInfo>
		{
			new PatchInfo(
				AccessTools.Method(typeof(GenClamor), nameof(GenClamor.DoClamor),
					new[] { typeof(Thing), typeof(float), typeof(ClamorDef) }),
				Prefix: new HarmonyMethod(GetType(), nameof(PreDoClamor))),
			new PatchInfo(
				AccessTools.Method(typeof(Pawn), nameof(Pawn.HearClamor)),
				Prefix: new HarmonyMethod(GetType(), nameof(PreHearClamor))),
		};
		
		//Footsteps on carpet are quieter
		private static bool PreDoClamor(Thing source, float radius, ClamorDef type)
		{
			if (type != ClamorDefOf.Movement) return true;
			if (!source.Map.terrainGrid.TerrainAt(source.Position).IsCarpet) return true;
			//Half the sound
			GenClamor.DoClamor(source, source.Position, radius / 2, type);
			return false;

		}

		private static bool PreHearClamor(Pawn __instance, Thing source, ClamorDef type)
		{
			if (type != ClamorDefOf.Movement) return true;

			ThingGrid thingGrid = __instance.Map.thingGrid;
			
			return !Enumerable.Any(GenSight.BresenhamCellsBetween(__instance.Position, source.Position), intVec3 => thingGrid.ThingsAt(intVec3).Any(thing => thing.def == ThingDefOf.Drape)) && GenSight.LineOfSight(__instance.Position, source.Position, __instance.Map);
		}
	}
}