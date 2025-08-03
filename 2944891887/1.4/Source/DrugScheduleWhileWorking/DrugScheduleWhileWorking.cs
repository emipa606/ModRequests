using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace DrugScheduleWhileWorking;

public class DrugScheduleWhileWorking : Mod
{
    readonly DrugScheduleWhileWorkingSettings settings;
    public DrugScheduleWhileWorking(ModContentPack content) : base(content)
    {
        
        settings = GetSettings<DrugScheduleWhileWorkingSettings>();
        
        
        var harmony = new Harmony("com.arquebus.rimworld.mod.drugschedulewhilesorking");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        SettingsDrawUtil.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "DrugScheduleWhileWorkingSettingsTitle".Translate();
    }
}
