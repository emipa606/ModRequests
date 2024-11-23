using RimWorld;
using HarmonyLib;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace HDream
{
	public abstract class Wish : IExposable
	{

		public WishDef def;

		public Pawn pawn;

		public MemoryThoughtHandler Memories => pawn.needs.mood.thoughts.memories;
		public int TierIndex => def.tier?.scale ?? 0;

		public int pendingTicks;
		public int ageTicks;
		public int upsetTicks;

		public int pendingCount;
		public int progressCount;
		public int regressCount;

		public bool preventDebuff = false;
		public bool preventBuff = false;

		private string cachedLabel = null;
		private string cachedDesc = null;
		private string cachedDescFulfill = null;

		protected int startDayEndChance = -1;

		public bool makeFailed = false;

		protected int actualCount = 0;

		protected int doAtTick = 0;
		public const int waitForTick = 300;

		public virtual string FormateText(string text)
		{
			return WishUtility.FormateTierKeyword(text, TierIndex).Formatted(pawn.Named("Pawn"));
		}
		public virtual string LabelCap
		{
			get
			{
				if(cachedLabel == null) cachedLabel = FormateText(def.LabelCap);
				return cachedLabel;
			}
		}
		public virtual string DescriptionToFulfill
		{
			get
			{
				if (cachedDescFulfill == null) cachedDescFulfill = FormateText(DescriptionNoDebuff + def.description);
				return cachedDescFulfill;
			}
		}
		public virtual string DescriptionTitle
		{
			get
			{
				if (cachedDesc == null)
				{
					if (def.descriptions.NullOrEmpty()) cachedDesc = "";
					else cachedDesc = FormateText(def.descriptions[Mathf.FloorToInt(Rand.Value * def.descriptions.Count)]);
				}
				return cachedDesc;
			}
		}

		public virtual string DescriptionTime
		{
			get
			{
				int ageDay = ageTicks / GenDate.TicksPerDay;
				return TranslationKey.WISH_AGE.Translate(ageDay)
					+ (startDayEndChance != -1
						? (ageDay < startDayEndChance
							? "\n" + (string)TranslationKey.WISH_WONT_END.Translate(startDayEndChance - ageDay)
							: "\n" + (string)TranslationKey.WISH_CAN_END.Translate())
						: "");
			}
		}
		public virtual string DescriptionNoDebuff
		{
			get
			{
				if (!preventDebuff || !pawn.wishes().wishes.Contains(this)) return "";
				return TranslationKey.WISH_UNREALISTIC.Translate() + "\n\n";
			}
		}

		public Wish() {}

		public Wish(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public Wish(Pawn pawn, WishDef def)
		{
			this.pawn = pawn;
			this.def = def;
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			//Scribe_Values.Look(ref cachedLabel, "cachedLabel", null);
			//Scribe_Values.Look(ref cachedDesc, "cachedDesc", null);
			//Scribe_Values.Look(ref cachedDescFulfill, "cachedDescFulfill", null);
			Scribe_Values.Look(ref pendingTicks, "pendingTicks", 0);
			Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
			Scribe_Values.Look(ref upsetTicks, "upsetTicks", 0);
			Scribe_Values.Look(ref pendingCount, "pendingCount", 0);
			Scribe_Values.Look(ref progressCount, "progressCount", 0);
			Scribe_Values.Look(ref regressCount, "regressCount", 0);
			Scribe_Values.Look(ref startDayEndChance, "startDayEndChance", -1);
			Scribe_Values.Look(ref preventDebuff, "preventDebuff", false);
			Scribe_Values.Look(ref preventBuff, "preventBuff", false);

		}

		public virtual void OnFulfill()
		{
			DoFulfill();
		}
		public virtual void DoFulfill()
		{
			if (!preventBuff)
			{ 
				if (def.fulfillTought != null)
				{
					if (def.fulfillTought.stages.Count > 1) MakeThought(def.fulfillTought, true);
					else MakeThoughtNoTier(def.fulfillTought, true);
				}
				else Log.Warning("Wish " + def.label + " for pawn " + pawn.Label + " miss a fulfillTought in the Def, you should add one");
			}
			DoRemove();
		}

		protected virtual void MakeThoughtNoTier(ThoughtDef thoughtDef, bool group)
		{
			InitThought((Thought_Wish)ThoughtMaker.MakeThought(thoughtDef), group);
		}
		protected virtual void MakeThought(ThoughtDef thoughtDef, bool group)
		{
			InitThought((Thought_Wish)ThoughtMaker.MakeThought(thoughtDef, TierIndex), group);
		}
		protected virtual void InitThought(Thought_Wish thought, bool group)
		{
			thought.fromWish = def;
			thought.groupPerWish = group;
			thought.wishDesc = LabelCap;
			Memories.TryGainMemory(thought);
		}

		public virtual void RefreshPending()
		{
			while (pendingCount > 0) RemoveOneMemoryOfDef(HDThoughtDefOf.WishPending, ref pendingCount);
		}

		protected virtual void DoPendingEffect()
		{
			pendingTicks++;
			if (!preventDebuff && pendingTicks >= (GenDate.TicksPerDay / def.upsetPerDay) * (upsetTicks + 1))
			{
				MakeThought(HDThoughtDefOf.WishPending, true);
				pendingCount++;
				upsetTicks++;
			}
			if (!def.IsPermanent() && ageTicks % GenDate.TicksPerHour == 0)
			{
				int ageHour = ageTicks / GenDate.TicksPerHour;
				if (Rand.Value <= def.endChancePerHour.Evaluate(ageHour))
				{
					DoTooOld();
				}
			}
		}
		public virtual void DoTooOld()
		{
			if(!preventDebuff) MakeThought(HDThoughtDefOf.WishTimeFail, true);
			DoRemove();
		}
		public virtual void ChangeProgress(int value)
		{
			if (value > 0)
			{
				for (int i = 0; i < value; i++)
				{
					if (regressCount > 0) RemoveOneMemoryOfDef(HDThoughtDefOf.WishRegressing, ref regressCount);
					else
					{
						progressCount++;
						if (def.progressAddThought && !preventBuff) MakeThought(HDThoughtDefOf.WishComingTrue, true);
						for (int j = 0; j < def.progressRemovePending; j++) RemoveOneMemoryOfDef(HDThoughtDefOf.WishPending, ref pendingCount);
					}
				}
			}
			else if (value < 0 && !preventDebuff)
			{
				for (int i = 0; i > value; i--)
				{
					MakeThought(HDThoughtDefOf.WishRegressing, true);
					regressCount++;
				}
			}
		}

		protected virtual void CountProgressStep(ref int oldCount, int newCount)
		{
			if (newCount == oldCount) return;
			if (newCount > oldCount)
			{
				if (regressCount > 0) ChangeProgress(Mathf.Min(Mathf.FloorToInt((newCount - (progressCount - regressCount) * def.progressStep * def.amountNeeded) / (def.progressStep * def.amountNeeded)), regressCount));
				ChangeProgress(Mathf.FloorToInt((newCount / def.amountNeeded) / (def.progressStep * (progressCount + 1f))));
			}
			else ChangeProgress(Mathf.FloorToInt((newCount - (progressCount - regressCount) * def.progressStep * def.amountNeeded) / (def.progressStep * def.amountNeeded)));
			oldCount = newCount;
		}

		public virtual void PostAdd() 
		{
			if (!def.endChancePerHour.EnumerableNullOrEmpty())
			{
				def.endChancePerHour.SortPoints();
				for (int i = 0; i < def.endChancePerHour.Count(); i++)
				{
					if (def.endChancePerHour[i].y == 0 && def.endChancePerHour[i + 1].y > 0)
						startDayEndChance = Mathf.FloorToInt(def.endChancePerHour[i].x / GenDate.HoursPerDay);
				}
			}
			preventDebuff = TierIndex > ExpectationsUtility.CurrentExpectationFor(pawn).order + 1;
		}
		public virtual void PostRemoved()
		{
			while (regressCount > 0) RemoveOneMemoryOfDef(HDThoughtDefOf.WishRegressing, ref regressCount);
			while (pendingCount > 0) RemoveOneMemoryOfDef(HDThoughtDefOf.WishPending, ref pendingCount);
			if (def.progressAddThought && !preventBuff) while (progressCount > 0) RemoveOneMemoryOfDef(HDThoughtDefOf.WishComingTrue, ref progressCount);
		}
		public void RemoveOneMemoryOfDef(ThoughtDef thoughtDef, ref int count)
		{
			if (count <= 0) return;
			
			for (int i = 0; i < Memories.Memories.Count; i++)
			{
				if (!(Memories.Memories[i] is Thought_Wish)) continue;
				Thought_Wish thought_Memory = (Thought_Wish)Memories.Memories[i];
				if (thought_Memory.def == thoughtDef && thought_Memory.CurStageIndex == TierIndex)
				{
					if (thought_Memory.fromWish != def) continue;
					Memories.RemoveMemory(thought_Memory);
					count--;
					return;
				}
			}
			Log.Warning("HDream : try to remove a thougth of def :" + thoughtDef.label + " for wish : " + def.label + " but no thought of that def was found for pawn : " + pawn.Label + "");
			count = 0;
		}
		public virtual void Tick() 
		{
			ageTicks++;
			DoPendingEffect();
		}
		protected virtual void TickToResolve()
		{
			if (Find.TickManager.TicksGame < doAtTick) return;
			doAtTick = Find.TickManager.TicksGame + waitForTick;
			CheckResolve();
		}

		protected virtual int CountMatch() { return 0; }

		protected virtual void CheckResolve() {}

		public virtual List<Texture2D> DreamIcon()
		{
			return null;
		}

		public virtual void PostTick() { }
		public virtual void PostMake() { }

		protected virtual void OnMakeFulfill()
		{
			int moodDegree = pawn.story.traits.DegreeOfTrait(TraitDefOf.NaturalMood);
			if ((moodDegree < 0 && Rand.Value > def.naturalMoodBaseKeepChance + def.lowNaturalMoodChanceImpact * -moodDegree)
				|| (moodDegree >= 0 && Rand.Value > def.naturalMoodBaseKeepChance + def.highNaturalMoodChanceImpact * moodDegree))
			{
				DoRemove();
			}
		}

		public virtual void DoRemove()
		{
			pawn.wishes().RemoveWish(this);
		}

		public virtual void MakeFailed()
		{
			Log.Warning("HDream : Wish creation failed for " + ToString() + " of def " + def.defName + " for pawn " + pawn.Label + "! Wish will be deleted!");
			makeFailed = true;
			DoRemove();
		}

	}
}
