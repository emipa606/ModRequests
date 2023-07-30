using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    public static class DesignatorUtility
    {
        private static Dictionary<Designator, DesignationDef> cachedDefs = new Dictionary<Designator, DesignationDef>();

        private static Dictionary<JobDriver_PlantWork, DesignationDef> cachedPlantWorkDefs = new Dictionary<JobDriver_PlantWork, DesignationDef>();

        private static List<Designator> cachedDesignators = null;

        private static List<Designation> savedDesignations = new List<Designation>();

        private static bool busy = false;

        public static bool getFudgedForWorkgiverCheck = false;

        public static bool getFudgedForToilCheck = false;

        public static DesignationDef lastCheckedDef = null;

        public static DesignationDef GetDesignationDef(this Designator designator)
        {
            if (cachedDefs.ContainsKey(designator))
                return cachedDefs[designator];

            PropertyInfo barGetter = typeof(Designator).GetProperty("Designation", BindingFlags.NonPublic | BindingFlags.Instance);
            DesignationDef designationDef = (DesignationDef)barGetter.GetValue(designator);

            if (designationDef != null)
                cachedDefs[designator] = designationDef;

            return designationDef;
        }

        public static DesignationDef GetRequiredDesignationDef(this JobDriver_PlantWork jobDriver)
        {
            if (cachedPlantWorkDefs.ContainsKey(jobDriver))
                return cachedPlantWorkDefs[jobDriver];

            PropertyInfo barGetter = typeof(JobDriver_PlantWork).GetProperty("RequiredDesignation", BindingFlags.NonPublic | BindingFlags.Instance);
            DesignationDef designationDef = (DesignationDef)barGetter.GetValue(jobDriver);

            if (designationDef != null)
                cachedPlantWorkDefs[jobDriver] = designationDef;

            return designationDef;
        }

        public static void ForceAllDesignationsOnCell(IntVec3 cell, Map map)
        {
            if (busy)
            {
                Logger.WarningFormat(cell, "Trying to force designations before restoring designations.");
                return;
            }

            busy = true;

            DesignatorUtility.getFudgedForWorkgiverCheck = true;

            if (cachedDesignators == null)
            {
                List<DesignationCategoryDef> allDesignationCategoryDefsForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
                cachedDesignators = allDesignationCategoryDefsForReading.SelectMany(category => category.ResolvedAllowedDesignators)
                                                                        .GroupBy(designator => designator.GetType())
                                                                        .Select(designatorGroup => designatorGroup.First())
                                                                        .Where(designator => designator as Designator_Area == null && 
                                                                               designator as Designator_Zone == null &&
                                                                               designator as Designator_Plan == null &&
                                                                              (designator.GetDesignationDef() != null ||
                                                                               designator as Designator_RemoveFloor != null))
                                                                        .ToList();

                foreach (Designator designator in cachedDesignators)
                {
                    // As of this writing, Designator_RemoveFloor and Designator_RemoveBridge have no designation def, so lets add them
                    if (designator as Designator_RemoveFloor != null && !cachedDefs.ContainsKey(designator))
                        cachedDefs[designator] = DesignationDefOf.RemoveFloor;
                    //Log.Message("Caching designator: " + designator.GetDesignationDef().defName);
                }
            }

            savedDesignations.Clear();

            List<Designation> allDesignations = map.designationManager.allDesignations;
            int count = allDesignations.Count;

            // First pull out all existing designations
            for (int i = count - 1; i >= 0; --i)
            {
                if (allDesignations[i].target.Cell == cell)
                {
                    savedDesignations.Add(allDesignations[i]);
                    allDesignations.RemoveAt(i);
                }
            }

            // Now add all temporary designations
            foreach (Designator designator in cachedDesignators)
            {
                DesignationDef designationDef = designator.GetDesignationDef();
                if (designationDef == null)
                    continue;

                if (designationDef.targetType == TargetType.Cell)
                {
                    if (designator.CanDesignateCell(cell).Accepted)
                        allDesignations.Add(new Designation(cell, designationDef));
                }
                else if (designationDef.targetType == TargetType.Thing)
                {
                    foreach (Thing thing in cell.GetThingList(map))
                    {
                        if (designator.CanDesignateThing(thing).Accepted)
                            allDesignations.Add(new Designation(thing, designationDef));
                    }
                }
            }
        }

        public static void ForceDesignationOnThingsInCell(IntVec3 cell, Map map, DesignationDef designationDef, Func<Thing, bool> validator = null)
        {
            if (designationDef == null)
                return;

            if (busy)
            {
                Logger.WarningFormat(cell, "Trying to force a designation before restoring designations.");
                return;
            }

            busy = true;

            DesignatorUtility.getFudgedForWorkgiverCheck = true;

            savedDesignations.Clear();

            List<Designation> allDesignations = map.designationManager.allDesignations;
            int count = allDesignations.Count;

            // First pull out all existing designations
            for (int i = count - 1; i >= 0; --i)
            {
                if (allDesignations[i].target.Cell == cell)
                {
                    savedDesignations.Add(allDesignations[i]);
                    allDesignations.RemoveAt(i);
                }
            }

            if (designationDef.targetType == TargetType.Thing)
            {
                foreach (Thing thing in cell.GetThingList(map))
                {
                    if (validator == null || validator(thing))
                        allDesignations.Add(new Designation(thing, designationDef));
                }
            }
        }

        public static void RestoreDesignationsOnCell(IntVec3 cell, Map map)
        {
            if (!busy)
            {
                Logger.MessageFormat(cell, "Trying to restore designations without having forced them.");
                return;
            }

            DesignatorUtility.getFudgedForWorkgiverCheck = false;

            // Take of designations on cell, but also designations on buildings that intersect the cell
            List<Thing> thingsHere = cell.GetThingList(map);
            List<Designation> allDesignations = map.designationManager.allDesignations.Where(designation => designation.target.Cell != cell && (!designation.target.HasThing || !thingsHere.Contains(designation.target.Thing))).ToList();
            allDesignations.AddRange(savedDesignations);
            map.designationManager.allDesignations = allDesignations;

            savedDesignations.Clear();

            busy = false;
        }
    }

}
