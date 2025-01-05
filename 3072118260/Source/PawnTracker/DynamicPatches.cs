using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld.QuestGen;
using System.Reflection;
using PawnTrackerMain;
using static PawnTrackerMain.PawnDeathDetails;
using static PawnTrackerMain.DocumentedEventDefOf;
using static PawnTrackerMain.PawnTrackerUtility;

namespace PawnTrackerMain
{
    public static class DynamicPatches
    {
        public static void PatchIfModActive(string modIdentifier, string targetNamespace, Harmony harmony)
        {
            if (ModsConfig.IsActive(modIdentifier))
            {
                Assembly modAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name == modIdentifier);

                if (modAssembly != null)
                {
                    Type[] allTypes = modAssembly.GetTypes();
                    foreach (Type type in allTypes)
                    {
                        if (type.Namespace == targetNamespace)
                        {
                            MethodInfo[] methodsToPatch = type.GetMethods();
                            foreach (MethodInfo method in methodsToPatch)
                            {
                                MethodInfo prefix = type.GetMethod("Prefix");
                                MethodInfo postfix = type.GetMethod("Postfix");

                                harmony.Patch(method,
                                    prefix != null ? new HarmonyMethod(prefix) : null,
                                    postfix != null ? new HarmonyMethod(postfix) : null);
                            }
                        }
                        
                    }
                }
            }
        }
    }
}