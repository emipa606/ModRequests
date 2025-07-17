using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace THVanillaPatches.Patches
{
	public class WasterPollutionPatch(THPatchDef def) : ToggleablePatchParent(def)
	{
		protected override List<PatchInfo> Patches => new List<PatchInfo>
		{
			new PatchInfo(
				AccessTools.Method(typeof(CompDissolutionEffect_Goodwill),
					nameof(CompDissolutionEffect_Goodwill.DoDissolutionEffectMap)),
				Postfix: new HarmonyMethod(GetType(), nameof(PostDoDissolutionEffectMap))),
			new PatchInfo(
				AccessTools.Method(typeof(CompDissolutionEffect_Goodwill),
					nameof(CompDissolutionEffect_Goodwill.DoDissolutionEffectWorld)),
				Postfix: new HarmonyMethod(GetType(), nameof(PostAddWorldDissolutionEvent)))
		};
		
		public static void PostDoDissolutionEffectMap(ref CompDissolutionEffect_Goodwill __instance, int amount)
		{
			if (!__instance.parent.Spawned || __instance.parent.Map.IsPlayerHome) return;
			TweakDissolutionEvent(__instance.parent.Map.Tile);
		}
		
		public static void PostAddWorldDissolutionEvent(ref CompDissolutionEffect_Goodwill __instance, int amount, int tileId)
		{
			if (PollutionUtility.IsExecutingPollutionIgnoredQuest()) return;
			TweakDissolutionEvent(tileId);
		}

		private static void TweakDissolutionEvent(int tileId)
		{
			Faction owner = Find.WorldObjects.WorldObjectAt(tileId, WorldObjectDefOf.Settlement)?.Faction;
			if (owner == null) return;
			bool isWaster = false;
			for (int i = 0; i < owner.def.xenotypeSet.Count; i++)
			{
				XenotypeChance chance = owner.def.xenotypeSet[i];
				if (chance.xenotype != THVanillaPatchesDefsOf.Waster || !(chance.chance >= .5)) continue;
				isWaster = true;
				break;
			}
			
			if (!isWaster) return;

			foreach (var pendingGoodwillEvent in Traverse.Create<CompDissolutionEffect_Goodwill>().Field("pendingGoodwillEvents").GetValue<IEnumerable>())
			{
				int eventTile = Traverse.Create(pendingGoodwillEvent).Field("tile").GetValue<int>();
				Traverse amountTraverse = Traverse.Create(pendingGoodwillEvent).Field("amount");
				int eventAmount = amountTraverse.GetValue<int>();
				if (eventTile == tileId)
				{
					amountTraverse.SetValue(eventAmount / 4);
				}
			}
		}
		
	}
}