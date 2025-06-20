using HarmonyLib;
using UnityEngine;
using Verse;

namespace CashRegister;

[StaticConstructorOnStartup]
public class Mod_CashRegister : Mod
{
    private static ModSettings_CashRegister settings;

    public Mod_CashRegister(ModContentPack content) : base(content)
    {
        settings = GetSettings<ModSettings_CashRegister>();
        var harmony = new Harmony(Content.PackageIdPlayerFacing);
        harmony.PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        settings.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory() => "Cash Register";

    public override void WriteSettings()
    {
        base.WriteSettings();
    }
}