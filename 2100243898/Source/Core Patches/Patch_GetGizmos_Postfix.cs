using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using HarmonyLib;
using Verse;
using DTechprinting.ShardBuilding;
using UnityEngine;

namespace DTechprinting.Core_Patches
{
	// Token: 0x0200000A RID: 10
	[HarmonyPatch(typeof(Building))]
	[HarmonyPatch("GetGizmos")]
	public class Patch_GetGizmos_Postfix
	{
		// Token: 0x06000013 RID: 19 RVA: 0x000025E4 File Offset: 0x000007E4
		private static void Postfix(ref IEnumerable<Gizmo> __result, ref Building __instance)
		{
			Designator_ShardBuilding designator = new Designator_ShardBuilding();
			if (designator.CanDesignateThing(__instance).Accepted)
			{
				Command_ShardBuilding shardBuilding = new Command_ShardBuilding(__instance);
				shardBuilding.activateSound = SoundDef.Named("Click");
				shardBuilding.defaultLabel = "techprint".Translate();
				shardBuilding.defaultDesc = "techprintDesc".Translate();
				shardBuilding.icon = ContentFinder<Texture2D>.Get("Things/Item/Special/Techshard/Techshard_c", true);
				shardBuilding.hotKey = KeyBindingDefOf.Misc10;
				shardBuilding.action = delegate ()
				{
					if (designator.CanDesignateThing(shardBuilding.building).Accepted)
					{
						designator.DesignateThing(shardBuilding.building);
					}
				};
				__result = CollectionExtensions.AddItem<Gizmo>(__result, shardBuilding);
			}
		}
	}
}
