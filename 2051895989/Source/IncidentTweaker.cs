using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace IncidentTweaker
{
    public class IncidentTweaker : Mod
    {
        public static IncidentTweakerSettings settings;

        public IncidentTweaker(ModContentPack pack) : base(pack)
        {
            new Harmony("com.github.automatic1111.incidenttweaker").PatchAll(Assembly.GetExecutingAssembly());

            settings = GetSettings<IncidentTweakerSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "IncidentTweakerTitle".Translate();
        }
    }
}
