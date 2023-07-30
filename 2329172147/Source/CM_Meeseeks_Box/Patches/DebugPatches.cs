using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    [StaticConstructorOnStartup]
    public static class DebugPatches
    {
        [HarmonyPatch(typeof(EditWindow_DebugInspector))]
        [HarmonyPatch("CurrentDebugString", MethodType.Normal)]
        public static class EditWindow_DebugInspector_CurrentDebugString_WriteMeeseeksInfo
        {
            private static string GetTargetString(string targetName, LocalTargetInfo targetInfo)
            {
                if (targetInfo != null && targetInfo.IsValid)
                {
                    if (targetInfo.HasThing)
                    {
                        return String.Format("{0}: {1}", targetName, targetInfo.Label);
                    }
                    else if (targetInfo.Cell != null)
                    {
                        return String.Format("{0}: {1}", targetName, targetInfo.Cell.ToString());
                    }
                }

                return null;
            }

            [HarmonyPostfix]
            public static void MonitorMeeseeksJob(EditWindow_DebugInspector __instance, ref string __result)
            {
                if (MeeseeksMod.settings.showDebugLogMessages)
                {
                    Map map = Find.CurrentMap;

                    if (map != null)
                    {
                        List<Pawn> allMeeseeks = map.mapPawns.AllPawnsSpawned.Where(p => p.GetComp<CompMeeseeksMemory>() != null).ToList();

                        if (allMeeseeks.Count > 0)
                        {
                            StringBuilder stringBuilder = new StringBuilder(__result);

                            foreach(Pawn imMrMeeseeksLookAtMe in allMeeseeks)
                            {
                                CompMeeseeksMemory meeseeksMemory = imMrMeeseeksLookAtMe.GetComp<CompMeeseeksMemory>();
                                Job currentJob = imMrMeeseeksLookAtMe.CurJob;
                                SavedJob savedJob = meeseeksMemory.savedJob;

                                stringBuilder.AppendLine(imMrMeeseeksLookAtMe.Name.ToStringFull);
                                if (currentJob != null)
                                {
                                    stringBuilder.AppendLine(String.Format("Current job: {0}", currentJob.def.defName));
                                    stringBuilder.AppendLine(meeseeksMemory.givenTaskTick.ToString());

                                    if (currentJob.workGiverDef != null && currentJob.workGiverDef.workType.relevantSkills != null)
                                    {
                                        foreach (SkillDef skillDef in currentJob.workGiverDef.workType.relevantSkills)
                                            stringBuilder.AppendLine(skillDef.skillLabel);
                                    }

                                    string targetAString = GetTargetString("Target A", currentJob.targetA);
                                    string targetBString = GetTargetString("Target B", currentJob.targetB);
                                    string targetCString = GetTargetString("Target C", currentJob.targetC);

                                    if (targetAString != null) stringBuilder.AppendLine(targetAString);
                                    if (targetBString != null) stringBuilder.AppendLine(targetBString);
                                    if (targetCString != null) stringBuilder.AppendLine(targetCString);
                                }

                                if (meeseeksMemory.jobStuck)
                                    stringBuilder.AppendLine("***JOB STUCK***");

                                if (savedJob != null)
                                {
                                    stringBuilder.AppendLine(String.Format("Saved job: {0}", savedJob.def.defName));

                                    if (savedJob.workGiverDef != null && savedJob.workGiverDef.workType.relevantSkills != null)
                                    {
                                        stringBuilder.AppendLine(savedJob.workGiverDef.defName);
                                        foreach (SkillDef skillDef in savedJob.workGiverDef.workType.relevantSkills)
                                            stringBuilder.AppendLine(skillDef.skillLabel);
                                    }

                                    string targetAString = GetTargetString("Target A", savedJob.targetA);
                                    string targetBString = GetTargetString("Target B", savedJob.targetB);
                                    string targetCString = GetTargetString("Target C", savedJob.targetC);

                                    if (targetAString != null) stringBuilder.AppendLine(targetAString);
                                    if (targetBString != null) stringBuilder.AppendLine(targetBString);
                                    if (targetCString != null) stringBuilder.AppendLine(targetCString);
                                }

                                foreach (SavedTargetInfo jobTarget in meeseeksMemory.jobTargets)
                                {
                                    stringBuilder.AppendLine(jobTarget.target.ToString());
                                    if (jobTarget.BuildableDef != null)
                                    {
                                        string nextLine = " - " + jobTarget.BuildableDef.defName;
                                        if (jobTarget.blueprintStuff != null)
                                            nextLine += " - " + jobTarget.blueprintStuff.defName;
                                        stringBuilder.AppendLine(nextLine);
                                    }
                                    if (jobTarget.bill != null)
                                    {
                                        Bill_Production billProduction = jobTarget.bill as Bill_Production;

                                        if (billProduction != null)
                                        {
                                            string nextLine = " - " + billProduction.ToString();
                                            if (billProduction.repeatMode == BillRepeatModeDefOf.RepeatCount)
                                                nextLine += " - x" + billProduction.repeatCount;
                                            else if (billProduction.repeatMode == BillRepeatModeDefOf.TargetCount)
                                                nextLine += " - until " + billProduction.targetCount;
                                            else
                                                nextLine += " - forever";
                                            stringBuilder.AppendLine(nextLine);
                                        }
                                    }
                                }

                                stringBuilder.AppendLine("");
                            }

                            __result = stringBuilder.ToString();
                        }
                    }
                }
            }
        }
    }
}
