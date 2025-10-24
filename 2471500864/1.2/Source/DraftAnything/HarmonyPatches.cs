using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

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
					Log.Message("__instance: " + __instance + " - " + __instance.IsColonistPlayerControlled);
					if (!__instance.IsColonistPlayerControlled)
                    {
						if (__instance.drafter is null)
						{
							if (__instance.RaceProps.Animal)
							{
								__instance.equipment = new Pawn_EquipmentTracker(__instance);
							}
							__instance.drafter = new Pawn_DraftController(__instance);
							Log.Message("0 __instance.Drafted: " + __instance.Drafted);
							__instance.drafter.Drafted = true;
							Log.Message("0 __instance.Drafted: " + __instance.Drafted);
							comp.isControled = true;
						}
						else
						{
							Log.Message("1 __instance.Drafted: " + __instance.Drafted);
							__instance.drafter.Drafted = false;
							Log.Message("1 __instance.Drafted: " + __instance.Drafted);

							if (!__instance.IsColonistPlayerControlled)
							{
								__instance.drafter = null;
								if (__instance.RaceProps.Animal)
								{
									__instance.equipment = null;
								}
								comp.isControled = false;
							}
						}
					}
					else
                    {
						Log.Message("2 __instance.Drafted: " + __instance.Drafted);
						__instance.drafter.Drafted = !__instance.Drafted;
						Log.Message("2 __instance.Drafted: " + __instance.Drafted);
					}
					Log.Message("__instance.drafter.Drafted: " + __instance.drafter.Drafted);
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
