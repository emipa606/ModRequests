// Nightvision NightVision Harmony_Patches.cs
// 
// 30 03 2018
// 
// 21 07 2018

#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif

using NightVision.Harmony.Manual;
using RimWorld;
using System.Reflection;
using Verse;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MissingAnnotation
// ReSharper disable RegionWithSingleElement
// ReSharper disable All

// ReSharper disable InconsistentNaming

namespace NightVision
{
#if DEBUG
            [HarmonyDebug]
#endif
    [StaticConstructorOnStartup]
    public static class NVHarmonyPatcher
    {
#if RW10
        public static HarmonyInstance NVHarmony;
#else
        public static HarmonyLib.Harmony NVHarmony;
#endif

        static NVHarmonyPatcher()
        {
#if RW10
            NVHarmony = HarmonyInstance.Create("drumad.rimworld.nightvision");
#else
            NVHarmony = new HarmonyLib.Harmony("drumad.rimworld.nightvision");

#endif

            MethodInfo addHediffMethod = AccessTools.Method(
                                                                    typeof(Pawn_HealthTracker),
                                                                    nameof(Pawn_HealthTracker.AddHediff),
                                                                    new[]
                                                                    {
                                                                        typeof(Hediff),
                                                                        typeof(BodyPartRecord),
                                                                        typeof(DamageInfo),
                                                                        typeof(DamageWorker.DamageResult)
                                                                    }
                                                                   );

            MethodInfo tryDropMethod = AccessTools.Method(
                                                                   typeof(Pawn_ApparelTracker),
                                                                   nameof(Pawn_ApparelTracker.TryDrop),
                                                                   new[]
                                                                   {
                                                                       typeof(Apparel),
                                                                       typeof(Apparel).MakeByRefType(),
                                                                       typeof(IntVec3),
                                                                       typeof(bool)
                                                                   }
                                                                  );

            NVHarmony.Patch(
                addHediffMethod,
                          null,
                          new HarmonyMethod(typeof(PawnHealthTracker_AddHediff), nameof(PawnHealthTracker_AddHediff.AddHediff_Postfix))
                         );

            NVHarmony.Patch(
                tryDropMethod,
                          null,
                          new HarmonyMethod(typeof(ApparelTracker_TryDrop), nameof(ApparelTracker_TryDrop.Postfix))
                         );


            NVHarmony.PatchAll();

        }

    }
}