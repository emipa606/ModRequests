using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using RimWorld.Utility;
using LudeonTK;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Grammar;
using Verse.Noise;
using Verse.Profile;
using Verse.Sound;
using Verse.Steam;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace NAT
{
	public class CompProperties_BoxOfFate : CompProperties_Interactable
	{
		public CompProperties_BoxOfFate()
		{
			compClass = typeof(CompBoxOfFate);
		}
	}
	public class CompBoxOfFate : CompInteractable
	{
		public List<BoxActionDef> actions;

		public bool switch1;

		public bool switch2;

		public bool switch3;

		public override string ExposeKey => "Interactor";

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref actions, "actions", LookMode.Def);
			Scribe_Values.Look(ref switch1, "switch1", false);
			Scribe_Values.Look(ref switch2, "switch2", false);
			Scribe_Values.Look(ref switch3, "switch3", false);
		}

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
			actions = new List<BoxActionDef>(8);
			for(int i = 0; i < actions.Capacity; i++)
            {
				actions.Add(DefDatabase<BoxActionDef>.AllDefsListForReading.Where((BoxActionDef a1) => a1.commonality > 0f).RandomElementByWeight((BoxActionDef a2) => a2.commonality));
			}
			switch1 = Rand.Bool;
			switch2 = Rand.Bool;
			switch3 = Rand.Bool;
		}

		private static readonly CachedTexture SwitchTex_Up = new CachedTexture("UI/NAT_BoxSwitch_Up");

		private static readonly CachedTexture SwitchTex_Down = new CachedTexture("UI/NAT_BoxSwitch_Down");

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (DebugSettings.ShowDevGizmos)
			{
				Command_Action command_Action4 = new Command_Action();
				command_Action4.defaultLabel = "DEV: Check list";
				command_Action4.groupable = false;
				command_Action4.action = delegate
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("======= Actions =======");
					for (int i = 0; i < actions.Count; i++)
					{
						List<bool> list = CombinationFromActionNum(i);
						stringBuilder.AppendLine(actions[i].defName + " " + list[0].ToString() + " " + list[1].ToString() + " " + list[2].ToString()); 
					}
					Log.Message(stringBuilder.ToString());
				};
				yield return command_Action4;
			}

			Command_Action command_Action1 = new Command_Action();
			command_Action1.defaultLabel = "NAT_TurnSwitch".Translate();
			command_Action1.groupable = false;
			if (switch1)
			{
				command_Action1.icon = SwitchTex_Down.Texture;
			}
			else
			{
				command_Action1.icon = SwitchTex_Up.Texture;
			}
			command_Action1.action = delegate
			{
				switch1 = !switch1;
			};
			yield return command_Action1;

			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "NAT_TurnSwitch".Translate();
			command_Action2.groupable = false;
			if (switch2)
			{
				command_Action2.icon = SwitchTex_Down.Texture;
			}
			else
			{
				command_Action2.icon = SwitchTex_Up.Texture;
			}
			command_Action2.action = delegate
			{
				switch2 = !switch2;
			};
			yield return command_Action2;

			Command_Action command_Action3 = new Command_Action();
			command_Action3.defaultLabel = "NAT_TurnSwitch".Translate();
			command_Action3.groupable = false;
			if (switch3)
			{
				command_Action3.icon = SwitchTex_Down.Texture;
			}
			else
			{
				command_Action3.icon = SwitchTex_Up.Texture;
			}
			command_Action3.action = delegate
			{
				switch3 = !switch3;
			};
			yield return command_Action3;

			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
		}

		private int ActionNumFromCombination(bool s1, bool s2, bool s3)
        {
			if (s1)
			{
				if (s2)
				{
					if (s3)
					{
						return 0;
					}
					return 1;
				}
				if (s3)
				{
					return 2;
				}
				return 3;
			}
			if (s2)
			{
				if (s3)
				{
					return 4;
				}
				return 5;
			}
			if (s3)
			{
				return 6;
			}
			return 7;
		}

		private List<bool> CombinationFromActionNum(int num)
        {
			switch (num){
				case 0:
					return new List<bool>(3) { true, true, true };
				case 1:
					return new List<bool>(3) { true, true, false };
				case 2:
					return new List<bool>(3) { true, false, true};
				case 3:
					return new List<bool>(3) { true, false, false };
				case 4:
					return new List<bool>(3) { false, true, true };
				case 5:
					return new List<bool>(3) { false, true, false };
				case 6:
					return new List<bool>(3) { false, false, true };
				default:
					return new List<bool>(3) { false, false, false};
			}
		}

		protected override void OnInteracted(Pawn caster)
		{
			actions[ActionNumFromCombination(switch1, switch2, switch3)].Worker.TryDoAction(parent, caster);
		}

        public override void CompTick()
        {
            if (parent.HitPoints < parent.MaxHitPoints)
            {
				parent.HitPoints++;
			}
			base.CompTick();
        }
    }

	public class BoxActionDef: Def
    {
        public float commonality;

        public float failChance;

        public BoxActionDef failAction = NATDefOf.TriedToDoSmth;

		public Type workerClass = typeof(BoxActionWorker);

        [Unsaved(false)]
        private BoxActionWorker workerInt;

        public BoxActionWorker Worker
        {
            get
            {
                if (workerInt == null && workerClass != null)
                {
                    workerInt = (BoxActionWorker)Activator.CreateInstance(workerClass);
                    workerInt.def = this;
                }
                return workerInt;
            }
        }
    }

    public abstract class BoxActionWorker
    {
        public BoxActionDef def;

		public virtual bool TryDoAction(Thing box, Pawn pawn)
		{
			if(pawn == null)
            {
				Log.Message("Pawn was null");
            }
			if(FailOn(box, pawn))
            {
				if(def.failAction == null)
                {
					Log.Message("fail action was null"); 
					return false;
				}
				if (def.failAction.Worker == null)
				{
					Log.Message("Worker was null");
					return false;
				}
				def.failAction.Worker.DoAction(box, pawn); 
				return false;
            }
            else
            {
				DoAction(box, pawn);
				return true;
			}
		}

		public virtual bool FailOn(Thing box, Pawn pawn)
        {
			if (def.failChance > 0f && Rand.Chance(def.failChance))
			{
				Log.Message("fail by chance");
				return true;
			}
			return false;
		}

        public virtual void DoAction(Thing box, Pawn pawn)
        {
			Log.Message("action began " + def.defName);
        }
    }

	public class NothingWorker : BoxActionWorker
    {
		public override void DoAction(Thing box, Pawn pawn)
		{
			base.DoAction(box, pawn);
			Messages.Message("NAT_BoxAction_Nothing".Translate(), pawn, MessageTypeDefOf.NeutralEvent);
		}
	}

	public class TriedToDoSmthWorker : BoxActionWorker
	{
		public override void DoAction(Thing box, Pawn pawn)
		{
			base.DoAction(box, pawn);
			Find.LetterStack.ReceiveLetter("NAT_BoxAction_TriedDoSmth".Translate(), "NAT_BoxAction_TriedDoSmth_Desc".Translate(pawn.Named("PAWN")), LetterDefOf.NeutralEvent, pawn);
		}
	}

	public class QualityMinusWorker : BoxActionWorker
	{
		public override bool FailOn(Thing box, Pawn pawn)
		{
			if (base.FailOn(box, pawn))
			{
				return true;
			}
			if (pawn.EquippedWornOrInventoryThings.EnumerableNullOrEmpty() || pawn.EquippedWornOrInventoryThings.Where((Thing t1) => t1.HasComp<CompQuality>() && t1.TryGetComp<CompQuality>().Quality > QualityCategory.Poor).EnumerableNullOrEmpty())
			{
				return true;
			}
			return false;
		}

		public override void DoAction(Thing box, Pawn pawn)
		{
			base.DoAction(box, pawn);
			CompQuality comp = pawn.EquippedWornOrInventoryThings.Where((Thing t1) => t1.HasComp<CompQuality>() && t1.TryGetComp<CompQuality>().Quality > QualityCategory.Poor).RandomElementByWeight((Thing t2) => t2.MarketValue).TryGetComp<CompQuality>();
			comp.SetQuality(comp.Quality - 2, null);
			Find.LetterStack.ReceiveLetter("NAT_BoxAction_QualityMinus".Translate(), "NAT_BoxAction_QualityMinus_Desc".Translate(pawn.Named("PAWN")), LetterDefOf.PositiveEvent, pawn);
		}
	}

	public class QualityPlusWorker : BoxActionWorker
	{
        public override bool FailOn(Thing box, Pawn pawn)
        {
            if(base.FailOn(box, pawn))
            {
				return true;
            }
			if(pawn.EquippedWornOrInventoryThings.EnumerableNullOrEmpty() || pawn.EquippedWornOrInventoryThings.Where((Thing t1) => t1.HasComp<CompQuality>() && t1.TryGetComp<CompQuality>().Quality <= QualityCategory.Excellent).EnumerableNullOrEmpty())
            {
				return true;
            }
			return false;
        }

        public override void DoAction(Thing box, Pawn pawn)
        {
			base.DoAction(box, pawn);
			CompQuality comp = pawn.EquippedWornOrInventoryThings.Where((Thing t1) => t1.HasComp<CompQuality>() && t1.TryGetComp<CompQuality>().Quality <= QualityCategory.Excellent).RandomElementByWeight((Thing t2) => t2.MarketValue).TryGetComp<CompQuality>();
			comp.SetQuality(comp.Quality + 2, null);
			Find.LetterStack.ReceiveLetter("NAT_BoxAction_QualityPlus".Translate(), "NAT_BoxAction_QualityPlus_Desc".Translate(pawn.Named("PAWN")), LetterDefOf.ThreatSmall, pawn);
			//Find.LetterStack.ReceiveLetter(, , LetterDef, pawn);
		}
	}

	public class ExplosionWorker : BoxActionWorker
	{
		public override void DoAction(Thing box, Pawn pawn)
		{
			base.DoAction(box, pawn);
			GenExplosion.DoExplosion(box.Position, box.MapHeld, 6.3f, DamageDefOf.Flame, box, 30, ignoredThings: new List<Thing>() { box });
			Find.LetterStack.ReceiveLetter("NAT_BoxAction_Explosion".Translate(), "NAT_BoxAction_Explosion_Desc".Translate(pawn.Named("PAWN")), LetterDefOf.NegativeEvent, pawn);
		}
	}

	public class BrainDamageWorker : BoxActionWorker
	{
		public override bool FailOn(Thing box, Pawn pawn)
		{
			if (base.FailOn(box, pawn))
			{
				return true;
			}
			if (pawn.health.hediffSet.GetBrain() == null)
			{
				return true;
			}
			return false;
		}

		public override void DoAction(Thing box, Pawn pawn)
		{
			base.DoAction(box, pawn);
			pawn.TakeDamage(new DamageInfo(DamageDefOf.Psychic, new FloatRange(2f, 7f).RandomInRange, 0f, -1f, null, pawn.health.hediffSet.GetBrain()));
			Find.LetterStack.ReceiveLetter("NAT_BoxAction_BrainDamage".Translate(), "NAT_BoxAction_BrainDamage_Desc".Translate(pawn.Named("PAWN")), LetterDefOf.ThreatBig, pawn);
		}
	}

	public class LobotomyWorker : BoxActionWorker
	{
		public override bool FailOn(Thing box, Pawn pawn)
		{
			if (base.FailOn(box, pawn))
			{
				return true;
			}
			if (pawn.health.hediffSet.GetBrain() == null)
			{
				return true;
			}
			return false;
		}

		public override void DoAction(Thing box, Pawn pawn)
		{
			base.DoAction(box, pawn);
			pawn.health.AddHediff(HediffDefOf.BlissLobotomy, pawn.health.hediffSet.GetBrain());
			Find.LetterStack.ReceiveLetter("NAT_BoxAction_Lobotomy".Translate(), "NAT_BoxAction_Lobotomy_Desc".Translate(pawn.Named("PAWN")), LetterDefOf.ThreatBig, pawn);
		}
	}

	public class TraitChangeWorker : BoxActionWorker
	{
		public override bool FailOn(Thing box, Pawn pawn)
		{
			if (base.FailOn(box, pawn))
			{
				return true;
			}
			if (pawn.story == null || pawn.story.traits.allTraits.NullOrEmpty() || !pawn.story.traits.allTraits.Where((Trait t) => t.sourceGene == null).Any())
			{
				return true;
			}
			return false;
		}

		public override void DoAction(Thing box, Pawn pawn)
		{
			base.DoAction(box, pawn);
			Trait trait = pawn.story.traits.allTraits.Where((Trait t) => t.sourceGene == null).RandomElement();
			pawn.story.traits.allTraits.Remove(trait);
			Find.LetterStack.ReceiveLetter("NAT_BoxAction_Lobotomy".Translate(), "NAT_BoxAction_Lobotomy_Desc".Translate(pawn.Named("PAWN")), LetterDefOf.ThreatBig, pawn);
		}
	}

	public class SkillUpWorker : BoxActionWorker
	{
		public override bool FailOn(Thing box, Pawn pawn)
		{
			if (base.FailOn(box, pawn))
			{
				return true;
			}
			if(!pawn.skills.skills.Any((SkillRecord s)=> !s.TotallyDisabled))
            {
				return true;
            }
			return false;
		}

		public override void DoAction(Thing box, Pawn pawn)
		{
			base.DoAction(box, pawn);
			SkillRecord skill = pawn.skills.skills.Where((SkillRecord s)=> !s.TotallyDisabled).RandomElementByWeight((SkillRecord r) => (float)r.passion + 1f);
			skill.Level += 5;
			Find.LetterStack.ReceiveLetter("NAT_BoxAction_LevelUp".Translate(skill.def.label), "NAT_BoxAction_LevelUp_Desc".Translate(pawn.Named("PAWN")), LetterDefOf.ThreatBig, pawn);
		}
	}

	public class PocketMapWorker : BoxActionWorker
	{
		public override void DoAction(Thing box, Pawn pawn)
		{
			base.DoAction(box, pawn);
			Map metalHell = PocketMapUtility.GeneratePocketMap(new IntVec3(21, 1, 21), NATDefOf.NAT_Box, null, box.MapHeld);
			IntVec3 cell = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(metalHell), metalHell);
			SkipUtility.SkipTo(pawn, cell, metalHell);
			pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			CameraJumper.TryJump(pawn, CameraJumper.MovementMode.Cut);
			Find.TickManager.slower.SignalForceNormalSpeed();
			Find.LetterStack.ReceiveLetter("NAT_BoxAction_Explosion".Translate(), "NAT_BoxAction_Explosion_Desc".Translate(pawn.Named("PAWN")), LetterDefOf.NegativeEvent, pawn);
		}
	}

	public class GenStep_Box : GenStep
	{
		private const float WallNoiseCutoff = 1.75f;

		private const float MassNoiseCutoff = 1.9f;

		private static readonly FloatRange InnerMassCutoffRange = new FloatRange(2f, 3f);

		private const float InnerMassChancePerCell = 0.05f;

		public float perlinFrequency = 0.05f;

		public float perlinLacunarity = 2f;

		public float perlinPersistence = 0.5f;

		public int perlinOctaves = 1;

		public override int SeedPart => 41234756;

		public override void Generate(Map map, GenStepParams parms)
		{
			List<ThingDef> source = new List<ThingDef>
			{
				ThingDefOf.VoidmetalMassSmall,
				ThingDefOf.VoidmetalMassMedium,
				ThingDefOf.VoidmetalMassLarge
			};
			//noise = new Perlin(perlinFrequency, perlinLacunarity, perlinPersistence, perlinOctaves, Rand.Range(0, int.MaxValue), QualityMode.Medium);
			foreach (IntVec3 allCell in map.AllCells)
			{
				map.terrainGrid.SetTerrain(allCell, TerrainDefOf.Gravel);
				map.terrainGrid.SetTerrain(allCell, TerrainDefOf.Voidmetal);
                if (allCell.x == 0 || allCell.z == 0 || allCell.x == map.Size.x - 1 || allCell.z == map.Size.z - 1)
                {
					GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.VoidmetalWall), allCell, map);
				}
			}
			//GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.Bed, ThingDefOf.Bioferrite), map.Center, map);
			Thing meal = ThingMaker.MakeThing(ThingDefOf.Column, ThingDefOf.Jade);
			GenSpawn.Spawn(meal, new IntVec3(map.Center.x + 1, 1, map.Center.z), map);
			MapGenerator.PlayerStartSpot = map.Center;
		}
	}
}