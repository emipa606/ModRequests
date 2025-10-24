using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DraftAnything
{

	public class CompProperties_ControlPawn : CompProperties
    {
		public CompProperties_ControlPawn()
        {
			this.compClass = typeof(CompControlPawn);
        }
	}

	public class CompControlPawn : ThingComp
    {
		public bool isControled;
		public bool shouldDisableSkills;
		public bool shouldDisableStory;
		public bool shouldDisableRoyalty;
		public static Dictionary<Pawn, CompControlPawn> instances = new Dictionary<Pawn, CompControlPawn>();
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			instances[this.parent as Pawn] = this;
		}
        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Values.Look(ref isControled, "isControled");
			Scribe_Values.Look(ref shouldDisableSkills, "shouldDisableSkills");
			Scribe_Values.Look(ref shouldDisableStory, "shouldDisableStory");
			Scribe_Values.Look(ref shouldDisableRoyalty, "shouldDisableRoyalty");
		}
	}


	[StaticConstructorOnStartup]
	internal static class HarmonyInit
	{
		static HarmonyInit()
		{
			Harmony harmony = new Harmony("Hamrick.DraftAnything");
			harmony.PatchAll();
			foreach (var pawnDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null))
            {
				pawnDef.comps.Add(new CompProperties_ControlPawn());
            }
		}
	}

	[HarmonyPatch(typeof(PawnRelationUtility), "GetMostImportantColonyRelative")]
	public static class GetMostImportantColonyRelative_Patch
	{
		private static Exception Finalizer(Exception __exception)
		{
			if (__exception != null)
			{
				return null;
			}
			return null;
		}
	}

	[HarmonyPatch(typeof(Pawn_HealthTracker), "NotifyPlayerOfKilled")]
	public static class NotifyPlayerOfKilled_Patch
	{
		private static Exception Finalizer(Exception __exception)
		{
			if (__exception != null)
			{
				return null;
			}
			return null;
		}
	}

	[HarmonyPatch(typeof(MusicManagerPlay), "AppropriateNow")]
	public static class AppropriateNow_Patch
	{
		private static Exception Finalizer(Exception __exception)
		{
			if (__exception != null)
			{
				return null;
			}
			return null;
		}
	}

	[HarmonyPatch(typeof(Pawn), "IsColonist", MethodType.Getter)]
	public static class IsColonist_Patch
	{
		private static void Postfix(Pawn __instance, ref bool __result)
		{
			if (CompControlPawn.instances.TryGetValue(__instance, out var comp) && comp.isControled)
			{
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(Thing), "Faction", MethodType.Getter)]
	public static class Faction_Patch
	{
		public static bool setToPlayer;
		private static void Postfix(Pawn __instance, ref Faction __result)
		{
			if (setToPlayer && CompControlPawn.instances.TryGetValue(__instance, out var comp) && comp.isControled)
			{
				__result = Faction.OfPlayer;
			}
		}
	}

	[HarmonyPatch(typeof(Pawn), "GetGizmos")]
	public class Pawn_GetGizmos_Patch
	{
		public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
		{
			if (CompControlPawn.instances.TryGetValue(__instance, out var comp))
			{
				Command_Toggle command_Toggle = new Command_Toggle();
				command_Toggle.hotKey = KeyBindingDefOf.Command_ColonistDraft;
				command_Toggle.isActive = (() => __instance.Drafted);
				command_Toggle.toggleAction = delegate
				{
					if (!__instance.IsColonistPlayerControlled)
                    {
                        if (__instance.drafter is null)
                        {
                            if (__instance.RaceProps.Animal)
							{
								__instance.equipment = new Pawn_EquipmentTracker(__instance);
							}
							__instance.drafter = new Pawn_DraftController(__instance);
							__instance.drafter.Drafted = true;
							comp.isControled = true;
						}
						else
						{
							__instance.drafter.Drafted = false;
							if (!__instance.IsColonistPlayerControlled)
							{
								__instance.drafter = null;
                                if (__instance.RaceProps.Animal)
								{
									__instance.equipment = null;
                                }
                                if (comp.shouldDisableSkills)
                                {
                                    __instance.skills = null;
                                }
								if (comp.shouldDisableStory)
                                {
									__instance.story = null;
                                }
								if (comp.shouldDisableRoyalty)
                                {
									__instance.royalty = null;
                                }
                                comp.isControled = false;
							}
						}
					}
					else
                    {
						__instance.drafter.Drafted = !__instance.Drafted;
					}

                    if (comp.isControled)
                    {
                        if (__instance.skills is null)
                        {
                            __instance.skills = new Pawn_SkillTracker(__instance);
                            comp.shouldDisableSkills = true;
                        }
                        if (__instance.story is null)
                        {
                            __instance.story = new Pawn_StoryTracker(__instance);
							comp.shouldDisableStory = true;
                        }
						if (__instance.royalty is null)
                        {
							__instance.royalty = new Pawn_RoyaltyTracker(__instance);
                        }
                    }
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Drafting, KnowledgeAmount.SpecificInteraction);
					if (__instance.drafter.Drafted)
					{
						LessonAutoActivator.TeachOpportunity(ConceptDefOf.QueueOrders, OpportunityType.GoodToKnow);
					}
				};
				command_Toggle.defaultDesc = "CommandToggleDraftDesc".Translate();
				command_Toggle.icon = TexCommand.Draft;
				command_Toggle.turnOnSound = SoundDefOf.DraftOn;
				command_Toggle.turnOffSound = SoundDefOf.DraftOff;
				command_Toggle.groupKey = 81729172;
				command_Toggle.defaultLabel = (__instance.Drafted ? "CommandUndraftLabel" : "CommandDraftLabel").Translate();
				if (__instance.Downed)
				{
					command_Toggle.Disable("IsIncapped".Translate(__instance.LabelShort, __instance));
				}
				if (!__instance.Drafted)
				{
					command_Toggle.tutorTag = "Draft";
				}
				else
				{
					command_Toggle.tutorTag = "Undraft";
				}
				yield return command_Toggle;

				if (comp.isControled)
                {
					Faction_Patch.setToPlayer = true;
				}
			}

			foreach (var g in __result)
			{
				if (g is Command_Toggle command && command.defaultDesc == "CommandToggleDraftDesc".Translate())
				{
					continue;
				}
				yield return g;
			}

			Faction_Patch.setToPlayer = false;
		}
	}
}
