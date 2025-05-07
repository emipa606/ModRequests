using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace DTechprinting
{
    public static class ShardApplier
    {

		public static void ApplyTechshards(ResearchProjectDef proj, Pawn applyingPawn, int amount)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("LetterTechshardAppliedPartIntro".Translate(amount, proj.Named("PROJECT")));
			stringBuilder.AppendLine();
			string letter = "";
			if (ApplyShardsToUnlock(proj, amount, stringBuilder, applyingPawn))
			{
				return;
			}
			else if (!TechprintingSettings.enableUnlockedTechPrinting)
			{
				stringBuilder.AppendLine("LetterTechshardAppliedPartAlreadyResearched".Translate(proj.Named("PROJECT")));
				stringBuilder.AppendLine();
				letter = "LetterTechsardNoEffectLabel".Translate(amount, proj.Named("PROJECT"));
			}
			else if (proj.IsFinished)
			{
				stringBuilder.AppendLine("LetterTechshardAppliedPartResearchDisabled".Translate(proj.Named("PROJECT")));
				stringBuilder.AppendLine();
				letter = "LetterTechsardNoEffectLabel".Translate(amount, proj.Named("PROJECT"));
			}
			else if (ApplyShardsToResearch(proj, amount, stringBuilder, applyingPawn))
			{
				return;
			}
			if (stringBuilder.Length > 0)
			{
				Find.LetterStack.ReceiveLetter(letter, stringBuilder.ToString().TrimEndNewlines(), LetterDefOf.PositiveEvent, null);
			}

		}

		public static void AddTechshards(ResearchProjectDef proj, int amount)
		{
			if (Current.Game == null)
				return;
			if (Current.Game.researchManager == null)
				return;
			if (proj.techprintCount == 0)
				return;

			FieldInfo techfield = AccessTools.Field(typeof(ResearchManager), "techprints");
			Dictionary<ResearchProjectDef, int> techprints = techfield.GetValue(Current.Game.researchManager) as Dictionary<ResearchProjectDef, int>;

			int num;
			if (techprints.TryGetValue(proj, out num))
			{
				num += amount;
				if (num > proj.techprintCount)
				{
					num = proj.techprintCount;
				}
				techprints[proj] = num;
				return;
			}
			techprints.Add(proj, amount);
		}

		public static ResearchProjectDef GetUnlockablePrereq(ResearchProjectDef rpd)
		{
			Queue<ResearchProjectDef> q = new Queue<ResearchProjectDef>();
			// try to find an unlockable prereq with its prereqs finished
			q.Enqueue(rpd);
			while (q.Count > 0)
			{
				ResearchProjectDef candidate = q.Dequeue();
				if (!candidate.TechprintRequirementMet && !candidate.IsFinished && candidate.PrerequisitesCompleted)
					return candidate;
				if (candidate.prerequisites != null)
					candidate.prerequisites.ForEach(prereq => q.Enqueue(prereq));
				if (candidate.hiddenPrerequisites != null)
					candidate.hiddenPrerequisites.ForEach(prereq => q.Enqueue(prereq));
			}
			// failing the above, try to find an unlockable prereq with no locked prereqs
			q.Enqueue(rpd);
			while (q.Count > 0)
			{
				ResearchProjectDef candidate = q.Dequeue();
				if (!candidate.TechprintRequirementMet && !candidate.IsFinished)
				{ // if we're here, the candidate does NOT have its prereqs finished, or above loop would have found it
					if 
					(		(candidate.prerequisites ?? Enumerable.Empty<ResearchProjectDef>()).All(p => p.TechprintRequirementMet)
						&&	(candidate.hiddenPrerequisites ?? Enumerable.Empty<ResearchProjectDef>()).All(p => p.TechprintRequirementMet)
					)
					{
						return candidate;
					}
				}
				if (candidate.prerequisites != null)
					candidate.prerequisites.ForEach(prereq => q.Enqueue(prereq));
				if (candidate.hiddenPrerequisites != null)
					candidate.hiddenPrerequisites.ForEach(prereq => q.Enqueue(prereq));
			}
			return null;
		}

		public static ResearchProjectDef FindBestProjectToUnlock(ResearchProjectDef proj)
		{
			if (!proj.TechprintRequirementMet)
			{
				return proj;
			}
			else if (TechprintingSettings.enablePrereqUnlocks)
			{
				return GetUnlockablePrereq(proj);
			}
			return null;
		}

		public static void AdvanceShardReq(ResearchProjectDef unlockTarget, int numShards, ResearchProjectDef shardProject)
		{
			int weightedShards = Mathf.RoundToInt(Mathf.Max(GetShardRatio(shardProject, unlockTarget) * numShards, 1));
			ShardApplier.AddTechshards(unlockTarget, weightedShards);
		}

		public static bool ApplyShardsToUnlock(ResearchProjectDef proj, int numShards, StringBuilder sb = null, Pawn applyingPawn = null)
		{
			ResearchProjectDef unlockTarget = FindBestProjectToUnlock(proj);
			if (unlockTarget == null)
			{
				return false;
			}

			if (applyingPawn != null)
				RefundUnlock(unlockTarget, applyingPawn, numShards);
			AdvanceShardReq(unlockTarget, numShards, proj);


			if (sb == null)
				sb = new StringBuilder();
			bool unlocked = false;
			string letter;
			if (unlockTarget.techprintCount <= Current.Game.researchManager.GetTechprints(unlockTarget))
			{
				sb.AppendLine("LetterTechshardAppliedPartJustUnlocked".Translate(unlockTarget.Named("PROJECT")));
				sb.AppendLine();
				unlocked = true;
				letter = "LetterTechshardAppliedLabel".Translate(numShards, unlockTarget.Named("PROJECT"));
			}
			else
			{
				sb.AppendLine("LetterTechshardAppliedPartNotUnlockedYet".Translate(Current.Game.researchManager.GetTechprints(unlockTarget), unlockTarget.techprintCount.ToString(), unlockTarget.Named("PROJECT")));
				sb.AppendLine();
				letter = "LetterTechshardAppliedLabel".Translate(numShards, unlockTarget.Named("PROJECT"));
			}
			Find.LetterStack.ReceiveLetter(letter, sb.ToString().TrimEndNewlines(), LetterDefOf.PositiveEvent, null);
			if (unlocked)
			{
				Find.LetterStack.ReceiveLetter("LetterTechshardUnlockedLabel".Translate(unlockTarget.LabelCap.Named("PROJECT")), "LetterTechshardAppliedPartJustUnlocked".Translate(unlockTarget.Named("PROJECT")), LetterDefOf.PositiveEvent, null);
				Base.UpdateTechshardRecipes();
			}
			return true;
		}

		public static ResearchProjectDef GetResearchablePrereq(ResearchProjectDef rpd)
		{
			Queue<ResearchProjectDef> q = new Queue<ResearchProjectDef>();
			q.Enqueue(rpd);
			while (q.Count > 0)
			{
				ResearchProjectDef candidate = q.Dequeue();
				if (candidate.PrerequisitesCompleted && candidate.TechprintRequirementMet && !candidate.IsFinished)
					return candidate;
				if (candidate.prerequisites != null)
					candidate.prerequisites.ForEach(prereq => q.Enqueue(prereq));
				if (candidate.hiddenPrerequisites != null)
					candidate.hiddenPrerequisites.ForEach(prereq => q.Enqueue(prereq));
			}
			return null;
		}

		public static ResearchProjectDef FindBestProjectToAdvance(ResearchProjectDef proj)
		{
			if (!TechprintingSettings.enableUnlockedTechPrinting)
				return null;
			else if (proj.PrerequisitesCompleted && !proj.IsFinished)
				return proj;
			else if (TechprintingSettings.enablePrereqResearch)
				return GetResearchablePrereq(proj);
			return null;
		}

		public static float GetShardRatio(ResearchProjectDef shardProj, ResearchProjectDef appliedProj)
		{
			if (!Base.researchDic.ContainsKey(appliedProj))
				return 1f;
			float shardValue = ShardMaker.RequiredValueForOneShard(shardProj);
			float appliedValue = ShardMaker.RequiredValueForOneShard(appliedProj);
			return shardValue / appliedValue;
		}

		public static int ConvertShardsToProgress(ResearchProjectDef proj, int numShards, ResearchProjectDef shardProj)
		{
			float ratio = GetShardRatio(shardProj, proj);
			return Mathf.RoundToInt(Mathf.Max(numShards * (proj.baseCost / 200f) * ratio, 1)); // 0.5% progress per shard (100% progress for 200), modified by ratio
		}

		public static void AdvanceResearch(ResearchProjectDef proj, int xpGained)
		{
			FieldInfo techfield = AccessTools.Field(typeof(ResearchManager), "progress");
			Dictionary<ResearchProjectDef, float> progress = techfield.GetValue(Current.Game.researchManager) as Dictionary<ResearchProjectDef, float>;

			float currentXP;
			if (!progress.TryGetValue(proj, out currentXP))
			{
				progress.Add(proj, Mathf.Min(xpGained, proj.baseCost));
			}
			else
			{
				progress[proj] = Mathf.Min(currentXP + xpGained, proj.baseCost);
			}
		}

		public static bool ApplyShardsToResearch(ResearchProjectDef proj, int numShards, StringBuilder sb = null, Pawn applyingPawn = null)
		{
			ResearchProjectDef xpTarget = FindBestProjectToAdvance(proj);
			if (xpTarget == null)
			{
				return false;
			}
			int xpGained = ConvertShardsToProgress(xpTarget, numShards, proj);
			if (applyingPawn != null)
				RefundXP(xpTarget, applyingPawn, proj, numShards, xpGained);
			AdvanceResearch(xpTarget, xpGained);

			if (sb == null)
				sb = new StringBuilder();
			sb.AppendLine("LetterTechshardAppliedPartAlreadyUnlocked".Translate(xpGained, xpTarget.Named("PROJECT"))); ;
			sb.AppendLine();
			string letter = "LetterTechshardAppliedToResearchLabel".Translate(xpGained, xpTarget.Named("PROJECT"));
			Find.LetterStack.ReceiveLetter(letter, sb.ToString().TrimEndNewlines(), LetterDefOf.PositiveEvent, null);
			if (xpTarget.IsFinished)
			{
				Find.ResearchManager.FinishProject(xpTarget, doCompletionDialog: false, researcher: applyingPawn);
				Find.LetterStack.ReceiveLetter("LetterTechshardResearchedLabel".Translate(xpTarget.LabelCap.Named("PROJECT")), "LetterTechshardAppliedPartJustResearched".Translate(xpTarget.LabelCap.Named("PROJECT")), LetterDefOf.PositiveEvent, null);
				Base.UpdateTechshardRecipes();
			}
			
			return true;
		}

		public static void RefundUnlock(ResearchProjectDef proj, Pawn applyingPawn, int shardsApplied)
		{
			if (applyingPawn == null || !Base.researchDic.ContainsKey(proj))
				return;
			int shardsRequired = proj.techprintCount - Current.Game.researchManager.GetTechprints(proj);
			if (shardsRequired > 0)
			{
				int shardRefund = shardsApplied - shardsRequired;
				if (shardRefund > 0)
				{
					ThingDef techShardDef = ShardMaker.ShardForProject(proj);
					Thing shards = ThingMaker.MakeThing(techShardDef);
					shards.stackCount = shardRefund;
					GenPlace.TryPlaceThing(shards, applyingPawn.Position, applyingPawn.Map, ThingPlaceMode.Near, null, null, default(Rot4));
				}
			}

		}

		public static void RefundXP(ResearchProjectDef proj, Pawn applyingPawn, ResearchProjectDef shardType, int shardsApplied, float xpGained)
		{
			if (applyingPawn == null || !Base.researchDic.ContainsKey(shardType))
				return;
			float xpRequired = proj.baseCost - proj.ProgressReal;
			if (xpRequired >= 0)
			{
				int shardRefund = shardsApplied - Mathf.RoundToInt((float)shardsApplied * (xpRequired / xpGained));
				if (shardRefund > 0)
				{
					ThingDef techShardDef = ShardMaker.ShardForProject(shardType);
					Thing shards = ThingMaker.MakeThing(techShardDef);
					shards.stackCount = shardRefund;
					GenPlace.TryPlaceThing(shards, applyingPawn.Position, applyingPawn.Map, ThingPlaceMode.Near, null, null, default(Rot4));
				}
			}

		}

	}
}
