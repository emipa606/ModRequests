using CompTanker.Compat;
using HarmonyLib;
using JetBrains.Annotations;
using Multiplayer.API;
using System.Linq;
using CompTankerCompat.HarmonyPatches;
using Verse;

namespace CompTanker
{
    [UsedImplicitly]
    public class CompTankerMod : Mod
    {
        public CompTankerMod(ModContentPack content) : base(content)
        {
            if (MP.enabled)
                MP.RegisterAll();

            if (IsModLoaded("dubwise.dubsbadhygiene"))
                BadHygieneCompat.Init();
            if (IsModLoaded("dubwise.rimefeller"))
                RimefellerCompat.Init();
            if (IsModLoaded("vanillaexpanded.helixiengas"))
                VanillaHelixienGasExpanded.Init();

            LongEventHandler.ExecuteWhenFinished(() =>
            {
                Harmony harmony = new Harmony("Dra.CompTankerMod");
                harmony.PatchAll();
                if(RimefellerCompat.IsActive) Harmony_Rimefeller_DrawOverlay.Patch(harmony);
                if(BadHygieneCompat.IsActive) Harmony_BadHygiene_DrawOverlay.Patch(harmony);
            });

#if DEBUG
            //ReferenceBuilder.Restore(content);
#endif
        }

        private static bool IsModLoaded(string s)
            => LoadedModManager.RunningMods
                .Any(x => x.PackageId.ToLower().NoModIdSuffix() == s);
    }
}