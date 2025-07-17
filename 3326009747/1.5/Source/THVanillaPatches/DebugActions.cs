using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LudeonTK;
using RimWorld;
using Verse;

namespace THVanillaPatches
{
	public static class DebugActions
	{
		[DebugAction("Pawns", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static void RemoveAbility(Pawn p)
		{
			List<DebugMenuOption> debugActionNodeList = new List<DebugMenuOption>();
			debugActionNodeList.Add(new DebugMenuOption("*All", DebugMenuOptionMode.Action, delegate
			{
				if (p.abilities == null)
					return;
				foreach (AbilityDef allDef in DefDatabase<AbilityDef>.AllDefs)
					p.abilities.RemoveAbility(allDef);
			}));
			foreach (AbilityDef allDef in p.abilities.abilities.Select(ability => ability.def))
			{
				AbilityDef localAb = allDef;
				debugActionNodeList.Add(new DebugMenuOption(allDef.label, DebugMenuOptionMode.Action,
					delegate { p.abilities?.RemoveAbility(localAb);}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(debugActionNodeList));
		}
		
		
		[DebugAction("Vanilla Patches", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static void HalvePawn(Pawn p)
		{
			foreach (string partDefLabel in new[]
			         {
				         "Arm",
				         "Leg",
				         "Eye",
				         "Ear",
				         "Kidney",
				         "Lung"
			         })
			{
				p.health.AddHediff(HediffDefOf.MissingBodyPart, p.RaceProps.body.GetPartsWithDef(DefDatabase<BodyPartDef>.GetNamed(partDefLabel)).RandomElement());
			}

			Hediff brainrot = HediffMaker.MakeHediff(HediffDefOf.SurgicalCut, p, p.health.hediffSet.GetBrain());
			brainrot.Severity = p.health.hediffSet.GetBrain().def.hitPoints / 2f;
			brainrot.TryGetComp<HediffComp_GetsPermanent>().IsPermanent = true;
			p.health.AddHediff(brainrot, p.health.hediffSet.GetBrain());
		}
		
		
		
		[DebugAction("Vanilla Patches", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static void QuarterPawn(Pawn p)
		{
			foreach (string partDefLabel in new[]
			         {
				         "Arm",
				         "Leg",
				         "Eye",
				         "Ear",
				         "Kidney",
				         "Lung"
			         })
			{

				List<BodyPartRecord> parts = p.RaceProps.body.GetPartsWithDef(DefDatabase<BodyPartDef>.GetNamed(partDefLabel));

				if (parts.Count == 2)
				{
					if (Rand.Bool)
					{
						p.health.AddHediff(HediffDefOf.MissingBodyPart, parts[0]);
						RemoveAmountFromPart(parts[1], 0.5f);
					}
					else
					{
						p.health.AddHediff(HediffDefOf.MissingBodyPart, parts[1]);
						RemoveAmountFromPart(parts[0], 0.5f);
					}
					
					
				}
				else if (parts.Count >= 1)
				{
					RemoveAmountFromPart(parts[0], 0.75f);
				}
			}

			RemoveAmountFromPart(p.health.hediffSet.GetBrain(), 0.75f);
			return;

			void RemoveAmountFromPart(BodyPartRecord part, float percentToRemove)
			{
				Hediff damage = HediffMaker.MakeHediff(HediffDefOf.SurgicalCut, p, part);
				damage.Severity = part.def.hitPoints * percentToRemove;
				damage.TryGetComp<HediffComp_GetsPermanent>().IsPermanent = true;
				p.health.AddHediff(damage, part);
			}
		}
		
		
		[DebugAction("Vanilla Patches", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
		private static void ProgressDisappearsAtTotalTendQualityDiseasesByFive(Pawn p)
		{
			foreach (Hediff hediff in p.health.hediffSet.hediffs)
			{
				
				
				if (hediff.TryGetComp(out HediffComp_TendDuration duration) &&
				    duration.TProps.disappearsAtTotalTendQuality > 0)
				{
					FieldInfo prop = duration.GetType().GetField("totalTendQuality", BindingFlags.NonPublic
					                                                           | BindingFlags.Instance);
					prop?.SetValue(duration, (float)prop.GetValue(duration) + 0.05f);
				}
			}
		}
	}
}