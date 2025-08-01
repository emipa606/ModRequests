﻿using HarmonyLib;
using StartupImpact.Patch;
using System;
using System.CodeDom;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Verse;

namespace StartupImpact
{
    public class StartupImpact :Mod
    {
        public static int mainThreadId;
        public static ModInfoList modlist = new ModInfoList();
        public static bool loadingTimeMeasured = false;
        public static int loadingTime = 0;
        public static ProfilerTickCount loadingProfiler;
        public static Profiler baseGameProfiler;
        public static StartupImpactSettings settings;

        CodeCompileUnit compileUnit = new CodeCompileUnit();

        public StartupImpact(ModContentPack pack) :base(pack) {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;

            settings = GetSettings<StartupImpactSettings>();
            baseGameProfiler = new Profiler("base game");
            loadingProfiler = new ProfilerTickCount();

            loadingProfiler.Start("loading");

            var harmony = new Harmony("com.github.automatic1111.startupimpact");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            ModConstructor.Create();

            if (settings.resolveReferences) {
                foreach (Type type in typeof(Def).AllSubclasses())
                {
                    var method = AccessTools.Method(type, "ResolveReferences");
                    if (method != null && method.DeclaringType == type)
                    {
                        harmony.Patch(method, new HarmonyMethod(typeof(ResolveReferences), "Prefix"), new HarmonyMethod(typeof(ResolveReferences), "Postfix"));
                    }
                }
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            if (listing.RadioButton("StartupImpactProfilerTicksLabel".Translate(), settings.profilerType == ProfilerType.Ticks, 0, "StartupImpactProfilerTicksTooltip".Translate()))
            {
                settings.profilerType = ProfilerType.Ticks;
            }

            if (listing.RadioButton("StartupImpactProfilerDateLabel".Translate(), settings.profilerType == ProfilerType.Date, 0, "StartupImpactProfilerDateTooltip".Translate()))
            {
                settings.profilerType = ProfilerType.Date;
            }

            if (listing.RadioButton("StartupImpactProfilerStopwatchLabel".Translate(), settings.profilerType == ProfilerType.Stopwatch, 0, "StartupImpactProfilerStopwatchTooltip".Translate()))
            {
                settings.profilerType = ProfilerType.Stopwatch;
            }

            listing.CheckboxLabeled("StartupImpactResolveReferencesLabel".Translate(), ref settings.resolveReferences, "StartupImpactResolveReferencesTooltip".Translate());

            listing.End();
        }

        public override string SettingsCategory()
        {
            return "StartupImpact".Translate();
        }

        public static string GetVersion() {
            try
            {
                return Assembly.GetAssembly(typeof(StartupImpact)).GetName().Version.ToString();
            }
            catch (Exception e) {
                return e.Message;
            }
        }
    }
}
