using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RadWorld
{
    class RadWorldMod : Mod
    {
        public static RadWorldSettings settings;
        public RadWorldMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<RadWorldSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "RadWorld";
        }
        public override void WriteSettings()
        {
            base.WriteSettings();
            DefsRemover.DoDefsRemoval();
        }
    }

    [StaticConstructorOnStartup]
    public static class DefsRemover
    {
        static DefsRemover()
        {
            DoDefsRemoval();
        }
        public static void DoDefsRemoval()
        {
            if (RadWorldMod.settings.disableGasMaskFilters)
            {
                foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
                {
                    var compProps = thingDef.GetCompProperties<CompProperties_Reloadable>();
                    if (compProps != null && compProps.compClass == typeof(CompGasMaskReloadable))
                    {
                        thingDef.comps.Remove(compProps);
                    }
                }
                DefDatabase<RecipeDef>.AllDefsListForReading.RemoveAll(x => x.ProducedThingDef == RW_DefOf.RW_GasMaskFilter);
                DefDatabase<ThingDef>.AllDefsListForReading.Remove(RW_DefOf.RW_GasMaskFilter);
            }
        }
    }
}