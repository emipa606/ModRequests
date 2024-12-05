using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using FilterableDesignator.ExtensionMethod;

namespace FilterableDesignator
{
	class Designator_Tame_Patch : Designator_Patch
	{
		// singleton
		private readonly static Designator_Tame_Patch instance = new Designator_Tame_Patch();

		public static Designator_Tame_Patch Instance
		{
			get
			{
				return instance;
			}
		}

		private Designator_Tame_Patch()
		{
		}

		private enum TameMode
		{
			All,
			Release,
		}
		private static TameMode tameMode = TameMode.All;

		public override void ClearMode()
		{
			tameMode = TameMode.All;
		}

		private protected override string CommandLabel(Designator instance)
		{
			if (tameMode == TameMode.Release)
			{
				return "DesignatorReleaseAnimalToWild".Translate();
			}
			return instance.defaultLabel;
		}

		[HarmonyPatch(typeof(Designator_Tame), nameof(Designator_Tame.CanDesignateCell))]
		public class CanDesignateCell_Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Designator_Tame __instance, ref AcceptanceReport __result, IntVec3 c)
			{
				if (tameMode == TameMode.Release)
				{
					if (!c.InBounds(Find.CurrentMap))
					{
						__result = AcceptanceReport.WasRejected;
						return false;
					}
					foreach (Thing thing in c.GetThingList(Find.CurrentMap))
					{
						if (__instance.CanDesignateThing(thing).Accepted)
						{
							__result = true;
							return false;
						}
					}
					__result = AcceptanceReport.WasRejected;
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Designator_Tame), nameof(Designator_Tame.CanDesignateThing))]
		public class CanDesignateThing_Patch
		{
			[HarmonyPostfix]
			static void Postfix(ref AcceptanceReport __result, Thing t)
			{
				/*	"Release"モードのときは手懐けの対象にならない動物も対象にする
				if (!__result.Accepted)
				{
					return;
				}
				*/
				if (tameMode == TameMode.All)
				{
					return;
				}
				if (IsReverseDesignator())
				{
					return;
				}
				if (tameMode == TameMode.Release)
				{
					if (t is Pawn pawn && pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer && Find.CurrentMap.designationManager.DesignationOn(t, DesignationDefOf.ReleaseAnimalToWild) is null && !pawn.Dead && pawn.RaceProps.canReleaseToWild)
					{
						__result = true;
					}
					else
					{
						__result = AcceptanceReport.WasRejected;
					}
					return;
				}
			}
		}

		[HarmonyPatch(typeof(Designator_Tame), nameof(Designator_Tame.DesignateSingleCell))]
		public class DesignateSingleCell_Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Designator_Tame __instance, IntVec3 loc)
			{
				if (tameMode == TameMode.Release)
				{
					foreach (Thing thing in loc.GetThingList(Find.CurrentMap))
					{
						if (__instance.CanDesignateThing(thing).Accepted)
						{
							__instance.DesignateThing(thing);
						}
					}
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(Designator_Tame), nameof(Designator_Tame.DesignateThing))]
		public class DesignateThing_Patch
		{
			[HarmonyPrefix]
			static bool Prefix(Thing t)
			{
				if (tameMode == TameMode.Release)
				{
					var manager = Find.CurrentMap.designationManager;
					manager.AddDesignation(new Designation((Pawn)t, DesignationDefOf.ReleaseAnimalToWild));
					Designation designation = manager.DesignationOn(t, DesignationDefOf.Slaughter);
					if (designation != null)
					{
						manager.RemoveDesignation(designation);
					}
					ReleaseAnimalToWildUtility.CheckWarnAboutBondedAnimal((Pawn)t);
					return false;
				}
				return true;
			}
		}

		private protected override void BuildFloatMenuOption(Designator instance, Event ev)
		{
			if (!Utility.ReversePatches.CheckCanInteract(instance))
			{
				return;
			}
			var floatMenuList = new List<(TameMode, ThingDef, string, Texture2D)>
			{
				(TameMode.All, null, instance.defaultLabel, ContentFinder<Texture2D>.Get("UI/Designators/Tame")),
				(TameMode.Release, null, "DesignatorReleaseAnimalToWild".Translate().ToString(), ContentFinder<Texture2D>.Get("UI/Designators/ReleaseToTheWild")),
			};
			Utility.BuildFloatMenuOption(instance, ev, floatMenuList, (mode, thingDef) =>
			{
				tameMode = mode;
				if (instance is Designator_Tame instanceTame)
				{
					if (mode == TameMode.Release)
					{
						instanceTame.soundSucceeded = SoundDefOf.Designate_ReleaseToWild;
						variableIcon = ContentFinder<Texture2D>.Get("UI/Designators/ReleaseToTheWild");
					}
					else
					{
						instanceTame.soundSucceeded = SoundDefOf.Designate_Claim;
						variableIcon = ContentFinder<Texture2D>.Get("UI/Designators/Tame");
					}
				}
			});
		}
	}
}
