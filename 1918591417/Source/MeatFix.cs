using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

using HugsLib;
using System.Reflection;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HugsLib.Settings;
using HarmonyLib;

namespace MeatFix
{
  public class MeatFix : HugsLib.ModBase
  {
    static JobGiver_ExitMapHunted jgemp = new JobGiver_ExitMapHunted();
		
		
    public MeatFix()
    {
      patch();
        
    }
      
    public override string ModIdentifier
    {
      get
      {
	return "MeatFix.MeatFix";
      }
    }

      
    private static float GetMass(Thing thing) {
      if (thing == null) {
	return 0f;
      }
      float num = thing.GetStatValue(StatDefOf.Mass, true);
      Pawn pawn = thing as Pawn;
      if (pawn != null) {

	num -= MassUtility.InventoryMass(pawn);

      }
      Corpse corpse = thing as Corpse;
      if (corpse != null && corpse.Spawned)
      {
	num -= MassUtility.GearAndInventoryMass(corpse.InnerPawn);
      
      }
      return num;
    }

    public static bool GetStatValue(Thing thing, StatDef stat, out float __result) {
      __result = 0;
      if (stat == StatDefOf.MeatAmount) {
	Pawn pawn = thing as Pawn;
	if (pawn != null) {
	  ThingDef meatDef = pawn.RaceProps.meatDef;
	  float meatMass = meatDef.GetStatValueAbstract(StatDefOf.Mass);
	  float mass = GetMass(pawn);
	  __result = 0.4f * mass / meatMass;
	  return false;
	}
      }
      return true;
    }

    public static void ButcherProducts(Pawn butcher, float efficiency, ref IEnumerable<Thing> __result, Pawn __instance) {
      Log.Error("butcher products");
      foreach (Thing thing in __result) {
	if (__instance.RaceProps.meatDef != null) {
	  if (thing.def != __instance.RaceProps.meatDef) {
	    continue;
	  }
	  
	  Thing meat = thing;
	  float mass = GetMass(__instance);
	  float meatMass = meat.GetStatValue(StatDefOf.Mass);
	  meat.stackCount = (int) (efficiency * mass * 0.4 / meatMass);
	  //Log.Error("meat" + meat.stackCount);
	}
      }
    }

    private static IEnumerable<Pawn> GetPackmates(Pawn pawn, float radius) {
      Room pawnRoom = pawn.GetRoom(RegionType.Set_Passable);
      List<Pawn> raceMates = pawn.Map.mapPawns.AllPawnsSpawned;
      for (int i = 0; i < raceMates.Count; i++)
      {
	if (raceMates[i].def == pawn.def && raceMates[i].Faction == pawn.Faction && raceMates[i].Position.InHorDistOf(pawn.Position, radius) && raceMates[i].GetRoom(RegionType.Set_Passable) == pawnRoom)
	{
	  yield return raceMates[i];
	}
      }
      yield break;
    }

    private static bool CanStartFleeingBecauseOfPawnAction(Pawn p)
    {
      return p.RaceProps.Animal && !p.InMentalState && !p.IsFighting() && !p.Downed && !p.Dead && !ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(p);
    }

    public static bool Notify_BulletImpactNearby(Pawn __instance, RimWorld.BulletImpactData impactData) {
      if (__instance == null) {
	return true;
      }
      if (__instance.RaceProps == null) {
	return true;
      }
      if (!__instance.RaceProps.Animal) {
	return true;
	
      }
      
      if ((__instance.Faction?.IsPlayer ?? false) || (__instance.Faction?.def == FactionDefOf.Insect)) {
	return true;
      }
      if (!(__instance.RaceProps?.herdAnimal ?? false)) {
	if (Rand.Chance(0.4f)) {
	  
	  if (CanStartFleeingBecauseOfPawnAction(__instance))
	  {
	    Job fleeMap = jgemp.GetExitJob(__instance);
	    if (fleeMap != null) {
	      __instance.jobs.StartJob(fleeMap);
	      return false;
	    }
	  }
	}
      }
      else {
	if (Rand.Chance(0.8f)) {
	  foreach (Pawn pawn in GetPackmates(__instance, 24f))
	  {
	    if (CanStartFleeingBecauseOfPawnAction(pawn))
	    {
	      Job fleeMap = jgemp.GetExitJob(pawn);
	      if (fleeMap != null) {
		pawn.jobs.StartJob(fleeMap);
	      }
	    }
	  }
	  return false;
	} 
      }
      return true;
    }

    public static bool StartFleeingBecauseOfPawnAction(Pawn_MindState __instance, Thing instigator) {

      if (!__instance.pawn.RaceProps.herdAnimal) {
//	Log.Error("not herd");
	return true;
      }
//      Log.Error("might flee");

      if (Rand.Chance(0.8f)) {
//	Log.Error("fleeing map");
	JobGiver_ExitMapHunted jgemp = new JobGiver_ExitMapHunted();
	foreach (Pawn pawn in GetPackmates(__instance.pawn, 24f))
	{
	  if (CanStartFleeingBecauseOfPawnAction(pawn))
	  {
	    Job fleeMap = jgemp.GetExitJob(pawn);
	    if (fleeMap != null) {
	      pawn.jobs.StartJob(fleeMap);
	    }
	  }
	}

	List<Thing> threats = new List<Thing>
	{
	  instigator
	};
	IntVec3 fleeDest = CellFinderLoose.GetFleeDest(__instance.pawn, threats, __instance.pawn.Position.DistanceTo(instigator.Position) + 14f);
	if (fleeDest != __instance.pawn.Position)
	{
	  __instance.pawn.jobs.StartJob(
					new Job(JobDefOf.Flee, fleeDest, instigator),
					JobCondition.InterruptOptional,
					null, //jobGiver
					true, //resumeCurJob
					true, //cancelBusyStatus
					null, //thinkTree
					null, //tag
					false); //fromQueue
	}
	
	return false;
      }
      return true;
    }
      
    public void patch() {
      var harmony = new Harmony("com.lp-programming.rimworld.meatfix");
      harmony.Patch(
		    typeof(StatExtension).GetMethod("GetStatValue", BindingFlags.Public | BindingFlags.Static),
		    new HarmonyMethod(typeof(MeatFix).GetMethod("GetStatValue", BindingFlags.Public | BindingFlags.Static)),
		    null,
		    null
		    );
      harmony.Patch(
		    typeof(Pawn_MindState).GetMethod("StartFleeingBecauseOfPawnAction", BindingFlags.Instance | BindingFlags.Public),
		    new HarmonyMethod(typeof(MeatFix).GetMethod("StartFleeingBecauseOfPawnAction", BindingFlags.Static | BindingFlags.Public)),
		    null,
		    null);
      harmony.Patch(
		    typeof(Pawn).GetMethod("Notify_BulletImpactNearby", BindingFlags.Instance | BindingFlags.Public),
		    new HarmonyMethod(typeof(MeatFix).GetMethod("Notify_BulletImpactNearby", BindingFlags.Static | BindingFlags.Public)),
		    null,
		    null);
      
    }
  }

  public class JobGiver_ExitMapHunted : JobGiver_ExitMapPanic {
    public Job GetExitJob(Pawn pawn) {
      return this.TryGiveJob(pawn);
    }
  }
    
}

