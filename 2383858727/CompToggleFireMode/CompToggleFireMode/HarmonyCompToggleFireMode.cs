using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CompToggleFireMode
{
	// Token: 0x02000005 RID: 5
	[StaticConstructorOnStartup]
	internal static class HarmonyCompToggleFireMode
	{
		// Token: 0x0600000F RID: 15 RVA: 0x000022D0 File Offset: 0x000004D0
		static HarmonyCompToggleFireMode()
		{
			Harmony harmony = new Harmony("rimworld.ogliss.comps.CompToggleFireMode");
			Type typeFromHandle = typeof(HarmonyCompToggleFireMode);
			harmony.Patch(AccessTools.Method(typeof(Pawn), "GetGizmos", null, null), null, new HarmonyMethod(typeFromHandle, "GetGizmos_PostFix", null), null, null);
			harmony.Patch(AccessTools.Method(typeof(CompEquippable), "get_PrimaryVerb", null, null), null, new HarmonyMethod(typeFromHandle, "PrimaryVerb_PostFix", null), null, null);
			harmony.PatchAll();
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002354 File Offset: 0x00000554
		public static void PrimaryVerb_PostFix(CompEquippable __instance, ref Verb __result)
		{
			CompToggleFireMode compToggleFireMode = __instance.parent.TryGetComp<CompToggleFireMode>();
			bool flag = compToggleFireMode != null;
			if (flag)
			{
				__result = compToggleFireMode.ActiveVerb;
			}
		}

		public static IEnumerable<Gizmo> GizmoGetter(CompToggleFireMode CompToggleFireMode)
		{
			if (!CompToggleFireMode.GizmosOnEquip)
			{
				yield break;
			}
			foreach (Gizmo item in CompToggleFireMode.EquippedGizmos())
			{
				yield return item;
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002390 File Offset: 0x00000590
		public static IEnumerable<Gizmo> GetGizmos_PostFix(IEnumerable<Gizmo> __result, Pawn __instance)
		{
			foreach (Gizmo item in __result)
			{
				yield return item;
			}
			Pawn_EquipmentTracker equipment = __instance.equipment;
			if (equipment == null)
			{
				yield break;
			}
			ThingWithComps primary = equipment.Primary;
			if (primary == null)
			{
				yield break;
			}
			CompToggleFireMode comp = primary.GetComp<CompToggleFireMode>();
			if (comp == null || __instance.Faction != Faction.OfPlayer)
			{
				yield break;
			}
			foreach (Gizmo item2 in GizmoGetter(comp))
			{
				yield return item2;
			}
		}

		// Token: 0x02000008 RID: 8
		[HarmonyPatch(typeof(VerbUtility), "HarmsHealth")]
		public class HarmsHealth_Patch
		{
			// Token: 0x0600001E RID: 30 RVA: 0x0000274C File Offset: 0x0000094C
			public static void Postfix(Verb verb, ref bool __result)
			{
				bool flag = !__result;
				if (flag)
				{
					CompToggleFireMode comp = verb.EquipmentSource.GetComp<CompToggleFireMode>();
					bool flag2 = comp != null;
					if (flag2)
					{
						foreach (Verb verb2 in comp.Equippable.AllVerbs)
						{
							DamageDef damageDef = verb2.GetDamageDef();
							bool flag3 = damageDef != null && damageDef.harmsHealth;
							if (flag3)
							{
								__result = true;
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x02000009 RID: 9
		[HarmonyPatch(typeof(VerbTracker), "GetVerbsCommands")]
		public class CreateVerbTargetCommand_Patch
		{
			// Token: 0x06000020 RID: 32 RVA: 0x000027E4 File Offset: 0x000009E4
			private static void Postfix(VerbTracker __instance, ref IEnumerable<Command> __result)
			{
				List<Command> list = __result.ToList<Command>();
				CompEquippable compEquippable = __instance.directOwner as CompEquippable;
				CompToggleFireMode comp = compEquippable.parent.TryGetComp<CompToggleFireMode>();
				bool flag = comp != null;
				if (flag)
				{
					list.RemoveAll(delegate(Command x)
					{
						Command_VerbTarget command_VerbTarget = x as Command_VerbTarget;
						return command_VerbTarget != null && command_VerbTarget.verb != comp.ActiveVerb;
					});
					__result = list;
				}
			}
		}
	}
}
