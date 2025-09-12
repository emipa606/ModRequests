using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Necro
{
	
	public class GameCondition_Necrotion : GameCondition
	{
		
		
		public override int TransitionTicks
		{
			get
			{
				return 5000;
			}
		}

		
		public override void Init()
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
		}

		
		public override void GameConditionTick()
		{
			List<Map> affectedMaps = base.AffectedMaps;
			if (Find.TickManager.TicksGame % 3451 == 0)
			{
				for (int i = 0; i < affectedMaps.Count; i++)
				{
					this.DoPawnsToxicDamage(affectedMaps[i]);
				}
			}
			for (int j = 0; j < this.overlays.Count; j++)
			{
				for (int k = 0; k < affectedMaps.Count; k++)
				{
					this.overlays[j].TickOverlay(affectedMaps[k]);
				}
			}
		}

		
		private void DoPawnsToxicDamage(Map map)
		{
			foreach (Pawn p in map.mapPawns.AllPawnsSpawned.ToList())
                GameCondition_Necrotion.DoPawnToxicDamage(p, true, 1f);
			
		}

		
		public static void DoPawnToxicDamage(Pawn p, bool protectedByRoof = true, float extraFactor = 1f)
		{
			if (!p.Spawned || !protectedByRoof || !p.Position.Roofed(p.Map))
			{
				float num = 0.023006668f;
				num *= Mathf.Max(1f - p.GetStatValue(NecroDefOf.NecrotionResistance, true, -1), 0f);
				if (ModsConfig.BiotechActive)
				{
					num *= Mathf.Max(1f - p.GetStatValue(NecroDefOf.NecrotionEnvironmentResistance, true, -1), 0f);
				}
				num *= extraFactor;
				if (num != 0f)
				{
					float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(p.thingIDNumber ^ 74374237));
					num *= num2;
					HealthUtility.AdjustSeverity(p, NecroDefOf.Necronoid_Infection, num);
				}
			}
		}

		
		public override void DoCellSteadyEffects(IntVec3 c, Map map)
		{
			if (!c.Roofed(map))
			{
				List<Thing> thingList = c.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing is Plant)
					{
						if (thing.def.plant.dieFromToxicFallout && Rand.Value < 0.0065f)
						{
							thing.Kill(null, null);
						}
					}
					else if (thing.def.category == ThingCategory.Item)
					{
						CompRottable compRottable = thing.TryGetComp<CompRottable>();
						if (compRottable != null && compRottable.Stage < RotStage.Dessicated)
						{
							compRottable.RotProgress += 3000f;
						}
					}
				}
			}
		}

		
		public override void GameConditionDraw(Map map)
		{
			for (int i = 0; i < this.overlays.Count; i++)
			{
				this.overlays[i].DrawOverlay(map);
			}
		}

		
		public override float SkyTargetLerpFactor(Map map)
		{
			return GameConditionUtility.LerpInOutValue(this, (float)this.TransitionTicks, 0.5f);
		}

		
		public override SkyTarget? SkyTarget(Map map)
		{
			return new SkyTarget?(new SkyTarget(0.85f, this.ToxicFalloutColors, 1f, 1f));
		}

		
		public override float AnimalDensityFactor(Map map)
		{
			return 0f;
		}

		
		public override float PlantDensityFactor(Map map)
		{
			return 0f;
		}

		
		public override bool AllowEnjoyableOutsideNow(Map map)
		{
			return false;
		}

		
		public override List<SkyOverlay> SkyOverlays(Map map)
		{
			return this.overlays;
		}

		
		public GameCondition_Necrotion()
		{
		}

		
		private const float MaxSkyLerpFactor = 0.5f;

		
		private const float SkyGlow = 0.1f;

		
		private SkyColorSet ToxicFalloutColors = new SkyColorSet(new ColorInt(136, 8, 8).ToColor, new ColorInt(136, 8, 8).ToColor, new Color(0f, 0.89f, 0.28f), 0.1f);

		
		private List<SkyOverlay> overlays = new List<SkyOverlay>
		{
			new WeatherOverlay_Fallout()
		};

		
		public const int CheckInterval = 3451;

		
		private const float ToxicPerDay = 0.9f;

		
		private const float PlantKillChance = 0.0065f;

		
		private const float CorpseRotProgressAdd = 3000f;
	}
}
