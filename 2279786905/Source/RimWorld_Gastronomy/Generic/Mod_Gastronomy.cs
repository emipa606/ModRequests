using CashRegister;
using HarmonyLib;
using Verse;

namespace Gastronomy
{
    [StaticConstructorOnStartup]
    public class Mod_Gastronomy : Mod
    {
        public Mod_Gastronomy(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony(this.Content.PackageIdPlayerFacing);
            harmony.PatchAll();
            PostLoad();
        }
        //public override void DoSettingsWindowContents(Rect inRect)
        //{
        //    settings.DoSettingsWindowContents(inRect);
        //}
        public override void WriteSettings()
        {
            base.WriteSettings();
        }
        public void PostLoad()
        {
            LongEventHandler.ExecuteWhenFinished(() => CheckRegister());
        }

        public void CheckRegister()
        {
            if (GenericDefOf.CashRegister_CashRegister == null)
            {
                GenUI.ErrorDialog("ErrorRequiresCashRegister".Translate());
            }
        }
    }
}