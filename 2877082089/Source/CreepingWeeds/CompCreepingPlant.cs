using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CreepingWeeds;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Helper
	{
	[DebugAction("General", allowedGameStates = AllowedGameStates.PlayingOnMap, hideInSubMenu = true)]
	private static void PrintPlants()
		{
		string num = "Plant count: " + Find.CurrentMap.PlantCount() + "\nMax Plants: " + Find.CurrentMap.MaxPlants();
		if (Debug.developerConsoleVisible) Log.Message(num);
		MoteMaker.ThrowText(UI.MouseMapPosition(), Find.CurrentMap, num);
		}

	public static float PlantCount(this Map map)
		{
		return map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).Count;
		}

	public static readonly AccessTools.FieldRef<WildPlantSpawner, float> CALCULATED_WHOLE_MAP_NUM_DESIRED_PLANTS =
		AccessTools.FieldRefAccess<float>(typeof(WildPlantSpawner), "calculatedWholeMapNumDesiredPlants");
	public static float MaxPlants(this Map map)
		{
		return CALCULATED_WHOLE_MAP_NUM_DESIRED_PLANTS(map.wildPlantSpawner);
		}
	}

public class CompProperties_CreepingPlant : CompProperties
	{
	public float chance = 1f;

	public CompProperties_CreepingPlant()
		{
		compClass = typeof (CompCreepingPlant);
		}
	}

public class CompCreepingPlant : ThingComp
	{
	public const float MinGrowthRate = 0.25f;
	public const int CheckEveryXPlants = 3;
	public CompProperties_CreepingPlant Props => (CompProperties_CreepingPlant) props;
	public Plant Parent => (Plant) parent;
	
	public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
		var stats = base.SpecialDisplayStats();
		if (!stats.EnumerableNullOrEmpty()) foreach (StatDrawEntry stat in base.SpecialDisplayStats()) yield return stat;

		yield return new StatDrawEntry(StatCategoryDefOf.Basics, "CW.SpreadRate".Translate(), Props.chance.ToStringPercent(), "CW.SpreadRate.desc".Translate(), 4156);
		}
	
	public override void CompTickLong()
		{
		base.CompTickLong();

		if (Parent.GrowthRate > MinGrowthRate && Parent.HashOffsetTicks() % CheckEveryXPlants == 0 && (Parent.HarvestableNow || Parent.IngestibleNow) && Rand.Chance(Props.chance) && Parent.Map.PlantCount() < Parent.Map.MaxPlants())
			{
			TrySpread();
			}
		}

	private static readonly List<int> I = new (){ 0, 1, 2, 3 };
	protected virtual void TrySpread()
		{
		I.Shuffle();
		foreach (int i in I)
			{
			var vec = IntVec3.North.RotatedBy(new Rot4(i)) + Parent.Position;
			if (!Parent.def.CanEverPlantAt(vec, Parent.Map)) continue;
			if (vec.GetPlant(Parent.Map) != null) continue;
			var fertility = Parent.Map.wildPlantSpawner.GetBaseDesiredPlantsCountAt(vec);
			if (fertility == 0f || Parent.def.plant.fertilityMin > fertility ||
				Parent.def.plant.fertilitySensitivity != 0 && !Rand.Chance(fertility)) continue;

			var newPlant = (Plant) GenSpawn.Spawn(Parent.def, vec, Parent.Map);
			newPlant.Growth = 0.0001f;
			newPlant.sown = Parent.sown;
			newPlant.Map.mapDrawer.MapMeshDirty(newPlant.Position, MapMeshFlag.Things);

			if (Prefs.DevMode && Prefs.LogVerbose)
				Log.Message("Plant spread: " + newPlant.def.label + " at " + vec.ToString());
			return;
			}
		}
	

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
		var gizmos =  base.CompGetGizmosExtra();
		if (!gizmos.EnumerableNullOrEmpty()) foreach (var gizmo in gizmos)
			{
			yield return gizmo;
			}

		if (DebugSettings.godMode)
			{
			Command_Action spread = new()
				{
				defaultLabel = "TrySpread".Translate(),
				action = TrySpread
				};

			yield return spread;
			}
		}
	public override string CompInspectStringExtra()
		{
		if (!Prefs.DevMode) return base.CompInspectStringExtra();

		float chance = 1f;

		for (int i = 0; i < 4; i++)
			{
			var vec = IntVec3.North.RotatedBy(new Rot4(i)) + Parent.Position;
			if (!Parent.def.CanEverPlantAt(vec, Parent.Map)) continue;
			var fertility = Mathf.Min(Parent.Map.wildPlantSpawner.GetBaseDesiredPlantsCountAt(vec), 1f);
			if (fertility == 0f || Parent.def.plant.fertilityMin > fertility || Parent.def.plant.fertilitySensitivity == 0) continue;
			chance *= 1 - fertility;
			}
		chance = 1 - chance;
		
		StringBuilder builder = new(base.CompInspectStringExtra());
		builder.AppendLineIfNotEmpty();
		builder.Append("Spread MTB: ");
		builder.Append((float) GenTicks.TickLongInterval / GenTicks.TicksPerRealSecond / 60
					 * CheckEveryXPlants
					 / Props.chance
					 / chance);
		builder.Append(" minutes");

		if (!Parent.HarvestableNow && !Parent.IngestibleNow)
			{
			builder.AppendInNewLine("ERROR: Growth incomplete");
			}
		if (Parent.Map.PlantCount() > Parent.Map.MaxPlants())
			{
			builder.AppendInNewLine("ERROR: Map full");
			}
		return builder.ToString();
		}
	}