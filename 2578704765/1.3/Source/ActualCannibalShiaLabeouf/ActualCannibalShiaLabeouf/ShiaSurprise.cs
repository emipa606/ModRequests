using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ActualCannibalShiaLabeouf
{
	public static class Consts_ShiaSurprise
	{
		public const string path = "Things/Pawn/Humanlike/Heads/Male/Male_Narrow_Wide";
		public const string letterLabel = "Shia Surprise!";
		public const string letterText = "You're walking in the woods. There's no one around and your phone is dead. Out of the corner of your eye, you spot him: Shia Labeouf!";
	};
	
	[DefOf]
	public static class DefOfs_ShiaSurprise
	{
		public static FactionDef HollywoodCannibals;
		public static PawnKindDef ShiaKind;
		public static BodyTypeDef Male;
		public static PawnsArrivalModeDef EdgeWalkIn;
		public static LetterDef ThreatSmall;
		public static TaleDef Raid;
	};

	[StaticConstructorOnStartup]
	public class IncidentWorker_ShiaSurprise : IncidentWorker
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			// Log.Message("Shia TryExecuteWorker Launched");
			parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
			parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			parms.points = 50f;
			bool flag = !this.TryResolveRaidFaction(parms);
			bool result;
			if (flag)
			{
				Log.Error("Failed to resolve shia raid faction");
				result = false;
			}
			else
			{
				Find.TickManager.slower.SignalForceNormalSpeedShort();
				Find.StoryWatcher.statsRecord.numRaidsEnemy++;
				List<Pawn> list = new List<Pawn>();
				// Log.Message("Trying to make a shia");
				Pawn shia = this.GenerateShia(parms);
				list.Add(shia);
				// Log.Message("Made a shia");
				bool flag2 = list.Count == 0;
				if (flag2)
				{
					Log.Error("Got no pawns spawning raid from parms " + ((parms != null) ? parms.ToString() : null));
					result = false;
				}
				else
				{
					bool flag3 = !parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms);
					if (flag3)
					{
						Log.Error("Failed to resolve raid spawn center");
						result = false;
					}
					else
					{
						TargetInfo target = shia;
						parms.raidArrivalMode.Worker.Arrive(list, parms);
						PawnComponentsUtility.AddComponentsForSpawn(shia);
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendLine("Points = " + parms.points.ToString("F0"));
						foreach (Pawn current2 in list)
						{
							string str = (current2.equipment == null || current2.equipment.Primary == null) ? "unarmed" : current2.equipment.Primary.LabelCap;
							stringBuilder.AppendLine(current2.KindLabel + " - " + str);
						}
						string pawnRelationsInfo;
						PawnRelationUtility.Notify_PawnsSeenByPlayer(list, out pawnRelationsInfo, false, true);
						Find.LetterStack.ReceiveLetter("Shia Surprise!", "You're walking in the woods. There's no one around and your phone is dead. Out of the corner of your eye, you spot him: Shia Labeouf!", DefOfs_ShiaSurprise.ThreatSmall, target, parms.faction, null, null, stringBuilder.ToString());
						TaleRecorder.RecordTale(DefOfs_ShiaSurprise.Raid, new object[0]);
						parms.raidStrategy.Worker.MakeLords(parms, list);
						LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
						SoundDef.Named("Shia").PlayOneShot(SoundInfo.OnCamera(MaintenanceType.None));
						result = true;
					}
				}
			}
			return result;
		}
		protected bool TryResolveRaidFaction(IncidentParms parms)
		{
			parms.faction = Find.FactionManager.FirstFactionOfDef(DefOfs_ShiaSurprise.HollywoodCannibals);
			bool flag = parms.faction == null;
			if (flag)
			{
				Log.Message("Trying to generate new faction");
				FactionGeneratorParms shiafaction = new FactionGeneratorParms(DefOfs_ShiaSurprise.HollywoodCannibals, default(IdeoGenerationParms), true);
				Faction newfac = FactionGenerator.NewGeneratedFaction(shiafaction);
				Find.FactionManager.Add(newfac);
				parms.faction = newfac;
				bool flag2 = newfac != null && newfac.def == DefOfs_ShiaSurprise.HollywoodCannibals;
				if (!flag2)
				{
					return false;
				}
				Log.Message("Generated successfully");
			}
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}
		private Pawn GenerateShia(IncidentParms parms)
		{
			bool flag = parms.faction == null || PawnKindDef.Named("ShiaKind") == null;
			if (flag)
			{
				Log.Error("shia mod cant find faction");
			}
			// Log.Message("Request");
			PawnGenerationRequest request = new PawnGenerationRequest(DefOfs_ShiaSurprise.ShiaKind, parms.faction, PawnGenerationContext.NonPlayer, parms.target.Tile, true, false, false, false, false, true, 0f, true, false, false, false, true, false, false, false, 0f, 0f, null, 1f, null, null, null, null, new float?(0f), new float?((float)31), new float?((float)31), new Gender?(Gender.Male), new float?(0.2691779f), "LaBeouf", null, null);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			pawn.health = new Pawn_HealthTracker(pawn);
			pawn.story.traits = new TraitSet(pawn);
			pawn.story.hairColor = Color.black;
			pawn.Name = new NameTriple("Shia", "Shia", "LaBeouf");
			pawn.story.hairDef = DefDatabase<HairDef>.GetNamed("Topdog", true);
			pawn.style.beardDef = DefDatabase<BeardDef>.GetNamed("Shia", true);
			pawn.story.bodyType = DefOfs_ShiaSurprise.Male;
			typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, "Things/Pawn/Humanlike/Heads/Male/Male_Narrow_Wide");
			pawn.story.crownType = CrownType.Average;
			// Log.Message("Setting skills");
			foreach (SkillRecord current3 in pawn.skills.skills)
			{
				bool flag2 = current3.def == SkillDefOf.Melee;
				if (flag2)
				{
					current3.Level = 20;
				}
			}
			string childB = "ChildStar74";
			string adultB = "Actor72";
			Backstory bsc;
			BackstoryDatabase.TryGetWithIdentifier(childB, out bsc, true);
			pawn.story.childhood = bsc;
			Backstory bsa;
			BackstoryDatabase.TryGetWithIdentifier(adultB, out bsa, true);
			pawn.story.adulthood = bsa;
			pawn.story.traits.allTraits.RemoveAll((Trait t) => true);
			pawn.story.traits.allTraits.Add(new Trait(TraitDefOf.Cannibal, 0, false));
			pawn.story.traits.allTraits.Add(new Trait(TraitDefOf.Bloodlust, 0, false));
			pawn.story.traits.allTraits.Add(new Trait(TraitDefOf.Psychopath, 0, false));
			return pawn;
		}
	}
}
