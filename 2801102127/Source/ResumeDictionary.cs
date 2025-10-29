using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ResumableJobProgress
{
	class ResumeDictionary
	{
		// singleton
		private ResumeDictionary() { }
		private static ResumeDictionary _instance;

		public static ResumeDictionary Instance => _instance;

		static ResumeDictionary()
		{
			_instance = new ResumeDictionary();
		}

		class ResumeJob : IExposable
		{
			private TargetInfo keyTarget;
			private Def keyDef;

			private Map keyMapTemp;
			private IntVec3 keyCellTemp;
			private ThingDef targetThingDefTemp;
			private JobDef jobDefTemp;
			private RecipeDef recipeDefTemp;

			private float valueWorkAmount;
			private float valueStatIntegral;

			public TargetInfo Target => keyTarget;
			public Def Def => keyDef;
			public float WorkAmount => valueWorkAmount;
			public float StatIntegral => valueStatIntegral;

			public Map MapTemp => keyMapTemp;
			public IntVec3 CellTemp => keyCellTemp;

			public ResumeJob()
			{
			}

			public ResumeJob(TargetInfo target, Def def, float workAmount, float efficiencyIntegral)
			{
				keyTarget = target;
				keyDef = def;
				valueWorkAmount = workAmount;
				valueStatIntegral = efficiencyIntegral;
			}

			public ResumeJob(Map map, IntVec3 cell, ThingDef thingDef, Def def, float workAmount, float efficiencyIntegral)
			{
				keyTarget = new TargetInfo(cell, map);
				keyDef = def;
				valueWorkAmount = workAmount;
				valueStatIntegral = efficiencyIntegral;

				targetThingDefTemp = thingDef;
			}

			public bool HasTempThing => targetThingDefTemp is not null;
			public ThingDef TempThing => targetThingDefTemp;

			public void ExposeData()
			{
				if (Scribe.mode == LoadSaveMode.LoadingVars)
				{
					keyMapTemp = null;
					keyCellTemp = IntVec3.Invalid;
					targetThingDefTemp = null;
					jobDefTemp = null;
					recipeDefTemp = null;
				}
				else if (Scribe.mode == LoadSaveMode.Saving)
				{
					keyMapTemp = keyTarget.Map;
					keyCellTemp = keyTarget.Cell;
					targetThingDefTemp = keyTarget.Thing?.def;
					jobDefTemp = keyDef as JobDef;
					recipeDefTemp = keyDef as RecipeDef;
				}

				Scribe_References.Look(ref keyMapTemp, "targetMap");
				Scribe_Values.Look(ref keyCellTemp, "targetCell");
				Scribe_Defs.Look(ref targetThingDefTemp, "targetThingDef");

				Scribe_Defs.Look(ref jobDefTemp, "jobDef");
				Scribe_Defs.Look(ref recipeDefTemp, "recipeDef");

				Scribe_Values.Look(ref valueWorkAmount, "workAmount");
				Scribe_Values.Look(ref valueStatIntegral, "efficiencyIntegral");

				if (Scribe.mode == LoadSaveMode.PostLoadInit)
				{
					if (keyMapTemp is null)
					{
						Log.Message("[ResumeJobProgress] keyMapTemp is null.");
						return;
					}
					keyTarget = new TargetInfo(keyCellTemp, keyMapTemp);
					if (recipeDefTemp is not null)
					{
						keyDef = recipeDefTemp;
					}
					else if (jobDefTemp is not null)
					{
						keyDef = jobDefTemp;
					}
				}
			}
		}

		class ResumeBill : IExposable
		{
			private RecipeDef keyRecipeDef;
			private Building_WorkTable keyWorkTable;

			private List<ThingDef> valueIngredients;
			private float valueWorkAmount;

			public (RecipeDef, Building_WorkTable) Key => (keyRecipeDef, keyWorkTable);
			public (List<ThingDef>, float) Value => (valueIngredients, valueWorkAmount);

			public ResumeBill()
			{
			}

			public ResumeBill(RecipeDef recipe, Building_WorkTable workTable, List<ThingDef> ingredients, float workAmount)
			{
				keyRecipeDef = recipe;
				keyWorkTable = workTable;
				valueIngredients = ingredients;
				valueWorkAmount = workAmount;
			}

			public void ExposeData()
			{
				Scribe_Defs.Look(ref keyRecipeDef, "recipeDef");
				Scribe_References.Look(ref keyWorkTable, "workTable");
				Scribe_Collections.Look(ref valueIngredients, "ingredients", LookMode.Def);
				Scribe_Values.Look(ref valueWorkAmount, "workAmount");
			}
		}

		private Dictionary<(TargetInfo target, Def def), (float, float)> resumeJobWorkLefts = [];

		public bool GetResumeWorkLeft(Map map, IntVec3 cell, Def def, out float workLeft, out float efficiencyIntegral)
		{
			return GetResumeWorkLeft(new TargetInfo(cell, map), def, out workLeft, out efficiencyIntegral);
		}

		public bool GetResumeWorkLeft(Thing thing, Def def, out float workLeft, out float efficiencyIntegral)
		{
			return GetResumeWorkLeft(new TargetInfo(thing), def, out workLeft, out efficiencyIntegral);
		}

		public bool GetResumeWorkLeft(TargetInfo target, Def def, out float workLeft, out float efficiencyIntegral)
		{
			if (resumeJobWorkLefts.TryGetValue((target, def), out (float workLeft, float efficiencyIntegral) value))
			{
				efficiencyIntegral = value.efficiencyIntegral;
				workLeft = value.workLeft;
				return true;
			}
			workLeft = -1f;
			efficiencyIntegral = 0f;
			return false;
		}

		public void SetResumeWorkLeft(Map map, IntVec3 cell, Def def, float workLeft, float efficiency = 0f)
		{
			SetResumeWorkLeft(new TargetInfo(cell, map), def, workLeft, efficiency);
		}

		public void SetResumeWorkLeft(Thing thing, Def def, float workLeft, float efficiency = 0f)
		{
			SetResumeWorkLeft(new TargetInfo(thing), def, workLeft, efficiency);
		}

		public void SetResumeWorkLeft(TargetInfo target, Def def, float workLeft, float efficiency = 0f, float baseWorkAmount = float.MaxValue)
		{
			if (!GetResumeWorkLeft(target, def, out float workLeftOld, out float efficiencyIntegral))
			{
				workLeftOld = baseWorkAmount;
			}
			var statIntegralDelta = (workLeftOld - workLeft) * efficiency;
			resumeJobWorkLefts[(target, def)] = (workLeft, efficiencyIntegral + statIntegralDelta);
		}

		public bool PopResumeWorkLeft(Map map, IntVec3 cell, Def def, out float workLeft, out float efficiency, float baseWorkAmount = float.MaxValue)
		{
			return PopResumeWorkLeft(new TargetInfo(cell, map), def, out workLeft, out efficiency, baseWorkAmount);
		}

		public bool PopResumeWorkLeft(Thing thing, Def def, out float workLeft, out float efficiency, float baseWorkAmount = float.MaxValue)
		{
			return PopResumeWorkLeft(new TargetInfo(thing), def, out workLeft, out efficiency, baseWorkAmount);
		}

		public bool PopResumeWorkLeft(TargetInfo target, Def def, out float workLeft, out float efficiency, float baseWorkAmount = float.MaxValue)
		{
			if (GetResumeWorkLeft(target, def, out workLeft, out float efficiencyIntegral))
			{
				resumeJobWorkLefts.Remove((target, def));
				efficiency = efficiencyIntegral / baseWorkAmount;
				return true;
			}
			efficiency = 0f;
			return false;
		}

		private Dictionary<(Thing, Def), (float, float)> resumeJobWorkDones = [];

		public bool GetResumeWorkDone(Thing thing, Def def, out float workAmount, out float efficiencyIntegral)
		{
			if (resumeJobWorkDones.TryGetValue((thing, def), out (float workAmount, float efficiencyIntegral) value))
			{
				(workAmount, efficiencyIntegral) = value;
				return true;
			}
			workAmount = -1f;
			efficiencyIntegral = 0f;
			return false;
		}

		public void SetResumeWorkDone(Thing thing, Def def, float workDone, float efficiency)
		{
			if (!GetResumeWorkDone(thing, def, out float workDoneOld, out float efficiencyIntegral))
			{
				workDoneOld = 0f;
			}
			var statIntegralDelta = (workDone - workDoneOld) * efficiency;
			resumeJobWorkDones[(thing, def)] = (workDone, efficiencyIntegral + statIntegralDelta);
		}

		public bool PopResumeWorkDone(Thing thing, Def def, out float workAmount, out float efficiency, float baseWorkAmount = float.MaxValue)
		{
			var key = (thing, def);
			if (resumeJobWorkDones.TryGetValue(key, out (float workAmount, float efficiencyIntegral) value))
			{
				resumeJobWorkDones.Remove(key);
				workAmount = value.workAmount;
				efficiency = value.efficiencyIntegral / baseWorkAmount;
				return true;
			}
			workAmount = -1f;
			efficiency = 0f;
			return false;
		}

		private Dictionary<(Map, IntVec3, ThingDef), float> resumeSowJobWorkDones = [];

		public bool GetResumeSowWorkDone(Plant plant, out float sowWorkDone)
		{
			return resumeSowJobWorkDones.TryGetValue((plant.Map, plant.Position, plant.def), out sowWorkDone);
		}

		public void SetResumeSowWorkDone(Plant plant, float sowWorkDone)
		{
			resumeSowJobWorkDones[(plant.Map, plant.Position, plant.def)] = sowWorkDone;
		}

		public bool PopResumeSowWorkDone(Plant plant, out float sowWorkDone)
		{
			if (GetResumeSowWorkDone(plant, out sowWorkDone))
			{
				resumeSowJobWorkDones.Remove((plant.Map, plant.Position, plant.def));
				return true;
			}
			return false;
		}

		public void ClearOtherSowPlants(Plant plant)
		{
			var idList = resumeSowJobWorkDones.Keys.ToList();
			foreach (var id in idList)
			{
				if (plant.Map == id.Item1 && plant.Position == id.Item2)
				{
					resumeSowJobWorkDones.Remove(id);
				}
			}
		}

		private Dictionary<(RecipeDef, Building_WorkTable), (List<ThingDef>, float)> resumeBillWorkLefts = [];

		public bool GetResumeBillWorkLeft(RecipeDef recipe, Building_WorkTable workBench, List<ThingDef> ingredients, out float workLeft)
		{
			if (resumeBillWorkLefts.TryGetValue((recipe, workBench), out (List<ThingDef> ingredients, float workLeft) value))
			{
				if (!Settings.IsStrictIngredient)
				{
					workLeft = value.workLeft;
					return true;
				}
				foreach (var ingredient in ingredients)
				{
					foreach (var cookedIngredient in value.ingredients)
					{
						if (ingredient == cookedIngredient)
						{
							workLeft = value.workLeft;
							return true;
						}
					}
				}
			}
			workLeft = -1f;
			return false;
		}

		public void SetResumeBillWorkLeft(RecipeDef recipe, Building_WorkTable workBench, List<ThingDef> ingredients, float workLeft)
		{
			resumeBillWorkLefts[(recipe, workBench)] = (ingredients, workLeft);
		}

		public bool PopResumeBillWorkLeft(RecipeDef recipe, Building_WorkTable workBench, List<ThingDef> ingredients, out float workLeft)
		{
			if (GetResumeBillWorkLeft(recipe, workBench, ingredients, out workLeft))
			{
				resumeBillWorkLefts.Remove((recipe, workBench));
				return true;
			}
			return false;
		}

		public void ClearTempData()
		{
			resumeJobWorkLeftsTemp.Clear();
			resumeJobWorkDonesTemp.Clear();
			resumeBillWorkLeftsTemp.Clear();
		}

		private List<ResumeJob> resumeJobWorkLeftsTemp = [];
		private List<ResumeJob> resumeJobWorkDonesTemp = [];
		private List<ResumeBill> resumeBillWorkLeftsTemp = [];
		public void ExposeData()
		{
			if (Scribe.EnterNode("resumeJobs"))
			{
				try
				{
					if (Scribe.mode == LoadSaveMode.Saving)
					{
						resumeJobWorkLeftsTemp.Clear();
						resumeJobWorkDonesTemp.Clear();
						resumeBillWorkLeftsTemp.Clear();
						foreach (var resumeJobWorkLeft in resumeJobWorkLefts)
						{
							var (target, def) = resumeJobWorkLeft.Key;
							var (workLeft, efficiencyIntegral) = resumeJobWorkLeft.Value;
							if (target.IsValid && target.Map is not null)
							{
								resumeJobWorkLeftsTemp.Add(new ResumeJob(target, def, workLeft, efficiencyIntegral));
							}
						}
						foreach (var resumeJobWorkDone in resumeJobWorkDones)
						{
							var (target, def) = resumeJobWorkDone.Key;
							var (workDone, efficiencyIntegral) = resumeJobWorkDone.Value;
							if (target is not null)
							{
								resumeJobWorkDonesTemp.Add(new ResumeJob(target, def, workDone, efficiencyIntegral));
							}
						}
						foreach (var resumeBillWorkLeft in resumeBillWorkLefts)
						{
							var (recipe, workBench) = resumeBillWorkLeft.Key;
							var (ingredients, workLeft) = resumeBillWorkLeft.Value;
							if (!ingredients.NullOrEmpty())
							{
								resumeBillWorkLeftsTemp.Add(new ResumeBill(recipe, workBench, ingredients, workLeft));
							}
						}
					}
					Scribe_Collections.Look(ref resumeJobWorkLeftsTemp, "resumeJobWorkLefts", LookMode.Deep);
					Scribe_Collections.Look(ref resumeJobWorkDonesTemp, "resumeJobWorkDones", LookMode.Deep);
					Scribe_Collections.Look(ref resumeBillWorkLeftsTemp, "resumeBillWorkLefts", LookMode.Deep);
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
		}

		public void ResolveCompressedThings()
		{
			var newLeftList = new Dictionary<(TargetInfo, Def), (float, float)>();
			var newDoneList = new Dictionary<(Thing, Def), (float, float)>();
			var newBillList = new Dictionary<(RecipeDef, Building_WorkTable), (List<ThingDef>, float)>();
			foreach (var resumeJob in resumeJobWorkLeftsTemp)
			{
				if (resumeJob.HasTempThing)
				{
					if (resumeJob.MapTemp is not null)
					{
						foreach (var thing in resumeJob.CellTemp.GetThingList(resumeJob.MapTemp))
						{
							if (thing.def == resumeJob.TempThing)
							{
								newLeftList.Add((thing, resumeJob.Def), (resumeJob.WorkAmount, resumeJob.StatIntegral));
								break;
							}
						}
					}
					else
					{
						Log.Message("[ResumeJobProgress] invalid data loading!");
					}
				}
				else
				{
					newLeftList.Add((resumeJob.Target, resumeJob.Def), (resumeJob.WorkAmount, resumeJob.StatIntegral));
				}
			}
			foreach (var resumeJob in resumeJobWorkDonesTemp)
			{
				if (resumeJob.HasTempThing && resumeJob.MapTemp is not null)
				{
					foreach (var thing in resumeJob.CellTemp.GetThingList(resumeJob.MapTemp))
					{
						if (thing.def == resumeJob.TempThing)
						{
							newDoneList.Add((thing, resumeJob.Def), (resumeJob.WorkAmount, resumeJob.StatIntegral));
							break;
						}
					}
				}
				else
				{
					Log.Message("[ResumeJobProgress] invalid data loading!");
				}
			}
			foreach (var resumeBillWork in resumeBillWorkLeftsTemp)
			{
				newBillList.Add(resumeBillWork.Key, resumeBillWork.Value);
			}
			resumeJobWorkLefts = newLeftList;
			resumeJobWorkDones = newDoneList;
			resumeBillWorkLefts = newBillList;
		}
	}
}
