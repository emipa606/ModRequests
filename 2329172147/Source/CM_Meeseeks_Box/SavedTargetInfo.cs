using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    public enum ConstructionStatus
    {
        Invalid,
        None,
        InProgress,
        Complete,
        Blocked
    }

    public class SavedTargetInfo : IExposable
    {
        public LocalTargetInfo target;

        public TerrainDef blueprintTerrainDef;

        public ThingDef blueprintThingDef;

        public ThingDef blueprintStuff;

        public Rot4 blueprintRotation;

        public Bill bill;

        public TrainableDef trainable;


        public bool HasThing => target.HasThing;

        public Thing Thing => target.Thing;

        public bool ThingDestroyed => target.ThingDestroyed;

        public IntVec3 Cell => target.Cell;

        public bool IsValid => target.IsValid;

        public bool IsConstruction => (blueprintTerrainDef != null || blueprintThingDef != null);

        public BuildableDef BuildableDef => ((BuildableDef)blueprintTerrainDef ?? (BuildableDef)blueprintThingDef);

        public SavedTargetInfo()
        {

        }

        public SavedTargetInfo(LocalTargetInfo targetInfo)
        {
            target = targetInfo;
        }

        public SavedTargetInfo(Thing thing)
        {
            target = thing;
        }

        public SavedTargetInfo(IntVec3 cell)
        {
            target = cell;
        }

        public static implicit operator SavedTargetInfo(Thing t)
        {
            return new SavedTargetInfo(t);
        }

        public static implicit operator SavedTargetInfo(IntVec3 c)
        {
            return new SavedTargetInfo(c);
        }

        public static explicit operator IntVec3(SavedTargetInfo targ)
        {
            if (targ.target == null)
                return IntVec3.Invalid;

            if (targ.target.HasThing)
            {
                Log.ErrorOnce("Casted LocalTargetInfo to IntVec3 but it had Thing " + targ.target.Thing, 6324165);
            }
            return targ.target.Cell;
        }

        public static explicit operator Thing(SavedTargetInfo targ)
        {
            if (targ.target == null)
                return null;

            if (!targ.target.HasThing)
            {
                Log.ErrorOnce("Casted LocalTargetInfo to Thing but it had cell " + targ.target.Cell, 631672);
            }
            return targ.target.Thing;
        }

        public static bool operator ==(SavedTargetInfo a, SavedTargetInfo b)
        {
            if (a is null)
                return (b is null);

            if (b is null)
                return false;

            if (a.target == null)
                return (b.target == null);

            if (b.target == null)
                return false;

            if (a.target.HasThing || b.target.HasThing)
            {
                return a.target.Thing == b.target.Thing;
            }
            if (a.target.Cell.IsValid || b.target.Cell.IsValid)
            {
                return a.target.Cell == b.target.Cell;
            }
            return true;
        }

        public static bool operator !=(SavedTargetInfo a, SavedTargetInfo b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SavedTargetInfo))
            {
                return false;
            }
            return Equals((SavedTargetInfo)obj);
        }

        public bool Equals(SavedTargetInfo other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            if (target.HasThing)
            {
                return target.Thing.GetHashCode();
            }
            return target.Cell.GetHashCode();
        }

        public override string ToString()
        {
            return target.ToString();
        }

        public void ExposeData()
        {
            Scribe_TargetInfo.Look(ref target, "target");
            Scribe_Defs.Look(ref blueprintTerrainDef, "blueprintTerrainDef");
            Scribe_Defs.Look(ref blueprintThingDef, "blueprintThingDef");
            Scribe_Defs.Look(ref blueprintStuff, "blueprintStuff");
            Scribe_Values.Look(ref blueprintRotation, "blueprintRotation");
            Scribe_References.Look(ref bill, "bill", false);

            Scribe_Defs.Look(ref trainable, "trainable");
        }

        public ConstructionStatus TargetConstructionStatus(Map map)
        {
            if (BuildableDef == null || target == null || target.HasThing || !target.IsValid)
                return ConstructionStatus.Invalid;

            if (target.Cell.GetTerrain(map) == blueprintTerrainDef)
                return ConstructionStatus.Complete;

            foreach(Thing thing in target.Cell.GetThingList(map))
            {
                Blueprint_Build blueprintBuild = thing as Blueprint_Build;
                Frame frame = thing as Frame;

                if (blueprintBuild != null && blueprintBuild.stuffToUse == blueprintStuff && thing.def.entityDefToBuild == BuildableDef)
                    return ConstructionStatus.InProgress;

                if (frame != null && thing.Stuff == blueprintStuff && thing.def.entityDefToBuild == BuildableDef)
                    return ConstructionStatus.InProgress;

                if (thing.Stuff == blueprintStuff && thing.def == blueprintThingDef && blueprintBuild == null && frame == null)
                    return ConstructionStatus.Complete;
            }

            if (blueprintTerrainDef != null && !GenConstruct.CanPlaceBlueprintAt(blueprintTerrainDef, target.Cell, blueprintRotation, map, false, null, null, blueprintStuff).Accepted)
            {
                return ConstructionStatus.Blocked;
            }

            if (blueprintThingDef != null && !GenConstruct.CanPlaceBlueprintAt(blueprintThingDef, target.Cell, blueprintRotation, map, false, null, null, blueprintStuff).Accepted)
            {
                return ConstructionStatus.Blocked;
            }

            return ConstructionStatus.None;
        }
    }
}
