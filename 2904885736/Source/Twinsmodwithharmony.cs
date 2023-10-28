using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;

namespace RimTwins
{
    [StaticConstructorOnStartup]

    public static class RimTwins
    {
        public static Harmony harmonyInstance;


        static RimTwins()
        {
            // Initialize Harmony
            harmonyInstance = new Harmony("com.spijkermenno.rimTwins");

            Log.Message("RimTwins is loaded...");

            // Apply the Harmony patch
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}